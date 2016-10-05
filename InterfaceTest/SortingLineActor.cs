using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;

namespace InterfaceTest
{
    public partial class SortingLineActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        public SortingLineActor()
        {
            Receive<object>(x => { });
        }
    }
}
