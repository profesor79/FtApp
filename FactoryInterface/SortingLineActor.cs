using System;
using System.Threading;
using Akka.Actor;
using Akka.Event;
using FactoryInterface.Enums;
using Newtonsoft.Json;

namespace FactoryInterface
{
    public partial class SortingLineActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private ColorEnum _colorValue;
        private bool _colorRead;

        public SortingLineActor()
        {

            Become(WaitingForMaterial);
            
        }

        private void WaitingForMaterial()
        {


            Receive<BeltConveyorSensorOneActivated>(m =>
            {
                // start belt conveyor
                Context.Parent.Tell(new FisherController.StartBeltConveyorMessage());
                _colorRead = false;
                Become(ColorReading);

                //set default blue  color value
                _colorValue = ColorEnum.Blue;
            });


            ReceiveAny(x =>
            {
                _log.Warning($"WaitingForMaterial: Received message {JsonConvert.SerializeObject(x)}");
            });

        }

        private void ColorReading()
        {

            Receive<CameraReadingChanged>(c =>
            {
                if (!_colorRead)
                {
                    _colorValue = c.ColorValue;
                    _colorRead = true;
                }
            });

            Receive<BeltConveyorSensorTwoActivated>(c =>
            {

                Context.Parent.Tell(new FisherController.StartCommpressor());
                Become(Ejecting);

            });

            ReceiveAny(x =>
            {
                _log.Warning($"ColorReading: Received message {JsonConvert.SerializeObject(x)}");

            });

        }

        private void Ejecting()
        {

            Receive<BeltConveyorSensorTwoDeactivated>(d =>
            {
                switch (_colorValue)
                {
                    case ColorEnum.White:
                        Thread.Sleep(200);
                        Context.Parent.Tell(new FisherController.StartPusher(OutputsEnum.BayOnePusher));
                        Thread.Sleep(1200);
                        Context.Parent.Tell(new FisherController.StopPusher(OutputsEnum.BayOnePusher));
                        break;
                    case ColorEnum.Red:
                        Thread.Sleep(1500);
                        Context.Parent.Tell(new FisherController.StartPusher(OutputsEnum.BayTwoPusher));
                        Thread.Sleep(1200);
                        Context.Parent.Tell(new FisherController.StopPusher(OutputsEnum.BayTwoPusher));
                        break;
                    case ColorEnum.Blue:
                        Thread.Sleep(2800);
                        Context.Parent.Tell(new FisherController.StartPusher(OutputsEnum.BayThreePusher));
                        Thread.Sleep(1200);
                        Context.Parent.Tell(new FisherController.StopPusher(OutputsEnum.BayThreePusher));
                        break;

                }

                Context.Parent.Tell(new FisherController.StopCommpressor());
                Context.Parent.Tell(new FisherController.StopBeltConveyorMessage());

                Become(WaitingForMaterial);

            });

            ReceiveAny(x =>
            {
                _log.Warning($"Ejecting: Received message {JsonConvert.SerializeObject(x)}");
            });

        }
    }
}
