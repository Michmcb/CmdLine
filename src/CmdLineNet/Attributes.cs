namespace CmdLineNet
{
	using System;
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class VerbAttribute : Attribute
	{
		public string? Help { get; init; }
	}
	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class SwitchAttribute : Attribute
	{
		public char ShortName{ get; init; }
		public string? LongName{ get; init; }
		public int Min { get; init; } = 1;
		public int Max { get; init; } = int.MaxValue;
		public string? Help { get; init; }
	}
	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class OptionAttribute : Attribute
	{
		public char ShortName{ get; init; }
		public string? LongName{ get; init; }
		public int Min { get; init; } = 1;
		public int Max { get; init; } = int.MaxValue;
		public string? Help { get; init; }
	}
	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class ValueAttribute : Attribute
	{
		public string? Help { get; init; }
		public int Min { get; init; } = 1;
		public int Max { get; init; } = int.MaxValue;
	}
}