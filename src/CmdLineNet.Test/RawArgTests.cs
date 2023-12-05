namespace CmdLineNet.Test
{
	using Xunit;

	public static class RawArgTests
	{
		[Fact]
		public static void OkProperty()
		{
			Assert.True(new RawArg<int>(1, "Content", ArgState.Value).Ok);
			Assert.True(new RawArg<int>(1, "Content", ArgState.LongOption).Ok);
			Assert.True(new RawArg<int>(1, "Content", ArgState.ShortOption).Ok);
			Assert.True(new RawArg<int>(1, "Content", ArgState.LongSwitch).Ok);
			Assert.True(new RawArg<int>(1, "Content", ArgState.ShortSwitch).Ok);
			Assert.True(new RawArg<int>(1, "Content", ArgState.StackedSwitch).Ok);
			Assert.False(new RawArg<int>(1, "Content", ArgState.OtherError).Ok);
			Assert.False(new RawArg<int>(1, "Content", ArgState.ShortUnrecognized).Ok);
			Assert.False(new RawArg<int>(1, "Content", ArgState.LongUnrecognized).Ok);
			Assert.False(new RawArg<int>(1, "Content", ArgState.ShortOptionFoundInStackedSwitches).Ok);
		}
	}
}
