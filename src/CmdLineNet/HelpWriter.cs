namespace CmdLineNet
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	/// <summary>
	/// Writes help to the console.
	/// </summary>
	public static class HelpWriter
	{
		/// <summary>
		/// Writes help for <paramref name="argMetas"/>, in the order of Options, Switches, and then Values.
		/// </summary>
		/// <param name="argMetas">The arguments to write.</param>
		/// <param name="settings">The settings.</param>
		public static void ConsoleWriteHelp<TId>(IEnumerable<ArgMeta<TId>> argMetas, HelpSettings settings) where TId : struct
		{
			List<ArgMeta<TId>> options = [];
			List<ArgMeta<TId>> switches = [];
			List<ArgMeta<TId>> values = [];
			foreach (var item in argMetas)
			{
				switch (item.Type)
				{
					case ArgType.Switch:
						switches.Add(item);
						break;
					case ArgType.Option:
						options.Add(item);
						break;
					case ArgType.Value:
						values.Add(item);
						break;
				}
			}
			ConsoleWriteHelp([new ArgMetasGroup<TId>("Options:", options), new ArgMetasGroup<TId>("Switches:", switches), new ArgMetasGroup<TId>("Values:", values)], settings);
		}
		/// <summary>
		/// Writes help for <paramref name="groups"/>, in the order provided.
		/// </summary>
		/// <param name="groups">The arguments to write.</param>
		/// <param name="settings">The settings.</param>
		public static void ConsoleWriteHelp<TId>(IReadOnlyCollection<ArgMetasGroup<TId>> groups, HelpSettings settings) where TId : struct
		{
			string longShortNameSeparator = settings.LongShortNameSeparator;
			int leftMargin = settings.LeftMargin;
			int rightMargin = settings.RightMargin;
			int valueNum = 1;
			int maxLen = 0;
			foreach (ArgMetasGroup<TId> g in groups)
			{
				if (!g.Args.Any()) continue;
				// min length is both margins, separator's length, short name's -, and long name's --
				int baseLenShort = leftMargin + rightMargin + 1;
				int baseLenLong = leftMargin + rightMargin + 2;
				int baseLenShortLong = longShortNameSeparator.Length + leftMargin + rightMargin + 3;
				foreach (ArgMeta<TId> a in g.Args)
				{
					switch (a.Type)
					{
						case ArgType.Switch:
						case ArgType.Option:
							maxLen = a.Name != null
								? a.ShortName != default
									? Math.Max(maxLen, baseLenShortLong + a.Name.Length + 1)
									: Math.Max(maxLen, baseLenLong + a.Name.Length)
								: a.ShortName != default
									? Math.Max(maxLen, baseLenShort + 1)
									: Math.Max(maxLen, leftMargin + rightMargin);
							break;
						case ArgType.Value:
							maxLen = a.Name != null
								? Math.Max(maxLen, leftMargin + rightMargin + a.Name.Length)
								: Math.Max(maxLen, leftMargin + rightMargin + 9);
							break;
					}
					if (!settings.AlignHelpTextAcrossAllGroups)
					{
						maxLen = 0;
					}
				}
				Console.WriteLine(g.Name);
				string lm = new(' ', leftMargin);
				foreach (ArgMeta<TId> a in g.Args)
				{
					switch (a.Type)
					{
						case ArgType.Switch:
						case ArgType.Option:
							if (a.Name != null)
							{
								if (a.ShortName != default)
								{
									string rm = new(' ', Math.Max(0, maxLen - (leftMargin + 1 + longShortNameSeparator.Length + 2 + a.Name.Length)));
									Console.WriteLine(string.Concat(lm, "-", a.ShortName, longShortNameSeparator, "--", a.Name, rm, a.Help));
								}
								else
								{
									string rm = new(' ', Math.Max(0, maxLen - (leftMargin + 2 + a.Name.Length)));
									Console.WriteLine(string.Concat(lm, "--", a.Name, rm, a.Help));
								}
							}
							else
							{
								if (a.ShortName != default)
								{
									string rm = new(' ', Math.Max(0, maxLen - (leftMargin + 2)));
									Console.WriteLine(string.Concat(lm, "-", a.ShortName, rm, a.Help));
								}
							}
							break;
						case ArgType.Value:
							if (a.Name != null)
							{
								string rm = new(' ', Math.Max(0, maxLen - (leftMargin + a.Name.Length)));
								Console.WriteLine(string.Concat(lm, a.Name, rm, a.Help));
							}
							else
							{
								string name = string.Concat("Value ", valueNum++.ToString("000"));
								string rm = new(' ', Math.Max(0, maxLen - (leftMargin + name.Length)));
								Console.WriteLine(string.Concat(lm, name, rm, a.Help));
							}
							break;
					}
					
					Console.WriteLine();
				}
			}
		}
	}
}