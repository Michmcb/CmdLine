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
				if (args[1] == "-")
				{
					using Stream s = Console.OpenStandardOutput();
					return DoTheThing(iniFile, s);
				}
				else
				{
					using FileStream s = new(args[1], FileMode.Create, FileAccess.Write);
					return DoTheThing(iniFile, s);
				}
			}
			else
			{
				string path = Path.ChangeExtension(args[0], ".g.cs");
				using FileStream s = new(path, FileMode.Create, FileAccess.Write);
				return DoTheThing(iniFile, s);
			}
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine(ex.ToString());
			return 2;
		}
	}
	public static ParseMethodReturnType ParseParseMethodReturnType(string? v, string keyName)
	{
		if (v == null) return ParseMethodReturnType.Boolean;
		switch (v)
		{
			case "bool":
			case "Boolean":
			case "System.Boolean":
				return ParseMethodReturnType.Boolean;
			case "string":
			case "string?":
			case "String":
			case "String?":
			case "System.String":
			case "System.String?":
				return ParseMethodReturnType.String;
			case "ParseResult":
			case "CmdLineNet.ParseResult":
				return ParseMethodReturnType.ParseResult;
			default:
				throw new IniException(IniErrorCode.ValueInvalid, string.Concat(keyName, " must be bool, string, or ParseResult: ", v));
		}
	}
	public static int DoTheThing(string iniFilePath, Stream sout)
	{
		const string stringAlias = "string";
		const string boolAlias = "bool";
		const string boolTypeName = "Boolean";
		const string boolFullTypeName = "System.Boolean";

		Dictionary<string, TypeMeta> typeMeta;
		{
			TypeMeta charType = new("char.TryParse", ParseMethodReturnType.Boolean, true, false);
			TypeMeta stringType = new(null, ParseMethodReturnType.Boolean, false, false);
			TypeMeta boolType = new("bool.TryParse", ParseMethodReturnType.Boolean, true, false);
			TypeMeta sbyteType = new("sbyte.TryParse", ParseMethodReturnType.Boolean, true, true);
			TypeMeta byteType = new("byte.TryParse", ParseMethodReturnType.Boolean, true, true);
			TypeMeta shortType = new("short.TryParse", ParseMethodReturnType.Boolean, true, true);
			TypeMeta ushortType = new("ushort.TryParse", ParseMethodReturnType.Boolean, true, true);
			TypeMeta intType = new("int.TryParse", ParseMethodReturnType.Boolean, true, true);
			TypeMeta uintType = new("uint.TryParse", ParseMethodReturnType.Boolean, true, true);
			TypeMeta longType = new("long.TryParse", ParseMethodReturnType.Boolean, true, true);
			TypeMeta ulongType = new("ulong.TryParse", ParseMethodReturnType.Boolean, true, true);
			TypeMeta decimalType = new("decimal.TryParse", ParseMethodReturnType.Boolean, true, false);
			TypeMeta floatType = new("float.TryParse", ParseMethodReturnType.Boolean, true, false);
			TypeMeta doubleType = new("double.TryParse", ParseMethodReturnType.Boolean, true, false);
			TypeMeta dateTimeType = new("DateTime.TryParse", ParseMethodReturnType.Boolean, true, false);
			TypeMeta dateOnlyType = new("DateOnly.TryParse", ParseMethodReturnType.Boolean, true, false);
			TypeMeta dateTimeOffsetType = new("DateTimeOffset.TryParse", ParseMethodReturnType.Boolean, true, false);
			TypeMeta guidType = new("Guid.TryParse", ParseMethodReturnType.Boolean, true, false);
			typeMeta = new()
			{
				["char"] = charType,
				["Char"] = charType,
				["System.Char"] = charType,

				[stringAlias] = stringType,
				["String"] = stringType,
				["System.String"] = stringType,

				[boolAlias] = boolType,
				[boolTypeName] = boolType,
				[boolFullTypeName] = boolType,

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
		string newline = "\n";
		Indent indent = new(1, '\t');
		Verb verb = new("", []);
		List<Verb> verbs = [];
		IniValueAcceptorDictionaryBuilder b = new(new(StringComparer.OrdinalIgnoreCase));

		// TODO once the value acceptors hold their keys this can be improved
		var iConfigNamespace = b.OnlyLast("Namespace");
		var iConfigIndent = b.OnlyLast("Indent");
		var iConfigNewLine = b.OnlyLast("NewLine");
		var configAcceptors = b.Acceptors;

		b = new(new(StringComparer.OrdinalIgnoreCase));
		var iVerbClass = b.OnlyLast("Class");
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
			while (ini.NextSection())
			{
				var sec = ini.Section;
				ArgType argType;
				switch (sec.Name)
				{
					case "Type":
						// When we see a new type, we have to know what method call we need to use to be able to parse that type.
						Util.ResetAll(parserAcceptors.Values);
						sec.AcceptAll(parserAcceptors).ThrowIfError();
						string type = iParserType.Get("Type");
						// Default for integral type and returning error message is false

						// If the type is a type that we already know of, then anything that the file does not define should be the defaults for that type
						// That way, for example we can just say that we want to use a different parse method for Int32 instead of having to redefine everything

						string? parseMethod;
						ParseMethodReturnType parseMethodReturnType;
						bool isValueType;
						bool isIntegralValue;
						if (typeMeta.TryGetValue(type, out TypeMeta? existingType))
						{
							isValueType = iParserIsValueType.HaveValue ? iParserIsValueType.Value : existingType.IsValueType;
							isIntegralValue = iIsIntegralType.HaveValue ? iIsIntegralType.Value : existingType.IntegralType;
							parseMethod = iParserParseMethod.HaveValue ? iParserParseMethod.Value : existingType.ParseMethod;
							parseMethodReturnType = iParserReturnType.HaveValue ? ParseParseMethodReturnType(iParserReturnType.Value, "ReturnType") : existingType.ParseMethodReturnType;
						}
						else
						{
							isValueType = iParserIsValueType.Value;
							parseMethod = iParserParseMethod.Get("ParseMethod");
							parseMethodReturnType = ParseParseMethodReturnType(iParserReturnType.Value, "ReturnType");
							isIntegralValue = iIsIntegralType.Value;
						}

						typeMeta[type] = new TypeMeta(parseMethod, parseMethodReturnType, isValueType, isIntegralValue);
						continue;
					case "Config":
						Util.ResetAll(configAcceptors.Values);
						sec.AcceptAll(configAcceptors).ThrowIfError();

						ns = iConfigNamespace.Value;
						string? rawNewLine = iConfigNewLine.Value;
						if (rawNewLine != null)
						{
							newline = rawNewLine switch
							{
								"lf" => "\n",
								"crlf" => "\r\n",
								"cr" => "\r",
								_ => throw new IniException(IniErrorCode.ValueInvalid, "NewLine must be one of lf, crlf, or cr: " + iConfigNewLine.Value),
							};
						}

						if (iConfigIndent.Value != null)
						{
							string[] parts = iConfigIndent.Value.Split(' ', StringSplitOptions.TrimEntries);
							if (parts.Length == 2)
							{
								if (int.TryParse(parts[0], out int amt))
								{
									var c = parts[1] switch
									{
										"tab" or "tabs" => '\t',
										"space" or "spaces" => ' ',
										_ => throw new IniException(IniErrorCode.ValueInvalid, "Indent must be a number and either tab or space, like \"1 tab\" or \"3 space\": " + iConfigIndent.Value),
									};
									indent = new(amt, c);
								}
								else
								{
									throw new IniException(IniErrorCode.ValueInvalid, "Indent must be a number and either tab or space, like \"1 tab\" or \"3 space\": " + iConfigIndent.Value);
								}
							}
							else
							{
								throw new IniException(IniErrorCode.ValueInvalid, "Indent must be a number and either tab or space, like \"1 tab\" or \"3 space\": " + iConfigIndent.Value);
							}
						}
						continue;
					case "Verb":
						Util.ResetAll(verbAcceptors.Values);
						sec.AcceptAll(verbAcceptors).ThrowIfError();

						string className = iVerbClass.Get("Class");

						verb = new(className, []);
						verbs.Add(verb);

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
					typeName = iArgType.Value ?? boolAlias;
					typeMeta.TryGetValue(typeName, out tm);
					required = iArgRequired.HaveValue && iArgRequired.Value;
					if (tm == null)
					{
						throw new IniException(IniErrorCode.ValueInvalid, "Switches have to be either bool or an integral type: " + iArgName.Value);
					}
					if (iArgMax.Value == 1)
					{
						if (typeName != boolAlias && typeName != boolTypeName && typeName != boolFullTypeName)
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
					typeName = iArgType.Value ?? stringAlias;
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

				verb.Args.Add(new ValidatedArg
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
		}

		using StreamWriter outputStream = new(sout, new UTF8Encoding(false, false));
		VerbWriter.WriteVerb(ns, verbs, outputStream, indent, newline);
		return 0;
	}
}