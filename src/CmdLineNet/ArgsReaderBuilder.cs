namespace CmdLineNet
{
	using System.Collections.Generic;
	/// <summary>
	/// Helps build up a <see cref="ArgsReader{TId}"/>.
	/// </summary>
	/// <typeparam name="TId">The type of the ID given to each Argument.</typeparam>
	public sealed class ArgsReaderBuilder<TId> where TId : struct
	{
		/// <summary>
		/// Creates a new instance using the default constructor for <see cref="Dictionary{TKey, TValue}"/>.
		/// </summary>
		public ArgsReaderBuilder() : this([], [], []) { }
		/// <summary>
		/// Creates a new instance using the provided key comparers.
		/// </summary>
		/// <param name="shortKeyComparer">The key comparer to use for short arguments.</param>
		/// <param name="longKeyComparer">The key comparer to use for long arguments.</param>
		public ArgsReaderBuilder(IEqualityComparer<char> shortKeyComparer, IEqualityComparer<string> longKeyComparer) : this(new Dictionary<char, ArgMeta<TId>>(shortKeyComparer), new Dictionary<string, ArgMeta<TId>>(longKeyComparer), []) { }
		/// <summary>
		/// Creates a new instance, using the provided dictionaries as is.
		/// </summary>
		/// <param name="shortArgs">The dictionary to use for short arguments.</param>
		/// <param name="longArgs">The dictionary to use for long arguments.</param>
		/// <param name="values">The list to use for values.</param>
		public ArgsReaderBuilder(Dictionary<char, ArgMeta<TId>> shortArgs, Dictionary<string, ArgMeta<TId>> longArgs, List<ArgValueMeta<TId>> values)
		{
			ShortArgs = shortArgs;
			LongArgs = longArgs;
			Values = values;
		}
		/// <summary>
		/// The configured short arguments.
		/// </summary>
		public Dictionary<char, ArgMeta<TId>> ShortArgs { get; }
		/// <summary>
		/// The configured long arguments.
		/// </summary>
		public Dictionary<string, ArgMeta<TId>> LongArgs { get; }
		/// <summary>
		/// The configured values.
		/// </summary>
		public List<ArgValueMeta<TId>> Values { get; }
		/// <summary>
		/// Adds an argument of <see cref="ArgType.Switch"/>, with the provided <paramref name="longName"/> and <paramref name="shortName"/>, identified by <paramref name="id"/>.
		/// </summary>
		/// <param name="id">The identifier for this argument.</param>
		/// <param name="shortName">The short name.</param>
		/// <param name="longName">The long name.</param>
		/// <param name="min">The minimum number of times this argument may appear.</param>
		/// <param name="max">The maximum number of times this argument may appear.</param>
		/// <returns>This instance so calls can be chained.</returns>
		public ArgsReaderBuilder<TId> Switch(TId id, char shortName, string longName, int min, int max)
		{
			return Do(id, ArgType.Switch, shortName, longName, min, max);
		}
		/// <summary>
		/// Adds an argument of <see cref="ArgType.Switch"/>, with the provided <paramref name="shortName"/>, identified by <paramref name="id"/>.
		/// </summary>
		/// <param name="id">The identifier for this argument.</param>
		/// <param name="shortName">The short name.</param>
		/// <param name="min">The minimum number of times this argument may appear.</param>
		/// <param name="max">The maximum number of times this argument may appear.</param>
		/// <returns>This instance so calls can be chained.</returns>
		public ArgsReaderBuilder<TId> Switch(TId id, char shortName, int min, int max)
		{
			return Do(id, ArgType.Switch, shortName, min, max);
		}
		/// <summary>
		/// Adds an argument of <see cref="ArgType.Switch"/>, with the provided <paramref name="longName"/>, identified by <paramref name="id"/>.
		/// </summary>
		/// <param name="id">The identifier for this argument.</param>
		/// <param name="longName">The long name.</param>
		/// <param name="min">The minimum number of times this argument may appear.</param>
		/// <param name="max">The maximum number of times this argument may appear.</param>
		/// <returns>This instance so calls can be chained.</returns>
		public ArgsReaderBuilder<TId> Switch(TId id, string longName, int min, int max)
		{
			return Do(id, ArgType.Switch, longName, min, max);
		}
		/// <summary>
		/// Adds an argument of <see cref="ArgType.Option"/>, with the provided <paramref name="longName"/> and <paramref name="shortName"/>, identified by <paramref name="id"/>.
		/// </summary>
		/// <param name="id">The identifier for this argument.</param>
		/// <param name="shortName">The short name.</param>
		/// <param name="longName">The long name.</param>
		/// <param name="min">The minimum number of times this argument may appear.</param>
		/// <param name="max">The maximum number of times this argument may appear.</param>
		/// <returns>This instance so calls can be chained.</returns>
		public ArgsReaderBuilder<TId> Option(TId id, char shortName, string longName, int min, int max)
		{
			return Do(id, ArgType.Option, shortName, longName, min, max);
		}
		/// <summary>
		/// Adds an argument of <see cref="ArgType.Option"/>, with the provided <paramref name="shortName"/>, identified by <paramref name="id"/>.
		/// </summary>
		/// <param name="id">The identifier for this argument.</param>
		/// <param name="shortName">The short name.</param>
		/// <param name="min">The minimum number of times this argument may appear.</param>
		/// <param name="max">The maximum number of times this argument may appear.</param>
		/// <returns>This instance so calls can be chained.</returns>
		public ArgsReaderBuilder<TId> Option(TId id, char shortName, int min, int max)
		{
			return Do(id, ArgType.Option, shortName, min, max);
		}
		/// <summary>
		/// Adds an argument of <see cref="ArgType.Option"/>, with the provided <paramref name="longName"/>, identified by <paramref name="id"/>.
		/// </summary>
		/// <param name="id">The identifier for this argument.</param>
		/// <param name="longName">The long name.</param>
		/// <param name="min">The minimum number of times this argument may appear.</param>
		/// <param name="max">The maximum number of times this argument may appear.</param>
		/// <returns>This instance so calls can be chained.</returns>
		public ArgsReaderBuilder<TId> Option(TId id, string longName, int min, int max)
		{
			return Do(id, ArgType.Option, longName, min, max);
		}
		/// <summary>
		/// Adds a value, identified by <paramref name="id"/>.
		/// </summary>
		/// <param name="id">The identifier for this argument.</param>
		/// <param name="min">The minimum number of times this value may appear.</param>
		/// <param name="max">The maximum number of times this value may appear.</param>
		/// <returns>This instance so calls can be chained.</returns>
		public ArgsReaderBuilder<TId> Value(TId id, int min, int max)
		{
			Values.Add(new(id, min, max, null));
			return this;
		}
		/// <summary>
		/// Creates a new instance of <see cref="ArgsReader{TId}"/> with the configured <see cref="ShortArgs"/> and <see cref="LongArgs"/>.
		/// </summary>
		/// <returns>A configured <see cref="ArgsReader{TId}"/>.</returns>
		public ArgsReader<TId> Build()
		{
			return new(ShortArgs, LongArgs, Values);
		}
		private ArgsReaderBuilder<TId> Do(TId id, ArgType type, char shortName, int min, int max)
		{
			CheckShortName(shortName);
			ShortArgs[shortName] = new(id, type, min, max, null);
			return this;
		}
		private ArgsReaderBuilder<TId> Do(TId id, ArgType type, string longName, int min, int max)
		{
			CheckLongName(longName);
			LongArgs[longName] = new(id, type, min, max, null);
			return this;
		}
		private ArgsReaderBuilder<TId> Do(TId id, ArgType type, char shortName, string longName, int min, int max)
		{
			CheckShortName(shortName);
			CheckLongName(longName);
			ShortArgs[shortName] = new(id, type, min, max, null);
			LongArgs[longName] = new(id, type, min, max, null);
			return this;
		}
		private void CheckShortName(char shortName)
		{
			if (char.IsControl(shortName))
			{
				throw new CmdLineArgumentException("Short argument is a control character. Use a character that can be typed easily. Argument: " + shortName);
			}
			if (ShortArgs.ContainsKey(shortName))
			{
				throw new CmdLineArgumentException("Short argument has already been used. Argument: " + shortName);
			}
			if (shortName == '-')
			{
				throw new CmdLineArgumentException("Short argument was a single dash");
			}
		}
		private void CheckLongName(string longName)
		{
			if (string.IsNullOrWhiteSpace(longName))
			{
				throw new CmdLineArgumentException("Long arguments was null, empty, or entirely whitespace");
			}
			if (longName.StartsWith("-"))
			{
				throw new CmdLineArgumentException("Long argument may not start with a dash. Argument: " + longName);
			}
			if (longName.Length == 1)
			{
				throw new CmdLineArgumentException("Long arguments may not be a single character; use a short argument if you want to use a single character. Argument: " + longName);
			}
			if (LongArgs.ContainsKey(longName))
			{
				throw new CmdLineArgumentException("Long argument has already been used. Argument: " + longName);
			}
		}
	}
}