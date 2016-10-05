using FactoryInterface.Enums;

namespace FactoryInterface
{
    public partial class SortingLineActor
    {

        public class BeltConveyorSensorOneActivated
        {

        }

        public class BeltConveyorSensorOneDeactivated
        {

        }
        public class BeltConveyorSensorTwoActivated
        {

        }

        public class BeltConveyorSensorTwoDeactivated
        {

        }

        public class CameraReadingChanged
        {
            public ColorEnum ColorValue { get; }

            public CameraReadingChanged(ColorEnum colorValue)
            {
                ColorValue = colorValue;
            }
        }


        public class StartCommpressor
        {

        }

        public class StopCommpressor
        {

        }

        public class TransferRedElementToSlot
        {

        }

        public class TransferWhiteElementToSlot
        {

        }

        public class TransferBlueElementToSlot
        {

        }





    }
}
