namespace CmdLineNet.CodeGenerator;

using IniFileNet;
using IniFileNet.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public static class Program
{
	public static string Get(this IniValueAcceptorOnlyLast v)
	{
		return v.Value ?? throw new IniException(IniErrorCode.ValueMissing, "Value cannot be empty");
	}
	public static void Main(string[] args)
	{
		Console.OutputEncoding = Encoding.UTF8;
		Dictionary<string, TypeMeta> typeMeta;
		{
			TypeMeta charType = new("char.TryParse", typeof(char), false);
			TypeMeta stringType = new(null, typeof(string), false);
			TypeMeta boolType = new("bool.TryParse", typeof(bool), false);
			TypeMeta sbyteType = new("sbyte.TryParse", typeof(sbyte), true);
			TypeMeta byteType = new("byte.TryParse", typeof(byte), true);
			TypeMeta shortType = new("short.TryParse", typeof(short), true);
			TypeMeta ushortType = new("ushort.TryParse", typeof(ushort), true);
			TypeMeta intType = new("int.TryParse", typeof(int), true);
			TypeMeta uintType = new("uint.TryParse", typeof(uint), true);
			TypeMeta longType = new("long.TryParse", typeof(long), true);
			TypeMeta ulongType = new("ulong.TryParse", typeof(ulong), true);
			TypeMeta decimalType = new("decimal.TryParse", typeof(decimal), false);
			TypeMeta floatType = new("float.TryParse", typeof(float), false);
			TypeMeta doubleType = new("double.TryParse", typeof(double), false);
			TypeMeta dateTimeType = new("DateTime.TryParse", typeof(DateTime), false);
			TypeMeta dateTimeOffsetType = new("DateTimeOffset.TryParse", typeof(DateTimeOffset), false);
			TypeMeta guidType = new("Guid.TryParse", typeof(Guid), false);
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

				["DateTimeOffset"] = dateTimeOffsetType,
				["System.DateTimeOffset"] = dateTimeOffsetType,

				["Guid"] = guidType,
				["System.Guid"] = guidType,
			};
		}
		string? ns = null;
		string? className = null;
		List<ValidatedArg> vargs = [];
		using (IniStreamSectionReader ini = new(new IniStreamReader(new StreamReader(args[0]), new IniReaderOptions(allowLineContinuations: true, ignoreComments: true))))
		{
			Dictionary<string, IIniValueAcceptor> verbAcceptors = [];
			Dictionary<string, IIniValueAcceptor> argAcceptors = [];
			IniValueAcceptorDictionaryBuilder b = new(verbAcceptors);
			var iNamespace = b.OnlyLast("Namespace");
			var iClass = b.OnlyLast("Class");

			b = new(argAcceptors);
			var iName = b.OnlyLast("Name");
			var iType = b.OnlyLast("Type");
			var iRequired = b.OnlyLast("Required", Parse.Boolean);
			var iLongName = b.OnlyLast("LongName");
			var iShortName = b.OnlyLast("ShortName", x => char.TryParse(x, out char c) ? new IniResult<char>(c, default) : new IniResult<char>(default, new(IniErrorCode.ValueInvalid, "Could not parse \"" + x + "\" as char")));
			var iMin = b.OnlyLast("Min", Parse.Int32);
			var iMax = b.OnlyLast("Max", x => x.Length == 0 ? new IniResult<int>(int.MaxValue, default) : int.TryParse(x, out int v) ? new IniResult<int>(v, default) : new IniResult<int>(default, new(IniErrorCode.ValueInvalid, "Could not parse \"" + x + "\" as int")));
			var iDefault = b.OnlyLast("Default");
			var iHelp = b.OnlyLast("Help");
			b = null!;
			// TODO we could put the output filename in the .ini file, perhaps?
			while (ini.TryReadNext(out var sec))
			{
				ArgType argType;
				switch (sec.Name)
				{
					case "Verb":
						Util.ResetAll(verbAcceptors.Values);
						sec.AcceptAll(verbAcceptors).ThrowIfError();
						ns = iNamespace.Get();
						className = iClass.Get();
						continue;
					case "Option":
						argType = ArgType.Option;
						Util.ResetAll(argAcceptors.Values);
						iMin.Value = 1;
						iMax.Value = 1;
						sec.AcceptAll(argAcceptors).ThrowIfError();
						if (string.IsNullOrEmpty(iLongName.Value) && !iShortName.HaveValue)
						{
							throw new IniException(IniErrorCode.ValueMissing, "LongName or ShortName must be provided for an Option: " + iName.Value);
						}
						break;
					case "Switch":
						argType = ArgType.Switch;
						Util.ResetAll(argAcceptors.Values);
						iMin.Value = 1;
						iMax.Value = 1;
						sec.AcceptAll(argAcceptors).ThrowIfError();
						if (string.IsNullOrEmpty(iLongName.Value) && !iShortName.HaveValue)
						{
							throw new IniException(IniErrorCode.ValueMissing, "LongName or ShortName must be provided for a swtich: " + iName.Value);
						}
						break;
					case "Value":
						argType = ArgType.Value;
						Util.ResetAll(argAcceptors.Values);
						iMin.Value = 1;
						iMax.Value = 1;
						sec.AcceptAll(argAcceptors).ThrowIfError();
						if (!string.IsNullOrEmpty(iLongName.Value) || iShortName.HaveValue)
						{
							throw new IniException(IniErrorCode.ValueInvalid, "LongName or ShortName must NOT be provided for a value: " + iName.Value);
						}
						break;
					default:
						throw new IniException(IniErrorCode.ValueInvalid, "Section name must be either Verb, Option, Switch, or Value, but was " + iName.Value);
				}
				if (iMin.Value > iMax.Value)
				{
					throw new IniException(IniErrorCode.ValueInvalid, "Min was larger than Max " + iName.Value);
				}
				string name = iName.Get();
				string typeName = iType.Get();
				if (iDefault.Value == "")
				{
					iDefault.Value = null;
				}
				typeMeta.TryGetValue(typeName, out var tm);
				// If it's a switch, then we require a boolean for singular switches or an integral type for multiple switches
				if (argType == ArgType.Switch)
				{
					if (tm == null)
					{
						throw new IniException(IniErrorCode.ValueInvalid, "Switches have to be either bool or an integral type: " + iName.Value);
					}
					if (iMax.Value == 1)
					{
						if (tm.Type != typeof(bool))
						{
							throw new IniException(IniErrorCode.ValueInvalid, "Switches that accept only 1 value must be of type bool: " + iName.Value);
						}
						iDefault.Value ??= "false";
					}
					if (iMax.Value > 1)
					{
						if (!tm.IntegralType)
						{
							throw new IniException(IniErrorCode.ValueInvalid, "Switches that accept many values must be an integral type: " + iName.Value);
						}
						iDefault.Value ??= "0";
					}
				}

				bool required = !iRequired.HaveValue || iRequired.Value;
				string initVal;
				bool localTypeNullable;
				if (iDefault.Value != null)
				{
					localTypeNullable = false;
					initVal = iDefault.Value;
				}
				else
				{
					if (iMax.Value == 1)
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
					CtorTypeNullable: !required && iDefault.Value == null,
					LocalTypeNullable: localTypeNullable,
					LongName: iLongName.Value,
					ShortName: iShortName.HaveValue ? iShortName.Value : null,
					Min: iMin.Value,
					Max: iMax.Value,
					InitialValue: initVal,
					Help: iHelp.Value
				));
			}
			ini.Reader.Error.ThrowIfError();
		}

		Indent indent = new(1);
		Console.Write("#nullable enable\n");
		Console.Write("namespace ");
		Console.Write(ns);
		Console.Write("\n{\n");
		Console.Write(indent);
		Console.Write("using System;\n");
		Console.Write(indent);
		Console.Write("using System.Collections.Generic;\n");
		Console.Write(indent);
		Console.Write("using CmdLineNet;\n");
		Console.Write(indent);
		Console.Write("public sealed partial record class ");
		Console.Write(className);
		Console.Write('(');

		Console.Write(string.Join(", ", vargs.Select(x => x.Max > 1
			? x.ArgType == ArgType.Switch ? string.Concat(x.TypeName, " ", x.Name) : string.Concat("List<", x.TypeName, "> ", x.Name)
			: string.Concat(x.TypeName, x.CtorTypeNullable ? "? " : " ", x.Name))));

		Console.Write(") : ICmdParseable<");
		Console.Write(className);
		Console.Write(".Id, ");
		Console.Write(className);
		Console.Write(">\n");
		Console.Write(indent);
		Console.Write("{\n");
		Console.Write(indent.In());
		Console.Write("public enum Id{");
		foreach (var a in vargs)
		{
			Console.Write(a.Name);
			Console.Write(',');
		}
		Console.Write("}\n");
		Console.Write(indent);

		Console.Write("private static ArgsReader<Id> _reader = new ArgsReaderBuilder<Id>()\n");
		indent.In();
		foreach (var a in vargs)
		{
			Console.Write(indent);
			switch (a.ArgType)
			{
				case ArgType.Option:
					Console.Write(".Option(Id.");
					WriteShortLongNameMinMax(a);
					break;
				case ArgType.Switch:
					Console.Write(".Switch(Id.");
					WriteShortLongNameMinMax(a);
					break;
				case ArgType.Value:
					Console.Write(".Value(Id.");
					Console.Write(a.Name);
					Console.Write(", ");
					Console.Write(a.Min);
					Console.Write(", ");
					Console.Write(a.Max);
					if (string.IsNullOrEmpty(a.Help))
					{
						Console.Write(")\n");
					}
					else
					{
						Console.Write(", \"");
						Console.Write(a.Help);
						Console.Write("\")\n");
					}
					break;
			}
		}
		Console.Write(indent);
		Console.Write(".Build();\n");
		indent.Out();

		Console.Write(indent);
		Console.Write("public static ArgsReader<Id> GetReader()\n");
		Console.Write(indent);
		Console.Write("{\n");
		Console.Write(indent.In());
		Console.Write("return _reader;\n");
		Console.Write(indent.Out());
		Console.Write("}\n");

		Console.Write(indent);
		Console.Write("public static ParseResult<");
		Console.Write(className);
		Console.Write("> Parse(IEnumerable<RawArg<Id>> args)\n");
		Console.Write(indent);
		Console.Write("{\n");
		indent.In();
		foreach (var a in vargs)
		{
			Console.Write(indent);
			if (a.Max > 1 && a.ArgType != ArgType.Switch)
			{
				Console.Write("List<");
				Console.Write(a.TypeName);
				Console.Write("> ");
				Console.Write(a.Name);
				Console.Write(" = ");
			}
			else
			{
				Console.Write(a.TypeName);
				if (a.LocalTypeNullable)
				{
					Console.Write("? ");
					Console.Write(a.Name);
					Console.Write(" = ");
				}
				else
				{
					Console.Write(' ');
					Console.Write(a.Name);
					Console.Write(" = ");
				}
			}
			Console.Write(a.InitialValue);
			Console.Write(";\n");
		}
		Console.Write(indent);
		Console.Write("foreach (var a in args)\n");
		Console.Write(indent);
		Console.Write("{\n");
		Console.Write(indent.In());
		Console.Write("if (!a.Ok) return a.Content;\n");
		Console.Write(indent);
		Console.Write("switch (a.Id)\n");
		Console.Write(indent);
		Console.Write("{\n");
		indent.In();
		int i = 0;
		foreach (var a in vargs)
		{
			Console.Write(indent);
			Console.Write("case Id.");
			Console.Write(a.Name);
			Console.Write(":\n");
			Console.Write(indent.In());

			if (a.ArgType == ArgType.Switch)
			{
				Console.Write(a.Name);
				if (a.Max > 1)
				{
					Console.Write("++;\n");
				}
				else
				{
					Console.Write(" = true;\n");
				}
			}
			else if (a.TypeMeta?.ParseMethod != null)
			{
				Console.Write("if (");
				Console.Write(a.TypeMeta.ParseMethod);
				Console.Write("(a.Content, out var v");
				Console.Write(i);
				Console.Write(")) { ");
				Console.Write(a.Name);
				if (a.Max > 1)
				{
					Console.Write(".Add(v");
					Console.Write(i);
					Console.Write("); }\n");
				}
				else
				{
					Console.Write(" = v");
					Console.Write(i);
					Console.Write("; }\n");
				}
				Console.Write(indent);
				Console.Write("else { return \"Unable to parse as ");
				Console.Write(a.TypeName);
				Console.Write(": \" + a.Content; }\n");
				i++;
			}
			else
			{
				if (a.Max > 1)
				{
					Console.Write(a.Name);
					Console.Write(".Add(a.Content);\n");
				}
				else
				{
					Console.Write(a.Name);
					Console.Write(" = a.Content;\n");
				}
			}
			Console.Write(indent);
			Console.Write("break;\n");
			indent.Out();
		}
		Console.Write(indent.Out());
		Console.Write("}\n");
		Console.Write(indent.Out());
		Console.Write("}\n");

		foreach (var a in vargs)
		{
			// TODO we aren't doing all the checks that we need to; some arguments are slipping through the cracks
			// If the argument is required and there's only min/max 1, then we check null or not null
			// If the argument is required and there's multiple values, we 
			// If argument's optional, then if we have at least one we check min/max

			if (a.LocalTypeNullable && !a.CtorTypeNullable)
			{
				// If the local type's nullable and the ctor type isn't, then we have to do a null check
				Console.Write("if (null == ");
				Console.Write(a.Name);
				Console.Write(')');
				switch (a.ArgType)
				{
					case ArgType.Switch:
						Console.Write(" return \"Missing required switch: ");
						break;
					case ArgType.Option:
						Console.Write(" return \"Missing required option: ");
						break;
					case ArgType.Value:
						Console.Write(" return \"Missing required value: ");
						break;
				}
				WriteName(a);
			}

			// TODO If min and max are the same, we don't need to say "At least 3 and at most 3", we can just say "Exactly 3"
			if (a.ArgType == ArgType.Switch)
			{
				// Make sure that the switch is between the min/max
				if (a.Max > 1)
				{
					Console.Write(indent);
					Console.Write("if (");
					if (!a.Required)
					{
						Console.Write(a.Name);
						Console.Write(" != 0 && (");
					}
					Console.Write(a.Name);
					Console.Write(" < ");
					Console.Write(a.Min);
					if (a.Max != int.MaxValue)
					{
						Console.Write(" || ");
						Console.Write(a.Name);
						Console.Write(" > ");
						Console.Write(a.Max);
					}
					if (a.Required)
					{
						Console.Write(") return \"Switch may appear at least ");
					}
					else
					{
						Console.Write(")) return \"Switch (if provided) may appear at least ");
					}
					Console.Write(a.Min);
					if (a.Max == int.MaxValue)
					{
						Console.Write(" times: ");
					}
					else
					{
						Console.Write(" times and at most ");
						Console.Write(a.Max);
						Console.Write(" times: ");
					}
					WriteName(a);
				}
				else if (a.Required)
				{
					Console.Write(indent);
					Console.Write("if (!");
					Console.Write(a.Name);
					Console.Write(") return \"Switch is required: ");
					WriteName(a);
				}
			}
			else
			{
				if (a.Max > 1)
				{
					// We already would have done the null check if needed
					// Collection types are never null (for now, but they might be in the future)

					Console.Write(indent);
					Console.Write("if (");
					if (!a.Required)
					{
						Console.Write(a.Name);
						Console.Write(".Count != 0 && (");
					}
					Console.Write(a.Name);
					Console.Write(".Count < ");
					Console.Write(a.Min);
					if (a.Max != int.MaxValue)
					{
						Console.Write(" || ");
						Console.Write(a.Name);
						Console.Write(".Count > ");
						Console.Write(a.Max);
					}
					string name = a.ArgType switch
					{
						ArgType.Option => "Option",
						ArgType.Value => "Value",
						_ => "Argument",
					};
					if (a.Required)
					{
						Console.Write(") return \"");
						Console.Write(name);
						Console.Write(" requires at least ");
					}
					else
					{
						Console.Write(")) return \"");
						Console.Write(name);
						Console.Write(" (if provided) requires at least ");
					}
					Console.Write(a.Min);
					if (a.Max == int.MaxValue)
					{
						Console.Write(" values: ");
					}
					else
					{
						Console.Write(" values and at most ");
						Console.Write(a.Max);
						Console.Write(" values: ");
					}
					WriteName(a);
				}
			}

			static void WriteName(ValidatedArg a)
			{
				if (a.LongName != null)
				{
					if (a.ShortName != null)
					{
						Console.Write('-');
						Console.Write(a.ShortName);
						Console.Write("|--");
						Console.Write(a.LongName);
						Console.Write("\";\n");
					}
					else
					{
						Console.Write("--");
						Console.Write(a.LongName);
						Console.Write("\";\n");
					}
				}
				else if (a.ShortName != null)
				{
					Console.Write('-');
					Console.Write(a.ShortName);
					Console.Write("\";\n");
				}
				else if (a.ArgType != ArgType.Value)
				{
					// Should never happen since it's checked earlier
					throw new InvalidDataException("Both LongName and ShortName are null for Argument " + a.Name);
				}
				else
				{
					Console.Write(a.Name);
					Console.Write("\";\n");
				}
			}
		}
		Console.Write(indent);
		Console.Write("return new ");
		Console.Write(className);
		Console.Write('(');
		Console.Write(string.Join(", ", vargs.Select(x =>
		{
			if (x.LocalTypeNullable && !x.CtorTypeNullable && x.TypeMeta != null && x.TypeMeta.Type.IsValueType)
			{
				return x.Name + ".Value";
			}
			else
			{
				return x.Name;
			}
		})));
		Console.Write(");\n");
		Console.Write(indent.Out());
		Console.Write("}\n");
		Console.Write(indent.Out());
		Console.Write("}\n");
		Console.Write(indent.Out());
		Console.Write("}\n");
		Console.Write("#nullable restore");
	}
	private static void WriteShortLongNameMinMax(ValidatedArg a)
	{
		Console.Write(a.Name);
		Console.Write(", ");

		if (a.ShortName != null)
		{
			Console.Write('\'');
			Console.Write(a.ShortName);
			Console.Write("\', ");
		}
		if (a.LongName != null)
		{
			Console.Write('\"');
			Console.Write(a.LongName);
			Console.Write("\", ");
		}
		Console.Write(a.Min);
		Console.Write(", ");
		Console.Write(a.Max);
		if (string.IsNullOrEmpty(a.Help))
		{
			Console.Write(")\n");
		}
		else
		{
			Console.Write(", \"");
			Console.Write(a.Help);
			Console.Write("\")\n");
		}
	}
}
