namespace CmdLine
{
	using System.Diagnostics.CodeAnalysis;
	/// <summary>
	/// The result of reading and parsing arguments.
	/// </summary>
	/// <typeparam name="TObj">The parsed object type.</typeparam>
	public readonly struct ParseResult<TObj>
	{
		private readonly bool ok;
		private readonly TObj parsed;
		private readonly string? errMsg;
		/// <summary>
		/// Holds a successful result.
		/// </summary>
		/// <param name="parsed">The parsed object.</param>
		public ParseResult(TObj parsed)
		{
			this.parsed = parsed;
			ok = true;
			errMsg = null;
		}
		/// <summary>
		/// Holds a failed result.
		/// </summary>
		/// <param name="errMsg">The error message.</param>
		public ParseResult(string errMsg)
		{
			parsed = default!;
			ok = false;
			this.errMsg = errMsg;
		}
		/// <summary>
		/// If true, sets <paramref name="parsed"/>.
		/// If false, sets <paramref name="errMsg"/>.
		/// </summary>
		/// <param name="parsed">The successful result.</param>
		/// <param name="errMsg">The failure result.</param>
		/// <returns>Whether or not parsing was successful</returns>
		public bool Ok([NotNullWhen(true)] out TObj parsed, [NotNullWhen(false)] out string? errMsg)
		{
			if (ok)
			{
				parsed = this.parsed;
				errMsg = null;
			}
			else
			{
				parsed = default!;
				errMsg = this.errMsg;
			}
			return ok;
		}
		/// <summary>
		/// Equivalent to new ParseResult(<paramref name="value"/>);
		/// </summary>
		public static implicit operator ParseResult<TObj>([DisallowNull] TObj value) => new(value);
		/// <summary>
		/// Equivalent to new ParseResult(<paramref name="error"/>);
		/// </summary>
		public static implicit operator ParseResult<TObj>([DisallowNull] string error) => new(error);
	}
}