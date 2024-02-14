namespace CmdLineNet
{
	using System;

	public sealed class CmdLineArgumentException : Exception
	{
		public CmdLineArgumentException() { }
		public CmdLineArgumentException(string? message) : base(message) { }
		public CmdLineArgumentException(string? message, Exception? innerException) : base(message, innerException) { }
	}
}