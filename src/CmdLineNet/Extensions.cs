namespace CmdLineNet
{
	public static class Extensions
	{
		public static string ToStr(this ArgType v)
		{
			return v switch
			{
				ArgType.Switch => "Switch",
				ArgType.Option => "Option",
				ArgType.Value => "Value",
				_ => "",
			};
		}
	}
}