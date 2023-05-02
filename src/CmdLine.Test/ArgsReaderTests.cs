namespace CmdLine.Test
{
	using CmdLine.Test.ArgsReaderBuilder;
	using Xunit;

	public static class ArgsReaderTests
	{
		private static readonly ArgsReader<ArgId> reader = new ArgsReaderBuilder<ArgId>()
			.Option('a', "alpha", ArgId.A)
			.Option('b', "bravo", ArgId.B)
			.Option('c', "charlie", ArgId.C)
			.Switch('d', "delta", ArgId.D)
			.Switch('e', "echo", ArgId.E)
			.Switch('f', "foxtrot", ArgId.F)
			.Build();
		[Fact]
		public static void MixOfStuff()
		{
			string[] args = new string[] { "-a", "alpha value", "--bravo", "bravo value", "lone value", "--echo", "-d", "some value", "some other value" };
			Assert.Collection(reader.Read(args),
				x => CheckArg(x, ArgId.A, "alpha value", ArgState.ShortOption),
				x => CheckArg(x, ArgId.B, "bravo value", ArgState.LongOption),
				x => CheckArg(x, default, "lone value", ArgState.Value),
				x => CheckArg(x, ArgId.E, null, ArgState.LongSwitch),
				x => CheckArg(x, ArgId.D, null, ArgState.ShortSwitch),
				x => CheckArg(x, default, "some value", ArgState.Value),
				x => CheckArg(x, default, "some other value", ArgState.Value)
			);
		}
		[Fact]
		public static void StackedSwitches()
		{
			string[] args = new string[] { "-def" };
			Assert.Collection(reader.Read(args),
				x => CheckArg(x, ArgId.D, null, ArgState.StackedSwitch),
				x => CheckArg(x, ArgId.E, null, ArgState.StackedSwitch),
				x => CheckArg(x, ArgId.F, null, ArgState.StackedSwitch)
			);
		}
		[Fact]
		public static void DashDashSignifiesOnlyValues()
		{
			string[] args = new string[] { "-a", "alpha value", "--", "-a", "-b", "-c", "value" };
			Assert.Collection(reader.Read(args),
				x => CheckArg(x, ArgId.A, "alpha value", ArgState.ShortOption),
				x => CheckArg(x, default, "-a", ArgState.Value),
				x => CheckArg(x, default, "-b", ArgState.Value),
				x => CheckArg(x, default, "-c", ArgState.Value),
				x => CheckArg(x, default, "value", ArgState.Value)
			);
		}
		[Fact]
		public static void LoneDash()
		{
			string[] args = new string[] { "-" };
			Assert.Collection(reader.Read(args), x => CheckArg(x, default, "-", ArgState.Value));
		}
		[Fact]
		public static void OptionFollowedByNothingIsNull()
		{
			Assert.Collection(reader.Read(new string[] { "-a" }), x => CheckArg(x, ArgId.A, null, ArgState.ShortOption));
			Assert.Collection(reader.Read(new string[] { "--alpha" }), x => CheckArg(x, ArgId.A, null, ArgState.LongOption));
		}
		[Fact]
		public static void Unrecognized()
		{
			Assert.Collection(reader.Read(new string[] { "-x" }), x => CheckArg(x, default, "Unrecognized short argument: -x", ArgState.ShortUnrecognized));
			Assert.Collection(reader.Read(new string[] { "--randomthing" }), x => CheckArg(x, default, "Unrecognized long argument: --randomthing", ArgState.LongUnrecognized));
			Assert.Collection(reader.Read(new string[] { "-dey" }),
				x => CheckArg(x, ArgId.D, null, ArgState.StackedSwitch),
				x => CheckArg(x, ArgId.E, null, ArgState.StackedSwitch),
				x => CheckArg(x, default, "Unrecognized switch found in stacked switches: y", ArgState.ShortUnrecognized)
			);
			Assert.Collection(reader.Read(new string[] { "-dea" }),
				x => CheckArg(x, ArgId.D, null, ArgState.StackedSwitch),
				x => CheckArg(x, ArgId.E, null, ArgState.StackedSwitch),
				x => CheckArg(x, default, "A short option was found in stacked switches: a", ArgState.ShortOptionFoundInStackedSwitches)
			);
		}
		[Fact]
		public static void BadArgTypeEnum()
		{
			ArgsReader<ArgId> reader = new(new Dictionary<char, ArgMeta<ArgId>>()
			{
				['o'] = new ArgMeta<ArgId>(ArgId.B, (ArgType)99999),
			}, new Dictionary<string, ArgMeta<ArgId>>()
			{
				["opt"] = new ArgMeta<ArgId>(ArgId.A, (ArgType)99999),
			});
			Assert.Collection(reader.Read(new string[] { "-o" }), x => CheckArg(x, default, "ArgType enum value was not valid for argument \"-o\": 99999", ArgState.OtherError));
			Assert.Collection(reader.Read(new string[] { "--opt" }), x => CheckArg(x, default, "ArgType enum value was not valid for argument \"--opt\": 99999", ArgState.OtherError));
		}
		private static void CheckArg(RawArg<ArgId> a, ArgId id, string? content, ArgState state)
		{
			Assert.Equal(id, a.Id);
			Assert.Equal(content, a.Content);
			Assert.Equal(state, a.State);
		}
	}
}
