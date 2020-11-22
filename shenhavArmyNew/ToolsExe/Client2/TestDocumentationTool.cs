using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using ClassesSolution;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Text;

namespace Client
{
    
    class DocumentationTool
    {
        //This whole project is for testing a "tool".
        //Server info declaration.
        const int PORT_NO = 5000;
        const string SERVER_IP = "127.0.0.1";
        /// Function - Main
        /// <summary>
        /// Handles the info recieving from the rest api server (Platform).
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string createParameters(ParametersType [] parameters)
        {
            string documentation = GeneralConsts.EMPTY_STRING;
            Console.WriteLine("length" + parameters.Length); ;
            for(int i=0;i<parameters.Length;i++)
            {
                documentation += "* " + parameters[i].parameterName + " - \r\n";
            }
            return documentation;
        }
        
        static async Task GetFromRestApi(string sourcePath,string destPath,string documentationPath)
        {
            //Communicating with rest api server
            Console.WriteLine("entered ");
            HttpClient client = new HttpClient();
            string regexPattern = GeneralConsts.EMPTY_STRING;
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //Functions GET.
            Console.WriteLine("before async");
            
            HttpResponseMessage response = await client.GetAsync(string.Format("http://127.0.0.1:8081?filePath={0}&readyPattern={1}&returnSize={2}", sourcePath, "StructPattern", "scope"));
            //HttpResponseMessage response = await client.GetAsync(string.Format("http://127.0.0.1:8081/functions?filePath={0}",sourcePath);
            Console.WriteLine("after async");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            //Deserialize.
            //Dictionary<string, FunctionInfoJson> dict = JsonConvert.DeserializeObject<Dictionary<string, FunctionInfoJson>>(responseBody);
            string[] arr = JsonConvert.DeserializeObject<string[]>(responseBody);
            Console.WriteLine(arr[0]);
            Console.Read();
            //Checking if it works (it does).
            /*Console.WriteLine(dict["static int* main(int* podd, int** odpdf, char a, char* retval)"].documentation);
            string documentationTemplate = new MyStream(documentationPath, System.Text.Encoding.UTF8).ReadToEnd();
            string tempDocumentation = GeneralConsts.EMPTY_STRING;
            MyStream myStream = new MyStream(sourcePath, System.Text.Encoding.UTF8);
            StreamWriter destStream = new StreamWriter(Path.Combine(destPath, "newFile.c"),false);
            destStream.AutoFlush = true;
            string newFile=myStream.ReadToEnd();
            foreach (string key in dict.Keys)
            {
                regexPattern = GeneralConsts.EMPTY_STRING;
                Console.WriteLine(key);
                ParametersType[] parameters = (ParametersType[])dict[key].parameters;
                for (int i=0;i<parameters.Length;i++)
                {
                    Console.WriteLine(parameters[i].parameterName);
                }
                regexPattern += @"(?s).*\@params.*\n";
                for (int i = 0; i < parameters.Length; i++)
                {
                    regexPattern += @".*" + parameters[i].parameterName + @".*\n";
                }
                regexPattern += @".*\@returns.*\n.*";
                Regex documentation = new Regex(regexPattern);
                if (!documentation.IsMatch(dict[key].documentation))
                {
                    tempDocumentation = createParameters(parameters);
                    string newDocumentation = documentationTemplate;
                    newDocumentation = newDocumentation.Replace("{0}", tempDocumentation);
                    newDocumentation = newDocumentation.Replace("{1}", "type - " + dict[key].returnType);
                    if(dict[key].documentation!= GeneralConsts.EMPTY_STRING)
                    {
                        newFile = newFile.Replace(dict[key].documentation, newDocumentation);
                    }
                    else
                    {
                        newFile = newFile.Replace(key, newDocumentation + "\r\n" + key);
                    }
                    


                }
                Console.WriteLine(dict[key].documentation);


            }
            destStream.Write(newFile);
            destStream.Close();
            /*Console.WriteLine(dict["void spoi()"].documentation);
            //Code Info GET.
            response = await client.GetAsync("http://127.0.0.1:8081/codeInfo");
            response.EnsureSuccessStatusCode();
            responseBody = await response.Content.ReadAsStringAsync();
            //Deserialize.
            CodeInfoJson code = JsonConvert.DeserializeObject<CodeInfoJson>(responseBody);
            //Checking if it works (it does).
            Console.WriteLine(code.definesAmount);
            Console.ReadLine();*/
            
        }
        static async Task Main(string[] args)
        {
            Console.WriteLine(args[0]+"\n"+args[1]);
            string destPath = args[1];
            string sourcePath = args[0];//.Split(' ')[0];
            //string destPath = args[1].Split(' ')[1];
            //Console.WriteLine(destPath);
            string documentationPath = Path.GetFullPath(Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"..\..\"));
            documentationPath += "Documentation.txt";
            await GetFromRestApi(sourcePath, destPath, documentationPath);
        }
    }
}
