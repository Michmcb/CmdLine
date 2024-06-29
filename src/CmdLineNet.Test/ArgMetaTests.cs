namespace CmdLineNet.Test.ArgsReaderBuilder
{
	using CmdLineNet;
	using Xunit;

	public static class ArgMetaTests
	{
		[Fact]
		public static void Ctor()
		{
			ArgMeta<ArgId> a = new(ArgId.C, ArgType.Option, 'g', "foo", 7, 129, "blah blah");
			Assert.Equal(ArgId.C, a.Id);
			Assert.Equal(ArgType.Option, a.Type);
			Assert.Equal('g', a.ShortName);
			Assert.Equal("foo", a.Name);
			Assert.Equal(7, a.Min);
			Assert.Equal(129, a.Max);
			Assert.Equal("blah blah", a.Help);
		}
	}
}