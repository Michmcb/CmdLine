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
		public ParamInfo(ParameterSyntax parameter, IParameterSymbol symbol, ISymbol? defaultValueSymbol, AttributeSyntax? attribute, AttribType attributeType, IReadOnlyDictionary<string, ExpressionSyntax> expressions)
		{
			Parameter = parameter;
			Symbol = symbol;
			DefaultValueSymbol = defaultValueSymbol;
			Attribute = attribute;
			AttributeType = attributeType;
			Expressions = expressions;
		}
		public ParameterSyntax Parameter { get; }
		public IParameterSymbol Symbol { get; }
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
			return new ParamInfo(p, symbol, defaultValueSymbol, attrib, attributeType, expressions);
		}

	}
}
