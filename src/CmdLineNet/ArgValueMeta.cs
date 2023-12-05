namespace CmdLineNet
{
	/// <summary>
	/// Holds a value argument's ID and the maximum/minimum number of times it may appear.
	/// </summary>
	/// <typeparam name="TId">The type of the ID.</typeparam>
	public sealed class ArgValueMeta<TId> where TId : struct
	{
		public ArgValueMeta(TId id, int min, int max, string? help)
		{
			Id = id;
			Min = min;
			Max = max;
			Help = help;
		}
		/// <summary>
		/// The ID for this argument.
		/// </summary>
		public TId Id { get; }
		/// <summary>
		/// The minimum number of times this argument may appear.
		/// </summary>
		public int Min { get; }
		/// <summary>
		/// The maximum number of times this argument may appear.
		/// </summary>
		public int Max { get; }
		/// <summary>
		/// Help text for this argument.
		/// </summary>
		public string? Help { get; }
	}
}