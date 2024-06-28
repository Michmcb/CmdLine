namespace CmdLineNet
{
	/// <summary>
	/// Stores an argument ID and the maximum number of times it is expected to appear
	/// </summary>
	/// <typeparam name="TId">The type of the ID given to each Argument.</typeparam>
	public readonly struct IdCount<TId> where TId : struct
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public IdCount(TId id, int max)
		{
			Id = id;
			Max = max;
		}
		/// <summary>
		/// The ID,
		/// </summary>
		public TId Id { get; }
		/// <summary>
		/// The maximum number of times this item may appear.
		/// </summary>
		public int Max { get; }
	}
}