namespace CmdLine
{
	/// <summary>
	/// Holds an argument's ID and its <see cref="ArgType"/>.
	/// </summary>
	/// <typeparam name="TId">The type of the ID.</typeparam>
	public readonly struct ArgMeta<TId> where TId : struct
	{
		public ArgMeta(TId id, ArgType type)
		{
			Id = id;
			Type = type;
		}
		/// <summary>
		/// The ID for this argument.
		/// </summary>
		public TId Id { get; }
		/// <summary>
		/// The type of this argument.
		/// </summary>
		public ArgType Type { get; }
	}
}