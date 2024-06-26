namespace CmdLineNet.Test.ArgsReaderBuilder
{
	using CmdLineNet;
	using Xunit;
	public static class ArgCountTests
	{
		[Fact]
		public static void Ctor()
		{
			ArgCount<ArgId> a = new(ArgId.A, 3);
			Assert.Equal(ArgId.A, a.Id);
			Assert.Equal(3, a.Max);
		}
	}
}