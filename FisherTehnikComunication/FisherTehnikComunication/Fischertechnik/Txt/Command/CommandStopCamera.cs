namespace FactoryInterface.Fischertechnik.Txt.Command
{
    class CommandStopCamera : CommandBase
    {
        public CommandStopCamera()
        {
            CommandId = TxtInterface.CommandIdStopCameraOnline;
        }
    }
}
