using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using ClassesSolution;
using System.Text.RegularExpressions;
using System.IO;

namespace Client
{
    
    class DocumentationTool
    {
        static string documentationPath = @"..\..\..\Documentation.txt";
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
            for(int i=0;i<parameters.Length;i++)
            {
                documentation += "* " + parameters[i].parameterName + " - \n";
            }
            return documentation;
        }
        
        static async Task GetFromRestApi(string sourcePath,string destPath)
        {
            //Communicating with rest api server
            HttpClient client = new HttpClient();
            string regexPattern = GeneralConsts.EMPTY_STRING;
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //Functions GET.
            HttpResponseMessage response = await client.GetAsync(string.Format("http://127.0.0.1:8081/functions?filePath={0}",sourcePath));
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            //Deserialize.
            Dictionary<string, FunctionInfoJson> dict = JsonConvert.DeserializeObject<Dictionary<string, FunctionInfoJson>>(responseBody);
            //Checking if it works (it does).
            string documentationTemplate = new MyStream(documentationPath, System.Text.Encoding.UTF8).ReadToEnd();
            string tempDocumentation = GeneralConsts.EMPTY_STRING;
            MyStream myStream = new MyStream(sourcePath, System.Text.Encoding.UTF8);
            StreamWriter destStream = new StreamWriter(destPath);
            string newFile=myStream.ReadToEnd();
            foreach (string key in dict.Keys)
            {
                ParametersType[] parameters = (ParametersType[])dict[key].parameters;
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
                    string newDocumentation = string.Format(tempDocumentation, tempDocumentation);
                    if(dict[key].documentation!= GeneralConsts.EMPTY_STRING)
                    {
                        newFile.Replace(dict[key].documentation, newDocumentation);
                    }
                    else
                    {
                        newFile.Replace(key, newDocumentation + '\n' + key);
                    }
                    Console.WriteLine(dict[key].documentation);


                }


            }
            destStream.Write(newFile);
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
        static void Main(string[] args)
        {
            string destPath = args[0];
            string sourcePath = args[1];//.Split(' ')[0];
            //string destPath = args[1].Split(' ')[1];
            //Console.WriteLine(destPath);
            Task task=GetFromRestApi(sourcePath, destPath);
            task.Start();
        }
    }
}
