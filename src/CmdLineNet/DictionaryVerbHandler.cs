namespace CmdLineNet
{
	using System.Collections.Generic;
	/// <summary>
	/// Stores all known verbs in a <see cref="List{T}"/>.
	/// Best used with a larger number of verbs, or where it's used many times, where the cost of creating a dictionary pays off.
	/// As always, if you are overly concerned with performance, it's best to benchmark and see what works for your use-case.
	/// </summary>
	/// <typeparam name="TReturn">The return type.</typeparam>
	public sealed class DictionaryVerbHandler<TReturn> : IVerbHandler<TReturn>
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public DictionaryVerbHandler(Dictionary<string, IVerb<TReturn>> allVerbs, UnknownVerbHandler<TReturn> unknownVerbHandler)
		{
			AllVerbs = allVerbs;
			UnknownVerbHandler = unknownVerbHandler;
		}
		/// <summary>
		/// All the configured verbs.
		/// </summary>
		public Dictionary<string, IVerb<TReturn>> AllVerbs { get; }
		/// <summary>
		/// The delegate that is invoked when an attempting to invoke an unknown verb.
		/// </summary>
		public UnknownVerbHandler<TReturn> UnknownVerbHandler { get; }
		/// <summary>
		/// Gets a verb from <see cref="AllVerbs"/> with the name <paramref name="verbName"/>, and invokes <see cref="Verb{T}.Execute(IEnumerable{string})"/>.
		/// If none matches, invokes <see cref="UnknownVerbHandler"/>.
		/// </summary>
		/// <param name="verbName">The verb to invoke.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>The returned value.</returns>
		public TReturn HandleVerb(string verbName, IEnumerable<string> args)
		{
			return AllVerbs.TryGetValue(verbName, out IVerb<TReturn>? Verb)
				? Verb.Execute(args)
				: UnknownVerbHandler(verbName, args);
		}
	}
}