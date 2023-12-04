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
			builder.Option(ArgId.A, 'a', "alpha", _ => { });
			builder.Option(ArgId.B, 'b', _ => { });
			builder.Option(ArgId.C, "charlie", _ => { });
			builder.Switch(ArgId.D, 'd', "delta", _ => { });
			builder.Switch(ArgId.E, 'e', _ => { });
			builder.Switch(ArgId.F, "foxtrot", _ => { });

			CheckDictionaries(builder.ShortOpts, builder.LongOpts);
			var reader = builder.Build();
			CheckDictionaries(reader.ShortOpts, reader.LongOpts);
		}
		private static void CheckDictionaries(IReadOnlyDictionary<char, ArgIdType<ArgId>> shortOpts, IReadOnlyDictionary<string, ArgIdType<ArgId>> longOpts)
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
			builder.Option(ArgId.A, "alpha", _ => { });
			Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, "ALPHA", _ => { }));
		}
		[Fact]
		public static void BadKeys()
		{
			ArgsReaderBuilder<ArgId> builder = new();
			builder.Option(ArgId.A, 'a', "alpha", _ => { });
			CmdLineArgumentException ex;
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, '\n', _ => { }));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, 'a', _ => { }));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, '-', _ => { }));

			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, null!, _ => { }));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, "", _ => { }));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, "   ", _ => { }));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, "a", _ => { }));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, "alpha", _ => { }));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, "-", _ => { }));
			ex = Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, "--", _ => { }));

			// Since both of those failed, we must NOT have added anything to the dictionaries!
			Assert.DoesNotContain('b', (IDictionary<char, ArgIdType<ArgId>>)builder.ShortOpts);
			Assert.DoesNotContain("bravo", (IDictionary<string, ArgIdType<ArgId>>)builder.LongOpts);
		}
		[Fact]
		public static void OneLetterLongArgument()
		{
			ArgsReaderBuilder<ArgId> builder = new();
			Assert.Throws<CmdLineArgumentException>(() => builder.Option(ArgId.A, "x", _ => { }));
			Assert.Throws<CmdLineArgumentException>(() => builder.Switch(ArgId.A, "x", _ => { }));
		}
		private static void CheckMeta(ArgIdType<ArgId> meta, ArgId id, ArgType type)
		{
			Assert.Equal(id, meta.Id);
			Assert.Equal(type, meta.Type);
		}
	}
}