namespace CmdLineNet.SourceGenerator
{
	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp.Syntax;
	using System.Collections.Generic;

	public sealed class ParamListInfo
	{
		public ParamListInfo(ParameterListSyntax parameterListSyntax, List<ParamInfo> paramInfo)
		{
			ParameterListSyntax = parameterListSyntax;
			ParamInfos = paramInfo;
		}
		public ParameterListSyntax ParameterListSyntax { get; }
		public List<ParamInfo> ParamInfos { get; }
		public static ParamListInfo Get(ParameterListSyntax pls, SemanticModel sm, GeneratorExecutionContext context)
		{
			List<ParamInfo> parameters = new(pls.Parameters.Count);
			foreach (var p in pls.Parameters)
			{
				parameters.Add(ParamInfo.Get(p, sm, context));
			}
			return new(pls, parameters);
		}
	}
}
