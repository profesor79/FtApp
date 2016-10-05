using FtApp.Fischertechnik.Txt.Events;
using System;
using System.Diagnostics;
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


        static  void Main()
        {


            // before we start ensure that we are connected
            PingTxT();

            // create and start actor system
            var actorSystem = new FisherActorSystem();
            actorSystem.Start();

            // wait for termination (keep the window open)
            while (!actorSystem.FisherSystem.WhenTerminated.IsCompleted)
            {
                Thread.Sleep(5000);
            }
            
            Console.WriteLine("ActorSystem terminated - exiting");
        }

        private static void PingTxT()
        {
            var pingSender = new Ping();
            var options = new PingOptions();

            // Create a buffer of 32 bytes of data to be transmitted.
            var data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            var buffer = Encoding.ASCII.GetBytes(data);
            var timeout = 120;

            var connected = false;
            while (!connected)
            {
                try
                {
                    Thread.Sleep(1000);
                    Console.WriteLine("Waiting for connection");
                    var reply = pingSender.Send(TxtInterface.ControllerUsbIp, timeout, buffer, options);
                    // ReSharper disable once PossibleNullReferenceException
                    connected = reply.Status == IPStatus.Success;
                }
                catch (Exception)
                {
                    Console.WriteLine("Waiting for connection");
                    Thread.Sleep(2000);
                }
                
            }
            
            
        }
    }
}
