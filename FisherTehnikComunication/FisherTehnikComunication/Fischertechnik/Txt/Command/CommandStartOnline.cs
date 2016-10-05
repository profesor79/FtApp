using System.Collections.Generic;
using System.Linq;

namespace FactoryInterface.Fischertechnik.Txt.Command
{
    class CommandStartOnline : CommandBase
    {
        private byte[] Name { get; set; }

        public CommandStartOnline()
        {
            Name = new byte[64];
            CommandId = TxtInterface.CommandIdStartOnline;
        }

        public override byte[] GetByteArray()
        {
            IList<byte> bytes = new List<byte>(base.GetByteArray());

            foreach (var b in Name)
            {
                bytes.Add(b);
            }

            return bytes.ToArray();
        }
    }
}
