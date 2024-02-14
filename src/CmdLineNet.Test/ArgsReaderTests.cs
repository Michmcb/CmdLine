namespace CmdLineNet.Test
{
	using CmdLineNet.Test.ArgsReaderBuilder;
	using Xunit;

	public static class ArgsReaderTests
	{
		[Fact]
		public static void ParsingToObject()
		{
			string[] args = new string[] { "-a", "alpha value", "--bravo", "bravo value", "--charlie", "charlie value", "--echo", "-f" };
			Assert.True(Args.Parse(Args.GetReader().Read(args)).Ok(out var parsed, out var errMsg));
			Check(parsed);

			static void Check(Args parsed)
			{
				Assert.Equal("alpha value", parsed.A);
				Assert.Equal("bravo value", parsed.B);
				Assert.Equal("charlie value", parsed.C);
				Assert.False(parsed.D);
				Assert.True(parsed.E);
				Assert.True(parsed.F);
			}
		}
		[Fact]
		public static void MixOfStuff()
		{
			string[] args = new string[] { "-a", "alpha value", "--bravo", "bravo value", "lone value", "--echo", "-d", "some value", "some other value" };
			Assert.Collection(Args.GetReader().Read(args),
				x => CheckArg(x, ArgId.A, "alpha value", ArgState.ShortOption),
				x => CheckArg(x, ArgId.B, "bravo value", ArgState.LongOption),
				x => CheckArg(x, ArgId.V, "lone value", ArgState.Value),
				x => CheckArg(x, ArgId.E, string.Empty, ArgState.LongSwitch),
				x => CheckArg(x, ArgId.D, string.Empty, ArgState.ShortSwitch),
				x => CheckArg(x, ArgId.V, "some value", ArgState.Value),
				x => CheckArg(x, ArgId.V, "some other value", ArgState.Value)
			);
		}
		[Fact]
		public static void StackedSwitches()
		{
			string[] args = new string[] { "-def" };
			Assert.Collection(Args.GetReader().Read(args),
				x => CheckArg(x, ArgId.D, string.Empty, ArgState.StackedSwitch),
				x => CheckArg(x, ArgId.E, string.Empty, ArgState.StackedSwitch),
				x => CheckArg(x, ArgId.F, string.Empty, ArgState.StackedSwitch)
			);
		}
		[Fact]
		public static void DashDashSignifiesOnlyValues()
		{
			string[] args = new string[] { "-a", "alpha value", "--", "-a", "-b", "-c", "value" };
			Assert.Collection(Args.GetReader().Read(args),
				x => CheckArg(x, ArgId.A, "alpha value", ArgState.ShortOption),
				x => CheckArg(x, ArgId.V, "-a", ArgState.Value),
				x => CheckArg(x, ArgId.V, "-b", ArgState.Value),
				x => CheckArg(x, ArgId.V, "-c", ArgState.Value),
				x => CheckArg(x, ArgId.V, "value", ArgState.Value)
			);
		}
		[Fact]
		public static void LoneDash()
		{
			string[] args = new string[] { "-" };
			Assert.Collection(Args.GetReader().Read(args), x => CheckArg(x, ArgId.V, "-", ArgState.Value));
		}
		[Fact]
		public static void OptionFollowedByNothingIsNull()
		{
			Assert.Collection(Args.GetReader().Read(new string[] { "-a" }), x => CheckArg(x, ArgId.A, string.Empty, ArgState.ShortOption));
			Assert.Collection(Args.GetReader().Read(new string[] { "--alpha" }), x => CheckArg(x, ArgId.A, string.Empty, ArgState.LongOption));
		}
		[Fact]
		public static void TooManyShortOptions()
		{
			Assert.Collection(Args.GetReader().Read(new string[] { "-a", "blah", "-a", "blah" }),
				x => CheckArg(x, ArgId.A, "blah", ArgState.ShortOption),
				x => CheckArg(x, ArgId.A, "Option was provided too many times: -a", ArgState.TooManyOptions));
		}
		[Fact]
		public static void TooManyLongOptions()
		{
			Assert.Collection(Args.GetReader().Read(new string[] { "--alpha", "blah", "--alpha", "blah" }),
				x => CheckArg(x, ArgId.A, "blah", ArgState.LongOption),
				x => CheckArg(x, ArgId.A, "Option was provided too many times: --alpha", ArgState.TooManyOptions));
		}
		[Fact]
		public static void TooManyOptions()
		{
			Assert.Collection(Args.GetReader().Read(new string[] { "--alpha", "blah", "-a", "blah" }),
				x => CheckArg(x, ArgId.A, "blah", ArgState.LongOption),
				x => CheckArg(x, ArgId.A, "Option was provided too many times: -a", ArgState.TooManyOptions));
		}
		[Fact]
		public static void TooManyShortSwitches()
		{
			Assert.Collection(Args.GetReader().Read(new string[] { "-d", "-d" }),
				x => CheckArg(x, ArgId.D, "", ArgState.ShortSwitch),
				x => CheckArg(x, ArgId.D, "Switch was provided too many times: -d", ArgState.TooManySwitches));
		}
		[Fact]
		public static void TooManyShortStackedSwitches()
		{
			Assert.Collection(Args.GetReader().Read(new string[] { "-dd", }),
				x => CheckArg(x, ArgId.D, "", ArgState.StackedSwitch),
				x => CheckArg(x, ArgId.D, "Switch was provided too many times in stacked switches: d", ArgState.TooManySwitches));
		}
		[Fact]
		public static void TooManySwitches()
		{
			Assert.Collection(Args.GetReader().Read(new string[] { "-d", "--delta" }),
				x => CheckArg(x, ArgId.D, "", ArgState.ShortSwitch),
				x => CheckArg(x, ArgId.D, "Switch was provided too many times: --delta", ArgState.TooManySwitches));
		}
		[Fact]
		public static void TooManyLongSwitches()
		{
			Assert.Collection(Args.GetReader().Read(new string[] { "--delta", "--delta" }),
				x => CheckArg(x, ArgId.D, "", ArgState.LongSwitch),
				x => CheckArg(x, ArgId.D, "Switch was provided too many times: --delta", ArgState.TooManySwitches));
		}
		[Fact]
		public static void TooManyValues()
		{
			var r = new ArgsReaderBuilder<ArgId>()
				.Value(ArgId.V, 0, 2)
				.Build();

			Assert.Collection(r.Read(new string[] { "value1", "value2", "value3" }),
				x => CheckArg(x, ArgId.V, "value1", ArgState.Value),
				x => CheckArg(x, ArgId.V, "value2", ArgState.Value),
				x => CheckArg(x, default, "Too many values were provided: value3", ArgState.TooManyValues));

			Assert.Collection(r.Read(new string[] { "-", "-", "-" }),
				x => CheckArg(x, ArgId.V, "-", ArgState.Value),
				x => CheckArg(x, ArgId.V, "-", ArgState.Value),
				x => CheckArg(x, default, "Too many values were provided: -", ArgState.TooManyValues));

			Assert.Collection(r.Read(new string[] { "--", "value1", "value2", "value3" }),
				x => CheckArg(x, ArgId.V, "value1", ArgState.Value),
				x => CheckArg(x, ArgId.V, "value2", ArgState.Value),
				x => CheckArg(x, default, "Too many values were provided: value3", ArgState.TooManyValues));
		}
		[Fact]
		public static void Unrecognized()
		{
			Assert.Collection(Args.GetReader().Read(new string[] { "-x" }), x => CheckArg(x, default, "Unrecognized short argument: -x", ArgState.ShortUnrecognized));
			Assert.Collection(Args.GetReader().Read(new string[] { "--randomthing" }), x => CheckArg(x, default, "Unrecognized long argument: --randomthing", ArgState.LongUnrecognized));
			Assert.Collection(Args.GetReader().Read(new string[] { "-dey" }),
				x => CheckArg(x, ArgId.D, string.Empty, ArgState.StackedSwitch),
				x => CheckArg(x, ArgId.E, string.Empty, ArgState.StackedSwitch),
				x => CheckArg(x, default, "Unrecognized switch found in stacked switches: y", ArgState.ShortUnrecognized)
			);
			Assert.Collection(Args.GetReader().Read(new string[] { "-dea" }),
				x => CheckArg(x, ArgId.D, string.Empty, ArgState.StackedSwitch),
				x => CheckArg(x, ArgId.E, string.Empty, ArgState.StackedSwitch),
				x => CheckArg(x, default, "A short option was found in stacked switches: a", ArgState.ShortOptionFoundInStackedSwitches)
			);
		}
		[Fact]
		public static void BadArgTypeEnum()
		{
			ArgsReader<ArgId> reader = new(new Dictionary<char, ArgMeta<ArgId>>()
			{
				['o'] = new ArgMeta<ArgId>(ArgId.B, (ArgType)99999, 1, 1, null),
			}, new Dictionary<string, ArgMeta<ArgId>>()
			{
				["opt"] = new ArgMeta<ArgId>(ArgId.B, (ArgType)99999, 1, 1, null),
			}, Array.Empty<ArgMeta<ArgId>>(), [new ArgMeta<ArgId>(ArgId.B, (ArgType)99999, 1, 1, null)]);
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
