namespace CmdLineNet
{
	using System.Collections.Generic;
	using System.Runtime.CompilerServices;

	/// <summary>
	/// Stores all known verbs in a <see cref="List{T}"/>.
	/// Best used with a larger number of verbs, or where it's used many times, where the cost of creating a dictionary pays off.
	/// As always, if you are overly concerned with performance, it's best to benchmark and see what works for your use-case.
	/// </summary>
	/// <typeparam name="T">The return type.</typeparam>
	public sealed class DictionaryVerbHandler<T> : IVerbHandler<T>
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public DictionaryVerbHandler(Dictionary<string, IVerb<T>> allVerbs, UnknownVerbHandler<T> unknownVerbHandler)
		{
			AllVerbs = allVerbs;
			UnknownVerbHandler = unknownVerbHandler;
		}
		/// <summary>
		/// All the configured verbs.
		/// </summary>
		public Dictionary<string, IVerb<T>> AllVerbs { get; }
		/// <summary>
		/// The delegate that is invoked when an attempting to invoke an unknown verb.
		/// </summary>
		public UnknownVerbHandler<T> UnknownVerbHandler { get; }
		/// <summary>
		/// Adds a verb, keyed by name.
		/// </summary>
		/// <param name="verb">The verb to add.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddVerb(IVerb<T> verb)
		{
			AllVerbs.Add(verb.Name, verb);
		}
		/// <summary>
		/// Gets a verb from <see cref="AllVerbs"/> with the name <paramref name="verbName"/>, and invokes <see cref="Verb{T}.Execute(IEnumerable{string})"/>.
		/// If none matches, invokes <see cref="UnknownVerbHandler"/>.
		/// </summary>
		/// <param name="verbName">The verb to invoke.</param>
		/// <param name="args">The arguments.</param>
		/// <returns>The returned value.</returns>
		public T HandleVerb(string verbName, IEnumerable<string> args)
		{
			return AllVerbs.TryGetValue(verbName, out IVerb<T>? Verb)
				? Verb.Execute(args)
				: UnknownVerbHandler(verbName, args);
		}
	}
}