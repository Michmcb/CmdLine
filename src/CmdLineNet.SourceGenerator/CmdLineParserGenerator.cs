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

		public void Initialize(GeneratorInitializationContext context)
		{
//#if DEBUG
//			if (!System.Diagnostics.Debugger.IsAttached)
//			{
//				System.Diagnostics.Debugger.Launch();
//			}
//#endif
		}
		public string FullyQualifiedName(ISymbol sym)
		{
			return sym.ContainingNamespace == null ? sym.Name : string.Concat(sym.ContainingNamespace.ToString(), ".", sym.Name);
		}
		public void Execute(GeneratorExecutionContext context)
		{
			CancellationToken ct = context.CancellationToken;

			IEnumerable<RecordDeclarationSyntax> targetClasses = context.Compilation.SyntaxTrees
				.SelectMany(x => x.GetRoot(ct).DescendantNodes())
				.OfType<RecordDeclarationSyntax>()
				.Where(x => x.AttributeLists.Any(al => al.Attributes.Any(a => a.Name.ToString() == Name.VerbAttribute)));

			/*
			 * Having a static interface makes things easy but it precludes any ability to have options and locks us to later versions of .net
			 * Having a class on which we generate the parse methods is just as good, it needs to implement one interface IParser<TObj> for each thing it can parse
			 * 
			 * For each of the classes that have the Verb attribute on them, we want to generate some stuff for them, and implement an interface
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

				ParamListInfo pli = ParamListInfo.Get(target.ParameterList, semanticModel, context);

				Dictionary<string, MethodStuff> paramParseMethods = [];

				foreach (var method in target
					.DescendantNodes()
					.OfType<MethodDeclarationSyntax>()
					.Where(x => x.Identifier.ToString().StartsWith("Parse")))
				{
					if (method.ReturnType is GenericNameSyntax gns)
					{
						TypeArgumentListSyntax tals = gns.DescendantNodes().OfType<TypeArgumentListSyntax>().FirstOrDefault();
						if (tals.Arguments.Count == 1)
						{
							IMethodSymbol symMethod = semanticModel.GetDeclaredSymbol(method) ?? throw new InvalidOperationException("Could not get semantic model for class method: " + method.ToString());
							if (symMethod.Parameters.Length == 1
								&& symMethod.Parameters[0].Type.SpecialType == SpecialType.System_String
								&& symMethod.ReturnType.ContainingNamespace?.Name == "CmdLineNet"
								&& symMethod.ReturnType.Name == "ParseResult")
							{
								// We check the type matches later
								paramParseMethods[method.Identifier.ToString().Substring(5)] = new(semanticModel.GetTypeInfo(tals.Arguments[0]).Type, symMethod);
							}
						}
					}
				}

				StringBuilder? getParserMethod = GenerateGetReaderMethod(indent, context, semanticModel, target, pli);

				StringBuilder? parseMethod = GenerateParseMethod(indent, context, semanticModel, target, pli, paramParseMethods);
				if (getParserMethod != null && parseMethod != null)
				{
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
		}
		public StringBuilder? GenerateParseMethod(Indent indent, GeneratorExecutionContext context, SemanticModel semanticModel, TypeDeclarationSyntax target, ParamListInfo pli, IReadOnlyDictionary<string, MethodStuff> paramParseMethods)
		{
			StringBuilder sb = new(indent.Val);
			sb.Append("public static ParseResult<").Append(target.Identifier.ToString()).Append("> Parse(IEnumerable<RawArg<Id>> args)\n")
				.Append(indent.Val).Append("{\n");

			indent.In();

			foreach (var pi in pli.ParamInfos)
			{
				var p = pi.Parameter;
				if (pi.IsCollection)
				{
					sb.Append(indent).Append("System.Collections.Generic.List<").Append(FullyQualifiedName(pi.ElementTypeSymbol)).Append("> ")
						.Append(p.Identifier.ToString()).Append(" = new System.Collections.Generic.List<").Append(FullyQualifiedName(pi.ElementTypeSymbol)).Append(">();\n");
					if (pi.DefaultValueSymbol != null)
					{
						// TODO warning about default value being ignored
					}
				}
				else
				{
					// If it's a switch it is never nullable; it's always either false or zero (for switches which can be specified multiple times)
					sb.Append(indent).Append(FullyQualifiedName(pi.Symbol.Type));
					if (pi.AttributeType == AttribType.Switch)
					{
						sb.Append(' ').Append(p.Identifier.ToString()).Append(" = ");
						if (pi.DefaultValueSymbol != null)
						{
							sb.Append(pi.DefaultValueSymbol.ToString());
						}
						else
						{
							sb.Append("default");
						}
					}
					else
					{
						if (pi.DefaultValueSymbol != null)
						{
							sb.Append(' ').Append(p.Identifier.ToString()).Append(" = ").Append(pi.DefaultValueSymbol.ToString());
						}
						else
						{
							sb.Append("? ").Append(p.Identifier.ToString()).Append(" = ").Append("null");
						}
					}
					sb.Append(";\n");
				}
			}
			sb.Append(indent).Append("foreach (var a in args)\n")
				.Append(indent).Append("{\n")
				.Append(indent.In()).Append("if (!a.Ok) return a.Content;\n")
				.Append(indent).Append("switch (a.Id)\n")
				.Append(indent).Append("{\n");

			indent.In();

			int i = 0;
			foreach (var pi in pli.ParamInfos)
			{
				if (pi.IsCollection && pi.AttributeType == AttribType.Switch)
				{
					// TODO Collection and switch incompatible
				}

				var p = pi.Parameter;
				var pSymbol = pi.Symbol;
				sb.Append(indent).Append("case Id.").Append(p.Identifier.ToString()).Append(":\n");
				indent.In();
				ITypeSymbol pType = pSymbol.Type;

				// If the type is Nullable<T>, then we need to get its generic type
				if (pType.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T && pType is INamedTypeSymbol nts1 && nts1.TypeArguments.Length == 1)
				{
					pType = nts1.TypeArguments[0];
				}

				// if we have a custom parse method defined, then use that
				if (paramParseMethods.TryGetValue(pType.Name, out var parseMethod))
				{
					if (SymbolEqualityComparer.IncludeNullability.Equals(pType, parseMethod.ReturnType))
					{
						sb.Append(indent).Append("if (").Append(parseMethod.Symbol.Name).Append("(a.Content).Ok(out var v").Append(i).Append(", out var pr").Append(i).Append(")) {")
							.Append(p.Identifier.ToString()).Append(" = v").Append(i).Append("; }\n");

						sb.Append(indent).Append("else { return pr").Append(i).Append("; }\n");
					}
					else
					{
						// TODO types do not match
					}
				}

				string? tryParseMethod = null;
				if (pi.ElementTypeSymbol.TypeKind == TypeKind.Enum && pi.ElementTypeSymbol is INamedTypeSymbol nts2)
				{
					tryParseMethod = string.Concat("System.Enum.TryParse<", pi.ElementTypeSymbol.ToString(), ">");
				}
				else
				{
					switch (pi.ElementTypeSymbol.SpecialType)
					{
						case SpecialType.System_Char:
							if (pi.AttributeType == AttribType.Switch)
							{
								// TODO incompatible
							}
							else
							{
								tryParseMethod = "char.TryParse";
							}
							break;
						case SpecialType.System_String:
							if (pi.IsCollection)
							{
								sb.Append(indent).Append(p.Identifier.ToString()).Append(".Add(a.Content);\n");
							}
							else
							{
								sb.Append(indent).Append(p.Identifier.ToString()).Append(" = a.Content;\n");
							}
							tryParseMethod = "";
							break;
						case SpecialType.System_Boolean:
							if (pi.AttributeType == AttribType.Switch)
							{
								tryParseMethod = "";
								sb.Append(indent).Append(p.Identifier.ToString()).Append(" = true;\n");
							}
							else
							{
								tryParseMethod = "bool.TryParse";
							}
							break;
						case SpecialType.System_SByte:
							if (pi.AttributeType == AttribType.Switch)
							{
								tryParseMethod = "";
								sb.Append(indent).Append(p.Identifier.ToString()).Append("++;\n");
							}
							else
							{
								tryParseMethod = "sbyte.TryParse";
							}
							break;
						case SpecialType.System_Byte:
							if (pi.AttributeType == AttribType.Switch)
							{
								tryParseMethod = "";
								sb.Append(indent).Append(p.Identifier.ToString()).Append("++;\n");
							}
							else
							{
								tryParseMethod = "byte.TryParse";
							}
							break;
						case SpecialType.System_Int16:
							if (pi.AttributeType == AttribType.Switch)
							{
								tryParseMethod = "";
								sb.Append(indent).Append(p.Identifier.ToString()).Append("++;\n");
							}
							else
							{
								tryParseMethod = "short.TryParse";
							}
							break;
						case SpecialType.System_UInt16:
							if (pi.AttributeType == AttribType.Switch)
							{
								tryParseMethod = "";
								sb.Append(indent).Append(p.Identifier.ToString()).Append("++;\n");
							}
							else
							{
								tryParseMethod = "ushort.TryParse";
							}
							break;
						case SpecialType.System_Int32:
							if (pi.AttributeType == AttribType.Switch)
							{
								tryParseMethod = "";
								sb.Append(indent).Append(p.Identifier.ToString()).Append("++;\n");
							}
							else
							{
								tryParseMethod = "int.TryParse";
							}
							break;
						case SpecialType.System_UInt32:
							if (pi.AttributeType == AttribType.Switch)
							{
								tryParseMethod = "";
								sb.Append(indent).Append(p.Identifier.ToString()).Append("++;\n");
							}
							else
							{
								tryParseMethod = "uint.TryParse";
							}
							break;
						case SpecialType.System_Int64:
							if (pi.AttributeType == AttribType.Switch)
							{
								tryParseMethod = "";
								sb.Append(indent).Append(p.Identifier.ToString()).Append("++;\n");
							}
							else
							{
								tryParseMethod = "long.TryParse";
							}
							break;
						case SpecialType.System_UInt64:
							if (pi.AttributeType == AttribType.Switch)
							{
								tryParseMethod = "";
								sb.Append(indent).Append(p.Identifier.ToString()).Append("++;\n");
							}
							else
							{
								tryParseMethod = "ulong.TryParse";
							}
							break;
						case SpecialType.System_Decimal:
							if (pi.AttributeType == AttribType.Switch)
							{
								// TODO incompatible
							}
							tryParseMethod = "decimal.TryParse";
							break;
						case SpecialType.System_Single:
							if (pi.AttributeType == AttribType.Switch)
							{
								// TODO incompatible
							}
							tryParseMethod = "float.TryParse";
							break;
						case SpecialType.System_Double:
							if (pi.AttributeType == AttribType.Switch)
							{
								// TODO incompatible
							}
							tryParseMethod = "double.TryParse";
							break;
						case SpecialType.System_DateTime:
							if (pi.AttributeType == AttribType.Switch)
							{
								// TODO incompatible (dude wtf, seriously?)
							}
							tryParseMethod = "System.DateTime.TryParse";
							break;
					}
				}

				if (tryParseMethod != null)
				{
					// Length of 0 means no parsing is required
					if (tryParseMethod.Length != 0)
					{
						sb.Append(indent).Append("if (").Append(tryParseMethod).Append("(a.Content, out var v").Append(i).Append(")) {").Append(p.Identifier.ToString());

						if (pi.IsCollection)
						{
							sb.Append(".Add(v").Append(i).Append("); }\n");
						}
						else
						{
							sb.Append(" = v").Append(i).Append("; }\n");
						}

						sb.Append(indent).Append("else { return \"Unable to parse as ").Append(FullyQualifiedName(pSymbol.Type)).Append(": \" + a.Content; }\n");
					}
				}
				else
				{
					// TODO no parse method was found
				}

				sb.Append(indent).Append("break;\n");
				indent.Out();
				i++;
			}

			sb.Append(indent.Out()).Append("}\n")
				.Append(indent.Out()).Append("}\n");

			foreach (var pi in pli.ParamInfos.Where(x => x.DefaultValueSymbol == null))
			{
				var p = pi.Parameter;
				bool required = pi.Symbol.Type.NullableAnnotation == NullableAnnotation.NotAnnotated;

				string collectionPropertyName = pi.IsArray ? ".Length" : ".Count";

				// TODO If there's min/max properties, make sure that the count adheres to it, whether or not it's required. 

				if (required)
				{
					// Make sure they don't start or end with quotes that breaks the string
					string? longName = pi.Expressions.TryGetValue(Name.LongNameProperty, out var longNameSyntax)
						? longNameSyntax.ToString()?.Trim('\"')
						: null;
					string? shortName = pi.Expressions.TryGetValue(Name.ShortNameProperty, out var shortNameSyntax)
						? shortNameSyntax.ToString()?.Trim('\'')
						: p.Identifier.ToString();

					sb.Append(indent).Append("if (null == ").Append(p.Identifier.ToString()).Append(')');

					if (longName != null)
					{
						if (shortName != null)
						{
							sb.Append(" return \"Missing required parameter: -").Append(shortName).Append("|--").Append(longName).Append("\";\n");
						}
						else
						{
							sb.Append(" return \"Missing required parameter: --").Append(longName).Append("\";\n");
						}
					}
					else if (shortName != null)
					{
						sb.Append(" return \"Missing required parameter: -").Append(shortName).Append("\";\n");
					}
					else
					{
						sb.Append(" return \"Missing required parameter: ").Append(p.Identifier.ToString()).Append("\";\n");
					}
				}
			}
			sb.Append(indent).Append("return new ").Append(target.Identifier.ToString()).Append('(')
				.Append(string.Join(", ", pli.ParamInfos.Select(x =>
				{
					if (x.IsCollection && x.IsArray)
					{
						return string.Concat("System.Linq.Enumerable.ToArray(", x.Parameter.Identifier.ToString(), ")");
					}
					else
					{
						return (x.Symbol.Type.IsValueType && x.DefaultValueSymbol == null) ? x.Parameter.Identifier.ToString() + ".Value" : x.Parameter.Identifier.ToString();
					}
				}))).Append(");\n")
				.Append(indent.Out()).Append("}\n");
			return sb;
		}
		public StringBuilder? GenerateGetReaderMethod(Indent indent, GeneratorExecutionContext context, SemanticModel semanticModel, TypeDeclarationSyntax target, ParamListInfo parameterInfo)
		{
			StringBuilder sb = new(indent.Val);
			sb.Append("public static ArgsReader<Id> GetReader()\n")
				.Append(indent.Val).Append("{\n")
				.Append(indent.In()).Append("return new ArgsReaderBuilder<Id>()\n");

			indent.In();

			foreach (var pi in parameterInfo.ParamInfos)
			{
				var p = pi.Parameter;
				// Now that we have the thing, we have to set it up
				switch (pi.AttributeType)
				{
					case AttribType.Switch:
						sb.Append(indent.Val).Append(".Switch(");
						break;
					case AttribType.Option:
						sb.Append(indent.Val).Append(".Option(");
						break;
					case AttribType.Value:
						sb.Append(indent.Val).Append(".Value()");
						continue;
					default:
						context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(id: "CL0004", title: "Parameter missing type attribute",
						messageFormat: "Parameter {0} must have a Switch/Option/Value attribute on it.",
						"category", DiagnosticSeverity.Error, isEnabledByDefault: true), Location.Create(p.SyntaxTree, p.Span), p.Identifier.ToString()));
						continue;
				}
				sb.Append("Id.").Append(p.Identifier.ToString()).Append(", ");
				// TODO we have to set the min/max correctly here. Min should be 0 or 1 and max should be 1 if it's a scalar, and Min/Max can be anything if it's a collection (but typically 0 and int.MaxValue)
				if (pi.Expressions.TryGetValue(Name.ShortNameProperty, out var shortNameSyntax))
				{
					if (pi.Expressions.TryGetValue(Name.LongNameProperty, out var longNameSyntax))
					{
						sb.Append(shortNameSyntax.ToString()).Append(", ").Append(longNameSyntax.ToString()).Append(", 1, 1)\n");
					}
					else
					{
						sb.Append(shortNameSyntax.ToString()).Append(", 1, 1)\n");
					}
				}
				else if (pi.Expressions.TryGetValue(Name.LongNameProperty, out var longNameSyntax))
				{
					sb.Append(longNameSyntax.ToString()).Append(", 1, 1)\n");
				}
				else
				{
					// TODO error about missing short/long name if it's a Switch or Option
				}
			}

			sb.Append(indent.Val).Append(".Build();\n");
			indent.Out();
			sb.Append(indent.Out()).Append("}\n");
			return sb;
		}
	}
}
