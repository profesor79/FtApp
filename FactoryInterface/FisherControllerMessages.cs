using FactoryInterface.Enums;

namespace FactoryInterface
{
public  partial  class FisherController
    {
        public class StartMessage
        {

        }

        public class StartBeltConveyorMessage
        {

        }

        public class StartCommpressor
        {

        }
        public class StopCommpressor
        {

        }

        public class StopBeltConveyorMessage
        {

        }


        public class StartPusher
        {
            public OutputsEnum Pusher { get; }
            public StartPusher(OutputsEnum pusher)
            {
                Pusher = pusher;
            }
        }


        public class StopPusher
        {
            public OutputsEnum Pusher { get; }
            public StopPusher(OutputsEnum pusher)
            {
                Pusher = pusher;
            }
        }


    }
}
