namespace CmdLineNet
{
	using System;
	using System.Runtime.Serialization;

	public sealed class CmdLineArgumentException : Exception
	{
		public CmdLineArgumentException() { }
		public CmdLineArgumentException(string? message) : base(message) { }
		public CmdLineArgumentException(SerializationInfo info, StreamingContext context) : base(info, context) { }
		public CmdLineArgumentException(string? message, Exception? innerException) : base(message, innerException) { }
	}
}