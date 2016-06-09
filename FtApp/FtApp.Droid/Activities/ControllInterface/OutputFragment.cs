using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using TXTCommunication.Fischertechnik;
using Fragment = Android.Support.V4.App.Fragment;

namespace FtApp.Droid.Activities.ControllInterface
{
    public class OutputFragment : Fragment, IFtInterfaceFragment
    {
        private IFtInterface _ftInterface;

        private ListAdapter _listAdapter;
        private ListView _listViewOutputPorts;

        private readonly List<OutputViewModel> _outputViewModels;

        public OutputFragment()
        {
            _outputViewModels = new List<OutputViewModel>();
        }

        private void FtInterfaceOnOnlineStarted(object sender, EventArgs eventArgs)
        {
            _outputViewModels.Clear();
            Activity?.RunOnUiThread(() => _listAdapter.NotifyDataSetChanged());

            for (int i = 0; i < _ftInterface.GetMotorCount(); i++)
            {
                var outputModel = new OutputViewModel(_ftInterface);

                outputModel.SetIndexes(i * 2, i * 2 + 1, i);
                outputModel.SetIsMotor(true);

                _outputViewModels.Add(outputModel);
            }
            Activity?.RunOnUiThread(() => _listAdapter.NotifyDataSetChanged());
        }

        private void FtInterfaceOnOnlineStopped(object sender, EventArgs eventArgs)
        {
            _outputViewModels.Clear();
            Activity?.RunOnUiThread(() => _listAdapter.NotifyDataSetChanged());
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.OutputFragmentLayout, container, false);

            _listViewOutputPorts = view.FindViewById<ListView>(Resource.Id.listViewOutputPorts);
            _listViewOutputPorts.Divider = null;
            _listViewOutputPorts.DividerHeight = 0;


            _listAdapter = new ListAdapter(Activity, _outputViewModels);

            _listViewOutputPorts.Adapter = _listAdapter;

            return view;
        }

        public void SetFtInterface(IFtInterface ftInterface)
        {
            _ftInterface = ftInterface;
            _ftInterface.OnlineStarted += FtInterfaceOnOnlineStarted;
            _ftInterface.OnlineStopped += FtInterfaceOnOnlineStopped;
        }

        public string GetTitle(Context context)
        {
            return context.GetText(Resource.String.ControlTxtActivity_tabOutputTitle);
        }

        /// <summary>
        /// This adapter displays the output views
        /// </summary>
        private class ListAdapter : BaseAdapter<OutputViewModel>
        {
            private readonly List<OutputViewModel> _items;
            private readonly Dictionary<OutputViewModel, PopupMenu> _popupMenus;
            private readonly Activity _context;

            public ListAdapter(Activity context, List<OutputViewModel> items)
            {
                _context = context;
                _items = items;
                _popupMenus = new Dictionary<OutputViewModel, PopupMenu>();
            }

            public override long GetItemId(int position)
            {
                return position;
            }

            public override OutputViewModel this[int position] => _items[position];

            public override int Count => _items.Count;


            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                OutputViewModel item = _items[position];
                View view = convertView;

                SeekBar seekBarValue1 = null;
                SeekBar seekBarValue2 = null;

                if (view == null) // no view to re-use, create new
                {
                    view = _context.LayoutInflater.Inflate(Resource.Layout.OutputMotorListViewItemLayout, null);

                    seekBarValue1 = view.FindViewById<SeekBar>(Resource.Id.seekBarOutput1);
                    seekBarValue2 = view.FindViewById<SeekBar>(Resource.Id.seekBarOutput2);

                    var imageViewContextualMenu = view.FindViewById<ImageView>(Resource.Id.imageViewContextualMenu);


                    seekBarValue1.ProgressChanged += delegate (object sender, SeekBar.ProgressChangedEventArgs args)
                    {
                        int itemIndex = position;

                        if (args.FromUser)
                        {
                            int threshold = 0;

                            if (_items[itemIndex].IsMotor)
                            {
                                threshold = _items[itemIndex].MaxOutput1/2;
                            }

                            if (Math.Abs(args.Progress - threshold) < 100)
                            {
                                args.SeekBar.Progress = threshold;
                            }
                        }

                        item.SetValueOutput1(args.SeekBar.Progress);
                    };

                    seekBarValue2.ProgressChanged += delegate (object sender, SeekBar.ProgressChangedEventArgs args)
                    {
                        if (args.FromUser)
                        {
                            if (args.Progress < 100)
                            {
                                args.SeekBar.Progress = 0;
                            }
                        }


                        item.SetValueOutput2(args.SeekBar.Progress);
                    };



                    imageViewContextualMenu.Click += delegate
                    {
                        ShowInputModeContextMenu(view.Context, imageViewContextualMenu, position);
                    };


                    FadeInAnimation(view, position);
                }

                if (seekBarValue1 == null)
                {
                    seekBarValue1 = view.FindViewById<SeekBar>(Resource.Id.seekBarOutput1);
                    seekBarValue2 = view.FindViewById<SeekBar>(Resource.Id.seekBarOutput2);
                }

                var textViewIndex1 = view.FindViewById<TextView>(Resource.Id.textViewOutputIndex1);
                var textViewIndex2 = view.FindViewById<TextView>(Resource.Id.textViewOutputIndex2);

