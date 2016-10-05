using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using FtApp.Fischertechnik.Txt.Events;
using TXTCommunication.Fischertechnik;
using TXTCommunication.Fischertechnik.Txt;

namespace InterfaceTest
{
    public partial class FisherController : ReceiveActor
    {
        private IFtInterface _controller;
        private readonly ILoggingAdapter log = Context.GetLogger();
        public FisherController()
        {
            Receive<StartMessage>(m =>
            {
                Setup();
            });
        }

        public void Stop()
        {


            Console.ReadLine();
            Console.WriteLine("Disconnecting...");

            // Stop the inline mode
            _controller.StopOnlineMode();

            // Disconnect from the interface
            _controller.Disconnect();

            // Don't forget to dispose
            _controller.Dispose();
            Console.WriteLine("Disconnected...");
            Console.ReadLine();

        }

        public void Setup()
        {

            SetupController();
        }



        private void SetupController()
        {
            _controller = new TxtInterface();

            // Hook events
            _controller.Connected += (sender, eventArgs) => Console.WriteLine("Connected");
            _controller.Disconnected += (sender, eventArgs) => Console.WriteLine("Disconnected");
            _controller.OnlineStarted += (sender, eventArgs) => Console.WriteLine("Online mode started");
            _controller.OnlineStopped += (sender, eventArgs) => Console.WriteLine("Online mode stopped");
            _controller.InputValueChanged += ControllerOnInputValueChanged;


            // Connect to the controller
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            //_controller.Connect(TxtInterface.ControllerWifiIp);
            _controller.Connect(TxtInterface.ControllerUsbIp);



            if (_controller.Connection != ConnectionStatus.Connected)
            {
                return;
            }


            // StartMessage the online mode
            _controller.StartOnlineMode();


            // Configure the input ports
            int inputPort = 0;
            _controller.ConfigureInputMode(inputPort++, InputMode.ModeR, true);
            _controller.ConfigureInputMode(inputPort++, InputMode.ModeU, true);
            _controller.ConfigureInputMode(inputPort++, InputMode.ModeR, true);
            _controller.ConfigureInputMode(inputPort++, InputMode.ModeR, true);
            _controller.ConfigureInputMode(inputPort++, InputMode.ModeR, true);
            _controller.ConfigureInputMode(inputPort++, InputMode.ModeR, true);
            _controller.ConfigureInputMode(inputPort++, InputMode.ModeR, true);
            _controller.ConfigureInputMode(inputPort, InputMode.ModeR, true);



            _controller.ConfigureOutputMode(7, false);
            _controller.ConfigureOutputMode(6, false);
            _controller.ConfigureOutputMode(5, false);
            _controller.ConfigureOutputMode(4, false);
            _controller.ConfigureOutputMode(3, false);
            _controller.ConfigureOutputMode(2, false);
            _controller.ConfigureOutputMode(1, false);
            _controller.ConfigureOutputMode(0, false);
        }

        private void ControllerOnInputValueChanged(object sender, InputValueChangedEventArgs inputValueChangedEventArgs)
        {
            //Console.SetCursorPosition(0, Console.CursorTop - 1);


            for (int i = 0; i < _controller.GetInputCount(); i++)
            {
                Console.Write("I{0} {1}  |", i + 1, _controller.GetInputValue(i, 0));
            }

            _controller.SetOutputValue(7, 512);
            //_controller.SetOutputValue(6, 512);
            //_controller.SetOutputValue(5, 512);
            //_controller.SetOutputValue(4, 512);
            //_controller.SetOutputValue(3, 512);
            _controller.SetOutputValue(2, 512);

            _controller.SetOutputValue(1, 512);




            var a = _controller.GetInputValue(0, 0) == 0;

            if (a)
            {
                _controller.SetOutputValue(5, 512);
            }
            else
            {
                _controller.SetOutputValue(5, 0);
            }



            Console.WriteLine();
        }

    }
}
