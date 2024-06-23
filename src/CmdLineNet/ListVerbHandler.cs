namespace CmdLineNet
{
	using System.Collections.Generic;

	/// <summary>
	/// Stores all known verbs in a <see cref="List{T}"/>.
	/// Best used with a small number of verbs, and where it's only used once, where the cost of creating a dictionary is not worth it.
	/// As always, if you are overly concerned with performance, it's best to benchmark and see what works for your use-case.
	/// </summary>
	/// <typeparam name="T">The return type.</typeparam>
	public sealed class ListVerbHandler<T> : IVerbHandler<T>
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public ListVerbHandler(List<IVerb<T>> allVerbs, IEqualityComparer<string> comparer, UnknownVerbHandler<T> unknownVerbHandler)
		{
			AllVerbs = allVerbs;
			Comparer = comparer;
			UnknownVerbHandler = unknownVerbHandler;
		}
		/// <summary>
		/// All the configured verbs.
		/// </summary>
		public List<IVerb<T>> AllVerbs { get; }
		/// <summary>
		/// The comparer to use when finding the appropriate verb to invoke
		/// </summary>
		public IEqualityComparer<string> Comparer { get; }
		/// <summary>
		/// The delegate that is invoked when an attempting to invoke an unknown verb.
		/// </summary>
		public UnknownVerbHandler<T> UnknownVerbHandler { get; }
		/// <summary>
		/// Searches <see cref="AllVerbs"/> for the first verb whose <see cref="Verb{T}.Name"/> matches <paramref name="verbName"/> using <see cref="Comparer"/>, and invokes <see cref="Verb{T}.Execute(IEnumerable{string})"/>.
		/// If none matches, invokes <see cref="UnknownVerbHandler"/>.
		/// </summary>
		/// <param name="verbName">The verb to invoke.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>The returned value.</returns>
		public T HandleVerb(string verbName, IEnumerable<string> args)
		{
			foreach (var v in AllVerbs)
			{
				if (Comparer.Equals(verbName, v.Name))
				{
					return v.Execute(args);
				}
			}
			return UnknownVerbHandler(verbName, args);
		}
	}
}