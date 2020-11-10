using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using Ookii.Dialogs.Wpf;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Windows.Markup;
using ClassesSolution;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //the real gui will not be like that and will have trees (this is just a test for me to see that 2 threads can run together)!
        const string EXIT_LINE = "exit";
        const int FirstThread = 0;
        const int SecondThread = 1;
        static int threadNumber = 0;
        internal static MainWindow main;
        public MainWindow()
        {
            InitializeComponent();
            main = this;
            SqlConnection cnn;
            string connectionString = "Data Source=DESKTOP-L628613\\SQLEXPRESS;Initial Catalog=ToolsDB;User ID=shenhav;Password=1234";
            cnn = new SqlConnection(connectionString);
            cnn.Open();
            SqlCommand command = new SqlCommand("Select tool_name,tool_desc,tool_exe_name from tools_table", cnn);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                
                while (reader.Read())
                {
                    CheckBox temp = new CheckBox();
                    temp.Content= reader["tool_name"].ToString();
                    StackPanelCheckBox.Children.Add(temp);
                }
            }
            Console.WriteLine(cnn);
        }
        private void When_Close_Event(object sender, CancelEventArgs e)
        {
            Thread clientThread;
            clientThread = new Thread(() => ClientConnection.ExecuteClient(EXIT_LINE, threadNumber));
            clientThread.Start();
        }
        public void setTextBlock(string data,int number)
        {
            switch (number)
            {
                case FirstThread:
                    Dispatcher.Invoke(new Action(() => { this.TextBlock1.Text = data; })) ;
                    break;
                case SecondThread:
                    Dispatcher.Invoke(new Action(() => { this.TextBlock2.Text = data; }));
                    break;
                default:
                    break;
            }
        }
        private void ConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            string content = (sender as Button).Name.ToString();
            string path;
            if(content=="Connect")
            {
                path = FileNameTextBox1.Text + ',' + FileNameTextBox2.Text + ',' + FileNameTextBox3.Text + ',' + FileNameTextBox4.Text+','+FileNameTextBoxDest;
                //Connect.IsEnabled = false;
                Console.WriteLine("connect1 pressed.");
            }
            else
            {
                path = FileNameTextBox5.Text + ',' + FileNameTextBox6.Text + ',' + FileNameTextBox7.Text + ',' + FileNameTextBox8.Text + ',' + FileNameTextBoxDest2;
                //Connect3.IsEnabled = false;
                Console.WriteLine("connect3 pressed.");

            }
            string tools = GeneralConsts.EMPTY_STRING;
            foreach(CheckBox tool in StackPanelCheckBox.Children)
            {
                if(tool.IsChecked==true)
                {
                    tools += "," + tool.Content;
                }
            }
            if(tools!= GeneralConsts.EMPTY_STRING)
            {
                path += "," + tools;
            }
            Thread clientThread;
            clientThread = new Thread(() => ClientConnection.ExecuteClient(path,threadNumber));
            clientThread.Start();
            threadNumber++;
        }

        private void BrowseButtonFile_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openFileDlg.ShowDialog();
            string content = (sender as Button).Name.ToString();
            // Get the selected file name and display in a TextBox.
            // Load content of file in a TextBlock
            if (result == true)
            {
                if(content=="Browse1")
                {
                    FileNameTextBox1.Text = openFileDlg.FileName;
                }
                else
                {
                    FileNameTextBox5.Text = openFileDlg.FileName;
                }
                
            }
        }
        private void BrowseButtonFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var folderResult = folderDialog.ShowDialog();
            if (folderResult.HasValue && folderResult.Value)
            {
                string content = (sender as Button).Name.ToString();
                switch (content)
                {
                    case "Browse2":
                        FileNameTextBox2.Text = folderDialog.SelectedPath;
                        break;
                    case "Browse3":
                        FileNameTextBox3.Text = folderDialog.SelectedPath;
                        break;
                    case "Browse4":
                        FileNameTextBox4.Text = folderDialog.SelectedPath;
                        break;
                    case "Browse6":
                        FileNameTextBox6.Text = folderDialog.SelectedPath;
                        break;
                    case "Browse7":
                        FileNameTextBox7.Text = folderDialog.SelectedPath;
                        break;
                    case "Browse8":
                        FileNameTextBox8.Text = folderDialog.SelectedPath;
                        break;
                    default:
                        break;

                }
                
            }
        }


    }
}
