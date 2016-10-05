using System.Text;

namespace FactoryInterface.Fischertechnik.Tx.Packets
{
    /// <summary>
    /// This packet is received after an request info packet. It contains the controller name and its firmware version
    /// </summary>
    class RequestInfoResponsePacket : Packet
    {
        public string ControllerName { get; private set; }
        public string FirmwareVersion { get; private set; }

        public RequestInfoResponsePacket()
        {
            CommandCode = 0x6A;
        }

        public override bool FromByteArray(byte[] data)
        {
            var baseReturn = base.FromByteArray(data);

            // Read the controller name from the payload
            ControllerName = Encoding.ASCII.GetString(PayloadBytes, 6, 17);
            ControllerName = ControllerName.TrimEnd('\0');


            // Read the firmware version
            var versionStartIndex = 60;
            var majorVercion = PayloadBytes[versionStartIndex++];
            var minorVersion = PayloadBytes[versionStartIndex++];
            var buildVersion = PayloadBytes[versionStartIndex];
            
            FirmwareVersion = $"{majorVercion}.{minorVersion}.{buildVersion}";

            return baseReturn;
        }

        public override int GetPacketLength()
        {
            return 16 * 5 + 15;
        }
    }
}
