namespace CmdLineNet
{
	/// <summary>
	/// Counts the number of times an argument has appeared.
	/// </summary>
	/// <typeparam name="TId">The type of the ID given to each Argument.</typeparam>
	public sealed class ArgCountable<TId> where TId : struct
	{
		public ArgCountable(TId id, ArgType type, int max)
		{
			Id = id;
			Type = type;
			Max = max;
		}
		/// <summary>
		/// The ID of the argument.
		/// </summary>
		public TId Id { get; }
		/// <summary>
		/// The type of this argument.
		/// </summary>
		public ArgType Type { get; }
		/// <summary>
		/// The maximum number of times this argument may appear.
		/// </summary>
		public int Max { get; }
		/// <summary>
		/// The number of times <see cref="Arg"/> has been seen.
		/// </summary>
		public int Count { get; set; }
	}
}