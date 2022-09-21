using NoteEditor.Core.Data;
using NoteEditor.Core.Events;
using NoteEditor.Core.Services;
using NoteEditor.Core.ViewModels;
using Prism.Commands;
using Prism.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace NoteEditor.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        // Window
        private string _title;
        private int _width;
        private int _height;
        private double _windowLeft;
        private double _windowTop;
        private const string _applicationName = "NoteEditor";
        private WindowState _windowState;

        // UI
        private string _maxNormalizeText;
        private Point _cursorPositionOnClick;
        private System.Windows.Threading.DispatcherTimer _dispatcherTimer;
        private double _timeBetweenClicks; // in ms
        // Regions
        private int _regionRelationSize;
        private int _regionSplitterSize;

        public MainWindowViewModel(IEventAggregator eventAggregator, IEditor editor, IDialogService dialogService)
            : base(eventAggregator, editor, dialogService)
        {
            Title = _applicationName;
            WindowWidth = 1080;
            WindowHeight = 720;
            WindowState = WindowState.Normal;
            MaxNormalizeText = "◻";
            _dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            _timeBetweenClicks = 16.0;
            RegionRelationSize = 250;
            RegionSplitterSize = 2;
        }

        protected override void CreateCommands()
        {
            Minimize = new DelegateCommand(() => { WindowState = WindowState.Minimized; });
            MaxNormalize = new DelegateCommand(ExecuteMaxNormalize);
            Exit = new DelegateCommand(ExecuteExit);
            TitlebarClick = new DelegateCommand<MouseButtonEventArgs>(ExecuteTitlebarClick);
        }

        protected override void SubscribeOnInitialized()
        {
        }

        private Point GetMousePoint()
        {
            return Application.Current.MainWindow.PointToScreen(Mouse.GetPosition(Application.Current.MainWindow));
        }

        /// <summary>
        /// This function starts a Timer, which calls the TitleBar_SingleClick() function,
        /// when the left mouse button wasn't clicked a second time.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startDragDispatcherTimer()
        {
            _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(_timeBetweenClicks);
            _dispatcherTimer.Tick += Move;
            _dispatcherTimer.Start();
        }
        /// <summary>
        /// Commands
        /// </summary>
        public DelegateCommand Minimize { get; private set; }
        public DelegateCommand MaxNormalize { get; private set; }
        public DelegateCommand<MouseButtonEventArgs> TitlebarClick { get; private set; }
        public DelegateCommand Exit { get; private set; }

        /// <summary>
        /// Command-Execute
        /// </summary>
        private void ExecuteExit()
        {
            _eventAggregator.GetEvent<ExitEvent>().Publish();
        }


        /// <summary>
        /// This function implements the drag process of the window.
        /// Gets called by DispatcherTimer.
        /// </summary>
        /// <param name="sender">Redundant parameter</param>
        /// <param name="e">Redundant parameter</param>
        private void ExecuteDragMove(object sender, EventArgs e)
        {
            // Check if left button is pressed, so that the if-body doesn't get
            // caught in another event, which throws an exception
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                _cursorPositionOnClick = GetMousePoint();
                _dispatcherTimer.Stop();
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                    MaxNormalizeText = "◻";
                    
                    WindowLeft += GetMousePoint().X - _cursorPositionOnClick.X;
                    WindowTop += GetMousePoint().Y - _cursorPositionOnClick.Y;
                }
                //lastPos = GetMousePoint();
                Application.Current.MainWindow.DragMove();
            }
        }
        private void ExecuteMaxNormalize()
        {
            if (WindowState == WindowState.Normal) // Window should be maximized
            {
                WindowState = WindowState.Maximized;
                MaxNormalizeText = "🗗";
            }
            else // Window should be minimized
            {
                WindowState = WindowState.Normal;
                MaxNormalizeText = "◻";
                WindowLeft = GetMousePoint().X - _cursorPositionOnClick.X;
                WindowTop = GetMousePoint().Y - _cursorPositionOnClick.Y;
            }
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        void Move(object sender, EventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                
                _dispatcherTimer.Stop();
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;
                    MaxNormalizeText = "◻";

                    WindowLeft += GetMousePoint().X - _cursorPositionOnClick.X;
                    WindowTop += GetMousePoint().Y - _cursorPositionOnClick.Y;
                }

                ReleaseCapture();
                SendMessage(new WindowInteropHelper(Application.Current.MainWindow).Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void ExecuteTitlebarClick(MouseButtonEventArgs e)
        {
            switch (e.ClickCount)
            {
                case 1: // Single-Click
                    if (!_dispatcherTimer.IsEnabled)
                    {
                        _cursorPositionOnClick = GetMousePoint();
                        startDragDispatcherTimer();
                    }
                    break;
                case 2: // Double-Click
                    _dispatcherTimer.Stop();
                    _cursorPositionOnClick = GetMousePoint();
                    MaxNormalize.Execute();
                    break;
                default: // Catch other cases.
                    _dispatcherTimer.Stop();
                    break;
            }
        }

        /// <summary>
        /// Getters/Setters
        /// </summary>
        public string Title { get => _title; set { SetProperty(ref _title, value); } }
        public int    WindowWidth { get => _width; set { SetProperty(ref _width, value); } }
        public int    WindowHeight { get => _height; set { SetProperty(ref _height, value); } }
        public double WindowLeft { get => _windowLeft; set => SetProperty(ref _windowLeft, value); }
        public double WindowTop { get => _windowTop; set => SetProperty(ref _windowTop, value); }
        public int RegionRelationSize { get => _regionRelationSize; set => SetProperty(ref _regionRelationSize, value); }
        public int RegionSplitterSize { get => _regionSplitterSize; set => SetProperty(ref _regionSplitterSize, value); }
        public WindowState WindowState { get => _windowState; set => SetProperty(ref _windowState, value); }
        public string MaxNormalizeText { get => _maxNormalizeText; set => SetProperty(ref _maxNormalizeText, value); }
    }
}
