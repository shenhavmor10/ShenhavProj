using System;

public class FunctionInfoJson
{

    public string content;
    public ParametersType[] parameters;
    public string returnType;
    public string documentation;
}
public class ParametersType
{
    public string parameterName;
    public string parameterType;
    public ParametersType(string parameterName, string parameterType)
    {
        this.parameterType = parameterType;
        this.parameterName = parameterName;
    }
}
