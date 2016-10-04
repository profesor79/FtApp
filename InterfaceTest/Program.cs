using FtApp.Fischertechnik.Txt.Events;
using System;
using TXCommunication;
using TXTCommunication.Fischertechnik;
using TXTCommunication.Fischertechnik.Txt;

namespace InterfaceTest
{
    static class Program
    {
       

        static void Main()
        {
            var actorSystem = new ActorSystemFisher();
            actorSystem.Setup();
            actorSystem.Run();





        }



      
    }
}
