namespace CmdLineNet.Test
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using Xunit;

	public static class VerbHandlerTests
	{
		private static readonly StringWriter sout;
		private sealed record class Alpha : ICmdParseable<byte, Alpha>
		{
			private static readonly ArgsReader<byte> reader = new ArgsReaderBuilder<byte>()
				.Option(0, 'a', 1, 1, "This is the first option")
				.Option(1, "beta", 1, 1, "This is the second option")
				.Option(2, 'c', "charlie", 1, 1, "This is the third option, which has a longer name than the other options, but it should wrap the text just fine")
				.Switch(3, 'e', "epsilon", 1, 1, "This is the first switch")
				.Value(4, "delta", 1, 1, "This is the first value")
				.Value(5, null, 1, 1, "Nameless value")
				.Build();
			public static ArgsReader<byte> GetReader() { return reader; }
			public static ParseResult<Alpha> Parse(IEnumerable<RawArg<byte>> args)
			{
				Span<bool> accepted = stackalloc bool[6];
				accepted.Clear();
				foreach (var a in args)
				{
					if (a.Ok)
					{
						accepted[a.Id] = true;
					}
					else
					{
						return new ParseResult<Alpha>(a.Content);
					}
				}
				for (int i = 0; i < accepted.Length; i++)
				{
					if (!accepted[i])
					{
						var am = GetReader().OrderedArguments[i];
						string argName;
						if (am.ShortName != default)
						{
							argName = am.Name != null ?
								string.Concat(am.ShortName.ToString(), "|", am.Name)
								: am.ShortName.ToString();
						}
						else
						{
							argName = am.Name ?? i.ToString();
						}
						return new ParseResult<Alpha>("Missing argument " + argName);
					}
				}
				return new ParseResult<Alpha>(new Alpha());
			}
		}
		static VerbHandlerTests()
		{
			sout = new();
			Console.SetOut(sout);
		}
		[Fact]
		public static void HandleVerbTests()
		{
			// I feel like testing console output like this is not the best way to do it....but at least it works...
			DictionaryVerbHandler<int> verbs = new([], DefaultDelegate.UnknownVerbHandler(1));
			string[] goodArgs = ["-a", "foo", "--beta", "bar", "-c", "baz", "-e", "value1", "value2"];

			verbs.AddVerb("alpha", "The first verb", DefaultDelegate.ExecuteOrErrorMessage<int, byte, Alpha>(s => 2, 5), () => HelpWriter.ConsoleWriteHelp(Alpha.GetReader().OrderedArguments, new HelpWriterSettings(" ", 1, 3, HelpTextAlign.None)));
			verbs.AddVerb("beta", "The second verb", DefaultDelegate.ExecuteOrFormatErrorMessage<int, byte, Alpha>(s => 3, "Got error \"{0}\" handling verb {1}", 6), () => HelpWriter.ConsoleWriteHelp(Alpha.GetReader().OrderedArguments, new HelpWriterSettings("|", 2, 2, HelpTextAlign.WithinGroups)));
			verbs.AddVerb("gamma", "The third verb", DefaultDelegate.ExecuteOrError<int, byte, Alpha>(s => 4, (v,e) => { Console.WriteLine("We got an error: " + e); return 7; }), DefaultDelegate.WriteVerbDetailedHelp<byte, Alpha>());
			verbs.AddHelpVerb("help", "The help verb", "This verb provides help for other verbs", 0, DefaultDelegate.WriteVerbGeneralHelp<int>(), DefaultDelegate.UnknownVerbHelp);

			Assert.Equal(1, verbs.HandleVerb("foo", []));
			Assert.Equal("Unrecognized verb: foo\r\n", sout.ToString());
			sout.GetStringBuilder().Clear();


			Assert.Equal(2, verbs.HandleVerb("alpha", goodArgs));
			Assert.Equal("", sout.ToString());
			sout.GetStringBuilder().Clear();

			Assert.Equal(3, verbs.HandleVerb("beta", goodArgs));
			Assert.Equal("", sout.ToString());
			sout.GetStringBuilder().Clear();

			Assert.Equal(4, verbs.HandleVerb("gamma", goodArgs));
			Assert.Equal("", sout.ToString());
			sout.GetStringBuilder().Clear();

			// This just returns the error message literally
			Assert.Equal(5, verbs.HandleVerb("alpha", []));
			Assert.Equal("Missing argument a\r\n", sout.ToString());
			sout.GetStringBuilder().Clear();

			// This formats the error message
			Assert.Equal(6, verbs.HandleVerb("beta", ["-a", "foo"]));
			Assert.Equal("Got error \"Missing argument beta\" handling verb beta\r\n", sout.ToString());
			sout.GetStringBuilder().Clear();

			// This one just invokes the 
			Assert.Equal(7, verbs.HandleVerb("gamma", ["-a", "foo", "--beta", "bar"]));
			Assert.Equal("We got an error: Missing argument c|charlie\r\n", sout.ToString());
			sout.GetStringBuilder().Clear();


			Assert.Equal(0, verbs.HandleVerb("help", Array.Empty<string>()));
			Assert.Equal("alpha: The first verb\r\nbeta: The second verb\r\ngamma: The third verb\r\nhelp: The help verb\r\n", sout.ToString());
			sout.GetStringBuilder().Clear();

			Assert.Equal(0, verbs.HandleVerb("help", ["foo"]));
			Assert.Equal("Unrecognized verb: foo\r\n", sout.ToString());
			sout.GetStringBuilder().Clear();

			Assert.Equal(0, verbs.HandleVerb("help", ["help"]));
			Assert.Equal("This verb provides help for other verbs\r\n", sout.ToString());
			sout.GetStringBuilder().Clear();

			// First, we want non-aligned, just a left margin of 1 spaces and a right margin of 3 spaces
			Assert.Equal(0, verbs.HandleVerb("help", ["alpha"]));
			Assert.Equal(
				"Options:\r\n" +
				" -a   This is the first option\r\n" +
				" --beta   This is the second option\r\n" +
				" -c --charlie   This is the third option, which has a longer name than the other options, but it should wrap the text just fine\r\n" +
				"\r\n" +
				"Switches:\r\n" +
				" -e --epsilon   This is the first switch\r\n" +
				"\r\n" +
				"Values:\r\n" +
				" delta   This is the first value\r\n" +
				" Value 001   Nameless value\r\n" +
				"\r\n", sout.ToString());
			sout.GetStringBuilder().Clear();

			// Next, we want aligned, but only within the same group
			Assert.Equal(0, verbs.HandleVerb("help", ["beta"]));
			Assert.Equal(
				"Options:\r\n" +
				"  -a            This is the first option\r\n" +
				"  --beta        This is the second option\r\n" +
				"  -c|--charlie  This is the third option, which has a longer name than the other options, but it should wrap the text just fine\r\n" +
				"\r\n" +
				"Switches:\r\n" +
				"  -e|--epsilon  This is the first switch\r\n" +
				"\r\n" +
				"Values:\r\n" +
				"  delta      This is the first value\r\n" +
				"  Value 001  Nameless value\r\n" +
				"\r\n", sout.ToString());
			sout.GetStringBuilder().Clear();

			// Finally, we want aligned, across all groups
			Assert.Equal(0, verbs.HandleVerb("help", ["gamma"]));
			Assert.Equal(
				"Options:\r\n" +
				" -a             This is the first option\r\n" +
				" --beta         This is the second option\r\n" +
				" -c --charlie   This is the third option, which has a longer name than the other options, but it should wrap the text just fine\r\n" +
				"\r\n" +
				"Switches:\r\n" +
				" -e --epsilon   This is the first switch\r\n" +
				"\r\n" +
				"Values:\r\n" +
				" delta          This is the first value\r\n" +
				" Value 001      Nameless value\r\n" +
				"\r\n", sout.ToString());
			sout.GetStringBuilder().Clear();
		}
	}
}
