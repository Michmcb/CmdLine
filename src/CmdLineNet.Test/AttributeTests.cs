namespace CmdLineNet.Test
{
	using Xunit;

	public static class AttributeTests
	{
		[Fact]
		public static void Verb()
		{
			VerbAttribute a = new();
			Assert.Null(a.Help);

			a = new()
			{
				Help = "Help",
			};
			Assert.Equal("Help", a.Help);
		}
		[Fact]
		public static void Switch()
		{
			SwitchAttribute a = new();
			Assert.Equal(default, a.ShortName);
			Assert.Null(a.LongName);
			Assert.Equal(1, a.Min);
			Assert.Equal(int.MaxValue, a.Max);
			Assert.Null(a.Help);

			a = new()
			{
				ShortName = 's',
				LongName ="Long",
				Min = 2,
				Max = 3,
				Help = "Help",
			};
			Assert.Equal('s', a.ShortName);
			Assert.Equal("Long", a.LongName);
			Assert.Equal(2, a.Min);
			Assert.Equal(3, a.Max);
			Assert.Equal("Help", a.Help);
		}
		[Fact]
		public static void Option()
		{
			OptionAttribute a = new();
			Assert.Equal(default, a.ShortName);
			Assert.Null(a.LongName);
			Assert.Equal(1, a.Min);
			Assert.Equal(int.MaxValue, a.Max);
			Assert.Null(a.Help);

			a = new()
			{
				ShortName = 's',
				LongName = "Long",
				Min = 2,
				Max = 3,
				Help = "Help",
			};
			Assert.Equal('s', a.ShortName);
			Assert.Equal("Long", a.LongName);
			Assert.Equal(2, a.Min);
			Assert.Equal(3, a.Max);
			Assert.Equal("Help", a.Help);
		}
		[Fact]
		public static void Value()
		{
			ValueAttribute a = new();
			Assert.Equal(1, a.Min);
			Assert.Equal(int.MaxValue, a.Max);
			Assert.Null(a.Help);

			a = new()
			{
				Min = 2,
				Max = 3,
				Help = "Help",
			};
			Assert.Equal(2, a.Min);
			Assert.Equal(3, a.Max);
			Assert.Equal("Help", a.Help);
		}
	}
}
