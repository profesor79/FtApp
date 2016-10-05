using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using FactoryInterface.Enums;
using FtApp.Fischertechnik.Txt.Events;
using TXTCommunication.Fischertechnik;
using TXTCommunication.Fischertechnik.Txt;

namespace InterfaceTest
{
    public partial class FisherController : ReceiveActor
    {
        private IFtInterface _controller;
        private readonly ILoggingAdapter _log = Context.GetLogger();
        private IActorRef _sortingLineActor;
        private IActorRef _getLineStatusActor;
        private IActorRef _sendProductionStatusActor;
        
        


        public FisherController()
        {
            Receive<StartMessage>(m =>
            {

                SetupController();
                // StartMessage the online mode
                _controller.StartOnlineMode();

                // Configure ports
                SetPorts();

                //starting belt conv
                _controller.SetOutputValue((int) OutputsEnum.BeltConveyor,512);
            });
        }

        private void SetPorts()
        {
            SetInputPorts();
            SetOutputPorts();
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy((exception) => Directive.Stop);
        }

        protected override void PreStart()
        {
            // read config and set elements

            // reset sensor state
            for (var i = 0; i < 8; i++)
            {
                _previousSensorsState[i] = 0;
            }

            CreateActors();

        }

        private void CreateActors()
        {
            _sortingLineActor = Context.ActorOf(Props.Create(typeof(SortingLineActor)), "sortingLineActor");
            //_getLineStatusActor = Context.ActorOf(Props.Create(typeof(FisherController)), "getLineStatusActor");
            //_sendProductionStatusActor = Context.ActorOf(Props.Create(typeof(FisherController)),"sendProductionStatusActor");

        }

        public void Stop()
        {


            // Stop the inline mode
            _controller.StopOnlineMode();

            // Disconnect from the interface
            _controller.Disconnect();

            // Don't forget to dispose
            _controller.Dispose();
            _log.Info("Disconnected...");
            

        }




        private void SetupController()
        {
            _controller = new TxtInterface();

            // Hook events
            HookEvents();

            // Connect to the controller
            _controller.Connect(TxtInterface.ControllerUsbIp);

            // todo: handle this
            if (_controller.Connection != ConnectionStatus.Connected)
            {
                return;
            }



        }

        private void SetOutputPorts()
        {
            _controller.ConfigureOutputMode(7, false);
            _controller.ConfigureOutputMode(6, false);
            _controller.ConfigureOutputMode(5, false);
            _controller.ConfigureOutputMode(4, false);
            _controller.ConfigureOutputMode(3, false);
            _controller.ConfigureOutputMode(2, false);
            _controller.ConfigureOutputMode(1, false);
            _controller.ConfigureOutputMode(0, false);
        }

        private void SetInputPorts()
        {
            _controller.ConfigureInputMode(0, InputMode.ModeR, true);
            _controller.ConfigureInputMode(1, InputMode.ModeU, false);
            _controller.ConfigureInputMode(2, InputMode.ModeR, true);
            _controller.ConfigureInputMode(3, InputMode.ModeR, true);
            _controller.ConfigureInputMode(4, InputMode.ModeR, true);
            _controller.ConfigureInputMode(5, InputMode.ModeR, true);
            _controller.ConfigureInputMode(6, InputMode.ModeR, true);
            _controller.ConfigureInputMode(7, InputMode.ModeR, true);
        }

        private void HookEvents()
        {
            _controller.Connected += (sender, eventArgs) => Console.WriteLine("Connected");
            _controller.Disconnected += (sender, eventArgs) => Console.WriteLine("Disconnected");
            _controller.OnlineStarted += (sender, eventArgs) => Console.WriteLine("Online mode started");
            _controller.OnlineStopped += (sender, eventArgs) => Console.WriteLine("Online mode stopped");
            _controller.InputValueChanged += ControllerOnInputValueChanged;
        }

        private int[] _previousSensorsState = new int[8];

        private void ControllerOnInputValueChanged(object sender, InputValueChangedEventArgs inputValueChangedEventArgs)
        {

            try
            {
                var currentSensorsState = new int[8];

                for (var i = 0; i < 8; i++)
                {
                    currentSensorsState[i] = _controller.GetInputValue(i, 0);
                    
                }

                // now check which one was changed

                for (var i = 0; i < 8; i++)
                {
                    if (currentSensorsState[i] != _previousSensorsState[i])
                    {
                        

                        switch ((InputsEnum)i)
                        {
                            case InputsEnum.BeltConveyorSensorOne:

                                if (currentSensorsState[i] > _previousSensorsState[i])
                                {
                                    _sortingLineActor.Tell(new SortingLineActor.BeltConveyorSensorOneActivated());
                                }
                                else
                                {
                                    _sortingLineActor.Tell(new SortingLineActor.BeltConveyorSensorOneDeactivated());
                                }
                                _log.Debug($"Updated input {i + 1}, from; {_previousSensorsState[i]} to: {currentSensorsState[i]}");
                                break;

                            case InputsEnum.ColorSenor:
                                
                                // firstly rewove noise, so when readind is > 1600 and delta is ~10 then skip it
                                if (currentSensorsState[i] > 1600 &&
                                    Math.Abs(_previousSensorsState[i] - currentSensorsState[i]) < 10)
                                {
                                    break;
                                }

                                _log.Debug($"Updated camera {i + 1}, from; \t{_previousSensorsState[i]}\t to: \t{currentSensorsState[i]}\t");

                                //todo: add to config
                                if (currentSensorsState[i] < 1280) //we have white 
                                {
                                    _log.Debug("White color");
                                    _sortingLineActor.Tell(new SortingLineActor.CameraReadingChanged(currentSensorsState[i]));
                                    break;
                                }




                                if (currentSensorsState[i] > 1280 && currentSensorsState[i] < 1480) //we have red
                                {
                                    _log.Debug("Red color");
                                }



                                
                                
                                break;

                            case InputsEnum.BeltConveyorSensorTwo:
                                if (currentSensorsState[i] > _previousSensorsState[i])
                                {
                                    _sortingLineActor.Tell(new SortingLineActor.BeltConveyorSensorTwoActivated());
                                }
                                else
                                {
                                    _sortingLineActor.Tell(new SortingLineActor.BeltConveyorSensorTwoDeactivated());
                                }

                                _log.Debug($"Updated input {i + 1}, from; {_previousSensorsState[i]} to: {currentSensorsState[i]}");
                                break;

                        }


                        _previousSensorsState[i] = currentSensorsState[i];

                        //_sortingLineActor.Tell("a");
                    }
                }
            }
            catch (Exception e)
            {
                
                _log.Error(e.Message);
            }
            
        }

    }
}
