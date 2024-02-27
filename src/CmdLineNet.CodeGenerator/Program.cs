namespace CmdLineNet.CodeGenerator;

using IniFileNet;
using IniFileNet.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class Program
{
	public static string Get(this IniValueAcceptorOnlyLast v, string key)
	{
		return v.Value ?? throw new IniException(IniErrorCode.ValueMissing, "Value cannot be empty: " + key);
	}
	public static int Main(string[] args)
	{
		if (args.Length == 0)
		{
			Console.WriteLine("Usage: config.ini [outputfile.g.cs]");
			return 1;
		}
		string iniFile;
		if (args.Length >= 1)
		{
			iniFile = args[0];
		}
		else
		{
			Console.WriteLine("Usage: config.ini [outputfile.g.cs]");
			return 1;
		}
		try
		{
			if (args.Length == 2)
			{
				using var s = new FileStream(args[1], FileMode.Create, FileAccess.Write);
				return DoTheThing(iniFile, s);
			}
			else
			{
				using var s = Console.OpenStandardOutput();
				return DoTheThing(iniFile, s);
			}
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine(ex.ToString());
			return 2;
		}
	}
	public static int DoTheThing(string iniFilePath, Stream sout)
	{
		Dictionary<string, TypeMeta> typeMeta;
		{
			TypeMeta charType = new("char.TryParse", true, ParseMethodReturnType.Boolean, false);
			TypeMeta stringType = new(null, false, ParseMethodReturnType.Boolean, false);
			TypeMeta boolType = new("bool.TryParse", true, ParseMethodReturnType.Boolean, false);
			TypeMeta sbyteType = new("sbyte.TryParse", true, ParseMethodReturnType.Boolean, true);
			TypeMeta byteType = new("byte.TryParse", true, ParseMethodReturnType.Boolean, true);
			TypeMeta shortType = new("short.TryParse", true, ParseMethodReturnType.Boolean, true);
			TypeMeta ushortType = new("ushort.TryParse", true, ParseMethodReturnType.Boolean, true);
			TypeMeta intType = new("int.TryParse", true, ParseMethodReturnType.Boolean, true);
			TypeMeta uintType = new("uint.TryParse", true, ParseMethodReturnType.Boolean, true);
			TypeMeta longType = new("long.TryParse", true, ParseMethodReturnType.Boolean, true);
			TypeMeta ulongType = new("ulong.TryParse", true, ParseMethodReturnType.Boolean, true);
			TypeMeta decimalType = new("decimal.TryParse", true, ParseMethodReturnType.Boolean, false);
			TypeMeta floatType = new("float.TryParse", true, ParseMethodReturnType.Boolean, false);
			TypeMeta doubleType = new("double.TryParse", true, ParseMethodReturnType.Boolean, false);
			TypeMeta dateTimeType = new("DateTime.TryParse", true, ParseMethodReturnType.Boolean, false);
			TypeMeta dateOnlyType = new("DateOnly.TryParse", true, ParseMethodReturnType.Boolean, false);
			TypeMeta dateTimeOffsetType = new("DateTimeOffset.TryParse", true, ParseMethodReturnType.Boolean, false);
			TypeMeta guidType = new("Guid.TryParse", true, ParseMethodReturnType.Boolean, false);
			typeMeta = new()
			{
				["char"] = charType,
				["Char"] = charType,
				["System.Char"] = charType,

				["string"] = stringType,
				["String"] = stringType,
				["System.String"] = stringType,

				["bool"] = boolType,
				["Boolean"] = boolType,
				["System.Boolean"] = boolType,

				["sbyte"] = sbyteType,
				["SByte"] = sbyteType,
				["System.SByte"] = sbyteType,

				["byte"] = byteType,
				["Byte"] = byteType,
				["System.Byte"] = byteType,

				["short"] = shortType,
				["Int16"] = shortType,
				["System.Int16"] = shortType,

				["ushort"] = ushortType,
				["UInt16"] = ushortType,
				["System.UInt16"] = ushortType,

				["int"] = intType,
				["Int32"] = intType,
				["System.Int32"] = intType,

				["uint"] = uintType,
				["UInt32"] = uintType,
				["System.UInt32"] = uintType,

				["long"] = longType,
				["Int64"] = longType,
				["System.Int64"] = longType,

				["ulong"] = ulongType,
				["UInt64"] = ulongType,
				["System.UInt64"] = ulongType,

				["decimal"] = decimalType,
				["Decimal"] = decimalType,
				["System.Decimal"] = decimalType,

				["float"] = floatType,
				["Single"] = floatType,
				["System.Single"] = floatType,

				["double"] = doubleType,
				["Double"] = doubleType,
				["System.Double"] = doubleType,

				["DateTime"] = dateTimeType,
				["System.DateTime"] = dateTimeType,

				["DateOnly"] = dateOnlyType,
				["System.DateOnly"] = dateOnlyType,

				["DateTimeOffset"] = dateTimeOffsetType,
				["System.DateTimeOffset"] = dateTimeOffsetType,

				["Guid"] = guidType,
				["System.Guid"] = guidType,
			};
		}
		string? ns = null;
		string? className = null;
		Indent indent = new(1, '\t');
		List<ValidatedArg> vargs = [];
		IniValueAcceptorDictionaryBuilder b = new(new(StringComparer.OrdinalIgnoreCase));
		var iVerbNamespace = b.OnlyLast("Namespace");
		var iVerbClass = b.OnlyLast("Class");
		var iVerbIndent = b.OnlyLast("Indent");
		var verbAcceptors = b.Acceptors;

		b = new(new(StringComparer.OrdinalIgnoreCase));
		var iParserType = b.OnlyLast("Type");
		var iParserParseMethod = b.OnlyLast("ParseMethod");
		var iParserIsValueType = b.OnlyLast("IsValueType", Parse.Boolean);
		var iIsIntegralType = b.OnlyLast("IsIntegralType", Parse.Boolean);
		var iParserReturnType = b.OnlyLast("ReturnType");
		var parserAcceptors = b.Acceptors;

		b = new(new(StringComparer.OrdinalIgnoreCase));
		var iArgName = b.OnlyLast("Name");
		var iArgType = b.OnlyLast("Type");
		var iArgRequired = b.OnlyLast("Required", Parse.Boolean);
		var iArgFriendlyName = b.OnlyLast("FriendlyName");
		var iArgLongName = b.OnlyLast("LongName");
		var iArgShortName = b.OnlyLast("ShortName", x => char.TryParse(x, out char c) ? new IniResult<char>(c, default) : new IniResult<char>(default, new(IniErrorCode.ValueInvalid, "Could not parse \"" + x + "\" as char")));
		var iArgMin = b.OnlyLast("Min", Parse.Int32);
		var iArgMax = b.OnlyLast("Max", x => x.Length == 0 ? new IniResult<int>(int.MaxValue, default) : int.TryParse(x, out int v) ? new IniResult<int>(v, default) : new IniResult<int>(default, new(IniErrorCode.ValueInvalid, "Could not parse \"" + x + "\" as int")));
		var iArgDefault = b.OnlyLast("Default");
		var iArgHelp = b.OnlyLast("Help");
		var argAcceptors = b.Acceptors;

		b = null!;
		using (IniStreamSectionReader ini = new(new IniStreamReader(new StreamReader(iniFilePath), new IniReaderOptions(allowLineContinuations: true, ignoreComments: true))))
		{
			while (ini.TryReadNext(out var sec))
			{
				ArgType argType;
				switch (sec.Name)
				{
					case "Type":
						// When we see a new type, we have to know what method call we need to use to be able to parse that type.
						Util.ResetAll(parserAcceptors.Values);
						sec.AcceptAll(parserAcceptors).ThrowIfError();
						var type = iParserType.Get("Type");
						var parseMethod = iParserParseMethod.Get("Method");
						// Default for integral type and returning error message is false
						ParseMethodReturnType pmrt;
						if (iParserReturnType.Value != null)
						{
							switch (iParserReturnType.Value)
							{
								case "bool":
								case "Boolean":
								case "System.Boolean":
									pmrt = ParseMethodReturnType.Boolean;
									break;
								case "string":
								case "string?":
								case "String":
								case "String?":
								case "System.String":
								case "System.String?":
									pmrt = ParseMethodReturnType.String;
									break;
								case "ParseResult":
								case "CmdLineNet.ParseResult":
									pmrt = ParseMethodReturnType.ParseResult;
									break;
								default:
									throw new IniException(IniErrorCode.ValueInvalid, "ReturnType must be bool, string, or ParseResult: " + iParserReturnType.Value);
							}
						}
						else
						{
							pmrt = ParseMethodReturnType.Boolean;
						}

						typeMeta[type] = new TypeMeta(parseMethod, iParserIsValueType.Value, pmrt, iIsIntegralType.Value);
						continue;
					case "Verb":
						Util.ResetAll(verbAcceptors.Values);
						sec.AcceptAll(verbAcceptors).ThrowIfError();
						ns = iVerbNamespace.Get("Namespace");
						className = iVerbClass.Get("Class");

						if (iVerbIndent.Value != null)
						{
							string[] parts = iVerbIndent.Value.Split(' ', StringSplitOptions.TrimEntries);
							if (parts.Length == 2)
							{
								if (int.TryParse(parts[0], out int amt))
								{
									var c = parts[1] switch
									{
										"tab" or "tabs" => '\t',
										"space" or "spaces" => ' ',
										_ => throw new IniException(IniErrorCode.ValueInvalid, "Indent must be a number and either tab or space, like \"1 tab\" or \"3 space\": " + iVerbIndent.Value),
									};
									indent = new(amt, c);
								}
								else
								{
									throw new IniException(IniErrorCode.ValueInvalid, "Indent must be a number and either tab or space, like \"1 tab\" or \"3 space\": " + iVerbIndent.Value);
								}
							}
							else
							{
								throw new IniException(IniErrorCode.ValueInvalid, "Indent must be a number and either tab or space, like \"1 tab\" or \"3 space\": " + iVerbIndent.Value);
							}
						}

						continue;
					case "Option":
						argType = ArgType.Option;
						Util.ResetAll(argAcceptors.Values);
						iArgMin.Value = 1;
						iArgMax.Value = 1;
						sec.AcceptAll(argAcceptors).ThrowIfError();
						if (string.IsNullOrEmpty(iArgLongName.Value) && !iArgShortName.HaveValue)
						{
							throw new IniException(IniErrorCode.ValueMissing, "LongName or ShortName must be provided for an Option: " + iArgName.Value);
						}
						break;
					case "Switch":
						argType = ArgType.Switch;
						Util.ResetAll(argAcceptors.Values);
						iArgMin.Value = 1;
						iArgMax.Value = 1;
						sec.AcceptAll(argAcceptors).ThrowIfError();
						if (string.IsNullOrEmpty(iArgLongName.Value) && !iArgShortName.HaveValue)
						{
							throw new IniException(IniErrorCode.ValueMissing, "LongName or ShortName must be provided for a swtich: " + iArgName.Value);
						}
						break;
					case "Value":
						argType = ArgType.Value;
						Util.ResetAll(argAcceptors.Values);
						iArgMin.Value = 1;
						iArgMax.Value = 1;
						sec.AcceptAll(argAcceptors).ThrowIfError();
						if (!string.IsNullOrEmpty(iArgLongName.Value) || iArgShortName.HaveValue)
						{
							throw new IniException(IniErrorCode.ValueInvalid, "LongName or ShortName must NOT be provided for a value: " + iArgName.Value);
						}
						break;
					default:
						throw new IniException(IniErrorCode.ValueInvalid, "Section name must be either Verb, Option, Switch, or Value, but was " + iArgName.Value);
				}
				if (iArgMin.Value > iArgMax.Value)
				{
					throw new IniException(IniErrorCode.ValueInvalid, "Min was larger than Max " + iArgName.Value);
				}
				string name = iArgName.Get("Name");
				string typeName;
				if (iArgDefault.Value == "")
				{
					iArgDefault.Value = null;
				}
				// If it's a switch, then we require a boolean for singular switches or an integral type for multiple switches
				bool required;
				TypeMeta? tm;
				if (argType == ArgType.Switch)
				{
					// Switches are only required if we explicitly say "true"
					typeName = iArgType.Value ?? "bool";
					typeMeta.TryGetValue(typeName, out tm);
					required = iArgRequired.HaveValue && iArgRequired.Value;
					if (tm == null)
					{
						throw new IniException(IniErrorCode.ValueInvalid, "Switches have to be either bool or an integral type: " + iArgName.Value);
					}
					if (iArgMax.Value == 1)
					{
						if (typeName != "bool" && typeName != "Boolean" && typeName != "System.Boolean")
						{
							throw new IniException(IniErrorCode.ValueInvalid, "Switches that accept only 1 value must be of type bool: " + iArgName.Value);
						}
						iArgDefault.Value ??= "false";
					}
					if (iArgMax.Value > 1)
					{
						if (!tm.IntegralType)
						{
							throw new IniException(IniErrorCode.ValueInvalid, "Switches that accept many values must be an integral type: " + iArgName.Value);
						}
						iArgDefault.Value ??= "0";
					}
				}
				else
				{
					typeName = iArgType.Value ?? "string";
					typeMeta.TryGetValue(typeName, out tm);
					// By default, options and values are required
					required = !iArgRequired.HaveValue || iArgRequired.Value;
				}

				string initVal;
				bool localTypeNullable;
				if (iArgDefault.Value != null)
				{
					localTypeNullable = false;
					initVal = iArgDefault.Value;
				}
				else
				{
					if (iArgMax.Value == 1)
					{
						localTypeNullable = true;
						initVal = "null";
					}
					else
					{
						localTypeNullable = false;
						initVal = string.Concat("new List<", typeName, ">()");
					}
				}

				vargs.Add(new ValidatedArg
				(
					Name: name,
					ArgType: argType,
					TypeName: typeName,
					TypeMeta: tm,
					Required: required,
					CtorTypeNullable: !required && iArgDefault.Value == null,
					LocalTypeNullable: localTypeNullable,
					LongName: iArgLongName.Value,
					ShortName: iArgShortName.HaveValue ? iArgShortName.Value : null,
					FriendlyName: iArgFriendlyName.Value?.Replace("\"", "\\\""),
					Min: iArgMin.Value,
					Max: iArgMax.Value,
					InitialValue: initVal,
					Help: iArgHelp.Value?.Replace("\"", "\\\"")
				));
			}
			ini.Reader.Error.ThrowIfError();
			if (ns == null) throw new IniException(IniErrorCode.ValueMissing, "Namespace is null");
			if (className == null) throw new IniException(IniErrorCode.ValueMissing, "Class Name is null");
		}

		using StreamWriter outputStream = new(sout, new UTF8Encoding(false, false));
		VerbWriter.WriteVerb(ns, className, vargs, outputStream, indent);
		return 0;
	}
}