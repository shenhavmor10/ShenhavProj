using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Server;
using ClassesSolution;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using System.IO;

namespace testServer2
{
    class MainProgram
    {
        const int FILE_PATH_INDEX = 0;
        const int START_TOOLS_INDEX = 5;
        const int DEST_PATH_INDEX = 4;
        const int GCC_INCLUDE_FOLDER_INDEX = 2;
        const int EXTRA_INCLUDE_FOLDER_INDEX = 3;
        const int PROJECT_FOLDER_INDEX = 1;
        const int START_INDEX_OF_TOOLS = 0;
        //paths for all files.
        const string toolExeFolder = @"..\..\..\ToolsExe";
        const string ignoreVariablesTypesPath = @"..\..\..\ignoreVariablesType.txt";
        //static string filePath = @"C:\Users\Shenhav\Desktop\Check\checkOne.c";
        const string ansiCFile = @"..\..\..\Ansikeywords.txt";
        const string CSyntextFile = @"..\..\..\CSyntext.txt";
        const string logFile = @"..\..\..\LogFile.txt";
        const string FINISH_SUCCESFULL = "Finished succesfully code is ready at the destination path.";
        
        const int TIMEOUT_SLEEP = 1000;
        static string logs = GeneralConsts.EMPTY_STRING;
        static bool compileError = false;
        static ArrayList currentDataList = new ArrayList();
        static int threadNumber = 0;
        static Dictionary<string, Dictionary<string, Object>> final_json = new Dictionary<string, Dictionary<string, Object>>();
        //static string librariesPath = @"C:\Users\Shenhav\Desktop\Check";
        //global variable declaration.

        //static ArrayList syntext = new ArrayList(); dont know if needed.

