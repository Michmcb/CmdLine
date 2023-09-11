namespace CmdLineNet.Test
{
	using Xunit;

	public static class ParseResultTests
	{
		[Fact]
		public static void Good()
		{
			Guid g = Guid.NewGuid();
			foreach (ParseResult<Guid> pr in new ParseResult<Guid>[] {new ParseResult<Guid>(g), g})
			{
				Assert.True(pr.Ok(out Guid parsed, out string? errMsg));
				Assert.Equal(g, parsed);
				Assert.Null(errMsg);
			}
		}
		[Fact]
		public static void Bad()
		{
			string b = "oh no";
			foreach (ParseResult<Guid> pr in new ParseResult<Guid>[] { new ParseResult<Guid>(b), b })
			{
				Assert.False(pr.Ok(out Guid parsed, out string? errMsg));
				Assert.Equal(default, parsed);
				Assert.Equal(b, errMsg);
			}
		}
	}
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
