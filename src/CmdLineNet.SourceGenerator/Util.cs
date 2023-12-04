namespace CmdLineNet.SourceGenerator
{
	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp.Syntax;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class Util
	{
		public static bool TryGetAttribute(ParameterSyntax p, out AttributeSyntax a, out AttribType t)
		{
			AttributeSyntax? sAttr = p.AttributeLists.SelectMany(x => x.Attributes).Where(x => x.Name.ToString() == Name.SwitchAttribute).FirstOrDefault();
			AttributeSyntax? oAttr = p.AttributeLists.SelectMany(x => x.Attributes).Where(x => x.Name.ToString() == Name.OptionAttribute).FirstOrDefault();
			AttributeSyntax? vAttr = p.AttributeLists.SelectMany(x => x.Attributes).Where(x => x.Name.ToString() == Name.ValueAttribute).FirstOrDefault();

			a = null!;
			t = default;
			if (sAttr != null)
			{
				if (oAttr != null) return false;
				if (vAttr != null) return false;
				a = sAttr;
				t = AttribType.Switch;
				return true;
			}
			else if (oAttr != null)
			{
				if (sAttr != null) return false;
				if (vAttr != null) return false;
				a = oAttr;
				t = AttribType.Option;
				return true;
			}
			else if (vAttr != null)
			{
				if (sAttr != null) return false;
				if (oAttr != null) return false;
				a = vAttr;
				t = AttribType.Value;
				return true;
			}
			return false;
		}
		public static Dictionary<string, ExpressionSyntax> GetExpressions(SeparatedSyntaxList<AttributeArgumentSyntax> args)
		{
			Dictionary<string, ExpressionSyntax> d = new(StringComparer.Ordinal);
			foreach (var a in args)
			{
				var nameIdentifier = a.NameEquals?.Name ?? a.NameColon?.Name;
				if (nameIdentifier != null)
				{
					d[nameIdentifier.ToString()] = a.Expression;
				}
			}
			return d;
		}
	}
}
