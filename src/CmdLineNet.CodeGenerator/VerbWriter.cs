namespace CmdLineNet.CodeGenerator
{
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	public static class VerbWriter
	{
		public static void WriteVerb(string ns, string className, List<ValidatedArg> args, StreamWriter sw, Indent indent)
		{
			sw.Write("#nullable enable\n");
			sw.Write("namespace ");
			sw.Write(ns);
			sw.Write("\n{\n");
			sw.Write(indent);
			sw.Write("using System;\n");
			sw.Write(indent);
			sw.Write("using System.Collections.Generic;\n");
			sw.Write(indent);
			sw.Write("using CmdLineNet;\n");
			sw.Write(indent);
			sw.Write("public sealed partial record class ");
			sw.Write(className);
			sw.Write('(');

			sw.Write(string.Join(", ", args.Select(x => x.Max > 1
				? x.ArgType == ArgType.Switch ? string.Concat(x.TypeName, " ", x.Name) : string.Concat("List<", x.TypeName, "> ", x.Name)
				: string.Concat(x.TypeName, x.CtorTypeNullable ? "? " : " ", x.Name))));

			sw.Write(") : ICmdParseable<");
			sw.Write(className);
			sw.Write(".Id, ");
			sw.Write(className);
			sw.Write(">\n");
			sw.Write(indent);
			sw.Write("{\n");
			sw.Write(indent.In());
			sw.Write("public enum Id{");
			foreach (var a in args)
			{
				sw.Write(a.Name);
				sw.Write(',');
			}
			sw.Write("}\n");
			sw.Write(indent);

			sw.Write("private static ArgsReader<Id> _reader = new ArgsReaderBuilder<Id>()\n");
			indent.In();
			foreach (var a in args)
			{
				sw.Write(indent);
				switch (a.ArgType)
				{
					case ArgType.Option:
						sw.Write(".Option(Id.");
						WriteShortLongNameMinMax(a, sw);
						break;
					case ArgType.Switch:
						sw.Write(".Switch(Id.");
						WriteShortLongNameMinMax(a, sw);
						break;
					case ArgType.Value:
						sw.Write(".Value(Id.");
						sw.Write(a.Name);
						sw.Write(", ");
						if (a.FriendlyName != null)
						{
							sw.Write('\"');
							sw.Write(a.FriendlyName);
							sw.Write('\"');
							sw.Write(", ");
						}
						else
						{
							sw.Write("null, ");
						}
						sw.Write(a.Min);
						sw.Write(", ");
						sw.Write(a.Max);
						if (string.IsNullOrEmpty(a.Help))
						{
							sw.Write(")\n");
						}
						else
						{
							sw.Write(", \"");
							sw.Write(a.Help);
							sw.Write("\")\n");
						}
						break;
				}
			}
			sw.Write(indent);
			sw.Write(".Build();\n");
			indent.Out();

			sw.Write(indent);
			sw.Write("public static ArgsReader<Id> GetReader()\n");
			sw.Write(indent);
			sw.Write("{\n");
			sw.Write(indent.In());
			sw.Write("return _reader;\n");
			sw.Write(indent.Out());
			sw.Write("}\n");

			sw.Write(indent);
			sw.Write("public static ParseResult<");
			sw.Write(className);
			sw.Write("> Parse(IEnumerable<RawArg<Id>> args)\n");
			sw.Write(indent);
			sw.Write("{\n");
			indent.In();
			foreach (var a in args)
			{
				sw.Write(indent);
				if (a.Max > 1 && a.ArgType != ArgType.Switch)
				{
					sw.Write("List<");
					sw.Write(a.TypeName);
					sw.Write("> ");
					sw.Write(a.Name);
					sw.Write(" = ");
				}
				else
				{
					sw.Write(a.TypeName);
					if (a.LocalTypeNullable)
					{
						sw.Write("? ");
						sw.Write(a.Name);
						sw.Write(" = ");
					}
					else
					{
						sw.Write(' ');
						sw.Write(a.Name);
						sw.Write(" = ");
					}
				}
				sw.Write(a.InitialValue);
				sw.Write(";\n");
			}
			sw.Write(indent);
			sw.Write("foreach (var a in args)\n");
			sw.Write(indent);
			sw.Write("{\n");
			sw.Write(indent.In());
			sw.Write("if (!a.Ok) return a.Content;\n");
			sw.Write(indent);
			sw.Write("switch (a.Id)\n");
			sw.Write(indent);
			sw.Write("{\n");
			indent.In();
			int i = 0;
			foreach (var a in args)
			{
				sw.Write(indent);
				sw.Write("case Id.");
				sw.Write(a.Name);
				sw.Write(":\n");
				sw.Write(indent.In());

				if (a.ArgType == ArgType.Switch)
				{
					sw.Write(a.Name);
					if (a.Max > 1)
					{
						sw.Write("++;\n");
					}
					else
					{
						sw.Write(" = true;\n");
					}
				}
				else if (a.TypeMeta?.ParseMethod != null)
				{
					switch (a.TypeMeta.ParseMethodReturnType)
					{
						case ParseMethodReturnType.Boolean:
							sw.Write("if (");
							sw.Write(a.TypeMeta.ParseMethod);
							sw.Write("(a.Content, out var v");
							sw.Write(i);
							sw.Write(")) { ");
							break;
						case ParseMethodReturnType.String:
							sw.Write("var errorMessage");
							sw.Write(i);
							sw.Write(" = ");
							sw.Write(a.TypeMeta.ParseMethod);
							sw.Write("(a.Content, out var v");
							sw.Write(i);
							sw.Write(");\n");
							sw.Write(indent);
							sw.Write("if (null == errorMessage");
							sw.Write(i);
							sw.Write(") { ");
							break;
						case ParseMethodReturnType.ParseResult:
							sw.Write("if (");
							sw.Write(a.TypeMeta.ParseMethod);
							sw.Write("(a.Content).Ok(out var v");
							sw.Write(i);
							sw.Write(", out var errorMessage");
							sw.Write(i);
							sw.Write(")) { ");
							break;
					}

					sw.Write(a.Name);
					if (a.Max > 1)
					{
						sw.Write(".Add(v");
						sw.Write(i);
						sw.Write("); }\n");
					}
					else
					{
						sw.Write(" = v");
						sw.Write(i);
						sw.Write("; }\n");
					}
					sw.Write(indent);
					sw.Write("else { return string.Concat(\"Unable to parse as ");
					sw.Write(a.TypeName);
					switch (a.TypeMeta.ParseMethodReturnType)
					{
						case ParseMethodReturnType.Boolean:
							sw.Write(": \", a.Content); }\n");
							break;
						case ParseMethodReturnType.ParseResult:
						case ParseMethodReturnType.String:
							sw.Write(": \", a.Content, \" because \", errorMessage");
							sw.Write(i);
							sw.Write("); }\n");
							break;
					}
					i++;
				}
				else
				{
					if (a.Max > 1)
					{
						sw.Write(a.Name);
						sw.Write(".Add(a.Content);\n");
					}
					else
					{
						sw.Write(a.Name);
						sw.Write(" = a.Content;\n");
					}
				}
				sw.Write(indent);
				sw.Write("break;\n");
				indent.Out();
			}
			sw.Write(indent.Out());
			sw.Write("}\n");
			sw.Write(indent.Out());
			sw.Write("}\n");

			foreach (var a in args)
			{
				// TODO we aren't doing all the checks that we need to; some arguments are slipping through the cracks
				// If the argument is required and there's only min/max 1, then we check null or not null
				// If the argument is required and there's multiple values, we 
				// If argument's optional, then if we have at least one we check min/max

				if (a.LocalTypeNullable && !a.CtorTypeNullable)
				{
					// If the local type's nullable and the ctor type isn't, then we have to do a null check
					sw.Write(indent);
					sw.Write("if (null == ");
					sw.Write(a.Name);
					sw.Write(')');
					switch (a.ArgType)
					{
						case ArgType.Switch:
							sw.Write(" return \"Missing required switch: ");
							break;
						case ArgType.Option:
							sw.Write(" return \"Missing required option: ");
							break;
						case ArgType.Value:
							sw.Write(" return \"Missing required value: ");
							break;
					}
					WriteName(a, sw);
				}

				// TODO If min and max are the same, we don't need to say "At least 3 and at most 3", we can just say "Exactly 3"
				if (a.ArgType == ArgType.Switch)
				{
					// Make sure that the switch is between the min/max
					if (a.Max > 1)
					{
						sw.Write(indent);
						sw.Write("if (");
						if (!a.Required)
						{
							sw.Write(a.Name);
							sw.Write(" != 0 && (");
						}
						sw.Write(a.Name);
						sw.Write(" < ");
						sw.Write(a.Min);
						if (a.Max != int.MaxValue)
						{
							sw.Write(" || ");
							sw.Write(a.Name);
							sw.Write(" > ");
							sw.Write(a.Max);
						}
						if (a.Required)
						{
							sw.Write(") return \"Switch may appear at least ");
						}
						else
						{
							sw.Write(")) return \"Switch (if provided) may appear at least ");
						}
						sw.Write(a.Min);
						if (a.Max == int.MaxValue)
						{
							sw.Write(" times: ");
						}
						else
						{
							sw.Write(" times and at most ");
							sw.Write(a.Max);
							sw.Write(" times: ");
						}
						WriteName(a, sw);
					}
					else if (a.Required)
					{
						sw.Write(indent);
						sw.Write("if (!");
						sw.Write(a.Name);
						sw.Write(") return \"Switch is required: ");
						WriteName(a, sw);
					}
				}
				else
				{
					if (a.Max > 1)
					{
						// We already would have done the null check if needed
						// Collection types are never null (for now, but they might be in the future)

						sw.Write(indent);
						sw.Write("if (");
						if (!a.Required)
						{
							sw.Write(a.Name);
							sw.Write(".Count != 0 && (");
						}
						sw.Write(a.Name);
						sw.Write(".Count < ");
						sw.Write(a.Min);
						if (a.Max != int.MaxValue)
						{
							sw.Write(" || ");
							sw.Write(a.Name);
							sw.Write(".Count > ");
							sw.Write(a.Max);
						}
						string name = a.ArgType switch
						{
							ArgType.Option => "Option",
							ArgType.Value => "Value",
							_ => "Argument",
						};
						if (a.Required)
						{
							sw.Write(") return \"");
							sw.Write(name);
							sw.Write(" requires at least ");
						}
						else
						{
							sw.Write(")) return \"");
							sw.Write(name);
							sw.Write(" (if provided) requires at least ");
						}
						sw.Write(a.Min);
						if (a.Max == int.MaxValue)
						{
							sw.Write(" values: ");
						}
						else
						{
							sw.Write(" values and at most ");
							sw.Write(a.Max);
							sw.Write(" values: ");
						}
						WriteName(a, sw);
					}
				}

				static void WriteName(ValidatedArg a, StreamWriter sw)
				{
					if (a.LongName != null)
					{
						if (a.ShortName != null)
						{
							sw.Write('-');
							sw.Write(a.ShortName);
							sw.Write("|--");
							sw.Write(a.LongName);
							sw.Write("\";\n");
						}
						else
						{
							sw.Write("--");
							sw.Write(a.LongName);
							sw.Write("\";\n");
						}
					}
					else if (a.ShortName != null)
					{
						sw.Write('-');
						sw.Write(a.ShortName);
						sw.Write("\";\n");
					}
					else if (a.ArgType != ArgType.Value)
					{
						// Should never happen since it's checked earlier
						throw new InvalidDataException("Both LongName and ShortName are null for Argument " + a.Name);
					}
					else
					{
						sw.Write(a.Name);
						sw.Write("\";\n");
					}
				}
			}
			sw.Write(indent);
			sw.Write("return new ");
			sw.Write(className);
			sw.Write('(');
			sw.Write(string.Join(", ", args.Select(x =>
			{
				if (x.LocalTypeNullable && !x.CtorTypeNullable && x.TypeMeta != null && x.TypeMeta.IsValueType)
				{
					return x.Name + ".Value";
				}
				else
				{
					return x.Name;
				}
			})));
			sw.Write(");\n");
			sw.Write(indent.Out());
			sw.Write("}\n");
			sw.Write(indent.Out());
			sw.Write("}\n");
			sw.Write(indent.Out());
			sw.Write("}\n");
			sw.Write("#nullable restore");
		}
		private static void WriteShortLongNameMinMax(ValidatedArg a, StreamWriter sw)
		{
			sw.Write(a.Name);
			sw.Write(", ");

			if (a.ShortName != null)
			{
				sw.Write('\'');
				sw.Write(a.ShortName);
				sw.Write("\', ");
			}
			if (a.LongName != null)
			{
				sw.Write('\"');
				sw.Write(a.LongName);
				sw.Write("\", ");
			}
			sw.Write(a.Min);
			sw.Write(", ");
			sw.Write(a.Max);
			if (string.IsNullOrEmpty(a.Help))
			{
				sw.Write(")\n");
			}
			else
			{
				sw.Write(", \"");
				sw.Write(a.Help);
				sw.Write("\")\n");
			}
		}
	}
}
