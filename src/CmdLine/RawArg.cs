namespace CmdLine
{
	/// <summary>
	/// An argument that has been read, but not yet parsed into a target type.
	/// May possibly also represent an error.
	/// </summary>
	/// <typeparam name="TId">The type of the ID.</typeparam>
	public readonly struct RawArg<TId> where TId : struct
	{
		public RawArg(TId id, string? content, ArgState argType)
		{
			Id = id;
			Content = content;
			State = argType;
		}
		/// <summary>
		/// The ID for this argument.
		/// </summary>
		public TId Id { get; }
		/// <summary>
		/// For options and values, the content that was read, if any.
		/// For switches, this will be null.
		/// For errors, this will be an error message.
		/// </summary>
		public string? Content { get; }
		/// <summary>
		/// The state of what was read. This indicates whether it was a short-form or long-form argument,
		/// or if <see cref="Content"/> represents an error message due to malformed or unrecognized arguments.
		/// </summary>
		public ArgState State { get; }
	}
}