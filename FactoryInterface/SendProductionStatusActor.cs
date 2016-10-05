using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;

namespace InterfaceTest
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
