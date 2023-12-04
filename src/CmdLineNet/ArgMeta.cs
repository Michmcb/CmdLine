namespace CmdLineNet
{
	/// <summary>
	/// Holds metadata about an argument.
	/// </summary>
	/// <typeparam name="TId">The type of the ID given to each Argument.</typeparam>
	public sealed class ArgMeta<TId> where TId : struct
	{
		public ArgMeta(string helpText, int arity, bool optional/*, IReadOnlySet<TId>? mutuallyExclusive*/)
		{
			HelpText = helpText;
			Arity = arity;
			Optional = optional;
			//MutuallyExclusive = mutuallyExclusive;
		}
		/// <summary>
		/// Help text for the argument.
		/// </summary>
		public string HelpText { get; }
		/// <summary>
		/// The number of times this argument may appear
		/// </summary>
		public int Arity { get; }
		/// <summary>
		/// Whether or not this argument is optional
		/// </summary>
		public bool Optional { get; }
		///// <summary>
		///// Indicates that this argument must NOT appear with any of these other arguments.
		///// </summary>
		//public IReadOnlySet<TId>? MutuallyExclusive { get; }
	}
}