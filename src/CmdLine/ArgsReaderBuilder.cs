namespace CmdLine
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
		public ArgsReaderBuilder() : this(new Dictionary<char, ArgMeta<TId>>(), new Dictionary<string, ArgMeta<TId>>()) { }
		/// <summary>
		/// Creates a new instance using the provided key comparers.
		/// </summary>
		/// <param name="shortKeyComparer">The key comparer to use for short options.</param>
		/// <param name="longKeyComparer">The key comparer to use for long options.</param>
		public ArgsReaderBuilder(IEqualityComparer<char> shortKeyComparer, IEqualityComparer<string> longKeyComparer) : this(new Dictionary<char, ArgMeta<TId>>(shortKeyComparer), new Dictionary<string, ArgMeta<TId>>(longKeyComparer)) { }
		/// <summary>
		/// Creates a new instance, using the provided dictionaries as is.
		/// </summary>
		/// <param name="shortOpts">The dictionary to use for short options.</param>
		/// <param name="longOpts">The dictionary to use for long options.</param>
		public ArgsReaderBuilder(Dictionary<char, ArgMeta<TId>> shortOpts, Dictionary<string, ArgMeta<TId>> longOpts)
		{
			ShortOpts = shortOpts;
			LongOpts = longOpts;
		}
		/// <summary>
		/// The configured short options.
		/// </summary>
		public Dictionary<char, ArgMeta<TId>> ShortOpts { get; }
		/// <summary>
		/// The configured long options.
		/// </summary>
		public Dictionary<string, ArgMeta<TId>> LongOpts { get; }
		/// <summary>
		/// Adds an argument of <see cref="ArgType.Switch"/>, with the provided <paramref name="longName"/> and <paramref name="shortName"/>, identified by <paramref name="id"/>.
		/// </summary>
		/// <param name="shortName">The short name.</param>
		/// <param name="longName">The long name.</param>
		/// <param name="id">The identifier for this argument.</param>
		/// <returns>This instance so calls can be chained.</returns>
		public ArgsReaderBuilder<TId> Switch(char shortName, string longName, TId id)
		{
			return Do(shortName, longName, ArgType.Switch, id);
		}
		/// <summary>
		/// Adds an argument of <see cref="ArgType.Switch"/>, with the provided <paramref name="shortName"/>, identified by <paramref name="id"/>.
		/// </summary>
		/// <param name="shortName">The short name.</param>
		/// <param name="id">The identifier for this argument.</param>
		/// <returns>This instance so calls can be chained.</returns>
		public ArgsReaderBuilder<TId> Switch(char shortName, TId id)
		{
			return Do(shortName, ArgType.Switch, id);
		}
		/// <summary>
		/// Adds an argument of <see cref="ArgType.Switch"/>, with the provided <paramref name="longName"/>, identified by <paramref name="id"/>.
		/// </summary>
		/// <param name="longName">The long name.</param>
		/// <param name="id">The identifier for this argument.</param>
		/// <returns>This instance so calls can be chained.</returns>
		public ArgsReaderBuilder<TId> Switch(string longName, TId id)
		{
			return Do(longName, ArgType.Switch, id);
		}
		/// <summary>
		/// Adds an argument of <see cref="ArgType.Option"/>, with the provided <paramref name="longName"/> and <paramref name="shortName"/>, identified by <paramref name="id"/>.
		/// </summary>
		/// <param name="shortName">The short name.</param>
		/// <param name="longName">The long name.</param>
		/// <param name="id">The identifier for this argument.</param>
		/// <returns>This instance so calls can be chained.</returns>
		public ArgsReaderBuilder<TId> Option(char shortName, string longName, TId id)
		{
			return Do(shortName, longName, ArgType.Option, id);
		}
		/// <summary>
		/// Adds an argument of <see cref="ArgType.Option"/>, with the provided <paramref name="shortName"/>, identified by <paramref name="id"/>.
		/// </summary>
		/// <param name="shortName">The short name.</param>
		/// <param name="id">The identifier for this argument.</param>
		/// <returns>This instance so calls can be chained.</returns>
		public ArgsReaderBuilder<TId> Option(char shortName, TId id)
		{
			return Do(shortName, ArgType.Option, id);
		}
		/// <summary>
		/// Adds an argument of <see cref="ArgType.Option"/>, with the provided <paramref name="longName"/>, identified by <paramref name="id"/>.
		/// </summary>
		/// <param name="longName">The long name.</param>
		/// <param name="id">The identifier for this argument.</param>
		/// <returns>This instance so calls can be chained.</returns>
		public ArgsReaderBuilder<TId> Option(string longName, TId id)
		{
			return Do(longName, ArgType.Option, id);
		}
		/// <summary>
		/// Creates a new instance of <see cref="ArgsReader{TId}"/> with the configured <see cref="ShortOpts"/> and <see cref="LongOpts"/>.
		/// </summary>
		/// <returns>A configured <see cref="ArgsReader{TId}"/>.</returns>
		public ArgsReader<TId> Build()
		{
			return new(ShortOpts, LongOpts);
		}
		private ArgsReaderBuilder<TId> Do(char shortName, ArgType type, TId id)
		{
			CheckShortName(shortName);
			ShortOpts[shortName] = new(id, type);
			return this;
		}
		private ArgsReaderBuilder<TId> Do(string longName, ArgType type, TId id)
		{
			CheckLongName(longName);
			LongOpts[longName] = new(id, type);
			return this;
		}
		private ArgsReaderBuilder<TId> Do(char shortName, string longName, ArgType type, TId id)
		{
			CheckShortName(shortName);
			CheckLongName(longName);
			ShortOpts[shortName] = new(id, type);
			LongOpts[longName] = new(id, type);
			return this;
		}
		private void CheckShortName(char shortName)
		{
			if (char.IsControl(shortName))
			{
				throw new CmdLineArgumentException("Short argument is a control character. Use a character that can be typed easily. Argument: " + shortName);
			}
			if (ShortOpts.ContainsKey(shortName))
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
			if (LongOpts.ContainsKey(longName))
			{
				throw new CmdLineArgumentException("Long argument has already been used. Argument: " + longName);
			}
		}
	}
}