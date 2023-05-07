namespace CmdLineNet
{
	using System.Collections.Generic;
	/// <summary>
	/// A parser which can produce an object of type <typeparamref name="TObj"/> from arguments.
	/// </summary>
	/// <typeparam name="TId">The type of the ID.</typeparam>
	/// <typeparam name="TObj">The parsed object type.</typeparam>
	public interface ICmdParser<TId, TObj>
		where TId : struct
	{
		/// <summary>
		/// Attempts to parse a <typeparamref name="TObj"/> from <paramref name="args"/>, using the rules configured in <paramref name="reader"/>.
		/// </summary>
		/// <param name="reader">The reader to use.</param>
		/// <param name="args">The arguments to parse.</param>
		/// <returns>A parsed <typeparamref name="TObj"/>, or an error message.</returns>
		ParseResult<TObj> Parse(ArgsReader<TId> reader, IEnumerable<string> args);
	}
}