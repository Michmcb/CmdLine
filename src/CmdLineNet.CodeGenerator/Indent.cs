namespace CmdLineNet.CodeGenerator;

public sealed class Indent
{
	public Indent(int amount)
	{
		Val = new string('\t', amount);
	}
	public string Val { get; private set; }
	public string In()
	{
		Val += "\t";
		return Val;
	}
	public string Out()
	{
		Val = Val.Substring(0, Val.Length - 1);
		return Val;
	}
	public static implicit operator string(Indent s) => s.Val;
}
