namespace CmdLineNet.SourceGenerator
{
	using Microsoft.CodeAnalysis;

	public sealed class MethodStuff
	{
		public MethodStuff(ITypeSymbol? returnType, IMethodSymbol symbol)
		{
			ReturnType = returnType;
			Symbol = symbol;
		}
		public ITypeSymbol? ReturnType { get; }
		public IMethodSymbol Symbol { get; }
	}
}
