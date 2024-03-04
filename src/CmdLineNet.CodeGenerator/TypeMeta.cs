namespace CmdLineNet.CodeGenerator;

public sealed record class TypeMeta(string? ParseMethod, ParseMethodReturnType ParseMethodReturnType, bool IsValueType, bool IntegralType);