using System;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using ClassesSolution;
using Server;

namespace testServer2
{
    public class SyncServer
    {
        //Patterns Declaration.
        static Regex FunctionPatternInC = new Regex(@"^([^ ]+\s)?[^ ]+\s(.*\s)?[^ ]+\([^()]*\)$");
        static Regex functionPatternInH = new Regex(@"^[a-zA-Z]+.*\s[a-zA-Z].*[(].*[)]\;$");
        static Regex staticFunctionPatternInC = new Regex(@"^.*static.*\s.*[a-zA-Z]+.*\s[a-zA-Z].*[(].*[)]$");
        /// Function - SyncServer
        /// <summary>
        /// Creation of the rest api server.
        /// </summary>
        /// <param name="filePath"> Path for the code file.</param>
        /// <param name="includes"> Hashtable for all of the includes in the code.</param>
        /// <param name="defines"> Dictionary that stores all of the defines in the code.</param>
        public SyncServer()
        {
            var listener = new HttpListener();
            //add prefixes.
            listener.Prefixes.Add("http://localhost:8081/");
            listener.Prefixes.Add("http://127.0.0.1:8081/");
            //start listening.
            listener.Start();

            while (ConnectionServer.GetCloseAllBool()==false)
            {
                try
                {
                    //if gets connection.
                    var context = listener.GetContext(); //Block until a connection comes in
                    context.Response.StatusCode = 200;
                    context.Response.SendChunked = true;
                    context.Response.ContentType = "application/json";
                    string dataJson = GeneralConsts.EMPTY_STRING;
                    Console.WriteLine(context.Request.Headers);
                    Dictionary<string, Dictionary<string, object>> final_json = MainProgram.GetFinalJson();
                    string filePath = context.Request.QueryString["filePath"];
                    Console.WriteLine("filePath  = "+filePath);
                    char[] trimChars = { '/', ' '};
                    int totalTime = 0;
                    string path = GeneralConsts.EMPTY_STRING;
                    //All GET commands.
                    if (context.Request.HttpMethod == "GET")
                    {
                        if(context.Request.QueryString["pattern"]!=null)
                        {
                            Console.WriteLine(context.Request.QueryString["pattern"]);
                            Console.WriteLine(context.Request.QueryString["returnSize"]);
                            Regex r = new Regex(context.Request.QueryString["pattern"]);
                            string retrunSize = context.Request.QueryString["returnSize"];
                            string [] result=GeneralRestApiServerMethods.SearchPattern(r, retrunSize, filePath);
                            dataJson = JsonConvert.SerializeObject(result);
                            Console.WriteLine(dataJson);
                        }
                        else
                        {
                            path = context.Request.RawUrl;
                            path = path.Trim(trimChars);
                            path = path.Split('?')[0];
                            Console.WriteLine(path);
                            Console.WriteLine(path);
                            //switch case for get commands.
                            switch (path)
                            {
                                case "functions":
                                    Console.WriteLine("dddd");
                                    dataJson = JsonConvert.SerializeObject(final_json[filePath]["function"]);
                                    Console.WriteLine(dataJson);
                                    break;
                                case "codeInfo":
                                    dataJson = JsonConvert.SerializeObject(final_json[filePath]["codeInfo"]);
                                    Console.WriteLine(dataJson);
                                    break;
                                default:
                                    Console.WriteLine();
                                    break;

                            }
                        }
                        var bytes = Encoding.UTF8.GetBytes(dataJson);
                        Stream OutputStream = context.Response.OutputStream;
                        //sends the message back.
                        OutputStream.Write(bytes, 0, bytes.Length);
                        //Close connection.
                        OutputStream.Close();

                    }
                }


                catch (Exception)
                {
                    // Client disconnected or some other error - ignored for this example
                }
            }
        }
        
    }
}
