namespace CmdLineNet
{
	using System.Collections.Generic;
	/// <summary>
	/// A parser which reads an <see cref="IEnumerable{T}"/> of <see cref="string"/>, and produces <see cref="RawArg{TId}"/>.
	/// </summary>
	/// <typeparam name="TId">The type of the ID given to each Argument.</typeparam>
	public sealed class ArgsReader<TId> where TId : struct
	{
		/// <summary>
		/// Creates a new instance which is configured to recognize arguments identified by <paramref name="shortOpts"/> and <paramref name="longOpts"/>.
		/// </summary>
		/// <param name="shortOpts">The short form of arguments.</param>
		/// <param name="longOpts">The long form of arguments. Keys must NOT have leading --./param>
		public ArgsReader(IReadOnlyDictionary<char, ArgIdType<TId>> shortOpts, IReadOnlyDictionary<string, ArgIdType<TId>> longOpts)
		{
			ShortOpts = shortOpts;
			LongOpts = longOpts;
		}
		/// <summary>
		/// All of the short-form options.
		/// </summary>
		public IReadOnlyDictionary<char, ArgIdType<TId>> ShortOpts { get; }
		/// <summary>
		/// All of the long-form options.
		/// </summary>
		public IReadOnlyDictionary<string, ArgIdType<TId>> LongOpts { get; }
		/// <summary>
		/// Reads <paramref name="args"/> and produces <see cref="RawArg{TId}"/>.
		/// </summary>
		/// <param name="args">The arguments to read.</param>
		/// <returns>The arguments</returns>
		public IEnumerable<RawArg<TId>> Read(IEnumerable<string> args)
		{
			using IEnumerator<string> e = args.GetEnumerator();
			foreach (var item in Read(e))
			{
				yield return item;
			}
		}
		/// <summary>
		/// Reads <paramref name="args"/> and produces <see cref="RawArg{TId}"/>.
		/// </summary>
		/// <param name="args">The arguments to read.</param>
		/// <returns>The arguments</returns>
		public IEnumerable<RawArg<TId>> Read(IEnumerator<string> args)
		{
			// TODO we need a way for the reader to enforce the arity of values, options, and switches (if this is the correct place for that)
			// TODO we need a way for the reader to enforce some arguments being mutually exclusive (if this is the correct place for that)
			// http://docopt.org/
			while (args.MoveNext())
			{
				string s = args.Current;
				if (s.StartsWith("--"))
				{
					if (s.Length == 2)
					{
						// It's "--" so treat the rest as values
						while (args.MoveNext())
						{
							yield return new(default, args.Current, ArgState.Value);
						}
					}
					else
					{
						// Long switch or option
						string option = s[2..];
						if (LongOpts.TryGetValue(option, out ArgIdType<TId> which))
						{
							switch (which.Type)
							{
								case ArgType.Switch:
									yield return new(which.Id, string.Empty, ArgState.LongSwitch);
									break;
								case ArgType.Option:
									yield return args.MoveNext()
										? new(which.Id, args.Current, ArgState.LongOption)
										: new(which.Id, string.Empty, ArgState.LongOption);
									break;
								default:
									yield return new(default, string.Concat("ArgType enum value was not valid for argument \"", s, "\": ", ((int)which.Type).ToString()), ArgState.OtherError);
									break;
							}
						}
						else
						{
							yield return new(default, "Unrecognized long argument: " + s, ArgState.LongUnrecognized);
						}
					}
				}
				else if (s.StartsWith('-'))
				{
					if (s.Length == 1)
					{
						// Lone dash
						yield return new(default, s, ArgState.Value);
					}
					else if (s.Length == 2)
					{
						// Short switch or option
						char c = s[1];
						if (ShortOpts.TryGetValue(c, out ArgIdType<TId> which))
						{
							switch (which.Type)
							{
								case ArgType.Switch:
									yield return new(which.Id, string.Empty, ArgState.ShortSwitch);
									break;
								case ArgType.Option:
									yield return args.MoveNext()
										? new(which.Id, args.Current, ArgState.ShortOption)
										: new(which.Id, string.Empty, ArgState.ShortOption);
									break;
								default:
									yield return new(default, string.Concat("ArgType enum value was not valid for argument \"", s, "\": ", ((int)which.Type).ToString()), ArgState.OtherError);
									break;
							}
						}
						else
						{
							yield return new(default, "Unrecognized short argument: " + s, ArgState.ShortUnrecognized);
						}
					}
					else
					{
						// Stacked switches
						foreach (char c in s[1..])
						{
							if (ShortOpts.TryGetValue(c, out ArgIdType<TId> which))
							{
								yield return which.Type == ArgType.Switch
									? new(which.Id, string.Empty, ArgState.StackedSwitch)
									: new(which.Id, MakeStr("A short option was found in stacked switches: ", c), ArgState.ShortOptionFoundInStackedSwitches);
							}
							else
							{
								yield return new(default, MakeStr("Unrecognized switch found in stacked switches: ", c), ArgState.ShortUnrecognized);
							}
						}
					}
				}
				else
				{
					// Value
					yield return new(default, s, ArgState.Value);
				}
			}
		}
		private static string MakeStr(string s, char c)
		{
			return string.Create(s.Length + 1, (s, c), (str, state) =>
			{
				s.CopyTo(str);
				str[^1] = c;
			});
		}
	}
}