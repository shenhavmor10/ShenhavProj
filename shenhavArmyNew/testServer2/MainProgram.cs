using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Server;
using Newtonsoft.Json;
using ClassesSolution;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using Microsoft.SqlServer.Server;

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
        const string FINISH_SUCCESFULL = "Finished succesfully code is ready at the destination path.";
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
        /// </summary>
        /// <returns>final json type Dictionary<string,Dictionary<string,Object>> </returns>
        public static Dictionary<string,Dictionary<string,Object>> GetFinalJson()
        {
            return final_json;
        }
        /// Function - RunAllChecks
        /// <summary>
        /// Thread starts all checks.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="pathes"></param>
        public static void RunAllChecks(string filePath,string destPath, string [] pathes,ArrayList tools)
        {
            //variable declaration.
            Hashtable keywords = new Hashtable();
            Hashtable includes = new Hashtable();
            Dictionary<string, string> defines = new Dictionary<string, string>();
            Console.WriteLine(filePath);
            //initialize 
            GeneralCompilerFunctions.initializeKeywordsAndSyntext(ansiCFile, filePath, CSyntextFile, ignoreVariablesTypesPath, keywords, includes, defines, pathes);
            Console.WriteLine(keywords.Count);
            //Syntax Check.
            compileError=GeneralCompilerFunctions.SyntaxCheck(filePath, keywords,threadNumber);
            if(!compileError)
            {
                GeneralCompilerFunctions.printArrayList(keywords);
                Console.WriteLine(keywords.Count);
                //just tests.
                GeneralRestApiServerMethods.CreateFinalJson(filePath, includes, defines, final_json);
                Thread threadOpenTools = new Thread(() => RunAllTasks(filePath, destPath, tools));
                threadOpenTools.Start();
                threadOpenTools.Join();
                ConnectionServer.CloseConnection(threadNumber, FINISH_SUCCESFULL,GeneralConsts.FINISHED_SUCCESFULLY);

            }
        }
        public static void RunAllTasks(string filePath,string destPath,ArrayList tools)
        {
            for (int i = START_INDEX_OF_TOOLS; i < tools.Count; i++)
            {
                tools[i] = toolExeFolder + "\\" + tools[i] + ".exe";
                Console.WriteLine(toolExeFolder + "\\" + tools[i] + ".exe");
            }
            for (int i= START_INDEX_OF_TOOLS; i<tools.Count;i++)
            {
                RunProcessAsync((string)tools[i],filePath,destPath);
            }
        }
        public static Task<int> RunProcessAsync(string fileName,string srcPath,string destPath)
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
            Console.WriteLine("started rest api");
            //Initialize all the things that needs to come before the syntax check.
            Thread serverThread;
            //start server socket.
            serverThread = new Thread(() => Server.ConnectionServer.ExecuteServer(11111));
            serverThread.Start();
            Console.WriteLine("started socket for client listen");
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
                    Console.WriteLine(currentDataList[currentDataList.Count - 1]);
                    string[] paths = Regex.Split((string)currentDataList[currentDataList.Count - 1], ",");
                    string filePath = paths[FILE_PATH_INDEX];
                    Console.WriteLine(filePath);
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
                    Thread.Sleep(1000);
                }
                
            }
            
            
        }
    }
}
