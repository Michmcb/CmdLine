//namespace CmdLineNet
//{
//	using System.Collections.Generic;

//	/// <summary>
//	/// Stores all known verbs in a <see cref="List{T}"/>.
//	/// Best used with a small number of verbs, and where it's only used once, where the cost of creating a dictionary is not worth it.
//	/// As always, if you are overly concerned with performance, it's best to benchmark and see what works for your use-case.
//	/// </summary>
//	/// <typeparam name="TReturn">The return type.</typeparam>
//	public sealed class ListVerbHandler<TReturn> : IVerbHandler<TReturn>
//	{
//		/// <summary>
//		/// Creates a new instance.
//		/// </summary>
//		public ListVerbHandler(List<IVerb<TReturn>> allVerbs, IEqualityComparer<string> comparer, UnknownVerbHandler<TReturn> unknownVerbHandler)
//		{
//			AllVerbs = allVerbs;
//			Comparer = comparer;
//			UnknownVerbHandler = unknownVerbHandler;
//		}
//		/// <summary>
//		/// All the configured verbs.
//		/// </summary>
//		public List<IVerb<TReturn>> AllVerbs { get; }
//		/// <summary>
//		/// The comparer to use when finding the appropriate verb to invoke
//		/// </summary>
//		public IEqualityComparer<string> Comparer { get; }
//		/// <summary>
//		/// The delegate that is invoked when an attempting to invoke an unknown verb.
//		/// </summary>
//		public UnknownVerbHandler<TReturn> UnknownVerbHandler { get; }
//		/// <summary>
//		/// Searches <see cref="AllVerbs"/> for the first verb whose <see cref="Verb{T}.Name"/> matches <paramref name="verbName"/> using <see cref="Comparer"/>, and invokes <see cref="Verb{T}.Execute(IEnumerable{string})"/>.
//		/// If none matches, invokes <see cref="UnknownVerbHandler"/>.
//		/// </summary>
//		/// <param name="verbName">The verb to invoke.</param>
//		/// <param name="args">The arguments.</param>
//		/// <returns>The returned value.</returns>
//		public TReturn HandleVerb(string verbName, IEnumerable<string> args)
//		{
//			foreach (var v in AllVerbs)
//			{
//				if (Comparer.Equals(verbName, v.Name))
//				{
//					return v.Execute(args);
//				}
//			}
//			return UnknownVerbHandler(verbName, args);
//		}
//	}
//}