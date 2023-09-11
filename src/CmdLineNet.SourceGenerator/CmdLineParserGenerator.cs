namespace CmdLineNet.SourceGenerator
{
	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp;
	using Microsoft.CodeAnalysis.CSharp.Syntax;
	using Microsoft.CodeAnalysis.Text;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading;
	[Generator]
	public sealed class CmdLineParserGenerator : ISourceGenerator
	{
		public const string CommandAttributeName = "Command";
		public const string CommandConstructorName = "CommandConstructor";
		public const string SwitchAttributeName = "Switch";
		public const string OptionAttributeName = "Option";
		public const string ValueAttributeName = "Value";
		private readonly HashSet<string> argAttributeNames = new();
		public void Initialize(GeneratorInitializationContext context)
		{
#if DEBUG
			if (!System.Diagnostics.Debugger.IsAttached)
			{
				System.Diagnostics.Debugger.Launch();
			}
#endif
			argAttributeNames.Add(SwitchAttributeName);
			argAttributeNames.Add(OptionAttributeName);
			argAttributeNames.Add(ValueAttributeName);
		}
		public string FullyQualifiedName(ISymbol sym)
		{
			return sym.ContainingNamespace == null ? sym.Name : string.Concat(sym.ContainingNamespace.ToString(), ".", sym.Name);
		}
		public void Execute(GeneratorExecutionContext context)
		{
			CancellationToken ct = context.CancellationToken;

			context.AddSource("CmdLineAttributes.g.cs", SourceText.From(@"#nullable enable
namespace CmdLine
{
	using System;

	[AttributeUsage(AttributeTargets.Class)]
	public sealed class " + CommandAttributeName + @"Attribute : Attribute
	{
		public string? Help { get; init; }
	}

	[AttributeUsage(AttributeTargets.Constructor)]
	public sealed class " + CommandConstructorName + @"Attribute : Attribute{}

	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class " + SwitchAttributeName + @"Attribute : Attribute
	{
		public char ShortName { get; init; }
		public string? LongName { get; init; }
		public int Arity { get; init; } = 1;
		public string? Help { get; init; }
	}

	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class " + OptionAttributeName + @"Attribute : Attribute
	{
		public char ShortName { get; init; }
		public string? LongName { get; init; }
		public bool Optional { get; init; }
		public int Arity { get; init; } = 1;
		public string? Help { get; init; }
	}

	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class " + ValueAttributeName + @"Attribute : Attribute
	{
		public bool Optional { get; init; }
		public int Arity { get; init; } = 1;
		public string? Help { get; init; }
	}
}
#nullable restore", Encoding.UTF8));

			IEnumerable<RecordDeclarationSyntax> targetClasses = context.Compilation.SyntaxTrees
				.SelectMany(x => x.GetRoot(ct).DescendantNodes())
				.OfType<RecordDeclarationSyntax>()
				.Where(x => x.AttributeLists.Any(al => al.Attributes.Any(a => a.Name.ToString() == CommandAttributeName)));

			/*
			 * Having a static interface makes things easy but it precludes any ability to have options and locks us to later versions of .net
			 * Having a class on which we generate the parse methods is just as good, it needs to implement one interface IParser<TObj> for each thing it can parse
			 * 
			 * For each of the classes that have the Command attribute on them, we want to generate some stuff for them, and implement an interface
			 * 
			 */
			foreach (var target in targetClasses)
			{
				if (target == null)
				{
					continue;
				}
				if (!target.Modifiers.Any(x => x.Text == "partial"))
				{
					context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(id: "CL0001", title: "Command class unusable",
						messageFormat: "Class {0} must be declared as partial to generate an implementation of the interface ICmdParseable generated on it.",
						"category", DiagnosticSeverity.Error, isEnabledByDefault: true), Location.Create(target.SyntaxTree, target.Span), target.Identifier.ToString()));
					continue;
				}

				SemanticModel semanticModel = context.Compilation.GetSemanticModel(target.SyntaxTree);
				ISymbol symTargetClass = semanticModel.GetDeclaredSymbol(target) ?? throw new InvalidOperationException("Could not get semantic model for class symbol: " + target.ToString());
				if (symTargetClass.ContainingNamespace == null)
				{
					context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(id: "CL0002", title: "Command class missing namespace",
						messageFormat: "Class {0} must be declared in a namespace.",
						"category", DiagnosticSeverity.Error, isEnabledByDefault: true), Location.Create(target.SyntaxTree, target.Span), target.Identifier.ToString()));
					continue;
				}

				if (target.ParameterList == null || target.ParameterList.Parameters.Count == 0)
				{
					context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(id: "CL0003", title: "Command missing parameters",
							messageFormat: "Class {0} needs parameters to have code generated to read it from an IDataRecord.",
							"category", DiagnosticSeverity.Warning, isEnabledByDefault: true), Location.Create(target.SyntaxTree, target.Span), target.Identifier.ToString()));
					continue;
				}

				Indent indent = new(0);
				StringBuilder sbStart = new("#nullable enable\nnamespace ");
				sbStart.Append(symTargetClass.ContainingNamespace.ToString()).Append('\n')
					.Append("{\n")
					.Append(indent.In())
					.Append("using System;\n")
					.Append(indent.Val).Append("using System.Collections.Generic;\n")
					.Append(indent.Val).Append("using CmdLineNet;\n")
					.Append(indent.Val).Append(target.Modifiers.ToString())
					.Append(" record class ")
					.Append(target.Identifier.ToString())
					.Append(" : ICmdParseable<")
					.Append(target.Identifier.ToString())
					.Append(".Id, ")
					.Append(target.Identifier.ToString())
					.Append(">\n")
					.Append(indent.Val).Append("{\n");

				indent.In();

				StringBuilder idEnum = new(indent.Val);
				idEnum.Append("public enum Id{");
				foreach (var p in target.ParameterList.Parameters)
				{
					idEnum.Append(p.Identifier.ToString()).Append(',');
				}
				idEnum.Append("}\n");

				StringBuilder getParserMethod = GenerateGetReaderMethod(indent, semanticModel, target, target.ParameterList);

				StringBuilder parseMethod = GenerateParseMethod(indent, semanticModel, target, target.ParameterList);

				indent.Out();
				StringBuilder sbEnd = new(indent.Val);
				sbEnd.Append("}\n")
					.Append("}\n")
					.Append("#nullable restore");

				StringBuilder sb = new();
				sb.Append(sbStart);
				foreach (StringBuilder m in new StringBuilder[] { idEnum, getParserMethod, parseMethod, })
				{
					sb.Append(m);
				}
				sb.Append(sbEnd);
				string source = sb.ToString();
				context.AddSource(target.Identifier.ToString() + ".g.cs", SourceText.From(source, Encoding.UTF8));
			}
		}
		public StringBuilder GenerateParseMethod(Indent indent, SemanticModel semanticModel, TypeDeclarationSyntax target, ParameterListSyntax parameterList)
		{
			StringBuilder sb = new(indent.Val);
			sb.Append("public static ParseResult<").Append(target.Identifier.ToString()).Append("> Parse(IEnumerable<RawArg<Id>> args)\n")
				.Append(indent.Val).Append("{\n");

			indent.In();

			foreach (var p in parameterList.Parameters)
			{
				var pSymbol = semanticModel.GetDeclaredSymbol(p);
				if (pSymbol != null)
				{
					sb.Append(indent).Append(FullyQualifiedName(pSymbol.Type)).Append(' ').Append(p.Identifier.ToString()).Append(";\n");
				}
				else
				{

				}
			}
			sb.Append(indent).Append("foreach (var a in args)\n")
				.Append(indent).Append("{\n")
				.Append(indent.In()).Append("if (!a.Ok) return a.Content;\n")
				.Append(indent).Append("switch (a.Id)\n")
				.Append(indent).Append("{\n");

			indent.In();

			int i = 0;
			foreach (var p in parameterList.Parameters)
			{
				var pSymbol = semanticModel.GetDeclaredSymbol(p);
				if (pSymbol != null)
				{
					sb.Append(indent).Append("case Id.").Append(p.Identifier.ToString()).Append(":\n");
					indent.In();

					if (pSymbol.Type.SpecialType == SpecialType.System_String)
					{
						sb.Append(indent).Append(p.Identifier.ToString()).Append(" = a.Content;\n");
					}
					else
					{
						sb.Append(indent).Append("if (").Append(FullyQualifiedName(pSymbol.Type)).Append(".TryParse(a.Content, out var v").Append(i).Append(") ")
							.Append(p.Identifier.ToString()).Append(" = v").Append(i).Append(";\n");

						sb.Append(indent).Append("else return \"Unable to parse as ").Append(FullyQualifiedName(pSymbol.Type)).Append(": \" + a.Content;");
					}
					sb.Append(indent).Append("break;\n");
					indent.Out();
				}
				i++;
			}

			sb.Append(indent.Out()).Append("}\n")
				.Append(indent.Out()).Append("}\n")
				.Append(indent).Append("return new ").Append(target.Identifier.ToString()).Append('(')
				.Append(string.Join(", ", parameterList.Parameters.Select(x => x.Identifier.ToString()))).Append(");\n")
				.Append(indent.Out()).Append("}\n");
			return sb;
		}
		public StringBuilder GenerateGetReaderMethod(Indent indent, SemanticModel semanticModel, TypeDeclarationSyntax target, ParameterListSyntax parameterList)
		{
			StringBuilder sb = new(indent.Val);
			sb.Append("public static ArgsReader<Id> GetReader()\n")
				.Append(indent.Val).Append("{\n")
				.Append(indent.In()).Append("return new ArgsReaderBuilder<Id>()\n");

			indent.In();
			List<IParameterSymbol> dtoClassParams = parameterList.Parameters
				.Select(x => semanticModel.GetDeclaredSymbol(x) ?? throw new InvalidOperationException("Could not get declared symbol for parameter symbol: " + x.ToString()))
				.ToList();

			foreach (var p in parameterList.Parameters)
			{
				// TODO make sure we only have 1 attribute of switch/value/option
				AttributeSyntax? attrib = p.AttributeLists
					.SelectMany(al => al.Attributes)
					.Where(al => argAttributeNames.Contains(al.Name.ToString()))
					.FirstOrDefault();
				if (attrib != null && attrib.ArgumentList != null)
				{
					// Now that we have the thing, we have to set it up
					SeparatedSyntaxList<AttributeArgumentSyntax> args = attrib.ArgumentList.Arguments;
					Dictionary<string, ExpressionSyntax> nameValues = new(StringComparer.Ordinal);
					foreach (var a in args)
					{
						var nameIdentifier = a.NameEquals?.Name ?? a.NameColon?.Name;
						if (nameIdentifier != null)
						{
							nameValues[nameIdentifier.ToString()] = a.Expression;
						}
					}
					switch (attrib.Name.ToString())
					{
						case SwitchAttributeName:
							sb.Append(indent.Val).Append(".Switch(");
							break;
						case OptionAttributeName:
							sb.Append(indent.Val).Append(".Option(");
							break;
						case ValueAttributeName:
							continue;
					}
					if (nameValues.TryGetValue("ShortName", out var shortNameSyntax))
					{
						if (nameValues.TryGetValue("LongName", out var longNameSyntax))
						{
							sb.Append(shortNameSyntax.ToString()).Append(", ").Append(longNameSyntax.ToString()).Append(", Id.").Append(p.Identifier.ToString()).Append(")\n");
						}
						else
						{
							sb.Append(shortNameSyntax.ToString()).Append(", Id.").Append(p.Identifier.ToString()).Append(")\n");
						}
					}
					else if (nameValues.TryGetValue("LongName", out var longNameSyntax))
					{
						sb.Append(longNameSyntax.ToString()).Append(", Id.").Append(p.Identifier.ToString()).Append(")\n");
					}
					else
					{
						// TODO error about missing short/long name
					}
				}
				else
				{

				}
			}

			sb.Append(indent.Val).Append(".Build();\n");
			indent.Out();
			sb.Append(indent.Out()).Append("}\n");
			return sb;
		}
	}
}
