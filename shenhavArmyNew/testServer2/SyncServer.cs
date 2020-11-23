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
        const string NOT_FOUND_500 = "Could not found pattern";
        const string NOT_FOUND_404 = "Not found";
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
                    Dictionary<string, Dictionary<string, object>> final_json = MainProgram.GetFinalJson();
                    string filePath = context.Request.QueryString["filePath"];
                    bool not_found_pattern = false;
                    bool not_found = false;
                    char[] trimChars = { '/', ' '};
                    int totalTime = 0;
                    string path = GeneralConsts.EMPTY_STRING;
                    Regex r;
                    //All GET commands.
                    if (context.Request.HttpMethod == "GET")
                    {
                        if(context.Request.QueryString["pattern"]!=null)
                        {
                            MainProgram.AddToLogString(filePath,context.Request.QueryString["pattern"]);
                            MainProgram.AddToLogString(filePath, context.Request.QueryString["returnSize"]);
                            r = new Regex(context.Request.QueryString["pattern"]);
                            string returnSize = context.Request.QueryString["returnSize"];
                            string [] result=GeneralRestApiServerMethods.SearchPattern(r, returnSize, filePath);
                            dataJson = JsonConvert.SerializeObject(result);
                            MainProgram.AddToLogString(filePath, dataJson);
                        }
                        else if(context.Request.QueryString["readyPattern"] != null)
                        {
                            MainProgram.AddToLogString(filePath, context.Request.QueryString["readyPattern"]);
                            MainProgram.AddToLogString(filePath, context.Request.QueryString["returnSize"]);
                            if (GeneralRestApiServerMethods.TakePatternFromFile(context.Request.QueryString["readyPattern"]) == GeneralConsts.EMPTY_STRING)
                                not_found_pattern = true;
                            else
                            {
                                r = new Regex(GeneralRestApiServerMethods.TakePatternFromFile(context.Request.QueryString["readyPattern"]));
                                string returnSize = context.Request.QueryString["returnSize"];
                                string[] result = GeneralRestApiServerMethods.SearchPattern(r, returnSize, filePath);
                                dataJson = JsonConvert.SerializeObject(result);
                            }
                        }
                        else
                        {
                            path = context.Request.RawUrl;
                            path = path.Trim(trimChars);
                            path = path.Split('?')[0];
                            MainProgram.AddToLogString(filePath, path);
                            //switch case for get commands.
                            switch (path)
                            {
                                case "functions":
                                    dataJson = JsonConvert.SerializeObject(final_json[filePath]["function"]);
                                    MainProgram.AddToLogString(filePath, dataJson);
                                    break;
                                case "codeInfo":
                                    dataJson = JsonConvert.SerializeObject(final_json[filePath]["codeInfo"]);
                                    MainProgram.AddToLogString(filePath, dataJson);
                                    break;
                                default:
                                    break;

                            }
                        }
                        byte [] bytes;
                        if (not_found)
                        {
                            bytes= Encoding.UTF8.GetBytes(NOT_FOUND_500);
                            context.Response.StatusCode = 500;
                        }
                        else
                        {
                            if (dataJson.Length < 0)
                            {
                                bytes = Encoding.UTF8.GetBytes(NOT_FOUND_404);
                                context.Response.StatusCode = 404;
                            }
                            else
                            {
                                bytes = Encoding.UTF8.GetBytes(dataJson);
                            }
                        }
                        
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
