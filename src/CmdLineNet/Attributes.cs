namespace CmdLineNet
{
	using System;
	//  public int Arity { get; init; } = 1;
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
		public string? Help { get; init; }
	}
	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class OptionAttribute : Attribute
	{
		public char ShortName{ get; init; }
		public string? LongName{ get; init; }
		public string? Help { get; init; }
	}
	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class ValueAttribute : Attribute
	{
		public string? Help { get; init; }
	}
}