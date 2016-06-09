﻿using System;
using System.Collections.Generic;
using System.Timers;
using FtApp.Fischertechnik;
using FtApp.Fischertechnik.Txt.Events;
using TXCommunication.Packets;
using TXTCommunication.Fischertechnik;
using TXTCommunication.Fischertechnik.Txt;

namespace TXCommunication
{
    class TxInterface : IFtInterface
    {
        public bool IsDebugEnabled { get; set; }

        #region Constants

        public const int DefaultUpdateInterval = 50;
        
        public const int PwmMaxValue = 512;

        public const int PwmOutputs = 8;
        public const int MotorOutputs = 4;
        public const int Counters = 4;
        public const int UniversalInputs = 8;

        #endregion

        private readonly FtExtension _masterInterface;

        private TxCommunication TxCommunication { get; set; }

        public int UpdateInterval { get; set; } = DefaultUpdateInterval;

        public string Mac { get; private set; }

        private bool _configurationChanged;
        

        private IRfcommAdapter SerialAdapter { get; set; }

        private Timer _updateValuesTimer;

        public TxInterface(IRfcommAdapter serialAdapter)
        {
            SerialAdapter = serialAdapter;
            Connection = ConnectionStatus.NotConnected;
            _masterInterface = new FtExtension(0);
        }

        public ConnectionStatus Connection { get; set; }

        public void Connect(string mac)
        {
            LogMessage($"Connecting to {mac}");
            Mac = mac;
            if (Connection == ConnectionStatus.Connected || Connection == ConnectionStatus.Online)
            {
                throw new InvalidOperationException("Already connected to an interface");
            }

            TxCommunication?.Dispose();
            TxCommunication = new TxCommunication(SerialAdapter);

            try
            {
                TxCommunication.OpenConnection(mac);
                Connection = TxCommunication.Connected ? ConnectionStatus.Connected : ConnectionStatus.NotConnected;
            }
            catch (Exception e)
            {
                LogMessage($"Exception while connecting: {e.Message}");
                Connection = ConnectionStatus.Invalid;
                HandleException(e);
            }


            _masterInterface.ResetValues();

            LogMessage("Connected");
            Connected?.Invoke(this, new EventArgs());
        }

        public void Disconnect()
        {
            LogMessage("Disconnecting");
            ThrowWhenNotConnected();

            try
            {
                TxCommunication.CloseConnection();
                Connection = ConnectionStatus.NotConnected;
            }
            catch (Exception e)
            {
                LogMessage($"Exception while disconnecting: {e.Message}");
                Connection = ConnectionStatus.Invalid;
                HandleException(e);
            }

            Disconnected?.Invoke(this, new EventArgs());

            TxCommunication.Dispose();
            TxCommunication = null;
            
            LogMessage("Disconnected");
            _masterInterface.ResetValues();
        }

        public void StartOnlineMode()
        {
            LogMessage("Starting online mode");
            if (Connection == ConnectionStatus.Online)
            {
                throw new InvalidOperationException("Already in online mode");
            }
            ThrowWhenNotConnected();

            _updateValuesTimer = new Timer(UpdateInterval);
            _updateValuesTimer.Elapsed += UpdateValuesTimerTick;
            _updateValuesTimer.AutoReset = true;
            

            try
            {
                // Send an echo packet to obtain the session id
                var echoResponsePacket = new EchoResponsePacket();
                TxCommunication.SendPacket(new EchoPacket(), echoResponsePacket);
                
                Connection = ConnectionStatus.Online;

                // Start update timer
                _updateValuesTimer.Start();

                // Fire event to notify that online mode has started
                OnlineStarted?.Invoke(this, new EventArgs());

                // Fire InputValueChanged event with default values
                List<int> inputPorts = new List<int>();
                for (int i = 0; i < UniversalInputs; i++)
                {
                    inputPorts.Add(i);
                }
                InputValueChangedEventArgs eventArgs = new InputValueChangedEventArgs(inputPorts);

                InputValueChanged?.Invoke(this, eventArgs);
            }
            catch (Exception e)
            {
                LogMessage($"Exception while starting online mode: {e.Message}");
                Connection = ConnectionStatus.Invalid;
                HandleException(e);
            }
            LogMessage("Online mode started");
        }

