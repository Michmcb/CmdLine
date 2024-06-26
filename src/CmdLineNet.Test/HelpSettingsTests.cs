namespace CmdLineNet.Test
{
	using Xunit;

	public static class HelpWriterTests
	{
		private static readonly StringWriter sout;
		static HelpWriterTests()
		{
			sout = new();
			Console.SetOut(sout);
		}
		[Fact]
		public static void Ctor()
		{
			DictionaryVerbHandler<int> verbs = new([], DefaultDelegate.UnknownVerbHandler(1));

			verbs.AddVerb(new Verb<int>("alpha", "The first verb", () => 0, HelpWriter.ConsoleWriteHelp(GetArgs.GetReader().OrderedArguments, HelpSettings.Default)));
		}
	}
	public static class HelpSettingsTests
	{
		[Fact]
		public static void Ctor()
		{
			HelpSettings s = new("x", 100, 200, false);
			Assert.Equal("x", s.LongShortNameSeparator);
			Assert.Equal(100, s.LeftMargin);
			Assert.Equal(200, s.RightMargin);
			Assert.False(s.AlignHelpTextAcrossAllGroups);
		}
		[Fact]
		public static void Default()
		{
			HelpSettings s = HelpSettings.Default;
			Assert.Equal(" ", s.LongShortNameSeparator);
			Assert.Equal(1, s.LeftMargin);
			Assert.Equal(3, s.RightMargin);
			Assert.True(s.AlignHelpTextAcrossAllGroups);
		}
	}
}
