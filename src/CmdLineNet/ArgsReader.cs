namespace CmdLineNet
{
	using System.Collections.Generic;
	using System.Linq;
	/// <summary>
	/// A parser which reads an <see cref="IEnumerable{T}"/> of <see cref="string"/>, and produces <see cref="RawArg{TId}"/>.
	/// </summary>
	/// <typeparam name="TId">The type of the ID given to each Argument.</typeparam>
	public sealed class ArgsReader<TId> where TId : struct
	{
		/// <summary>
		/// Creates a new instance which is configured to recognize arguments identified by <paramref name="shortArgs"/> and <paramref name="longArgs"/>.
		/// </summary>
		/// <param name="shortArgs">The short form of arguments.</param>
		/// <param name="longArgs">The long form of arguments. Keys must NOT have leading --.</param>
		/// <param name="values">The values.</param>
		/// <param name="orderedArguments">The list to use for ordered arguments.</param>
		public ArgsReader(IReadOnlyDictionary<char, ArgMeta<TId>> shortArgs, IReadOnlyDictionary<string, ArgMeta<TId>> longArgs, IReadOnlyList<ArgMeta<TId>> values, IReadOnlyList<ArgMeta<TId>> orderedArguments)
		{
			ShortArgs = shortArgs;
			LongArgs = longArgs;
			Values = values;
			OrderedArguments = orderedArguments;
		}
		/// <summary>
		/// All of the short-form arguments.
		/// </summary>
		public IReadOnlyDictionary<char, ArgMeta<TId>> ShortArgs { get; }
		/// <summary>
		/// All of the long-form arguments.
		/// </summary>
		public IReadOnlyDictionary<string, ArgMeta<TId>> LongArgs { get; }
		/// <summary>
		/// All of the values.
		/// </summary>
		public IReadOnlyList<ArgMeta<TId>> Values { get; }
		/// <summary>
		/// All arguments and values, in the order of configuration.
		/// </summary>
		public IReadOnlyList<ArgMeta<TId>> OrderedArguments { get; }
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
			// TODO we need a way for the reader to enforce some arguments being mutually exclusive (if this is the correct place for that)
			// http://docopt.org/
			List<IdCount<TId>> valueArgs = new(Values.Count);
			valueArgs.AddRange(Values.Select(x => new IdCount<TId>(x.Id, x.Max)));
			DuplicatingValuesEnumerator<TId> vEnum = new(valueArgs.GetEnumerator());

			// TODO if the enum starts from 0 and is contiguous, then we can do a sneaky trick here, and just have an array, indexing into it by using TId as an integer index
			//public Func<IEnumerable<ArgMeta<TId>>, IReadOnlyDictionary<TId, ArgCountable<TId>>> GetStuff { get; }
			Dictionary<char, ArgCountable<TId>> shortArgs = [];
			Dictionary<string, ArgCountable<TId>> longArgs = [];
			{
				Dictionary<TId, ArgCountable<TId>> dict = [];
				foreach (var v in ShortArgs.Values)
				{
					dict[v.Id] = new ArgCountable<TId>(v.Id, v.Type, v.Max);
				}
				foreach (var v in LongArgs.Values)
				{
					dict[v.Id] = new ArgCountable<TId>(v.Id, v.Type, v.Max);
				}
				foreach (var kvp in ShortArgs)
				{
					shortArgs[kvp.Key] = dict[kvp.Value.Id];
				}
				foreach (var kvp in LongArgs)
				{
					longArgs[kvp.Key] = dict[kvp.Value.Id];
				}
			}

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
							yield return vEnum.MoveNext()
								? new(vEnum.Current, args.Current, ArgState.Value)
								: new(default, "Too many values were provided: " + args.Current, ArgState.TooManyValues);
						}
					}
					else
					{
						// Long switch or option
						string arg = s[2..];
						if (longArgs.TryGetValue(arg, out var which))
						{
							switch (which.Type)
							{
								case ArgType.Switch:
									if (++which.Count > which.Max)
									{
										yield return new(which.Id, "Switch was provided too many times: " + s, ArgState.TooManySwitches);
									}
									else
									{
										yield return new(which.Id, string.Empty, ArgState.LongSwitch);
									}
									break;
								case ArgType.Option:
									if (++which.Count > which.Max)
									{
										args.MoveNext(); // Skip the option's value
										yield return new(which.Id, "Option was provided too many times: " + s, ArgState.TooManyOptions);
									}
									else
									{
										yield return args.MoveNext()
											? new(which.Id, args.Current, ArgState.LongOption)
											: new(which.Id, string.Empty, ArgState.LongOption);
									}
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
						yield return vEnum.MoveNext()
							? new(vEnum.Current, s, ArgState.Value)
							: new(default, "Too many values were provided: " + s, ArgState.TooManyValues);
					}
					else if (s.Length == 2)
					{
						// Short switch or option
						char c = s[1];
						if (shortArgs.TryGetValue(c, out var which))
						{
							switch (which.Type)
							{
								case ArgType.Switch:
									if (++which.Count > which.Max)
									{
										yield return new(which.Id, "Switch was provided too many times: " + s, ArgState.TooManySwitches);
									}
									else
									{
										yield return new(which.Id, string.Empty, ArgState.ShortSwitch);
									}
									break;
								case ArgType.Option:
									if (++which.Count > which.Max)
									{
										args.MoveNext(); // Skip the option's value
										yield return new(which.Id, "Option was provided too many times: " + s, ArgState.TooManyOptions);
									}
									else
									{
										yield return args.MoveNext()
											? new(which.Id, args.Current, ArgState.ShortOption)
											: new(which.Id, string.Empty, ArgState.ShortOption);
									}
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
							if (shortArgs.TryGetValue(c, out var which))
							{
								if (++which.Count > which.Max)
								{
									yield return new(which.Id, MakeStr("Switch was provided too many times in stacked switches: ", c), ArgState.TooManySwitches);
								}
								else
								{
									yield return which.Type == ArgType.Switch
									? new(which.Id, string.Empty, ArgState.StackedSwitch)
									: new(which.Id, MakeStr("A short option was found in stacked switches: ", c), ArgState.ShortOptionFoundInStackedSwitches);
								}
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
					yield return vEnum.MoveNext()
						? new(vEnum.Current, s, ArgState.Value)
						: new(default, "Too many values were provided: " + s, ArgState.TooManyValues);
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