using System;
using System.Collections.Generic;

namespace FactoryInterface.Fischertechnik.Txt.Command
{
    class CommandExchangeData : CommandBase
    {
        public short[] PwmOutputValues { get; }
        public short[] MotorMasterValues { get; }
        public short[] MotorDistanceValues { get; }
        public short[] MotorCommandId { get; }
        public short[] CounterResetCommandId { get; }

        public ushort SoundCommandId { get; set; }
        public ushort SoundIndex { get; set; }
        public ushort SoundRepeat { get; set; }

        public CommandExchangeData()
        {
            PwmOutputValues = new short[TxtInterface.PwmOutputs];
            MotorMasterValues = new short[TxtInterface.MotorOutputs];
            MotorDistanceValues = new short[TxtInterface.MotorOutputs];
            MotorCommandId = new short[TxtInterface.MotorOutputs];
            CounterResetCommandId = new short[TxtInterface.Counters];


            CommandId = TxtInterface.CommandIdExchangeData;
        }

        public override byte[] GetByteArray()
        {
            var bytes = new List<byte>(base.GetByteArray());

            // Construct the byte array

            foreach (var s in PwmOutputValues)
            {
                bytes.AddRange(BitConverter.GetBytes(s));
            }
            foreach (var s in MotorMasterValues)
            {
                bytes.AddRange(BitConverter.GetBytes(s));
            }
            foreach (var s in MotorDistanceValues)
            {
                bytes.AddRange(BitConverter.GetBytes(s));
            }
            foreach (var s in MotorCommandId)
            {
                bytes.AddRange(BitConverter.GetBytes(s));
            }
            foreach (var s in CounterResetCommandId)
            {
                bytes.AddRange(BitConverter.GetBytes(s));
            }

            bytes.AddRange(BitConverter.GetBytes(SoundCommandId));
            bytes.AddRange(BitConverter.GetBytes(SoundIndex));
            bytes.AddRange(BitConverter.GetBytes(SoundRepeat));


            // Add empty bytes because of an outdated documentation
            bytes.Add(new byte());
            bytes.Add(new byte());


            return bytes.ToArray();
        }
    }
}
