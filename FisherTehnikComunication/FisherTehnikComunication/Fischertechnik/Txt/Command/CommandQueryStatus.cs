﻿namespace FactoryInterface.Fischertechnik.Txt.Command
{
    public class CommandQueryStatus : CommandBase
    {
        public CommandQueryStatus()
        {
            CommandId = TxtInterface.CommandIdQueryStatus;
        }
    }
}