        public void StopOnlineMode()
        {
            LogMessage("Stopping online mode");
            if (Connection == ConnectionStatus.Connected)
            {
                throw new InvalidOperationException("Interface is not in online mode");
            }
            ThrowWhenNotConnected();

            _updateValuesTimer.Stop();
            _updateValuesTimer.Close();
            _updateValuesTimer = null;

            StopAllOutputs();

            try
            {
                // TODO: Stop all motors
                OnlineStopped?.Invoke(this, new EventArgs());

                Connection = ConnectionStatus.Connected;
            }
            catch (Exception e)
            {
                LogMessage($"Exception while stopping online mode: {e.Message}");
                HandleException(e);
            }
            LogMessage("Online mode stopped");
        }

        public bool CanSendCommand()
        {
            return Connection == ConnectionStatus.Online || Connection == ConnectionStatus.Connected;
        }

        public string GetInterfaceVersionCode()
        {
            ThrowWhenNotConnected();

            try
            {
                var requestInfoResponsePacket = new RequestInfoResponsePacket();
                TxCommunication.SendPacket(new RequestInfoPacket(), requestInfoResponsePacket);
                return requestInfoResponsePacket.FirmwareVersion;
            }
            catch (Exception e)
            {
                HandleException(e);
            }
            return string.Empty;
        }

        public string GetInterfaceName()
        {
            return "TXT";
        }

        public bool IsInterfaceReachable(string adress)
        {
            return SerialAdapter.IsAvaliable(adress);
        }

        public int GetInputCount()
        {
            return UniversalInputs;
        }

        public int GetOutputCount()
        {
            return PwmOutputs;
        }

        public int GetMotorCount()
        {
            return MotorOutputs;
        }

        public int GetMaxOutputValue()
        {
            return PwmMaxValue;
        }

        public ControllerType GetControllerType() => ControllerType.Tx;

        public string RequestControllerName(string adress)
        {
            if (TxCommunication == null)
            {
                TxCommunication = new TxCommunication(SerialAdapter);
            }

            return TxCommunication.RequestControllerName(adress);
        }

        public bool IsValidInterface(string adress)
        {
            if (TxCommunication == null)
            {
                TxCommunication = new TxCommunication(SerialAdapter);
            }

            return TxCommunication.IsValidInterface(adress);
        }

        public int GetMotorIndex(int outputIndex)
        {
            return (int)Math.Ceiling((double)outputIndex / 2);
        }

        public void SetOutputValue(int outputIndex, int value, int extension = 0)
        {
            if (extension > 0)
            {
                throw new NotImplementedException("More than one extension is not implemented right now");
            }
            ThrowWhenNotConnected();

            _masterInterface.SetOutputValue(outputIndex, value);
        }

        public void SetMotorValue(int motorIndex, int value, MotorDirection direction, int extension = 0)
        {
            if (extension > 0)
            {
                throw new NotImplementedException("More than one extension is not implemented right now");
            }
            ThrowWhenNotConnected();

            _masterInterface.SetMotorDirection(motorIndex, direction);
            _masterInterface.SetMotorValue(motorIndex, value);
        }

        public void ConfigureOutputMode(int outputIndex, bool isMotor, int extension = 0)
        {
            if (extension > 0)
            {
                throw new NotImplementedException("More than one extension is not implemented right now");
            }
            ThrowWhenNotConnected();

            _masterInterface.SetOutputMode(outputIndex, isMotor);
            _configurationChanged = true;
        }

        public int GetInputValue(int index, int extension = 0)
        {
            if (extension > 0)
            {
                throw new NotImplementedException("More than one extension is not implemented right now");
            }
            ThrowWhenNotConnected();

            return _masterInterface.GetInputValue(index);
        }

        public void ConfigureInputMode(int inputIndex, InputMode inputMode, bool digital, int extension = 0)
        {
            if (extension > 0)
            {
                throw new NotImplementedException("More than one extension is not implemented right now");
            }
            ThrowWhenNotConnected();

            _masterInterface.SetInputMode(inputIndex, inputMode, digital);

            _configurationChanged = true;
        }
        
