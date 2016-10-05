using FtApp.Fischertechnik.Txt.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using TXTCommunication.Fischertechnik;
using TXTCommunication.Fischertechnik.Txt;

namespace InterfaceTest
{
   public class FisherActorSystem

    {
        public ActorSystem FisherSystem { get; }

        public FisherActorSystem()
        {
            FisherSystem = ActorSystem.Create("fisher");
             _fisherController = FisherSystem.ActorOf(Props.Create(typeof(FisherController)), "FisherController");
            _fisherController.Tell(new FisherController.StartMessage());
        }

        

        private IActorRef _fisherController;

        public void Run()
        {
            throw new NotImplementedException();
        }
    }
}
