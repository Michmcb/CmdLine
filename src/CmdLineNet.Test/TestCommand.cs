namespace CmdLineNet.Test.ArgsReaderBuilder
{
	using CmdLine;

	[Command]
	public sealed partial record class TestCommand([Option(LongName = "value", ShortName = 'v')]string Value);
}