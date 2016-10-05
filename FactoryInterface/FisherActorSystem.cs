using System;
using Akka.Actor;

namespace FactoryInterface
{
   public class FisherActorSystem

    {
        public ActorSystem FisherSystem { get; }

        public FisherActorSystem()
        {
            FisherSystem = ActorSystem.Create("fisher");
         
        }


        public void Start()
        {
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
