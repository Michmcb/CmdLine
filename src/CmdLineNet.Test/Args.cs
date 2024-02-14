namespace CmdLineNet.Test.ArgsReaderBuilder
{
	using System.Collections.Generic;

	public sealed record class Args(string A, string B, string C, bool D, bool E, bool F) : ICmdParseable<ArgId, Args>
	{
		private static readonly ArgsReader<ArgId> reader = new ArgsReaderBuilder<ArgId>()
			.Option(ArgId.A, 'a', "alpha", 1, 1, null)
			.Option(ArgId.B, 'b', "bravo", 1, 1, null)
			.Option(ArgId.C, 'c', "charlie", 1, 1, null)
			.Switch(ArgId.D, 'd', "delta", 1, 1, null)
			.Switch(ArgId.E, 'e', "echo", 1, 1, null)
			.Switch(ArgId.F, 'f', "foxtrot", 1, 1, null)
			.Value(ArgId.V, 0, int.MaxValue, null)
			.Build();
		public static ArgsReader<ArgId> GetReader()
		{
			return reader;
		}
		public static ParseResult<Args> Parse(IEnumerable<RawArg<ArgId>> args)
		{
			string? a = null, b = null, c = null;
			bool d = false, e = false, f = false;
			foreach (RawArg<ArgId> arg in args)
			{
				switch (arg.State)
				{
					case ArgState.Value:
						return "No values accepted";
					case ArgState.LongOption:
					case ArgState.ShortOption:
					case ArgState.LongSwitch:
					case ArgState.ShortSwitch:
					case ArgState.StackedSwitch:
						switch (arg.Id)
						{
							case ArgId.A:
								a = arg.Content;
								break;
							case ArgId.B:
								b = arg.Content;
								break;
							case ArgId.C:
								c = arg.Content;
								break;
							case ArgId.D:
								d = true;
								break;
							case ArgId.E:
								e = true;
								break;
							case ArgId.F:
								f = true;
								break;
							default:
								return "Unrecognized argument!";
						}
						break;
					case ArgState.OtherError:
					case ArgState.ShortUnrecognized:
					case ArgState.LongUnrecognized:
					case ArgState.ShortOptionFoundInStackedSwitches:
					default:
						return arg.Content;
				}
			}
			if (a == null || b == null || c == null)
			{
				return "Missing arguments";
			}
			return new Args(a, b, c, d, e, f);
		}
	}
}