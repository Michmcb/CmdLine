namespace CmdLineNet.Test
{
	using Xunit;
	public static class HelpSettingsTests
	{
		[Fact]
		public static void Ctor()
		{
			HelpWriterSettings s = new("x", 100, 200, HelpTextAlign.None);
			Assert.Equal("x", s.LongShortNameSeparator);
			Assert.Equal(100, s.LeftMargin);
			Assert.Equal(200, s.RightMargin);
			Assert.Equal(HelpTextAlign.None, s.HelpTextAlign);
		}
		[Fact]
		public static void Default()
		{
			HelpWriterSettings s = HelpWriterSettings.Default;
			Assert.Equal(" ", s.LongShortNameSeparator);
			Assert.Equal(1, s.LeftMargin);
			Assert.Equal(3, s.RightMargin);
			Assert.Equal(HelpTextAlign.AcrossAllGroups, s.HelpTextAlign);
		}
	}
}
