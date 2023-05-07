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
			builder.Option('a', "alpha", ArgId.A);
			builder.Option('b', ArgId.B);
			builder.Option("charlie", ArgId.C);
			builder.Switch('d', "delta", ArgId.D);
			builder.Switch('e', ArgId.E);
			builder.Switch("foxtrot", ArgId.F);

			CheckDictionaries(builder.ShortOpts, builder.LongOpts);
			var reader = builder.Build();
			CheckDictionaries(reader.ShortOpts, reader.LongOpts);
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
			builder.Option("alpha", ArgId.A);
			Assert.Throws<CmdLineArgumentException>(() => builder.Option("ALPHA", ArgId.A));
		}
		[Fact]
		public static void BadKeys()
		{
			ArgsReaderBuilder<ArgId> builder = new();
			builder.Option('a', "alpha", ArgId.A);
			CmdLineArgumentException ex;
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option('\n', ArgId.A));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option('a', ArgId.A));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option('-', ArgId.A));

			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option(null!, ArgId.A));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option("", ArgId.A));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option("   ", ArgId.A));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option("a", ArgId.A));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option("alpha", ArgId.A));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option("-", ArgId.A));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option("--", ArgId.A));

			// Since both of those failed, we must NOT have added anything to the dictionaries!
			Assert.DoesNotContain('b', (IDictionary<char, ArgMeta<ArgId>>)builder.ShortOpts);
			Assert.DoesNotContain("bravo", (IDictionary<string, ArgMeta<ArgId>>)builder.LongOpts);
		}
		[Fact]
		public static void OneLetterLongArgument()
		{
			ArgsReaderBuilder<ArgId> builder = new();
			Assert.Throws<CmdLineArgumentException>(() => builder.Option("x", ArgId.A));
			Assert.Throws<CmdLineArgumentException>(() => builder.Switch("x", ArgId.A));
		}
		private static void CheckMeta(ArgMeta<ArgId> meta, ArgId id, ArgType type)
		{
			Assert.Equal(id, meta.Id);
			Assert.Equal(type, meta.Type);
		}
	}
}