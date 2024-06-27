namespace CmdLineNet.Test
{
	using Xunit;
	public static class DictionaryVerbHandlerTests
	{
		[Fact]
		public static void VerbArgHandling()
		{
			DictionaryVerbHandler<int> dict = new([], (verb, args) => throw new Exception("Unknown Verb:" + verb));
			bool v1Called = false;
			bool h1Called = false;
			IEnumerable<string>? v1Args = null;

			bool v2Called = false;
			bool h2Called = false;
			IEnumerable<string>? v2Args = null;

			bool v3Called = false;
			bool h3Called = false;
			IEnumerable<string>? v3Args = null;

			List<IVerb<int>> helpVerbs = [];
			bool badHelpCalled = false;
			dict.AddVerb(new Verb<int>("verb1", "descr1", (verb, args) => { v1Called = true; v1Args = args; return 1; }, () => h1Called = true));
			dict.AddVerb(new Verb<int>("verb2", "descr2", (verb, args) => { v2Called = true; v2Args = args; return 2; }, () => h2Called = true));
			dict.AddVerb(new Verb<int>("verb3", "descr3", (verb, args) => { v3Called = true; v3Args = args; return 3; }, () => h3Called = true));
			dict.AddVerb(new HelpVerb<int>("help", "help", "help me", dict.AllVerbs, 4, helpVerbs.Add, (verb) => badHelpCalled = true));

			string[] a = ["a1", "a2", "a3"];
			Assert.Equal(1, dict.HandleVerb("verb1", a));
			Assert.Equal(a, v1Args);
			Assert.True(v1Called);

			a = ["b1", "b2", "b3"];
			Assert.Equal(2, dict.HandleVerb("verb2", a));
			Assert.Equal(a, v2Args);
			Assert.True(v2Called);

			a = ["c1", "c2", "c3"];
			Assert.Equal(3, dict.HandleVerb("verb3", a));
			Assert.Equal(a, v3Args);
			Assert.True(v3Called);

			Assert.Equal(4, dict.HandleVerb("help", ["verb1"]));
			Assert.True(h1Called);
			Assert.Equal(4, dict.HandleVerb("help", ["verb2"]));
			Assert.True(h2Called);
			Assert.Equal(4, dict.HandleVerb("help", ["verb3"]));
			Assert.True(h3Called);

			Assert.Equal(4, dict.HandleVerb("help", []));
			Assert.Equal(dict.AllVerbs.Values, helpVerbs);

			Assert.Equal(4, dict.HandleVerb("help", ["falsdgujps"]));
			Assert.True(badHelpCalled);
		}
	}
}
