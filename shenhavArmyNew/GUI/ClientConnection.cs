// A C# program for Client 
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace GUI
{

    class ClientConnection
    {
        public static int sendMessage(string data,Socket sender)
        {
            byte[] messageSent;
            messageSent = Encoding.ASCII.GetBytes(data);
            int byteSent = sender.Send(messageSent);
            // Data buffer 
            
            return byteSent;
        }
        public static string recieveMessage(Socket sender)
        {
            byte[] messageReceived = new byte[1024];
            int byteRecv = sender.Receive(messageReceived);
            string recieve=Encoding.ASCII.GetString(messageReceived,
                                             0, byteRecv);
            return recieve;
        }
        public static void ExecuteClient(string data,int number)
        {
            Socket sender = null;
            try
            {

                // Establish the remote endpoint  
                // for the socket. This example  
                // uses port 11111 on the local  
                // computer. 
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddr = ipHost.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddr,11111);

                // Creation TCP/IP Socket using  
                // Socket Class Costructor 
                sender = new Socket(ipAddr.AddressFamily,
                           SocketType.Stream, ProtocolType.Tcp);

                try
                {

                    // Connect Socket to the remote  
                    // endpoint using method Connect() 
                    sender.Connect(localEndPoint);

                    // We print EndPoint information  
                    // that we are connected 
                    Console.WriteLine("Socket connected to -> {0} ",
                                  sender.RemoteEndPoint.ToString());

                    // Creation of messagge that 
                    // we will send to Server 
                    sendMessage(data, sender);
                    if(data!="exit")
                    {
                        string newData = recieveMessage(sender);
                        //Change the textBlock to the error or final path depends if the code is good or bad.
                        MainWindow.main.setTextBlock(newData, number);
                    }
                    else
                    {
                        recieveMessage(sender);
                    }
                    sender.Close();
                    // We receive the messagge using  
                    // the method Receive(). This  
                    // method returns number of bytes 
                    // received, that we'll use to  
                    // convert them to string 
                    

                    // Close Socket using  
                    // the method Close() 
                    //sender.Shutdown(SocketShutdown.Both);
                    //sender.Close();
                }

                // Manage of Socket's Exceptions 
                catch (ArgumentNullException ane)
                {

                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }

                catch (SocketException se)
                {

                    Console.WriteLine("SocketException : {0}", se.ToString());
                }

                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }
            }

            catch (Exception e)
            {

                Console.WriteLine(e.ToString());
            }
            
        }
        // Main Method 
        /*public static void Main(string[] args)
        {
            ExecuteClient("kfkfkf");
        }*/
    }
}