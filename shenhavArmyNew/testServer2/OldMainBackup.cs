using System;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using ClassesSolution;

namespace testServer2
{
    class Program
    {
        static Regex OpenBlockPattern = new Regex(@"{");
        static Regex CloseBlockPattern = new Regex(@"}");
        static Regex functionPatternInH = new Regex(@"^[a-zA-Z]+.*\s[a-zA-Z].*[(].*[)]\;$");
        static Regex staticFunctionPatternInC = new Regex(@"^.*static.*\s.*[a-zA-Z]+.*\s[a-zA-Z].*[(].*[)]$");
        static Regex FunctionPatternInC = new Regex(@"^([^ ]+\s)?[^ ]+\s(.*\s)?[^ ]+\([^()]*\)$");
        static Regex StructPattern = new Regex(@".*struct(\s.+{$|[^\s]+$|.*{.+;$)");
        static Regex TypedefOneLine = new Regex(@"^.*typedef(\sstruct)?\s.+\s.+;$");
        static Regex VariableDecleration = new Regex(@"^(?!.*return)(?=(\s)?[^\s()]+\s((\*)*(\s))?[^\s()=]+(\s?=.+;|[^()=]*;)$)");
        static Regex DefineDecleration = new Regex(@"^(\s)?#define ([^ ]+) [^\d][^ ()]*( [^ ()]+)?$");
        //include <NAME>
        static Regex IncludeTrianglesPattern = new Regex(@"^(\s)?#include.{0,2}<.+>$");
        static Regex IncludeRegularPattern = new Regex(@"^(\s)?#include\s{0,2}"".+\""$");
        static Regex IncludePathPattern = new Regex(@"^(\s)?#include\s{0,2}"".+\""$");
        static string filePath = @"C:\Users\Shenhav\Desktop\shenhavArmyNew\tsetCCode\tsetCCode\test.c";
        static string projectPath = @"C:\Users\Shenhav\Desktop\shenhavArmyNew\tsetCCode\tsetCCode";
        //static string filePath = @"C:\Users\Shenhav\Desktop\Check\checkOne.c";
        static string ansiCFile = @"C:\Users\Shenhav\Desktop\shenhavArmyNew\Ansikeywords.txt";
        static string CSyntextFile = @"C:\Users\Shenhav\Desktop\shenhavArmyNew\CSyntext.txt";
        static string librariesPath = @"C:\Users\Shenhav\Desktop\mingw-w64-v7.0.0\mingw-w64-headers\crt";
        //static string librariesPath = @"C:\Users\Shenhav\Desktop\Check";
        static Hashtable keywords = new Hashtable();
        static Hashtable includes = new Hashtable();
        static ArrayList syntext = new ArrayList();
        //static int threadNumber=0;

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
        public static string FunctionCode(MyStream sr, ref string s)
        {
            uint curPos = sr.Pos;
            int functionLength = 0;
            string finalCode = "";
            Stack myStack = new Stack();
            s = sr.ReadLine();
            myStack.Push(s);
            while ((s != null && myStack.Count > 0))
            {
                s = sr.ReadLine();
                finalCode += s + "\n\r";
                functionLength++;
                if (OpenBlockPattern.IsMatch(s))
                {
                    myStack.Push(s);
                }
                if (CloseBlockPattern.IsMatch(s))
                {
                    myStack.Pop();
                }
                //here will be where i will store the function code.

            }
            myStack.Clear();
            return finalCode;
        }
        public static int FunctionLength(MyStream sr, string s)
        {
            int count = 0;
            uint curPos = sr.Pos;
            Stack myStack = new Stack();
            s = sr.ReadLine();
            myStack.Push(s);
            bool found = false;
            while ((s != null && myStack.Count > 0))
            {
                count++;
                s = sr.ReadLine();
                if (s.IndexOf("{") != -1)
                {
                    myStack.Push(s);
                }
                if (s.IndexOf("}") != -1)
                {
                    myStack.Pop();
                }

            }
            if (myStack.Count == 0)
            {
                found = true;
            }
            count = count - 1;
            myStack.Clear();
            sr.Seek(curPos);
            return count;

        }
        public static string findFunction(MyStream sr, Regex pattern)
        {
            bool found = false;
            string s = sr.ReadLine();
            while ((!pattern.IsMatch(s)) && ((s = sr.ReadLine()) != null)) ;
            return s;
        }
        public static void findAllFunctionNames(string path, Regex pattern)
        {
            string s = "";
            MyStream sr = new MyStream(path, System.Text.Encoding.UTF8);
            while (s != null)
            {
                s = findFunction(sr, pattern);
                //enter function to where i store it.
                //add it to where i store the function code.
            }
            sr.Close();
        }
        public static string takeSecondNotNullString(string[] str)
        {
            int i;
            string result = "";
            int count = 0;
            for (i = 0; i < str.Length; i++)
            {
                if (str[i] != "" && str[i] != " ")
                {
                    count++;
                }
                if (count == 2)
                {
                    result = str[i];
                    break;
                }
            }
            return result;
        }
        public static ParametersType[] findParameters2(string s)
        {
            string[] tempSplit;
            string[] finalSplit;
            string tempSplit2;
            string finalType;
            int i, j;
            tempSplit = Regex.Split(s, @"\(");
            tempSplit2 = tempSplit[1];
            tempSplit = Regex.Split(tempSplit2, @"\,|\)");
            ParametersType[] finalParameters = new ParametersType[tempSplit.Length - 1];
            char[] charsToTrim = { '*', '&' };
            if (tempSplit2.Length > 2)
            {
                for (i = 0; i < tempSplit.Length - 1; i++)
                {
                    tempSplit2 = tempSplit[i];
                    if (tempSplit2.IndexOf("*") != -1)
                    {
                        finalSplit = Regex.Split(tempSplit2, @"\*");
                    }
                    else
                    {
                        finalSplit = Regex.Split(tempSplit2, @"\s");

                    }

                    if (finalSplit.Length == 1)
                    {
                        tempSplit2 = finalSplit[0];
                    }
                    else
                    {
                        tempSplit2 = takeSecondNotNullString(finalSplit);
                    }
                    if (tempSplit2.IndexOf("&") != -1 || tempSplit2.IndexOf("*") != -1)
                    {
                        tempSplit2 = tempSplit2.Trim(charsToTrim);
                    }
                    //trimEnd
                    tempSplit[i] = tempSplit[i].Substring(0, tempSplit[i].Length - (tempSplit2.Length));
                    finalType = tempSplit[i].Replace(" ", "");
                    tempSplit2 = tempSplit2.Replace(" ", "");
                    finalParameters[i] = new ParametersType(tempSplit2, finalType);

                }
            }
            else
            {
                finalParameters = new ParametersType[0];
            }
            return finalParameters;
        }
        public static string findDocumentation(MyStream sr, uint documentation, string firstLineDocumentation, uint functionPos)
        {
            string documetationString = firstLineDocumentation + "\n\r";
            sr.Seek(documentation);
            string s = sr.ReadLine();
            documetationString += s + "\n\r";
            if (!(firstLineDocumentation.IndexOf("//") != -1) && !(firstLineDocumentation.IndexOf("/*") != -1))
            {
                documetationString = "No documentation for this function";
            }
            if ((firstLineDocumentation.IndexOf("/*") != -1))
            {
                while (!(s.IndexOf("*/") != -1))
                {
                    s = sr.ReadLine();
                    documetationString += s + "\n\r";
                }

            }
            sr.Seek(functionPos);
            return documetationString;

        }
        public static string findAllFunctionNamesAndCode(string path, Regex pattern)
        {
            string s = "";
            string fName;
            string[] temp;
            string returnType = "";
            bool exitFlag = false;
            bool found;
            string firstLineDocumentation = "";
            uint curPos;
            Dictionary<string, FunctionInfoJson> tempDict = new Dictionary<string, FunctionInfoJson>();
            MyStream sr = new MyStream(path, System.Text.Encoding.UTF8);
            uint documentPos = sr.Pos;
            while (s != null)
            {
                while (!exitFlag && !FunctionPatternInC.IsMatch(s))
                {
                    if (s != null)
                    {
                        s = sr.ReadLine();
                    }
                    firstLineDocumentation = "";
                    if (s == null)
                    {
                        exitFlag = true;
                    }
                    if (s.IndexOf("//") != -1)
                    {
                        documentPos = sr.Pos;
                        firstLineDocumentation = s;
                    }
                    while ((s.IndexOf("//") != -1))
                    {
                        if (s != null)
                            s = sr.ReadLine();
                    }
                    if ((s.IndexOf("/*") != -1))
                    {
                        documentPos = sr.Pos;
                        firstLineDocumentation = s;
                        while (!(s.IndexOf("*/") != -1))
                        {
                            if (s != null)
                                s = sr.ReadLine();
                        }
                        if ((s.IndexOf("*/") != -1))
                        {
                            if (s != null)
                                s = sr.ReadLine();
                        }
                    }
                    if (s == null)
                    {
                        exitFlag = true;
                    }
                }
                if (s == null)
                {
                    exitFlag = true;
                }
                if (!exitFlag)
                {
                    fName = s;
                    if (fName != null)
                    {
                        temp = Regex.Split(fName, @"\*|\s");
                        if (fName.IndexOf("static") != -1)
                        {
                            returnType = takeSecondNotNullString(temp);
                        }
                        else
                        {
                            returnType = temp[0];
                        }

                        returnType = returnType.Trim();
                        //enter function to where i store it. 
                        FunctionInfoJson tempStorage = new FunctionInfoJson();
                        tempStorage.content = FunctionCode(sr, ref s);
                        tempStorage.parameters = findParameters2(fName);
                        tempStorage.returnType = returnType;
                        curPos = sr.Pos;
                        tempStorage.documentation = findDocumentation(sr, documentPos, firstLineDocumentation, curPos);
                        tempDict.Add(fName, tempStorage);
                    }
                    else
                    {
                        exitFlag = true;
                    }
                }


                //add it to where i store the function code.
            }
            string finalJson = JsonConvert.SerializeObject(tempDict);
            sr.Close();
            return finalJson;
        }
        public static bool NextScopeLength(MyStream sr, ref string s, ref int count)
        {
            Stack myStack = new Stack();
            s = sr.ReadLine();
            myStack.Push(s);
            bool found = false;
            while ((s != null && myStack.Count > 0))
            {
                count++;
                s = sr.ReadLine();
                if (s.IndexOf("{") != -1)
                {
                    myStack.Push(s);
                }
                if (s.IndexOf("}") != -1)
                {
                    myStack.Pop();
                }

            }
            if (myStack.Count == 0)
            {
                found = true;
            }
            count = count - 1;
            myStack.Clear();
            return found;
        }
        public static bool CheckIfStringInHash(Hashtable a, string s)
        {
            bool found = false;
            char[] trimChars = { '\t', ' ', ';' };
            string result = s.Trim(trimChars);
            if (a.ContainsKey(CreateMD5(result)))
            {
                found = true;
            }
            return found;
        }
        public static int KeywordsAmountOnVariableDeclaration(string s)
        {
            int count = 0;
            int pos = s.IndexOf(' ');
            bool endLoop = false;
            while (pos > 0 && !endLoop)
            {
                if (s.IndexOf('=') != -1)
                {
                    endLoop = true;
                }
                count++;
                if (s.IndexOf('*') != -1)
                    count = count - 1;

                pos = s.IndexOf(' ', pos + 1);
            }
            return count;
        }
        //the checks that are being mad in syntax Check are being written here.
        public static void ChecksInSyntaxCheck(MyStream sr, ref string s, bool IsFunction, int functionLength = 1)
        {
            ArrayList tempStructInFunc = new ArrayList();
            string temp;
            int loopCount;
            char[] cutChars = { '*', '&' };
            int pos = 0;
            bool found;
            int i, j;
            ArrayList results = new ArrayList();
            for (i = 0; i < functionLength; i++)
            {
                if (IsFunction)
                {
                    s = sr.ReadLine();
                }
                pos = 0;
                found = true;
                if (StructPattern.IsMatch(s) || TypedefOneLine.IsMatch(s))
                {
                    results = AddStructNames(sr, s);
                    tempStructInFunc.Add(s);
                }
                if (VariableDecleration.IsMatch(s) && !(s.IndexOf("typedef") != -1))
                {
                    loopCount = KeywordsAmountOnVariableDeclaration(s);
                    for (j = 0; j < loopCount; j++)
                    {

                        found = found && CheckIfStringInHash(keywords, s.Substring(pos, s.Substring(pos, s.Length - pos).IndexOf(' ')).Trim(cutChars));
                        pos = s.IndexOf(' ', pos + 1) + 1;
                    }
                    if (loopCount == 0)
                    {
                        found = found && CheckIfStringInHash(keywords, s.Substring(pos, s.Substring(pos, s.Length - pos).IndexOf(' ')).Trim(cutChars));
                    }
                    if (s.IndexOf("struct") != -1)
                    {
                        pos = s.IndexOf("struct");
                        temp = s.Substring(pos, s.IndexOf(" ", pos + 7) - pos);
                        found = CheckIfStringInHash(keywords, temp.Trim(cutChars));
                    }
                }
                if (!found)
                {
                    Console.WriteLine(s.Trim() + " is written wrong (bad keyword usage).");
                }
            }
            if (IsFunction)
            {
                for (i = 0; i < results.Count; i++)
                {
                    keywords.Remove(results[i]);
                }
            }
        }
        public static void SyntaxCheck(string path)
        {
            MyStream sr = new MyStream(path, System.Text.Encoding.UTF8);
            //in order to delete struct keywords when they come in a function at the end of the function.
            string s;
            int function_Length = 0;
            while ((s = sr.ReadLine()) != null)
            {
                if (FunctionPatternInC.IsMatch(s))
                {
                    function_Length = FunctionLength(sr, s);
                    ChecksInSyntaxCheck(sr, ref s, true, function_Length);
                }
                else
                {
                    ChecksInSyntaxCheck(sr, ref s, false);
                }
            }

        }
        public static void AddToHashFromFile(string path, Hashtable a, string splitBy)
        {
            MyStream sr = new MyStream(path, System.Text.Encoding.UTF8);
            string temp = sr.ReadLine();
            string[] tempArr = Regex.Split(temp, splitBy);
            ICollection keys = keywords.Keys;
            for (int i = 0; i < tempArr.Length; i++)
            {
                a.Add(CreateMD5(tempArr[i]), tempArr[i]);
            }
            sr.Close();
        }
        //path to the code;
        public static void PreprocessorActions(string path, int threadNumber)
        {
            bool endLoop = false;
            MyStream sr = null;
            try
            {
                sr = new MyStream(path, System.Text.Encoding.UTF8);
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Second exception caught.", e);
                endLoop = true;
            }

            string s;
            string firstNewVariableWord;
            string secondNewVariableWord;
            string[] newkeyWords;
            string newKeyword;
            char[] charsToTrim = { '\t', ' ', '*', '&' };
            while (!endLoop && (s = sr.ReadLine()) != null)
            {

                if (DefineDecleration.IsMatch(s))
                {
                    firstNewVariableWord = s.Substring(s.IndexOf(' '), s.Length - s.IndexOf(' '));
                    firstNewVariableWord = firstNewVariableWord.Trim();
                    firstNewVariableWord = firstNewVariableWord.Substring(firstNewVariableWord.IndexOf(' '), firstNewVariableWord.Length - firstNewVariableWord.IndexOf(' '));
                    firstNewVariableWord = firstNewVariableWord.Trim();
                    firstNewVariableWord = firstNewVariableWord.Trim(charsToTrim);
                    if (firstNewVariableWord.IndexOf(" ") != -1)
                    {
                        newkeyWords = Regex.Split(firstNewVariableWord, " ");
                        secondNewVariableWord = newkeyWords[1];
                        firstNewVariableWord = newkeyWords[0];
                        if (CheckIfStringInHash(keywords, firstNewVariableWord) && CheckIfStringInHash(keywords, secondNewVariableWord))
                        {
                            newKeyword = Regex.Split(s, " ")[1];
                            newKeyword = newKeyword.Trim();
                            if (!keywords.ContainsKey(CreateMD5(newKeyword)))
                            {
                                keywords.Add(CreateMD5(newKeyword), newKeyword);
                            }
                        }
                    }
                    else
                    {
                        if (CheckIfStringInHash(keywords, firstNewVariableWord))
                        {
                            newKeyword = Regex.Split(s, " ")[1];
                            newKeyword = newKeyword.Trim();
                            if (!keywords.ContainsKey(CreateMD5(newKeyword)))
                            {
                                keywords.Add(CreateMD5(newKeyword), newKeyword);
                            }
                        }
                    }

                }
                if (StructPattern.IsMatch(s) && threadNumber != 0)
                {
                    AddStructNames(sr, s);
                }
                else if (TypedefOneLine.IsMatch(s) && threadNumber != 0)
                {
                    AddStructNames(sr, s);
                }
                else if (IncludeTrianglesPattern.IsMatch(s) || IncludeRegularPattern.IsMatch(s))
                {
                    string CurrentPath;
                    string result;
                    if (s.IndexOf("<") != -1 && s.IndexOf(">") != -1)
                    {
                        result = CutBetween2Strings(s, "<", ">");
                        CurrentPath = librariesPath;
                    }
                    else
                    {
                        result = CutBetween2Strings(s, "\"", "\"");
                        CurrentPath = projectPath;
                    }
                    Console.WriteLine(result);
                    if (!includes.Contains(CreateMD5(result)))
                    {
                        includes.Add(CreateMD5(result), result);
                        Thread thread;
                        if (result.IndexOf("\\") != -1)
                        {
                            thread = new Thread(() => PreprocessorActions(result, threadNumber + 1));
                        }
                        else
                        {
                            thread = new Thread(() => PreprocessorActions(CurrentPath + "\\" + result, threadNumber + 1));
                        }
                        thread.Start();
                        thread.Join();
                        Console.WriteLine("thread " + threadNumber + "stopped");
                    }
                }

            }
            if (sr != null)
            {
                sr.Close();
            }
        }
        public static string CutBetween2Strings(string s, string first, string second)
        {
            int pFrom = s.IndexOf(first) + first.Length;
            int pTo = s.LastIndexOf(second);

            string result = s.Substring(pFrom, pTo - pFrom);
            result = result.Trim();
            return result;
        }
        public static ArrayList AddStructNames(MyStream sr, string s)
        {
            ArrayList results = new ArrayList();
            int count = 0;
            char[] trimArr = { ' ', '{', '}', ';', '*', '&', '\t' };
            int temp;
            string[] tempSplit;
            string tempString;
            string tempNewVariableName;
            if (s.IndexOf("typedef") != -1)
            {
                if (!TypedefOneLine.IsMatch(s))
                {
                    if (NextScopeLength(sr, ref s, ref count))
                    {
                        s = s.Trim(trimArr);

                        tempSplit = Regex.Split(s, @",");
                        for (int i = 0; i < tempSplit.Length; i++)
                        {
                            tempSplit[i] = tempSplit[i].Trim(trimArr);
                            if (!keywords.ContainsKey(CreateMD5(tempSplit[i])))
                            {
                                keywords.Add(CreateMD5(tempSplit[i]), tempSplit[i]);
                                results.Add(CreateMD5(tempSplit[i]));
                            }

                        }
                    }
                }
                else
                {
                    temp = s.IndexOf(" ") + 1;
                    tempString = tempNewVariableName = s.Substring(temp);
                    tempString = tempString.TrimEnd(' ').Remove(tempString.LastIndexOf(' ') + 1);
                    tempString = tempString.Trim(trimArr);
                    tempNewVariableName = CutBetween2Strings(s, tempString, ";");
                    tempNewVariableName = tempNewVariableName.Trim(trimArr);
                    if (keywords.Contains(CreateMD5(tempString)) && !keywords.Contains(CreateMD5(tempNewVariableName)))
                    {
                        keywords.Add(CreateMD5(tempNewVariableName), tempNewVariableName);
                        results.Add(CreateMD5(tempNewVariableName));
                    }
                }
            }
            else
            {
                s = s.Trim(trimArr);
                if (!keywords.Contains(CreateMD5(s)))
                {
                    keywords.Add(CreateMD5(s), s);
                    results.Add(CreateMD5(s));
                }


            }
            return results;


        }
        public static void CheckUninitializeVariableType(string path)
        {
            MyStream sr = new MyStream(path, System.Text.Encoding.UTF8);
            string s;
            bool endLoop = false;
            string[] structNames;
            while ((s = sr.ReadLine()) != null)
            {
                if (StructPattern.IsMatch(s) || TypedefOneLine.IsMatch(s))
                {
                    AddStructNames(sr, s);
                }

            }
            sr.Close();
        }
        public static void printArrayList(Hashtable a)
        {
            ICollection keys = keywords.Keys;
            foreach (string key in keys)
            {
                Console.WriteLine(a[key]);
            }
        }
        public static void initializeKeywordsAndSyntext(string ansiPath, string cFilePath, string CSyntextPath)
        {
            //ansiC File To Keywords ArrayList.
            AddToHashFromFile(ansiPath, keywords, ",");
            //C Syntext File To Syntext ArrayList.
            //AddToListFromFile(CSyntextPath, syntext, " ");
            PreprocessorActions(cFilePath, 0);
        }
        /*static void Main(string[] args)
        {
            initializeKeywordsAndSyntext(ansiCFile, filePath, CSyntextFile);
            printArrayList(keywords);
            Console.WriteLine(keywords.Count);
            SyntaxCheck(filePath);
            printArrayList(keywords);
            Console.WriteLine(keywords.Count);
            Console.ReadLine();
            //string VarPattern = Console.ReadLine();
            //Regex VarRegexPattern = new Regex(VarPattern);
            //new SyncServer();
        }*/

    }
}
