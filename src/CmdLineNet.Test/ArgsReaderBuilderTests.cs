namespace CmdLineNet.Test.ArgsReaderBuilder
{
	using CmdLineNet;
	using Xunit;
	public static class ArgsReaderBuilderTests
	{
		[Fact]
		public static void AllGood()
		{
			ArgsReaderBuilder<ArgId> builder = new();
			builder.Option(ArgId.A, 'a', "alpha", 1, 1);
			builder.Option(ArgId.B, 'b', 1, 1);
			builder.Option(ArgId.C, "charlie", 1, 1);
			builder.Switch(ArgId.D, 'd', "delta", 1, 1);
			builder.Switch(ArgId.E, 'e', 1, 1);
			builder.Switch(ArgId.F, "foxtrot", 1, 1);

			CheckDictionaries(builder.ShortArgs, builder.LongArgs);
			var reader = builder.Build();
			CheckDictionaries(reader.ShortArgs, reader.LongArgs);
		}
		private static void CheckDictionaries(IReadOnlyDictionary<char, ArgMeta<ArgId>> shortOpts, IReadOnlyDictionary<string, ArgMeta<ArgId>> longOpts)
		{
			CheckMeta(Assert.Contains('a', shortOpts), ArgId.A, ArgType.Option);
			CheckMeta(Assert.Contains('b', shortOpts), ArgId.B, ArgType.Option);
			CheckMeta(Assert.Contains('d', shortOpts), ArgId.D, ArgType.Switch);
			CheckMeta(Assert.Contains('e', shortOpts), ArgId.E, ArgType.Switch);

			CheckMeta(Assert.Contains("alpha", longOpts), ArgId.A, ArgType.Option);
			CheckMeta(Assert.Contains("charlie", longOpts), ArgId.C, ArgType.Option);
			CheckMeta(Assert.Contains("delta", longOpts), ArgId.D, ArgType.Switch);
			CheckMeta(Assert.Contains("foxtrot", longOpts), ArgId.F, ArgType.Switch);
		}
		[Fact]
		public static void CaseInsensitiveKeys()
		{
			ArgsReaderBuilder<ArgId> builder = new(EqualityComparer<char>.Default, StringComparer.OrdinalIgnoreCase);
			builder.Option(ArgId.A, "alpha", 1, 1);
			Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, "ALPHA", 1, 1));
		}
		[Fact]
		public static void BadKeys()
		{
			ArgsReaderBuilder<ArgId> builder = new();
			builder.Option(ArgId.A, 'a', "alpha", 1, 1);
			CmdLineArgumentException ex;
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, '\n', 1, 1));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, 'a', 1, 1));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, '-', 1, 1));

			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, null!, 1, 1));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, "", 1, 1));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, "   ", 1, 1));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, "a", 1, 1));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, "alpha", 1, 1));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, "-", 1, 1));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, "--", 1, 1));

			// Since both of those failed, we must NOT have added anything to the dictionaries!
			Assert.DoesNotContain('b', (IDictionary<char, ArgMeta<ArgId>>)builder.ShortArgs);
			Assert.DoesNotContain("bravo", (IDictionary<string, ArgMeta<ArgId>>)builder.LongArgs);
		}
		[Fact]
		public static void OneLetterLongArgument()
		{
			ArgsReaderBuilder<ArgId> builder = new();
			Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, "x", 1, 1));
			Assert.Throws<CmdLineArgumentException>(() => builder.Switch(ArgId.A, "x", 1, 1));
		}
		private static void CheckMeta(ArgMeta<ArgId> meta, ArgId id, ArgType type)
		{
			Assert.Equal(id, meta.Id);
			Assert.Equal(type, meta.Type);
		}
	}
}