                textViewIndex1.Text = item.GetDecoratedIndex1();
                textViewIndex2.Text = item.GetDecoratedIndex2();

                seekBarValue1.Max = item.MaxOutput1;
                seekBarValue2.Max = item.MaxOutput2;

                seekBarValue1.Progress = item.ValueOutput1;
                seekBarValue2.Progress = item.ValueOutput2;

                if (item.IsMotor)
                {
                    textViewIndex2.Visibility = ViewStates.Gone;
                    seekBarValue2.Visibility = ViewStates.Gone;
                }
                else
                {
                    textViewIndex2.Visibility = ViewStates.Visible;
                    seekBarValue2.Visibility = ViewStates.Visible;
                }
                
                return view;
            }
            
            private void FadeInAnimation(View view, int position)
            {
                view.Alpha = 0;

                view.TranslationY = 20;

                view.Animate().Alpha(1).SetDuration(225).SetStartDelay(position * 20).Start();
                view.Animate().TranslationY(0).SetDuration(100).SetStartDelay(position * 20).Start();
            }

            private void ShowInputModeContextMenu(Context context, ImageView imageView, int position)
            {
                if (_popupMenus.ContainsKey(_items[position]))
                {
                    _popupMenus[_items[position]].Show();
                }
                else
                {
                    var popup = new PopupMenu(context, imageView);
                    popup.MenuInflater.Inflate(Resource.Menu.ConfigureOutputPopupMenu, popup.Menu);
                    popup.Show();

                    popup.MenuItemClick += delegate (object sender, PopupMenu.MenuItemClickEventArgs args)
                    {
                        ConfigureInputPopupOnMenuItemClick(args, position);
                    };

                    _popupMenus.Add(_items[position], popup);
                }
            }

            private void ConfigureInputPopupOnMenuItemClick(PopupMenu.MenuItemClickEventArgs args, int position)
            {
                int id = args.Item.ItemId;

                args.Item.SetChecked(!args.Item.IsChecked);

                var item = _items[position];

                switch (id)
                {
                    case Resource.Id.menuOutpuModeMotor:
                        item.SetIsMotor(true);
                        break;

                    case Resource.Id.menuOutpuModeOutputs:
                        item.SetIsMotor(false);
                        break;
                }

                NotifyDataSetChanged();
            }
        }
        
        /// <summary>
        /// This class holds the output values
        /// </summary>
        private class OutputViewModel
        {
            public int ValueOutput1 { get; private set; }
            public int ValueOutput2 { get; private set; }

            public int IndexOutput1 { get; private set; }
            public int IndexOutput2 { get; private set; }

            public int MaxOutput1 { get; private set; }
            public int MaxOutput2 { get; private set; }



            public bool IsMotor { get; private set; }
            public int IndexMotor { get; private set; }

            public IFtInterface FtInterface { get; set; }

            public OutputViewModel(IFtInterface ftInterface)
            {
                FtInterface = ftInterface;
            }

            public string GetDecoratedIndex1()
            {
                if (IsMotor)
                {
                    return $"M{IndexMotor + 1}";
                }

                return $"O{IndexOutput1 + 1}";
            }

            public string GetDecoratedIndex2()
            {
                return $"O{IndexOutput2 + 1}";
            }

            public void SetValueOutput1(int value)
            {
                if (IsMotor)
                {
                    int outputValue = value - MaxOutput1/2;

                    MotorDirection direction = outputValue > 0
                        ? MotorDirection.Left
                        : MotorDirection.Right;


                    int absoluteValue = Math.Abs(outputValue);

                    if (absoluteValue > MaxOutput1)
                    {
                        absoluteValue = MaxOutput1;
                    }

                    if (FtInterface.CanSendCommand())
                    {
                        FtInterface.SetMotorValue(IndexMotor, absoluteValue, direction);
                    }
                }
                else
                {
                    if (value > MaxOutput1)
                    {
                        value = MaxOutput1;
                    }

                    if (FtInterface.CanSendCommand())
                    {
                        FtInterface.SetOutputValue(IndexOutput1, value);
                    }
                }
            }

            public void SetValueOutput2(int value)
            {
                if (value > MaxOutput2)
                {
                    value = MaxOutput2;
                }

                if (!IsMotor)
                {
                    if (FtInterface.CanSendCommand())
                    {
                        FtInterface.SetOutputValue(IndexOutput2, value);
                    }
                }
            }

            public void SetIsMotor(bool isMotor)
            {
                if (isMotor)
                {
                    MaxOutput1 = FtInterface.GetMaxOutputValue() * 2;
                    MaxOutput2 = 0;

                    ValueOutput1 = FtInterface.GetMaxOutputValue();
                    ValueOutput2 = 0;
                }
                else
                {
                    MaxOutput1 = FtInterface.GetMaxOutputValue();
                    MaxOutput2 = FtInterface.GetMaxOutputValue();

                    ValueOutput1 = 0;
                    ValueOutput2 = 0;
                }

                FtInterface.ConfigureOutputMode(IndexOutput1, isMotor);
                FtInterface.ConfigureOutputMode(IndexOutput2, isMotor);


                IsMotor = isMotor;
            }

            public void SetIndexes(int indexOutput1, int indexOutput2, int indexMotor)
            {
                IndexOutput1 = indexOutput1;
                IndexOutput2 = indexOutput2;
                IndexMotor = indexMotor;
            }
        }
    }
}