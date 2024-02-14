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
			TypeMeta charType = new("char.TryParse", typeof(char));
			TypeMeta stringType = new(null, typeof(string));
			TypeMeta boolType = new("bool.TryParse", typeof(bool));
			TypeMeta sbyteType = new("sbyte.TryParse", typeof(sbyte));
			TypeMeta byteType = new("byte.TryParse", typeof(byte));
			TypeMeta shortType = new("short.TryParse", typeof(short));
			TypeMeta ushortType = new("ushort.TryParse", typeof(ushort));
			TypeMeta intType = new("int.TryParse", typeof(int));
			TypeMeta uintType = new("uint.TryParse", typeof(uint));
			TypeMeta longType = new("long.TryParse", typeof(long));
			TypeMeta ulongType = new("ulong.TryParse", typeof(ulong));
			TypeMeta decimalType = new("decimal.TryParse", typeof(decimal));
			TypeMeta floatType = new("float.TryParse", typeof(float));
			TypeMeta doubleType = new("double.TryParse", typeof(double));
			TypeMeta dateTimeType = new("DateTime.TryParse", typeof(DateTime));
			TypeMeta dateTimeOffsetType = new("DateTimeOffset.TryParse", typeof(DateTimeOffset));
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
						break;
					case "Switch":
						argType = ArgType.Switch;
						break;
					case "Value":
						argType = ArgType.Value;
						break;
					default:
						throw new IniException(IniErrorCode.ValueInvalid, "Section name must be either Verb, Option, Switch, or Value, but was " + sec.Name);
				}
				Util.ResetAll(argAcceptors.Values);
				iMin.Value = 1;
				iMax.Value = 1;
				sec.AcceptAll(argAcceptors).ThrowIfError();
				string name = iName.Get();
				string typeName = iType.Get();
				typeMeta.TryGetValue(typeName, out var tm);
				if (argType != ArgType.Value && string.IsNullOrEmpty(iLongName.Value) && !iShortName.HaveValue)
				{
					throw new IniException(IniErrorCode.ValueMissing, "LongName or ShortName must be provided for a Switch or Option: " + sec.Name);
				}
				vargs.Add(new ValidatedArg
				(
					Name: name,
					ArgType: argType,
					TypeName: typeName,
					TypeMeta: tm,
					Required: !iRequired.HaveValue || iRequired.Value,
					LongName: iLongName.Value,
					ShortName: iShortName.HaveValue ? iShortName.Value : null,
					Min: iMin.Value,
					Max: iMax.Value,
					Default: iDefault.Value,
					Help: iHelp.Value
				));
			}
			ini.Reader.Error.ThrowIfError();
		}

		Indent indent = new(1);
		Console.Write("#nullable enable\n");
		Console.Write("namespace ");
		Console.Write(ns);
		Console.Write("\n{\n\tusing System;\n\tusing System.Collections.Generic;\n\tusing CmdLineNet;\n\tpublic sealed partial record class ");
		Console.Write(className);
		Console.Write(" : ICmdParseable<");
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
		
		Console.Write("private static ArgsReader _reader = new ArgsReaderBuilder<Id>()\n");
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
			if (a.Max > 1)
			{
				Console.Write("List<");
				Console.Write(a.TypeName);
				Console.Write("> ");
				Console.Write(a.Name);
				Console.Write(" = new List<");
				Console.Write(a.TypeName);
				Console.Write(">();\n");
			}
			else
			{
				Console.Write(a.TypeName);
				if (a.Default != null)
				{
					Console.Write(' ');
					Console.Write(a.Name);
					Console.Write(" = ");
					Console.Write(a.Default);
					Console.Write(";\n");
				}
				else
				{
					Console.Write("? ");
					Console.Write(a.Name);
					Console.Write(" = null;\n");
				}
			}
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

			if (a.TypeMeta?.ParseMethod != null)
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
			if (a.Required)
			{
				Console.Write(indent);
				if (a.Max > 1)
				{
					Console.Write("if (");
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
					Console.Write(") return \"Argument requires at least ");
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
				}
				else
				{
					Console.Write("if (null == ");
					Console.Write(a.Name);
					Console.Write(')');
					Console.Write(" return \"Missing required argument: ");
				}

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
				else
				{
					throw new InvalidDataException("Both LongName and ShortName are null for Argument " + a.Name);
				}
			}
			else if (a.Max > 1)
			{
				Console.Write(indent);
				Console.Write("if (");
				Console.Write(a.Name);
				Console.Write(".Count != 0 && (");
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

				Console.Write(")) return \"Argument (if provided) requires at least ");
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
			}
		}
		Console.Write(indent);
		Console.Write("return new ");
		Console.Write(className);
		Console.Write('(');
		Console.Write(string.Join(", ", vargs.Select(x =>
		{
			return x.Max > 1
			? x.Name
			: x.TypeMeta != null && x.TypeMeta.Type.IsValueType && x.Default == null ? x.Name + ".Value" : x.Name;
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
public sealed record class ValidatedArg(string Name, ArgType ArgType, string TypeName, TypeMeta? TypeMeta, bool Required, string? LongName, char? ShortName, int Min, int Max, string? Default, string? Help);