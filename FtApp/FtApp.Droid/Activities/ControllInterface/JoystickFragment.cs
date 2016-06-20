using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using FtApp.Droid.Views;
using FtApp.Utils;
using System;
using System.ComponentModel;
using Android.Widget;
using TXTCommunication.Fischertechnik;

namespace FtApp.Droid.Activities.ControllInterface
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class JoystickFragment : Fragment, IFtInterfaceFragment
    {
        private JoystickView _joystickViewLeft;
        private JoystickView _joystickViewRight;

        private ImageView _imageViewCameraStream;

        bool _firstFrame = true;

        public JoystickFragment()
        {
            FtInterfaceInstanceProvider.InstanceChanged += FtInterfaceInstanceProviderOnInstanceChanged;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.FragmentJoystick, container, false);

            view.Touch += (sender, args) => { args.Handled = true; };

            _imageViewCameraStream = view.FindViewById<ImageView>(Resource.Id.joystickCameraView);

            _joystickViewLeft = view.FindViewById<JoystickView>(Resource.Id.joystickLeft);
            _joystickViewRight = view.FindViewById<JoystickView>(Resource.Id.joystickRight);
            
            _joystickViewLeft.ValuesChanged += JoystickViewLeftOnValuesChanged;
            _joystickViewRight.ValuesChanged += JoystickViewRightOnValuesChanged;

            return view;
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);

            HookEvents();

            _firstFrame = true;
        }

        public override void OnDetach()
        {
            base.OnDetach();
            UnhookEvents();
        }


        private void HookEvents()
        {
            if (FtInterfaceInstanceProvider.Instance != null)
            {
                FtInterfaceCameraProxy.CameraFrameDecoded -= FtInterfaceCameraProxyOnCameraFrameDecoded;
                FtInterfaceCameraProxy.CameraFrameDecoded += FtInterfaceCameraProxyOnCameraFrameDecoded;

                FtInterfaceCameraProxy.ImageBitmapCleanup -= FtInterfaceCameraProxyOnImageBitmapCleanup;
                FtInterfaceCameraProxy.ImageBitmapCleanup += FtInterfaceCameraProxyOnImageBitmapCleanup;

                FtInterfaceCameraProxy.ImageBitmapInitialized -= FtInterfaceCameraProxyOnImageBitmapInitialized;
                FtInterfaceCameraProxy.ImageBitmapInitialized += FtInterfaceCameraProxyOnImageBitmapInitialized;
            }
        }

        private void UnhookEvents()
        {
            if (FtInterfaceInstanceProvider.Instance != null)
            {
                FtInterfaceCameraProxy.CameraFrameDecoded -= FtInterfaceCameraProxyOnCameraFrameDecoded;
                FtInterfaceCameraProxy.ImageBitmapCleanup -= FtInterfaceCameraProxyOnImageBitmapCleanup;
                FtInterfaceCameraProxy.ImageBitmapInitialized -= FtInterfaceCameraProxyOnImageBitmapInitialized;
            }
        }

        private void InitializeCameraView()
        {
            Activity.RunOnUiThread(() =>
            {
                _imageViewCameraStream?.SetImageBitmap(FtInterfaceCameraProxy.ImageBitmap);
                _imageViewCameraStream?.Invalidate();
                _firstFrame = false;


                View noCameraView = View.FindViewById(Resource.Id.noCameraStateLayout);

                if (noCameraView != null)
                {
                    noCameraView.Visibility = ViewStates.Gone;
                }
            });
        }

        private void CleanupCameraView()
        {
            Activity.RunOnUiThread(() =>
            {
                _imageViewCameraStream?.SetImageBitmap(null);
                _imageViewCameraStream?.Invalidate();
            });
        }

        private void JoystickViewLeftOnValuesChanged(object sender, EventArgs eventArgs)
        {
            SetMotor(0, _joystickViewLeft.ThumbX);
            SetMotor(1, _joystickViewLeft.ThumbY);
        }

        private void JoystickViewRightOnValuesChanged(object sender, EventArgs eventArgs)
        {
            var angle = _joystickViewRight.ThumbAngle;
            var distance = _joystickViewRight.ThumbDistance;

            float motor1 = 0;
            float motor2 = 0;

            if (angle >= 270 && angle <= 360)
            {
                //up right
                motor1 = (float) Math.Round(Math.Sin(MathUtils.ToRadians(angle*2f - 630f))*distance*2f);
                motor2 = (float) -Math.Round(distance);
            }
            else if (angle >= 180 && angle <= 270)
            {
                //up left
                motor1 = (float) -Math.Round(distance);
                motor2 = (float) -Math.Round(Math.Sin(MathUtils.ToRadians(angle*2f + 270f))*distance*2f);
            }
            else if (angle >= 90 && angle <= 180)
            {
                // down left
                motor1 = (float) Math.Round(Math.Sin(MathUtils.ToRadians(angle*2f - 90f))*distance*2f);
                motor2 = (float) Math.Round(distance);
            }
            else if (angle >= 0 && angle <= 90)
            {
                // down right
                motor1 = (float) Math.Round(distance);
                motor2 = (float) Math.Round(Math.Sin(MathUtils.ToRadians(angle*2f - 90f))*distance*2f);
            }

            SetMotor(0, motor1);
            SetMotor(1, motor2);
        }


        private void FtInterfaceInstanceProviderOnInstanceChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            HookEvents();
        }

        private void FtInterfaceCameraProxyOnImageBitmapCleanup(object sender, EventArgs eventArgs)
        {
            CleanupCameraView();
        }
        
        private void FtInterfaceCameraProxyOnImageBitmapInitialized(object sender, EventArgs eventArgs)
        {
            InitializeCameraView();
        }


        private void FtInterfaceCameraProxyOnCameraFrameDecoded(object sender, FrameDecodedEventArgs eventArgs)
        {
            Activity?.RunOnUiThread(() =>
            {
                if (_imageViewCameraStream != null && FtInterfaceCameraProxy.ImageBitmap != null)
                {
                    if (_firstFrame && !FtInterfaceCameraProxy.ImageBitmap.IsRecycled)
                    {
                        InitializeCameraView();
                    }
                    else if (!FtInterfaceCameraProxy.ImageBitmap.IsRecycled)
                    {
                        _imageViewCameraStream?.Invalidate();
                    }
                }
            });
        }
        

        private void SetMotor(int motorIndex, float percentage)
        {
            var direction = percentage > 0 ? MotorDirection.Left : MotorDirection.Right;

            int value = (int) Math.Abs(percentage/100* FtInterfaceInstanceProvider.Instance.GetMaxOutputValue());

            if (value > FtInterfaceInstanceProvider.Instance.GetMaxOutputValue())
            {
                value = FtInterfaceInstanceProvider.Instance.GetMaxOutputValue();
            }

            FtInterfaceInstanceProvider.Instance.SetMotorValue(motorIndex, value, direction);
        }

        public void Activate()
        {
            for (int i = 0; i < FtInterfaceInstanceProvider.Instance?.GetOutputCount(); i++)
            {
                FtInterfaceInstanceProvider.Instance?.ConfigureOutputMode(i, true);
            }
        }
        
        public string GetTitle(Context context)
        {
            return string.Empty;
        }
    }
}