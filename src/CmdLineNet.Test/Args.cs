﻿namespace CmdLineNet.Test.ArgsReaderBuilder
{
	using System.Collections.Generic;

	public sealed record class Args(string A, string B, string C, bool D, bool E, bool F) : ICmdParseable<ArgId, Args>
	{
		private static readonly ArgsReader<ArgId> reader = new ArgsReaderBuilder<ArgId>()
			.Option('a', "alpha", ArgId.A)
			.Option('b', "bravo", ArgId.B)
			.Option('c', "charlie", ArgId.C)
			.Switch('d', "delta", ArgId.D)
			.Switch('e', "echo", ArgId.E)
			.Switch('f', "foxtrot", ArgId.F)
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