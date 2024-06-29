namespace CmdLineNet.Test
{
	using CmdLineNet;
	using System;
	using System.Collections.Generic;
	using Xunit;
	public static class ArgsReaderBuilderTests
	{
		[Fact]
		public static void AllGood()
		{
			ArgsReaderBuilder<ArgId> builder = new();
			builder.Option(ArgId.A, 'a', "alpha", 1, 1);
			builder.Option(ArgId.B, 'b', 1, 2);
			builder.Option(ArgId.C, "charlie", 2, 2, "help");
			builder.Switch(ArgId.D, 'd', "delta", 1, 1, "");
			builder.Switch(ArgId.E, 'e', 1, 1);
			builder.Switch(ArgId.F, "foxtrot", 1, 1);
			builder.Value(ArgId.V, "Value", 1, 1);

			{
				var a = builder.OrderedArguments[0];
				var b = builder.OrderedArguments[1];
				var c = builder.OrderedArguments[2];
				var d = builder.OrderedArguments[3];
				var e = builder.OrderedArguments[4];
				var f = builder.OrderedArguments[5];
				var v = builder.OrderedArguments[6];

				CheckMeta(a, ArgId.A, ArgType.Option, 1, 1, null);
				CheckMeta(b, ArgId.B, ArgType.Option, 1, 2, null);
				CheckMeta(c, ArgId.C, ArgType.Option, 2, 2, "help");
				CheckMeta(d, ArgId.D, ArgType.Switch, 1, 1, "");
				CheckMeta(e, ArgId.E, ArgType.Switch, 1, 1, null);
				CheckMeta(f, ArgId.F, ArgType.Switch, 1, 1, null);
				CheckMeta(v, ArgId.V, ArgType.Value, 1, 1, null);

				Assert.Equal(a, builder.ShortArgs['a']);
				Assert.Equal(b, builder.ShortArgs['b']);
				Assert.Equal(d, builder.ShortArgs['d']);
				Assert.Equal(e, builder.ShortArgs['e']);
				Assert.Equal(a, builder.LongArgs["alpha"]);
				Assert.Equal(c, builder.LongArgs["charlie"]);
				Assert.Equal(d, builder.LongArgs["delta"]);
				Assert.Equal(f, builder.LongArgs["foxtrot"]);
				Assert.Equal(v, builder.Values[0]);
			}
			var reader = builder.Build();

			{
				var a = reader.OrderedArguments[0];
				var b = reader.OrderedArguments[1];
				var c = reader.OrderedArguments[2];
				var d = reader.OrderedArguments[3];
				var e = reader.OrderedArguments[4];
				var f = reader.OrderedArguments[5];
				var v = reader.OrderedArguments[6];

				CheckMeta(a, ArgId.A, ArgType.Option, 1, 1, null);
				CheckMeta(b, ArgId.B, ArgType.Option, 1, 2, null);
				CheckMeta(c, ArgId.C, ArgType.Option, 2, 2, "help");
				CheckMeta(d, ArgId.D, ArgType.Switch, 1, 1, "");
				CheckMeta(e, ArgId.E, ArgType.Switch, 1, 1, null);
				CheckMeta(f, ArgId.F, ArgType.Switch, 1, 1, null);
				CheckMeta(v, ArgId.V, ArgType.Value, 1, 1, null);

				Assert.Equal(a, reader.ShortArgs['a']);
				Assert.Equal(b, reader.ShortArgs['b']);
				Assert.Equal(d, reader.ShortArgs['d']);
				Assert.Equal(e, reader.ShortArgs['e']);
				Assert.Equal(a, reader.LongArgs["alpha"]);
				Assert.Equal(c, reader.LongArgs["charlie"]);
				Assert.Equal(d, reader.LongArgs["delta"]);
				Assert.Equal(f, reader.LongArgs["foxtrot"]);
				Assert.Equal(v, reader.Values[0]);
			}
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
		private static void CheckMeta(ArgMeta<ArgId> meta, ArgId id, ArgType type, int min, int max, string? help)
		{
			Assert.Equal(id, meta.Id);
			Assert.Equal(type, meta.Type);
			Assert.Equal(min, meta.Min);
			Assert.Equal(max, meta.Max);
			Assert.Equal(help, meta.Help);
		}
	}
}