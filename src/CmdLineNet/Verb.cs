namespace CmdLineNet
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// A verb.
	/// </summary>
	/// <typeparam name="T">The return type.</typeparam>
	public sealed class Verb<T> : IVerb<T>
	{
		private readonly VerbHandler<T> execute;
		private readonly Action writeHelp;
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public Verb(string name, string description, VerbHandler<T> execute, Action writeHelp)
		{
			Name = name;
			Description = description;
			this.execute = execute;
			this.writeHelp = writeHelp;
		}
		/// <inheritdoc/>
		public string Name { get; }
		/// <inheritdoc/>
		public string Description { get; }
		/// <inheritdoc/>
		public T Execute(IEnumerable<string> args)
		{
			return execute(Name, args);
		}
		/// <inheritdoc/>
		public void WriteHelp()
		{
			 writeHelp();
		}
	}
}