        /// Function - GetFinalJson
        /// <summary>
        /// Function returns the final json.
        /// final json is the json that is being created after the compilation checks, this json contains
        /// all information of the code and the rest api is prasing the information to the GET requests.
        /// </summary>
        /// <returns>final json type Dictionary<string,Dictionary<string,Object>> </returns>
        public static Dictionary<string,Dictionary<string,Object>> GetFinalJson()
        {
            return final_json;
        }
        /// Function - AddToLogString
        /// <summary>
        /// logs is the string that saves all logs and at the end it writes it to the logs txt.
        /// </summary>
        /// <param name="content">what you add to the string</param>
        public static void AddToLogString(string content)
        {
            logs += content + GeneralConsts.NEW_LINE;
        }
        /// Function - RunAllChecks
        /// <summary>
        /// Thread starts all checks.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="pathes"></param>
        static void RunAllChecks(string filePath,string destPath, string [] pathes,ArrayList tools)
        {
            //variable declaration.
            Hashtable keywords = new Hashtable();
            Hashtable includes = new Hashtable();
            Dictionary<string, string> defines = new Dictionary<string, string>();
            Dictionary<string, ArrayList> funcVariables = new Dictionary<string, ArrayList>();
            ArrayList globalVariable = new ArrayList();
            //initialize 
            try
            {
                GeneralCompilerFunctions.initializeKeywordsAndSyntext(ansiCFile, filePath, CSyntextFile, ignoreVariablesTypesPath, keywords, includes, defines, pathes);
            }
            catch(Exception e)
            {
                AddToLogString("ERROR IN PREPROCESSOR");
                ConnectionServer.CloseConnection(threadNumber,"ERROR IN PREPROCESSOR "+e.ToString() , GeneralConsts.ERROR);

            }
            
            AddToLogString(keywords.Count.ToString());
            //Syntax Check.
            try
            {
                compileError = GeneralCompilerFunctions.SyntaxCheck(filePath, globalVariable, keywords, funcVariables, threadNumber);
            }
            catch(Exception e)
            {
                AddToLogString("ERROR IN SyntaxCheck");
                ConnectionServer.CloseConnection(threadNumber, "ERROR IN SyntaxCheck " + e.ToString(), GeneralConsts.ERROR);
            }
            
            if(!compileError)
            {
                GeneralCompilerFunctions.printArrayList(keywords);
                AddToLogString(keywords.Count.ToString());
                //just tests.
                try
                {
                    GeneralRestApiServerMethods.CreateFinalJson(filePath, includes, globalVariable, funcVariables, defines, final_json);
                }
                catch (Exception e)
                {
                    AddToLogString("ERROR Creating final json");
                    ConnectionServer.CloseConnection(threadNumber, "ERROR Creating final json " + e.ToString(), GeneralConsts.ERROR);
                }
               
                string dataJson = JsonConvert.SerializeObject(final_json[filePath]["codeInfo"]);
                AddToLogString("new json "+dataJson);
                Thread threadOpenTools = new Thread(() => RunAllTasks(filePath, destPath, tools));
                threadOpenTools.Start();
                threadOpenTools.Join(GeneralConsts.TIMEOUT_JOIN);
                ConnectionServer.CloseConnection(threadNumber, FINISH_SUCCESFULL,GeneralConsts.FINISHED_SUCCESFULLY);

            }

        }
        /// Function - RunAllTasks
        /// <summary>
        /// runs all tools picked by the client by the order.
        /// </summary>
        /// <param name="filePath"> the path of the file.</param>
        /// <param name="destPath"> the path of the destionation.</param>
        /// <param name="tools"> The array of the tools sorted from low to high priority.</param>
        static void RunAllTasks(string filePath,string destPath,ArrayList tools)
        {
            //runs on all tools recieved and adds to them .exe
            for (int i = START_INDEX_OF_TOOLS; i < tools.Count; i++)
            {
                tools[i] = File.ReadAllText(toolExeFolder + "\\" + tools[i]);
            }
            //runs the tools one by one.
            for (int i= START_INDEX_OF_TOOLS; i<tools.Count;i++)
            {
                RunProcessAsync((string)tools[i],filePath,destPath);
            }
        }
        /// Function - RunProcessAsync
        /// <summary>
        /// starts a tool (task) and sends him 2 parameters src path and dest path.
        /// </summary>
        /// <param name="fileName"> name of the file.</param>
        /// <param name="srcPath"> the source path of the file</param>
        /// <param name="destPath"> the destination of the new file.</param>
        /// <returns></returns>
        static Task<int> RunProcessAsync(string fileName,string srcPath,string destPath)
        {
            var tcs = new TaskCompletionSource<int>();

            var process = new Process
            {
                StartInfo = { FileName = fileName, Arguments = String.Format("{0} {1}",srcPath,destPath) },
                EnableRaisingEvents = true
            };

            process.Exited += (sender, args) =>
            {
                tcs.SetResult(process.ExitCode);
                process.Dispose();
            };

            process.Start();
            //process.WaitForExit(); might need for synchronize.
            return tcs.Task;
        }
        static void Main(string[] args)
        {
            //open Rest API.
            Thread restApi = new Thread(()=>new SyncServer());
            restApi.Start();
            AddToLogString("started rest api");
            //Initialize all the things that needs to come before the syntax check.
            Thread serverThread;
            //start server socket.
            serverThread = new Thread(() => Server.ConnectionServer.ExecuteServer(11111));
            serverThread.Start();
            AddToLogString("started socket for client listen");
            while(ConnectionServer.GetCloseAllBool()==false)
            {
                //checks if something got added to the server list by the gui. if it did 
                //it copies it to the main current list and start to run all the checks on the paths
                //got by the gui (the data inside the List is the user paths.).
                ArrayList list = Server.ConnectionServer.GetThreadsData();
                if (list.Count > currentDataList.Count)
                {
                    //adds to the current data list the original server data list last node.
                    ArrayList tools = new ArrayList();
                    currentDataList.Add(list[currentDataList.Count]);
                    AddToLogString(currentDataList[currentDataList.Count - 1].ToString());
                    string[] paths = Regex.Split((string)currentDataList[currentDataList.Count - 1], ",");
                    string filePath = paths[FILE_PATH_INDEX];
                    AddToLogString(filePath);
                    string[] pathes = { paths[PROJECT_FOLDER_INDEX], paths[GCC_INCLUDE_FOLDER_INDEX], paths[EXTRA_INCLUDE_FOLDER_INDEX] };
                    string destPath = paths[DEST_PATH_INDEX];
                    for(int i=START_TOOLS_INDEX; i<paths.Length;i++)
                    {
                        tools.Add(paths[i]);
                    }
                    Thread runChecksThread = new Thread(() => RunAllChecks(filePath,destPath, pathes,tools));
                    runChecksThread.Start();
                    
                }
                else
                {
                    Thread.Sleep(TIMEOUT_SLEEP);
                }
                
            }
            File.WriteAllText(logFile, logs);
            
            
            
        }
    }
}
