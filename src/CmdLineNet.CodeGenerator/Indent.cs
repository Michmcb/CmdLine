namespace CmdLineNet.CodeGenerator;

public sealed class Indent
{
	public Indent(int amt, char c)
	{
		Char = c;
		Amt = amt;
		Val = new string(c, amt);
	}
	public char Char { get; }
	public int Amt { get; }
	public string Val { get; private set; }
	public string In()
	{
		Val += new string(Char, Amt);
		return Val;
	}
	public string Out()
	{
		Val = Val[..^Amt];
		return Val;
	}
	public static implicit operator string(Indent s) => s.Val;
}
