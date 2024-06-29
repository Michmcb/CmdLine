namespace CmdLineNet
{
	using System;

	/// <summary>
	/// An exception for errors specific to command line errors.
	/// </summary>
	public sealed class CmdLineArgumentException : Exception
	{
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public CmdLineArgumentException() { }
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public CmdLineArgumentException(string? message) : base(message) { }
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		public CmdLineArgumentException(string? message, Exception? innerException) : base(message, innerException) { }
	}
}