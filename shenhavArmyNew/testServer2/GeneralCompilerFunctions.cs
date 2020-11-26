using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using ClassesSolution;
using Server;

namespace testServer2
{
    public class GeneralCompilerFunctions
    {
        const string FILE_NOT_FOUND = "No such file found.";
        //All Patterns That is being searched in the code.
        static Regex OpenBlockPattern = new Regex(@".*{.*");
        static Regex CloseBlockPattern = new Regex(@".*}.*");
        static Regex functionPatternInH = new Regex(@"^[a-zA-Z]+.*\s[a-zA-Z].*[(].*[)]\;$");
        static Regex staticFunctionPatternInC = new Regex(@"^.*static.*\s.*[a-zA-Z]+.*\s[a-zA-Z].*[(].*[)]$");
        static Regex FunctionPatternInC = new Regex(@"^([^ ]+\s)?[^ ]+\s(.*\s)?[^ ]+\([^()]*\)$");
        static Regex StructPattern = new Regex(@"^([^\s\/\*()]+)?(\s)?(struct\s(.+{$|.*{$|[^\s;]+$))");
        static Regex TypedefOneLine = new Regex(@"^.*typedef\sstruct\s[^\s]+\s[^\s]+;$");
        static Regex VariableDecleration = new Regex(@"^(?!.*return)(?=(\s)?[^\s()]+\s((\*)*(\s))?[^\s()=]+(\s?=.+;|[^()=]*;))");
        static Regex VariableEquation = new Regex(@"^(?!.*return)(?=(\s)?([^\s()]+\s)?((\*)*(\s))?[^\s()]+(\s)?=(\s)?[A-Za-z][^\s()]*;$)");
        static Regex DefineDecleration = new Regex(@"^(\s)?#define ([^ ]+) [^\d][^ ()]*( [^ ()]+)?$");
        //include <NAME>
        static Regex IncludeTrianglesPattern = new Regex(@"^(\s)?#include.{0,2}<.+>$");
        static Regex IncludeRegularPattern = new Regex(@"^(\s)?#include\s{0,2}"".+\""$");
        static Regex IncludePathPattern = new Regex(@"^(\s)?#include\s{0,2}"".+\""$");
        const string Documentation = @"(\/\*.*\*\/)|((?!.*\/\*).*\*\/)|(\/\/.*)";
        //chars to trim.
        static char[] CharsToTrim = { '&', '*', '\t', ' ', ';', '{', '}' };
        static bool CompileError = false;
        static ArrayList ignoreVarialbesType = new ArrayList();
        /// Function - CreateMd5
        /// <summary>
        /// Function gets a string as an input and turns it to an MD5.
        /// </summary>
        /// <param name="input"> this paramter is the string that is being changed to an MD5 format.</param>
        /// <returns>MD5 format type string.</returns>
        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }
        /// Function - NextScopeLength
        /// <summary>
        ///  in order to find function length or struct length or "next scope" this function can be used.
        /// </summary>
        /// <param name="sr"> type MyStream buffer for the file. </param>
        /// <param name="codeLine"> refference of the current code line type string. </param>
        /// <param name="count"> refference of the length of the scope type int. </param>
        /// <param name="Seek"> bool type parameter for returning the buffer to where it started from
        ///                     or to keep the buffer where it is after the scope ends. </param>
        /// <returns></returns>
        public static bool NextScopeLength(MyStream sr, ref string codeLine, ref int count, bool Seek)
        {
            //stack to count the blocks.
            Stack myStack = new Stack();
            //saving the current position of the buffer.
            uint curPos = sr.Pos;
            string ScopeName = new string(codeLine.ToCharArray());
            codeLine = sr.ReadLine();
            myStack.Push(codeLine);
            bool found = false;
            while ((codeLine != null && myStack.Count > 0))
            {
                codeLine = sr.ReadLine();
                count++;
                if (codeLine.IndexOf("{") != GeneralConsts.NOT_FOUND_STRING)
                {
                    myStack.Push(codeLine);
                }
                if (codeLine.IndexOf("}") != GeneralConsts.NOT_FOUND_STRING)
                {
                    myStack.Pop();
                }
                

            }
            if (myStack.Count == 0)
            {
                found = true;
            }
            count = count - 1;
            //checking the bool for seeking.
            if (Seek)
            {
                sr.Seek(curPos);
                codeLine = ScopeName;
                Console.WriteLine(codeLine);
            }
            myStack.Clear();
            return found;
        }
        /// Function - CheckIfStringInHash
        /// <summary>
        /// function checks if in the given hashtable it can find a key 
        /// that is equal to the s parameter.
        /// </summary>
        /// <param name="a"> Hashtable </param>
        /// <param name="codeLine"> string </param>
        /// <returns> returns if the string is in the hash type bool</returns>
        static bool CheckIfStringInHash(Hashtable a, string codeLine)
        {
            bool foundString = false;
            string result = codeLine.Trim(CharsToTrim);
            if (a.ContainsKey(CreateMD5(result)))
            {
                foundString = true;
            }
            return foundString;
        }
        /// Function - skipDocumentation
        /// <summary>
        /// skips the documentation in c file in a buffer. 
        /// </summary>
        /// <param name="sr"> buffer type MyStream</param>
        /// <param name="codeLine"> string </param>
        /// <returns> returns the amount of rows the documentation was.</returns>
        public static int skipDocumentation(MyStream sr, string codeLine)
        {
            int count = 0;
            uint pos = sr.Pos;
            if (codeLine.IndexOf("//") != GeneralConsts.NOT_FOUND_STRING)
            {
                while ((codeLine.IndexOf("//") != GeneralConsts.NOT_FOUND_STRING))
                {
                    pos = sr.Pos;
                    count++;
                    codeLine = sr.ReadLine();
                }
                sr.Seek(pos);
                count--;
            }
            if (codeLine.IndexOf("/*") != GeneralConsts.NOT_FOUND_STRING)
            {
                while (!(codeLine.IndexOf("*/") != GeneralConsts.NOT_FOUND_STRING))
                {
                    count++;
                    codeLine = sr.ReadLine();
                }
            }
            return count;
        }
        /// Function - KeywordsAmountOnVariableDeclaration
        /// <summary>
        /// Checks how many words are in the variable declaration before the name of the variable.
        /// </summary>
        /// <param name="codeLine"> variable declaration line type string.</param>
        /// <returns> returns type int word count.</returns>
        static int KeywordsAmountOnVariableDeclaration(string codeLine)
        {
            int count = 0;
            int pos = codeLine.IndexOf(GeneralConsts.SPACEBAR);
            bool endLoop = false;
            while (pos > 0 && !endLoop)
            {
                if (codeLine.IndexOf(GeneralConsts.EQUAL_SIGN) != GeneralConsts.NOT_FOUND_STRING)
                {
                    endLoop = true;
                }
                count++;
                if (codeLine.IndexOf(GeneralConsts.ASTERIX) != GeneralConsts.NOT_FOUND_STRING)
                    count = count - 1;

                pos = codeLine.IndexOf(GeneralConsts.SPACEBAR, pos + 1);
            }
            return count;
        }
        //the checks that are being mad in syntax Check are being written here.
        /// Function - IsExistInArrayList
        /// <summary>
        /// Checks if the string can be found in the ArrayList.
        /// </summary>
        /// <param name="a"> ArrayList </param>
        /// <param name="name"> the string that is being checked.</param>
        /// <returns> returns the ArrayList Node.</returns>
        public static ParametersType IsExistInArrayList(ArrayList a, string name)
        {
            ParametersType result = null;
            bool found = false;
            for (int i = 0; i < a.Count && !found; i++)
            {
                if (((ParametersType)a[i]).parameterName == name)
                {
                    result = (ParametersType)a[i];
                    found = true;
                }
            }
            return result;
        }
        /// Function getParameterNameFromLine
        /// <summary>
        /// Gets a code line of a variable Declaration and gives back the node of the variable (name,type).
        /// </summary>
        /// <param name="line"> Code line type string.</param>
        /// <returns> returns type - ParametersType the variable that was being declared.</returns>
        static ParametersType GetParameterNameFromLine(string line)
        {
            string name;
            string type = line;
            int lastIndex;
            //takes the substring that starts in the last space accurance and his length.
            name = line.Substring(line.LastIndexOf(GeneralConsts.SPACEBAR) + 1, (line.Length - (line.LastIndexOf(GeneralConsts.SPACEBAR) + 1)));
            //if there are no spaces in the line then it must be a '*', so doing the same thing with '*' instead of space.
            if (name == GeneralConsts.EMPTY_STRING)
            {
                name = line.Substring(line.LastIndexOf(GeneralConsts.ASTERIX), (line.Length - (line.LastIndexOf(GeneralConsts.ASTERIX) + 1)));
            }
            //last index of space
            lastIndex = line.LastIndexOf(GeneralConsts.SPACEBAR);
            if (lastIndex != GeneralConsts.NOT_FOUND_STRING)
            {
                //removes the variable name from the line to get the type
                type = line.Remove(lastIndex).Trim();
            }
            //all variables that does not matter to the checks are being removed.
            foreach (string var in ignoreVarialbesType)
            {
                if (type.IndexOf(var) != GeneralConsts.NOT_FOUND_STRING)
                {
                    type = type.Replace(var, GeneralConsts.EMPTY_STRING);
                    type = type.Trim();
                }
            }
            ParametersType result = new ParametersType(name, type);
            return result;
        }
        /// Function - VariableDeclarationHandler
        /// <summary>
        /// Handling the variable declaration part in the function "ChecksInSyntaxCheck" by
        /// checking the whole variable declaration syntax.. (Checks the keywords and if the variable is being equal so checks
        /// the types of both of variables are equal and adding the new variable to "blocksAndNames" ArrayList).
        /// </summary>
        /// <param name="codeLine"> refference of the string s (code line).</param>
        /// <param name="pos"></param>
        /// <param name="keywords"> Hashtable type that stores all keywords in the code.</param>
        /// <param name="blocksAndNames"> ArrayList Type that stores all variables in the scopes.</param>
        /// <param name="IsScope"> Variable that checks if the function is being called inside a scope or 
        ///                        outside a scope.</param>
        /// <param name="sr"> buffer type MyStream.</param>
        /// <returns></returns>
        static bool VariableDeclarationHandler(ref string codeLine, ref int pos, Hashtable keywords, int threadNumber, bool IsScope, MyStream sr,string typeEnding, ArrayList variables=null, ArrayList globalVariables=null, ArrayList blocksAndNames=null)
        {
            bool DifferentTypes = true;
            int loopCount;
            string parameterType = GeneralConsts.EMPTY_STRING;
            int j;
            bool found = true;
            char[] trimChars = { '\t', ' ', ';' };
            
            loopCount = KeywordsAmountOnVariableDeclaration(codeLine);
            for (j = 0; j < loopCount; j++)
            {
                //checks if the keywords in the declaration is exist.
                found = found && CheckIfStringInHash(keywords, codeLine.Substring(pos, codeLine.Substring(pos, codeLine.Length - pos).IndexOf(GeneralConsts.SPACEBAR)).Trim(CharsToTrim));
                pos = codeLine.IndexOf(GeneralConsts.SPACEBAR, pos + 1) + 1;
            }
            if (loopCount == 0)
            {
                //gets in the if section only if there is only 1 keyword.
                found = found && CheckIfStringInHash(keywords, codeLine.Substring(pos, codeLine.Substring(pos, codeLine.Length - pos).IndexOf(GeneralConsts.SPACEBAR)).Trim(CharsToTrim));
            }
            if (codeLine.IndexOf("struct") != GeneralConsts.NOT_FOUND_STRING)
            {
                //gets in if the variable type includes a struct without a typedef.
                pos = codeLine.IndexOf("struct");
                parameterType = codeLine.Substring(pos, codeLine.IndexOf(GeneralConsts.SPACEBAR, pos + 7) - pos);
                found = CheckIfStringInHash(keywords, parameterType.Trim(CharsToTrim));
            }
            string name;
            ParametersType result;
            //if the line has equation in the declaration.
            if (codeLine.IndexOf(GeneralConsts.EQUAL_SIGN) != -1)
            {
                parameterType = Regex.Split(codeLine, GeneralConsts.EQUAL_SIGN)[0];
                parameterType = parameterType.Trim(trimChars);
                result = GetParameterNameFromLine(parameterType);
            }
            //only declaration.
            else
            {
                parameterType = codeLine;
                parameterType = parameterType.Trim(trimChars);
                result = GetParameterNameFromLine(parameterType);
            }
            name = result.parameterName;
            parameterType = result.parameterType;
            parameterType = parameterType.Replace(GeneralConsts.SPACEBAR, GeneralConsts.EMPTY_STRING);
            // checks if there is already the same name in the same scope.
            if (IsExistInArrayList(((ArrayList)blocksAndNames[blocksAndNames.Count - 1]), name) != null)
            {
                Server.ConnectionServer.CloseConnection(threadNumber, ("you have used the same name for multiple variables in row " + sr.curRow + ". name - " + name), GeneralConsts.ERROR);
                CompileError = true;
            }
            else
            {
                ((ArrayList)blocksAndNames[blocksAndNames.Count - 1]).Add(new ParametersType(name, parameterType));
                variables.Add(new ParametersType(name, parameterType));
                if (!IsScope)
                {
                    try
                    {
                        globalVariables.Add(new ParametersType(name, parameterType));
                    }
                    catch
                    {
                        ConnectionServer.CloseConnection(threadNumber, ("you have used the same name for multiple variables in row " + sr.curRow + ". name - " + name), GeneralConsts.ERROR);
                    }

                }
            }
            //if the declaration is also a equation.
            if (VariableEquation.IsMatch(codeLine))
            {
                DifferentTypes = VariableEquationHandler(sr, codeLine, blocksAndNames, threadNumber);
            }
            if (!DifferentTypes)
            {
                Server.ConnectionServer.CloseConnection(threadNumber, codeLine + " types of both variables are different in row : " + sr.curRow, GeneralConsts.ERROR);
                CompileError = true;
            }

            return found;
        }
        /// Function - getVariableTypeParameterFromArrayList
        /// <summary>
        /// Get the whole parameterType node out of the ArrayList.
        /// </summary>
        /// <param name="blocksAndNames"> ArrayList type.</param>
        /// <param name="name"> the name to get the whole node from.</param>
        /// <returns> returns parameterType type of the node named like "name".</returns>
        static ParametersType GetVariableTypeParameterFromArrayList(ArrayList blocksAndNames, string name)
        {
            bool endLoop = false;
            ParametersType result = null;
            for (int i = blocksAndNames.Count; i > 0 && !endLoop; i--)
            {
                if ((result = IsExistInArrayList((ArrayList)blocksAndNames[i - 1], name)) != null)
                {
                    endLoop = true;
                }
            }
            return result;
        }
        /// Function - VariableEquationHandler
        /// <summary>
        /// Handling the variable equation part in the function "ChecksInSyntaxCheck" by
        /// make sure every variable is exist in the code and that their type of the equation
        /// is the same.
        /// </summary>
        /// <param name="threadNumber"> the number of the current thread.</param>
        /// <param name="sr"> buffer type MyStream.</param>
        /// <param name="codeLine"> the code line type string.</param>
        /// <param name="blocksAndNames"> ArrayList of variables.</param>
        /// <returns>returns if the variable equation is good.</returns>
        static bool VariableEquationHandler(MyStream sr, string codeLine, ArrayList blocksAndNames, int threadNumber)
        {
            char[] trimChars = { '\t', ' ' };
            bool isSameType = true;
            //splits the equation to 2 lines before the '=' and after it.
            string temp = Regex.Split(codeLine, GeneralConsts.EQUAL_SIGN)[0].Trim(trimChars);
            //takes the first param name.
            ParametersType result = GetParameterNameFromLine(temp);
            string varName1 = result.parameterName;
            temp = Regex.Split(codeLine, GeneralConsts.EQUAL_SIGN)[1];
            char[] searchingChars = { ';' };
            //takes the second param name.
            string varName2 = temp.Substring(0, temp.IndexOfAny(searchingChars));
            varName2 = varName2.Trim(trimChars);
            //takes the whole parameterType type by the function - "getVariableTypeParameterFromArrayList".
            ParametersType var1 = GetVariableTypeParameterFromArrayList(blocksAndNames, varName1.Trim(GeneralConsts.ASTERIX));
            ParametersType var2 = GetVariableTypeParameterFromArrayList(blocksAndNames, varName2.Trim(GeneralConsts.ASTERIX));
            //make sures the variable 2 is exist.
            if (var2 == null)
            {
                Server.ConnectionServer.CloseConnection(threadNumber, "There is no parameter named " + varName2 + " in row : " + sr.curRow, GeneralConsts.ERROR);
                CompileError = true;
                isSameType = false;
            }
            //checks if their type is the same.
            if (isSameType && var1.parameterType != var2.parameterType)
            {
                isSameType = false;
            }
            return isSameType;
        }
        static string cleanLineFromDoc(string codeLine)
        {
            char[] trimChar = { '\t', ' ' };
            string result = "";
            result=Regex.Replace(codeLine, Documentation, "");
            result = result.Trim(trimChar);
            return result;
        }
        /// Function - ChecksInSyntaxCheck
        /// <summary>
        /// this function take cares of the whole syntax check of the program.
        /// it uses the functions mentioned in the documentations before.
        /// it take cares scopes and no scopes with the parameter IsScope type bool.
        /// </summary>
        /// <param name="path"> the path of the file.</param>
        /// <param name="globalVariables"> variables that all file have.</param>
        /// <param name="threadNumber"> the number of the current thread running.</param>
        /// <param name="variables"> all of the variables that the scope knows.</param>
        /// <param name="sr"> buffer type MyStream.</param>
        /// <param name="codeLine"> code line type string.</param>
        /// <param name="IsScope"> bool type IsScope.</param>
        /// <param name="keywords"> keywords type Hashtable that conatins the code keywords.</param>
        /// <param name="blocksAndNames"> blocksAndNames type ArrayList that conatins the code variables in the scope.</param>
        /// <param name="parameters"> parameters type ArrayList conatins the function parameters.</param>
        /// <param name="functionLength"> scopeLength type int default is 0 if the code line is outside any scopes.</param>
        static void ChecksInSyntaxCheck(string path, MyStream sr, string codeLine, bool IsScope, Hashtable keywords, int threadNumber,string typeEnding, ArrayList variables, ArrayList globalVariables, ArrayList blocksAndNames, ArrayList parameters = null, int functionLength = 0)
        {
            //adds the parameters of the function to the current ArrayList of variables.
            int i;
            if (parameters != null)
            {
                if(typeEnding=="c")
                {
                    blocksAndNames.Add(new ArrayList());
                    ((ArrayList)blocksAndNames[1]).AddRange(parameters);
                }
                else if(typeEnding=="h")
                {
                    for (i = 0; i < parameters.Count; i++)
                    {
                        if (!keywords.ContainsKey(((ParametersType)parameters[i]).parameterType))
                        {
                            CompileError = true;
                        }
                    }
                }
            }
            codeLine = cleanLineFromDoc(codeLine);
            if (StructPattern.IsMatch(codeLine))
            {
                //Add struct keywords to the keywords Hashtable.
                AddStructNames(sr, codeLine, keywords);
            }
            if (codeLine.Trim(GeneralConsts.TAB_SPACE).IndexOf("{") != GeneralConsts.NOT_FOUND_STRING)
            {
                codeLine = sr.ReadLine();
            }
            //how to convert to array list
            bool keywordCheck = true;
            bool DifferentTypesCheck = true;
            int pos = 0;
            ArrayList keywordResults = new ArrayList();
            for (i = 0; i < functionLength + 1&&!CompileError; i++)
            {
                if (codeLine.Trim(GeneralConsts.TAB_SPACE) == GeneralConsts.EMPTY_STRING)
                {
                    codeLine = sr.ReadLine();
                }
                //take cares to all of those situations.
                if (StructPattern.IsMatch(codeLine) || TypedefOneLine.IsMatch(codeLine))
                {
                    keywordResults = AddStructNames(sr, codeLine, keywords);
                }
                if (VariableDecleration.IsMatch(codeLine) && !(codeLine.IndexOf("typedef") != GeneralConsts.NOT_FOUND_STRING))
                {
                    keywordCheck = VariableDeclarationHandler(ref codeLine, ref pos, keywords, threadNumber, IsScope, sr,typeEnding, variables, globalVariables, blocksAndNames);
                    if (!keywordCheck)
                    {
                        string error = (codeLine + " keyword does not exist. row : " + sr.curRow);
                        try
                        {
                            MainProgram.AddToLogString(path, sr.curRow.ToString());
                        }
                        catch (Exception e)
                        {
                            MainProgram.AddToLogString(path, e.ToString());
                        }
                        MainProgram.AddToLogString(path, error);
                        Server.ConnectionServer.CloseConnection(threadNumber, error, GeneralConsts.ERROR);
                        CompileError = true;

                    }
                }
                else if (typeEnding=="c"&&VariableEquation.IsMatch(codeLine))
                {
                    DifferentTypesCheck = VariableEquationHandler(sr, codeLine, blocksAndNames, threadNumber);
                    if (!DifferentTypesCheck)
                    {
                        string error = (codeLine + " types of both variables are different in row : " + sr.curRow);
                        Server.ConnectionServer.CloseConnection(threadNumber, error, GeneralConsts.ERROR);
                        CompileError = true;
                    }
                }
                codeLine = codeLine.Trim();
                //checks if any of the error bools is on.
                pos = 0;
                //resets the error bools.
                keywordCheck = DifferentTypesCheck = true;
                if (codeLine.IndexOf("//") != GeneralConsts.NOT_FOUND_STRING || codeLine.IndexOf("/*") != GeneralConsts.NOT_FOUND_STRING)
                {
                    //skips documentation if needed.
                    i += skipDocumentation(sr, codeLine);
                }
                //adds a new ArrayList inside the keywordsAndNames ArrayList for the scope that has started.
                if (OpenBlockPattern.IsMatch(codeLine))
                {
                    blocksAndNames.Add(new ArrayList());
                }
                if (CloseBlockPattern.IsMatch(codeLine))
                {
                    try
                    {
                        //close the last scope that just closed.
                        blocksAndNames.RemoveAt(blocksAndNames.Count - 1);

                    }
                    catch (Exception e)
                    {
                        //bad scoping causes the function to remove from an ArrayList something while its already 0.
                        Server.ConnectionServer.CloseConnection(threadNumber, "bad scoping in function in row " + sr.curRow, GeneralConsts.ERROR);
                        CompileError = true;
                    }
                }
                //if the code line is in a scope or if its not the last line in the scope continute to the next line.
                if (IsScope && i != functionLength)
                {
                    codeLine = sr.ReadLine();
                }
            }
            //if that was a scope it removes all the keywords of the scope.
            if (IsScope)
            {
                for (i = 0; i < keywordResults.Count; i++)
                {
                    keywords.Remove(keywordResults[i]);
                }
            }
        }
        static bool CheckInSyntaxCheckH(string path, MyStream sr, string codeLine, Hashtable keywords, bool IsScope, int threadNumber, ArrayList parameters = null, int functionLength = 0)
        {
            int i;
            if (parameters != null)
            {
                for (i = 0; i < parameters.Count; i++)
                {
                    if (!keywords.ContainsKey(((ParametersType)parameters[i]).parameterType))
                    {
                        CompileError = true;
                    }
                }
            }
            if (StructPattern.IsMatch(codeLine))
            {
                //Add struct keywords to the keywords Hashtable.
                AddStructNames(sr, codeLine, keywords);
            }
            if (codeLine.Trim(GeneralConsts.TAB_SPACE).IndexOf("{") != GeneralConsts.NOT_FOUND_STRING)
            {
                codeLine = sr.ReadLine();
            }
            int pos = 0;
            bool keywordCheck = true;
            ArrayList keywordResults = new ArrayList();
            for (i = 0; i < functionLength + 1; i++)
            {
                if (codeLine.Trim(GeneralConsts.TAB_SPACE) == GeneralConsts.EMPTY_STRING)
                {
                    codeLine = sr.ReadLine();
                }
                //take cares to all of those situations.
                if (StructPattern.IsMatch(codeLine) || TypedefOneLine.IsMatch(codeLine))
                {
                    keywordResults = AddStructNames(sr, codeLine, keywords);
                }
                if (VariableDecleration.IsMatch(codeLine) && !(codeLine.IndexOf("typedef") != GeneralConsts.NOT_FOUND_STRING))
                {
                    //keywordCheck = VariableDeclarationHandler(ref codeLine, ref pos, keywords, threadNumber, variables, globalVariables, blocksAndNames, IsScope, sr);
                }
                if (!keywordCheck)
                {
                    string error = (codeLine + " keyword does not exist. row : " + sr.curRow);
                    try
                    {
                        MainProgram.AddToLogString(path, sr.curRow.ToString());
                    }
                    catch (Exception e)
                    {
                        MainProgram.AddToLogString(path, e.ToString());
                    }
                    MainProgram.AddToLogString(path, error);
                    Server.ConnectionServer.CloseConnection(threadNumber, error, GeneralConsts.ERROR);
                    CompileError = true;

                }
                pos = 0;
                keywordCheck = true;
                if (codeLine.IndexOf("//") != GeneralConsts.NOT_FOUND_STRING || codeLine.IndexOf("/*") != GeneralConsts.NOT_FOUND_STRING)
                {
                    //skips documentation if needed.
                    i += skipDocumentation(sr, codeLine);
                }
                if (IsScope && i != functionLength)
                {
                    codeLine = sr.ReadLine();
                }

            }
            return CompileError;

        }
           
        /// Function - SyntaxCheck
        /// <summary>
        /// that function uses the Function "ChecksInSyntaxCheck" if that is in a scope
        /// or outside a scope according to the situation.
        /// </summary>
        /// <param name="path"> The path of the c code type string.</param>
        /// <param name="keywords"> keywords type Hashtable that conatins the code keywords.</param>
        public static bool SyntaxCheck(string path,ArrayList globalVariable, Hashtable keywords,Dictionary<string,ArrayList> funcVariables,int threadNumber,string typeEnding)
        {
            MyStream sr=null;
            try
            {
                 sr= new MyStream(path, System.Text.Encoding.UTF8);
            }
            catch(Exception e)
            {
                Server.ConnectionServer.CloseConnection(threadNumber, FILE_NOT_FOUND, GeneralConsts.ERROR);
            }
            if(sr!=null)
            {
                //in order to delete struct keywords when they come in a function at the end of the function.
                ArrayList parameters = new ArrayList();
                ArrayList blocksAndNames = new ArrayList();
                ArrayList variables = new ArrayList();
                //adds an ArrayList inside blocksAndNames ArrayList for the action outside the scopes.
                blocksAndNames.Add(new ArrayList());
                string codeLine=sr.ReadLine();
                int scopeLength = 0;
                string lastFuncLine = "";
                while (((codeLine = sr.ReadLine())!=null) && !CompileError)
                {
                        
                    scopeLength = 0;
                    //handling the scopes.
                    if (OpenBlockPattern.IsMatch(codeLine))
                    {
                        NextScopeLength(sr, ref codeLine, ref scopeLength, true);
                        ChecksInSyntaxCheck(path, sr, codeLine, true, keywords, threadNumber,typeEnding, variables, globalVariable, blocksAndNames, parameters, scopeLength + 1);
                        parameters.Clear();
                    }
                    // if there is a function it saves its parameters.
                    else if (FunctionPatternInC.IsMatch(codeLine))
                    {
                        
                        parameters.AddRange(GeneralRestApiServerMethods.FindParameters(cleanLineFromDoc(codeLine)));
                        if (lastFuncLine != "")
                        {
                            funcVariables.Add(lastFuncLine, new ArrayList(variables));
                            variables.Clear();
                        }
                        lastFuncLine = codeLine;
                    }
                    else if (functionPatternInH.IsMatch(codeLine))
                    {
                        parameters.AddRange(GeneralRestApiServerMethods.FindParameters(cleanLineFromDoc(codeLine)));
                        lastFuncLine = codeLine;
                        ChecksInSyntaxCheck(path, sr, codeLine, false, keywords, threadNumber, typeEnding, variables, globalVariable, blocksAndNames,parameters);
                    }
                    //handling outside the scopes.
                    else
                    {
                        ChecksInSyntaxCheck(path, sr, codeLine, false, keywords, threadNumber,typeEnding, variables, globalVariable, blocksAndNames);
                    }

                }
                if (lastFuncLine != "")
                {
                    funcVariables.Add(lastFuncLine, new ArrayList(variables));
                    variables.Clear();
                }
            }
            return CompileError;
        }
        /// Function - AddToHashFromFile
        /// <summary>
        /// Adds from a file splited by "splitBy" parameter to the Hashtable fromt he path.
        /// </summary>
        /// <param name="path"> The path for the file.</param>
        /// <param name="a"> Hashtable to store the keywords.</param>
        /// <param name="splitBy"> String that the file needs to split by.</param>
        public static void AddToHashFromFile(string path, Hashtable a, string splitBy)
        {

            MyStream sr = null;
            try
            {
                sr = new MyStream(path, System.Text.Encoding.UTF8);
            }
            catch(Exception e)
            {
                MainProgram.AddToLogString(path,e.ToString());
            }
            string line = sr.ReadLine();
            string[] keysArr = Regex.Split(line, splitBy);
            ICollection keys = a.Keys;
            for (int i = 0; i < keysArr.Length; i++)
            {
                a.Add(CreateMD5(keysArr[i]), keysArr[i]);
            }
            sr.Close();
        }
        //path to the code;
        /// Function - PreprocessorActions
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"> The path for the C code.</param>
        /// <param name="threadNumber"> thread number is a parameter to make sure when the main file is in.
        ///                             number 0 means its the main file any other number means it  is currently
        ///                             reads a file that is not the main. (Import).</param>
        /// <param name="keywords"> Hashtable to store the keywords.</param>
        /// <param name="includes"> Hashtable to store the includes.</param>
        /// <param name="defines"> Dictionary to store the defines . (key - new keyword, value - old Definition)</param>
        /// <param name="pathes"> Paths for all the places where the imports might be.</param>
        static void PreprocessorActions(string path, int threadNumber, Hashtable keywords, Hashtable includes,Dictionary<string,string> defines,string [] pathes,int fileThreadNumber)
        {
            bool endLoop = false;
            MyStream sr = null;
            //try to open the buffer.
            try
            {
                sr = new MyStream(path, System.Text.Encoding.UTF8);
            }
            catch (Exception e)
            {
                MainProgram.AddToLogString(path,String.Format("{0} Second exception caught.", e.ToString()));
                Server.ConnectionServer.CloseConnection(fileThreadNumber, FILE_NOT_FOUND, GeneralConsts.ERROR);
                endLoop = true;
            }

            string codeLine;
            string firstNewVariableWord;
            string secondNewVariableWord;
            string[] newkeyWords;
            string newKeyword;
            string defineOriginalWord;
            while (!endLoop && (codeLine = sr.ReadLine()) != null)
            {
                codeLine = cleanLineFromDoc(codeLine);
                if (DefineDecleration.IsMatch(codeLine))
                {
                    //getting both of the names in two variables.
                    firstNewVariableWord = codeLine.Substring(codeLine.IndexOf(' '), codeLine.Length - codeLine.IndexOf(' '));
                    firstNewVariableWord = firstNewVariableWord.Trim();
                    firstNewVariableWord = firstNewVariableWord.Substring(firstNewVariableWord.IndexOf(GeneralConsts.SPACEBAR), firstNewVariableWord.Length - firstNewVariableWord.IndexOf(GeneralConsts.SPACEBAR));
                    firstNewVariableWord = firstNewVariableWord.Trim(CharsToTrim);
                    //old definition.
                    defineOriginalWord = firstNewVariableWord;
                    
                    if (firstNewVariableWord.IndexOf(GeneralConsts.SPACEBAR) != GeneralConsts.NOT_FOUND_STRING)
                    {
                        //checks if the definition exists.
                        if(keywords.ContainsKey(CreateMD5(firstNewVariableWord)))
                        {
                            //new keyword
                            //takes the part after the first space.
                            newKeyword = Regex.Split(codeLine, GeneralConsts.SPACEBAR)[1];
                            newKeyword = newKeyword.Trim();
                            //make sure that the keyword isn't already existed.
                            if (!keywords.ContainsKey(CreateMD5(newKeyword)))
                            {
                                //adds her if not
                                keywords.Add(CreateMD5(newKeyword), newKeyword);
                                //adds the new definition.
                                defines.Add(newKeyword, defineOriginalWord);
                                //types the dont mind the variable are being ingored. for an example static : so if
                                //the type for example is static and i define a new static definition it will add it to
                                //the ignoreVariablesType.
                                if (ignoreVarialbesType.Contains(defineOriginalWord))
                                {
                                    ignoreVarialbesType.Add(newKeyword);
                                }
                            }
                        }
                        else
                        {
                            //splits when there are 2 types that you define for an example "unsinged int"
                            //so what this section is doing is checking if both of the types exist.
                            newkeyWords = Regex.Split(firstNewVariableWord, GeneralConsts.SPACEBAR);
                            secondNewVariableWord = newkeyWords[1];
                            firstNewVariableWord = newkeyWords[0];
                            //checks both types.
                            if (CheckIfStringInHash(keywords, firstNewVariableWord) && CheckIfStringInHash(keywords, secondNewVariableWord))
                            {
                                newKeyword = Regex.Split(codeLine, GeneralConsts.SPACEBAR)[1];
                                newKeyword = newKeyword.Trim();
                                //creates the keywords if they dont exist.
                                if (!keywords.ContainsKey(CreateMD5(newKeyword)))
                                {
                                    keywords.Add(CreateMD5(newKeyword), newKeyword);
                                    defines.Add(newKeyword, defineOriginalWord);
                                    MainProgram.AddToLogString(path, "new Keywords :" + newkeyWords[0]);
                                }
                            }
                        }
                        
                    }
                    else
                    {
                        //if there is only one type in the old definition.
                        if (CheckIfStringInHash(keywords, firstNewVariableWord))
                        {
                            newKeyword = Regex.Split(codeLine, GeneralConsts.SPACEBAR)[1];
                            newKeyword = newKeyword.Trim();
                            if (!keywords.ContainsKey(CreateMD5(newKeyword)))
                            {
                                keywords.Add(CreateMD5(newKeyword), newKeyword);
                                defines.Add(newKeyword, defineOriginalWord);
                                MainProgram.AddToLogString(path, "new : " +newKeyword);
                            }
                        }
                    }

                }
                //Handling almost the same patterns as the syntaxCheck function.
                if (StructPattern.IsMatch(codeLine) && threadNumber != 0)
                {
                    AddStructNames(sr, codeLine, keywords);
                }
                else if (TypedefOneLine.IsMatch(codeLine) && threadNumber != 0)
                {
                    AddStructNames(sr, codeLine, keywords);
                }
                //if the code line is an include it creates a thread and enters to the defines , structs and more to 
                //the Hashtables and Dictionaries.
                else if (IncludeTrianglesPattern.IsMatch(codeLine) || IncludeRegularPattern.IsMatch(codeLine))
                {
                    string currentPath= GeneralConsts.EMPTY_STRING;
                    string result;
                    if (codeLine.IndexOf("<") != -1 && codeLine.IndexOf(">") != -1)
                    {
                        result = CutBetween2Strings(codeLine, "<", ">");
                    }
                    else
                    {
                        result = CutBetween2Strings(codeLine, "\"", "\"");
                    }
                    MainProgram.AddToLogString(path, result);
                    //only enters an include if it didnt already included him.
                    if (!includes.Contains(CreateMD5(result)))
                    {
                        includes.Add(CreateMD5(result), result);
                        Thread preprocessorThread;
                        //if the include includes a path inside of it.
                        if (result.IndexOf("\\") != -1)
                        {
                            //opens the thread (thread number +1).
                            preprocessorThread = new Thread(() => PreprocessorActions(result, threadNumber + 1, keywords, includes,defines, pathes,fileThreadNumber));
                        }
                        //if it does not include an exact path.
                        else
                        {
                            //runs on the pathes that the import files might be in.
                            for(int i=0;i<pathes.Length;i++)
                            {
                                //checks if the file exists in one of those folders.
                                if(File.Exists(pathes[i]+"\\"+result))
                                {
                                    currentPath = pathes[i];
                                    break;                                
                                }
                            }    
                            //creats a thread.
                            preprocessorThread = new Thread(() => PreprocessorActions(currentPath+"\\" + result, threadNumber + 1, keywords, includes,defines, pathes,fileThreadNumber));
                        }
                        preprocessorThread.Start();
                        preprocessorThread.Join(GeneralConsts.TIMEOUT_JOIN);
                        MainProgram.AddToLogString(path, "thread " + threadNumber + "stopped");
                    }
                }

            }
            if (sr != null)
            {
                sr.Close();
            }
            printArrayList(path, keywords);
        }
        /// Function - CutBetween2Strings
        /// <summary>
        /// Cut a string s from the first parameter to the second parameter.
        /// </summary>
        /// <param name="codeLine"> string s that the function cuts.</param>
        /// <param name="first"> first cut.</param>
        /// <param name="second"> second cut.</param>
        /// <returns> returns the cutted string.</returns>
        public static string CutBetween2Strings(string codeLine, string first, string second)
        {
            int startCut = codeLine.IndexOf(first) + first.Length;
            int endCut= codeLine.LastIndexOf(second);
            string result = codeLine.Substring(startCut, endCut - startCut);
            result = result.Trim();
            return result;
        }
        /// Function - AddStructNames
        /// <summary>
        /// adds all the names of the struct or typedef struct to the keywords Hashtable.
        /// </summary>
        /// <param name="sr"> buffer type MyStream.</param>
        /// <param name="s"> struct first line</param>
        /// <param name="keywords"> Hashtable to store the keywords.</param>
        /// <returns> returns an ArrayList with the keywords.</returns>
        static ArrayList AddStructNames(MyStream sr, string codeLine, Hashtable keywords)
        {
            ArrayList results = new ArrayList();
            int count = 0;
            int spaceIndex;
            string[] tempSplit;
            string tempString;
            string newVariableName;
            //if thats a typedef.
            uint curpos = sr.Pos;
            
            if (codeLine.IndexOf("typedef") != GeneralConsts.NOT_FOUND_STRING)
            {
                //if thats not a typedef declaration.
                if (!TypedefOneLine.IsMatch(codeLine))
                {
                    string structKeyword = codeLine.Trim(CharsToTrim);
                    structKeyword = structKeyword.Substring(structKeyword.IndexOf("struct"));
                    if (!keywords.Contains(CreateMD5(structKeyword)))
                    {
                        //adds the new keyword.
                        keywords.Add(CreateMD5(structKeyword), structKeyword);
                        results.Add(CreateMD5(structKeyword));
                    }
                    if (NextScopeLength(sr, ref codeLine, ref count,false))
                    {
                        codeLine = codeLine.Trim(CharsToTrim);

                        tempSplit = Regex.Split(codeLine, @",");
                        for (int i = 0; i < tempSplit.Length; i++)
                        {
                            tempSplit[i] = tempSplit[i].Trim(CharsToTrim);
                            if (!keywords.ContainsKey(CreateMD5(tempSplit[i])))
                            {
                                //adds the keywords.
                                keywords.Add(CreateMD5(tempSplit[i]), tempSplit[i]);
                                results.Add(CreateMD5(tempSplit[i]));
                            }

                        }
                    }
                }
                else
                {
                    //if thats one line of typedef.
                    spaceIndex = codeLine.IndexOf(GeneralConsts.SPACEBAR) + 1;
                    //take after the line after the first space.
                    tempString = newVariableName = codeLine.Substring(spaceIndex);
                    tempString = tempString.TrimEnd(' ').Remove(tempString.LastIndexOf(GeneralConsts.SPACEBAR) + 1);
                    tempString = tempString.Trim(CharsToTrim);
                    newVariableName = CutBetween2Strings(codeLine, tempString, ";");
                    newVariableName = newVariableName.Trim(CharsToTrim);
                    if (keywords.Contains(CreateMD5(tempString)) && !keywords.Contains(CreateMD5(newVariableName)))
                    {
                        //adds the keyword for the line.
                        keywords.Add(CreateMD5(newVariableName), newVariableName);
                        results.Add(CreateMD5(newVariableName));
                    }
                }
            }
            //if thats a regular struct.
            else
            {
                codeLine = codeLine.Trim(CharsToTrim);
                if (!keywords.Contains(CreateMD5(codeLine)))
                {
                    //adds the new keyword.
                    keywords.Add(CreateMD5(codeLine), codeLine);
                    results.Add(CreateMD5(codeLine));
                }


            }
            //returns the ArrayList.
            sr.Seek(curpos);
            return results;


        }
        /// Function - printArrayList
        /// <summary>
        /// prints an arrayList.
        /// </summary>
        /// <param name="a"> Hashtable A to print</param>
        public static void printArrayList(string path,Hashtable a)
        {
            ICollection keys = a.Keys;
            foreach (string key in keys)
            {
                MainProgram.AddToLogString(path,a[key].ToString());
            }
        }
        /// Function - AddToArrayListFromFile
        /// <summary>
        /// Adds from a file the names that split by "split by" in the path "path"
        /// </summary>
        /// <param name="path"> path of the c code type string.</param>
        /// <param name="a"> ArrayList </param>
        /// <param name="splitBy"> What to split by type string.</param>
        public static void AddToArrayListFromFile(string path,ArrayList a,string splitBy)
        {
            MyStream sr = new MyStream(path, System.Text.Encoding.UTF8);
            string s = sr.ReadLine();
            string [] temp = Regex.Split(s, splitBy);
            foreach (string i in temp)
            {
                a.Add(i);
            }
            sr.Close();
        }
        /// Function - initializeKeywordsAndSyntext
        /// <summary>
        /// Function initialize all of the functions that needs to come before the syntax Check.
        /// </summary>
        /// <param name="ansiPath">  Path for the file the conatins the ansi c keywords.</param>
        /// <param name="cFilePath"> Path for the c file that contains the file.</param>
        /// <param name="CSyntextPath"> Path of all the ansi c syntext file (not sure yet if it is needed.)</param>
        /// <param name="ignoreVariablesTypesPath"> Path for all the variables types that i ignore in the checking.</param>
        /// <param name="keywords"> Hashtable to store the keywords.</param>
        /// <param name="includes"> Hashtable to store the includes.</param>
        /// <param name="defines"> Dictionary to store the defines.</param>
        /// <param name="pathes"> Paths for all the places where the imports might be.</param>
        public static void initializeKeywordsAndSyntext(string ansiPath, string cFilePath, string CSyntextPath, string ignoreVariablesTypesPath, Hashtable keywords, Hashtable includes,Dictionary<string,string> defines,string [] pathes,int threadNumber)
        {
            //ansiC File To Keywords ArrayList.
            AddToHashFromFile(ansiPath, keywords, ",");
            AddToArrayListFromFile(ignoreVariablesTypesPath, ignoreVarialbesType, ",");
            //C Syntext File To Syntext ArrayList.
            //AddToListFromFile(CSyntextPath, syntext, " ");
            PreprocessorActions(cFilePath, 0, keywords, includes,defines,pathes,threadNumber);
        }
    }
}





