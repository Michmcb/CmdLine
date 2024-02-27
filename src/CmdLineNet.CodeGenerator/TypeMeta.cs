namespace CmdLineNet.CodeGenerator;

public sealed record class TypeMeta(string? ParseMethod, bool IsValueType, ParseMethodReturnType ParseMethodReturnType, bool IntegralType);