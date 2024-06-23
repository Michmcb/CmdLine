namespace CmdLineNet
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	/// <summary>
	/// A verb specialized for writing help.
	/// </summary>
	/// <typeparam name="T">The return type.</typeparam>
	public sealed class DefaultHelpVerb<T> : IVerb<T>
	{
		private readonly IReadOnlyDictionary<string, IVerb<T>> allVerbs;
		private readonly T returnValue;
		private readonly VerbHelp<T> generalHelp;
		private readonly UnknownVerbHelp unrecognizedVerbHelp;
		/// <summary>
		/// Cdreates a new instance
		/// </summary>
		/// <param name="name">The name of this verb.</param>
		/// <param name="description">The </param>
		/// <param name="helpText">The text that is written on invoking <see cref="WriteHelp"/>.</param>
		/// <param name="allVerbs">All the verbs that are searched when invoking <see cref="Execute(IEnumerable{string})"/>, to find the verb to invoke <see cref="WriteHelp"/> on.</param>
		/// <param name="returnValue">The return value that <see cref="Execute(IEnumerable{string})"/> should return.</param>
		/// <param name="generalHelp">The delegate to invoke on every verb when no arguments are provided to <see cref="Execute(IEnumerable{string})"/>.</param>
		/// <param name="unrecognizedVerbHelp">The delegate to invoke when no verb could be found to provide help on.</param>
		public DefaultHelpVerb(string name, string description, string helpText, IReadOnlyDictionary<string, IVerb<T>> allVerbs, T returnValue, VerbHelp<T> generalHelp, UnknownVerbHelp unrecognizedVerbHelp)
		{
			Name = name;
			Description = description;
			HelpText = helpText;
			this.allVerbs = allVerbs;
			this.returnValue = returnValue;
			this.generalHelp = generalHelp;
			this.unrecognizedVerbHelp = unrecognizedVerbHelp;
		}
		/// <inheritdoc/>
		public string Name { get; }
		/// <inheritdoc/>
		public string Description { get; }
		/// <summary>
		/// The text that is written on invoking <see cref="WriteHelp"/>.
		/// </summary>
		public string HelpText { get; }
		/// <inheritdoc/>
		public T Execute(IEnumerable<string> args)
		{
			string? helpVerb = args.FirstOrDefault();
			if (helpVerb != null)
			{
				if (allVerbs.TryGetValue(helpVerb, out IVerb<T>? v))
				{
					v.WriteHelp();
				}
				else
				{
					unrecognizedVerbHelp(helpVerb);
				}
			}
			else
			{
				foreach (var v in allVerbs.Values)
				{
					generalHelp(v);
				}
			}
			return returnValue;
		}
		/// <inheritdoc/>
		public void WriteHelp()
		{
			Console.WriteLine(HelpText);
		}
	}
}