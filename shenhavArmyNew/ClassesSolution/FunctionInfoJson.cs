using System.Collections.Generic;

namespace ClassesSolution
{
    //Both classes are being used to store a json file that the program sends to the Tools !
    public class FunctionInfoJson
    {

        public string content;
        public ParametersType[] parameters;
        public string returnType;
        public string documentation;
        public int codeLength;
    }
    public class CodeInfoJson
    {
        public string[] includes;
        public int includesAmount;
        public int definesAmount;
        public Dictionary<string, string> defines;
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
    class Program
    {
        static void Main(string[] args)
        {
        }
    }
}
    
