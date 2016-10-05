using Akka.Actor;
using Akka.Event;

namespace FactoryInterface
{
    public partial class SortingLineActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        public SortingLineActor()
        {

            Become(WaitingForMaterial);
            Receive<object>(x => { });


        }

        private void WaitingForMaterial()
        {


            Receive<BeltConveyorSensorOneActivated>(m =>
            {
                // start belt conveyor

            });


            ReceiveAny(x =>
            {
                _log.Warning($"WaitingForMaterial: Received message {nameof(x)}");
            });
            
        }
    }
}
