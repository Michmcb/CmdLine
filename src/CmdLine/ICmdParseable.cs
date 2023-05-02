namespace CmdLine
{
	using System.Collections.Generic;
	/// <summary>
	/// An object which can parse an instance of itself.
	/// </summary>
	/// <typeparam name="TId">The type of the ID.</typeparam>
	/// <typeparam name="TSelf">The parsed object type.</typeparam>
	public interface ICmdParseable<TId, TSelf>
		where TSelf : ICmdParseable<TId, TSelf>
		where TId : struct
	{
		/// <summary>
		/// Returns a <see cref="ArgsReader{TId}"/> which can be used to read arguments required by this object.
		/// </summary>
		/// <returns></returns>
		static abstract ArgsReader<TId> GetReader();
		/// <summary>
		/// Reads arguments using <paramref name="reader"/>, and then parses using <see cref="Parse(IEnumerable{RawArg{TId}})"/>.
		/// </summary>
		/// <param name="reader">The reader.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>A parsed <typeparamref name="TSelf"/>, or an error message.</returns>
		static ParseResult<TSelf> Parse(ArgsReader<TId> reader, IEnumerable<string> args)
		{
			return TSelf.Parse(reader.Read(args));
		}
		/// <summary>
		/// Attempts to parse <paramref name="args"/> into a <typeparamref name="TSelf"/>.
		/// </summary>
		/// <param name="args"></param>
		/// <returns>A parsed <typeparamref name="TSelf"/>, or an error message.</returns>
		static abstract ParseResult<TSelf> Parse(IEnumerable<RawArg<TId>> args);
	}
}