        public event InputValueChangedEventHandler InputValueChanged;
        

        public event ConnectedEventHandler Connected;
        public event ConnectionLostEventHandler ConnectionLost;
        public event DisconnectedEventHandler Disconnected;
        public event OnlineStartedEventHandler OnlineStarted;
        public event OnlineStoppedEventHandler OnlineStopped;

        public void Dispose()
        {
            Connection = ConnectionStatus.NotConnected;

            if (TxCommunication != null)
            {
                TxCommunication.Dispose();
                TxCommunication = null;
            }
            
            _masterInterface.ResetValues();
        }
        

        public void UpdateValues()
        {
            ThrowWhenNotConnected();

            if (_configurationChanged)
            {
                UpdateConfiguration();
                _configurationChanged = false;
            }


            var outputPacket = new OutputPacket();
            var inputPacket = new InputPacket();


            for (int i = 0; i < _masterInterface.OutputValues.Length; i++)
            {
                outputPacket.PwmOutputValues[i] = (short)_masterInterface.OutputValues[i];
            }

            

            try
            {
                TxCommunication.SendPacket(outputPacket, inputPacket);
            }
            catch (Exception e)
            {
                HandleException(e);
                return;
            }


            IList<int> valueChanged = new List<int>();

            for (int i = 0; i < inputPacket.UniversalInputs.Length; i++)
            {
                var newInputValue = inputPacket.UniversalInputs[i];

                if (_masterInterface.GetInputValue(i) != newInputValue)
                {
                    _masterInterface.SetInputValue(i, (short)newInputValue);

                    valueChanged.Add(i);
                }
            }

            if (valueChanged.Count > 0)
            {
                // Fire an event when an input value has changed
                InputValueChangedEventArgs eventArgs = new InputValueChangedEventArgs(valueChanged);
                InputValueChanged?.Invoke(this, eventArgs);
            }
        }

        private void StopAllOutputs()
        {
            for (int i = 0; i < PwmOutputs; i++)
            {
                if (!_masterInterface.OutputModes[i / 2])
                {
                    _masterInterface.SetOutputValue(i, 0);
                }
            }
            for (int i = 0; i < MotorOutputs; i++)
            {
                if (_masterInterface.OutputModes[i / 2])
                {
                    _masterInterface.SetMotorValue(i, 0);
                }
            }

            UpdateValues();
        }

        private void UpdateConfiguration()
        {
            ThrowWhenNotConnected();


            // ReSharper disable once UseObjectOrCollectionInitializer
            var configPacket = new ConfigPacket();
            var configResponsePacket = new ConfigResponsePacket();
            

            for (int i = 0; i < _masterInterface.InputModes.Length; i++)
            {
                configPacket.UniversalInputs[i].Mode = _masterInterface.InputModes[i];
            }
            for (int i = 0; i < _masterInterface.InputIsDigital.Length; i++)
            {
                configPacket.UniversalInputs[i].Digital = _masterInterface.InputIsDigital[i];
            }

            for (int i = 0; i < _masterInterface.OutputModes.Length; i++)
            {
                configPacket.Motor[i] = _masterInterface.OutputModes[i];
            }

            try
            {
                TxCommunication.SendPacket(configPacket, configResponsePacket);
            }
            catch (Exception e)
            {
                HandleException(e);
            }

            _configurationChanged = false;
        }

        private void ThrowWhenNotConnected()
        {
            if (Connection == ConnectionStatus.NotConnected || TxCommunication == null || _masterInterface == null)
            {
                throw new InvalidOperationException("Not connected to an interface");
            }
            if (Connection == ConnectionStatus.Invalid)
            {
                throw new InvalidOperationException("The connection is invalid");
            }
        }

        private void HandleException(Exception exception)
        {
            Connection = ConnectionStatus.Invalid;

            ConnectionLost?.Invoke(this, new EventArgs());
        }

        internal void LogMessage(string message)
        {
            if (IsDebugEnabled)
            {
                Console.WriteLine(message);
            }
        }

        private void UpdateValuesTimerTick(object sender, ElapsedEventArgs eventArgs)
        {
            _updateValuesTimer.Stop();

            UpdateValues();

            _updateValuesTimer.Start();
        }
    }
}