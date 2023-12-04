namespace CmdLineNet
{
	/// <summary>
	/// Allows you to build a <see cref="ArgMeta{TId}"/> in a fluent way.
	/// </summary>
	/// <typeparam name="TId">The type of the ID given to each Argument.</typeparam>
	public sealed class ArgMetaBuilder<TId> where TId : struct
	{
		public ArgMetaBuilder()
		{
			HelpText = "";
			Arity = 1;
			//MutuallyExclusive = [];
		}
		/// <summary>
		/// Help text for the argument.
		/// </summary>
		public string HelpText { get; set; }
		/// <summary>
		/// The number of times this argument may appear
		/// </summary>
		public int Arity { get; set; }
		/// <summary>
		/// Whether or not this argument is optional
		/// </summary>
		public bool Optional { get; set; }
		///// <summary>
		///// Indicates that this argument must NOT appear with any of these other arguments.
		///// </summary>
		//public HashSet<TId> MutuallyExclusive { get; set; }
		public ArgMetaBuilder<TId> WithHelpText(string helpText)
		{
			HelpText = helpText;
			return this;
		}
		public ArgMetaBuilder<TId> WithArity(int arity)
		{
			Arity = arity;
			return this;
		}
		public ArgMetaBuilder<TId> IsRequired()
		{
			Optional = false;
			return this;
		}
		public ArgMetaBuilder<TId> IsOptional()
		{
			Optional = true;
			return this;
		}
		//public ArgMetaBuilder<TId> MutuallyExclusiveWith(TId id)
		//{
		//	MutuallyExclusive.Add(id);
		//	return this;
		//}
		public ArgMeta<TId> Build()
		{
			return new ArgMeta<TId>(HelpText, Arity, Optional/*, MutuallyExclusive*/);
		}
	}
}