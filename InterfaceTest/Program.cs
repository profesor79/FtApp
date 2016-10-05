using FtApp.Fischertechnik.Txt.Events;
using System;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using TXCommunication;
using TXTCommunication.Fischertechnik;
using TXTCommunication.Fischertechnik.Txt;

namespace InterfaceTest
{
    static class Program
    {
       

        static void Main()
        {


            // before we start ensure that we are connected
            PingTxT();


            var actorSystem = new MasterActor();

            Console.ReadLine();



        }

        private static void PingTxT()
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

      

            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;
            
            bool connected = false;
            while (!connected)
            {
            var reply = pingSender.Send(TxtInterface.ControllerUsbIp, timeout, buffer, options);
                connected = reply.Status == IPStatus.Success;
                Console.WriteLine("Waiting for interface");
                Thread.Sleep(1000);
            }
            
            
        }
    }
}
