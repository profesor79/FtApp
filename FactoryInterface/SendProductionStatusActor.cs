using Akka.Actor;
using Akka.Event;

namespace FactoryInterface
{
    public class SendProductionStatusActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        protected override void PreStart()
        {
            // read config and set elements


        }
    }
}
