namespace CmdLineNet.SourceGenerator
{
	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp;
	using Microsoft.CodeAnalysis.CSharp.Syntax;
	using System;
	using System.Collections.Generic;

	public sealed class ParamInfo
	{
		public static readonly IReadOnlyDictionary<string, ExpressionSyntax> EmptyExpressions = new Dictionary<string, ExpressionSyntax>();
		public ParamInfo(ParameterSyntax parameter, IParameterSymbol symbol, ITypeSymbol elementTypeSymbol, bool isCollection, bool isArray, ISymbol? defaultValueSymbol, AttributeSyntax? attribute, AttribType attributeType, IReadOnlyDictionary<string, ExpressionSyntax> expressions)
		{
			Parameter = parameter;
			Symbol = symbol;
			ElementTypeSymbol = elementTypeSymbol;
			IsCollection = isCollection;
			IsArray = isArray;
			DefaultValueSymbol = defaultValueSymbol;
			Attribute = attribute;
			AttributeType = attributeType;
			Expressions = expressions;
		}
		public ParameterSyntax Parameter { get; }
		public IParameterSymbol Symbol { get; }
		public ITypeSymbol ElementTypeSymbol { get; }
		public bool IsCollection { get; }
		public bool IsArray { get; }
		public ISymbol? DefaultValueSymbol { get; }
		public AttributeSyntax? Attribute { get; }
		public AttribType AttributeType { get; }
		public IReadOnlyDictionary<string, ExpressionSyntax> Expressions { get; }
		public static ParamInfo Get(ParameterSyntax p, SemanticModel sm, GeneratorExecutionContext context)
		{
			AttribType attributeType = AttribType.None;
			IReadOnlyDictionary<string, ExpressionSyntax> expressions = EmptyExpressions;
			var symbol = sm.GetDeclaredSymbol(p) ?? throw new InvalidOperationException("Could not get semantic model for class symbol: " + p.ToString());
			ISymbol? defaultValueSymbol = null;
			if (p.Default != null) defaultValueSymbol = sm.GetSymbolInfo(p.Default.Value).Symbol;

			// If the type implement IEnumerable, then it's collection-like. In that case, just use a List
			// If it's an array, use a list but we need to convert it to an array at the end
			// That's all for now; other collection types like Stack or Queue need to call Push/Enqueue
			bool isCollection = false;
			bool isArray = false;
			ITypeSymbol elementSymbol = symbol.Type;
			if (symbol.Type is IArrayTypeSymbol array)
			{
				isCollection = true;
				isArray = true;
				elementSymbol = array.ElementType;
			}

			if (Util.TryGetAttribute(p, out var attrib, out AttribType t))
			{
				attributeType = t;
				if (attrib.ArgumentList != null)
				{
					expressions = Util.GetExpressions(attrib.ArgumentList.Arguments);
				}
			}
			else
			{
				// TODO emit error
			}
			return new ParamInfo(p, symbol, elementSymbol, isCollection, isArray, defaultValueSymbol, attrib, attributeType, expressions);
		}
	}
}
