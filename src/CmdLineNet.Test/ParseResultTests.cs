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
}
