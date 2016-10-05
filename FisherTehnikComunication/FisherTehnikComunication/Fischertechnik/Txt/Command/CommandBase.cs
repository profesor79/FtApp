using System;

namespace FactoryInterface.Fischertechnik.Txt.Command
{
    public abstract class CommandBase
    {
        public uint CommandId { get; protected set; }

        public virtual byte[] GetByteArray()
        {
            return BitConverter.GetBytes(CommandId);
        }
    }
}
