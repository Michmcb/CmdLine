namespace CmdLineNet
{
	/// <summary>
	/// Holds an argument's ID and other information about it.
	/// </summary>
	/// <typeparam name="TId">The type of the ID.</typeparam>
	public sealed class ArgMeta<TId> where TId : struct
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public ArgMeta(TId id, ArgType type, int min, int max, string? help)
		{
			Id = id;
			Type = type;
			Min = min;
			Max = max;
			Help = help;
		}
		/// <summary>
		/// The ID for this argument.
		/// </summary>
		public TId Id { get; }
		/// <summary>
		/// The type of this argument.
		/// </summary>
		public ArgType Type { get; }
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