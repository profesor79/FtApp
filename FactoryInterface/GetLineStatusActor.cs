using Akka.Actor;
using Akka.Event;

namespace FactoryInterface
{
    public class GetLineStatusActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

    }
}
