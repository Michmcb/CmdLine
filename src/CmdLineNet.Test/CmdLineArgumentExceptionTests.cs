namespace CmdLineNet.Test
{
	using System;
	using Xunit;

	public static class CmdLineArgumentExceptionTests
	{
		[Fact]
		public static void Ctor()
		{
			Exception ex = new();
			CmdLineArgumentException ex1 = new();
			Assert.Equal("Exception of type 'CmdLineNet.CmdLineArgumentException' was thrown.", ex1.Message);
			Assert.Null(ex1.InnerException);

			CmdLineArgumentException ex2 = new("Message2");
			Assert.Equal("Message2", ex2.Message);
			Assert.Null(ex2.InnerException);


			CmdLineArgumentException ex3 = new("Message3", ex);
			Assert.Equal("Message3", ex3.Message);
			Assert.Same(ex3.InnerException, ex);
		}
	}
}
