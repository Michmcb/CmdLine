namespace CmdLineNet
{
	/// <summary>
	/// Stores an argument ID and the maximum number of times it is expected to appear
	/// </summary>
	/// <typeparam name="TId">The type of the ID given to each Argument.</typeparam>
	public readonly struct ArgCount<TId> where TId : struct
	{
		public ArgCount(ArgMeta<TId> meta)
		{
			Id = meta.Id;
			Max = meta.Max;
		}
		public ArgCount(TId id, int max)
		{
			Id = id;
			Max = max;
		}
		/// <summary>
		/// The ID of the argument.
		/// </summary>
		public TId Id { get; }
		/// <summary>
		/// The maximum number of times this argument may appear.
		/// </summary>
		public int Max { get; }
	}
}