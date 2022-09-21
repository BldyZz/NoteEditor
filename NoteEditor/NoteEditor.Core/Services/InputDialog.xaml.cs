using Prism.Mvvm;
using System.Windows;

namespace NoteEditor.Core.Services
{
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        private InputDialogViewModel _iDialogViewModel = new InputDialogViewModel();

        internal InputDialog(string message, string title, string defaultResponse,
            string message2, string defaultResponse2)
        {
            InitializeComponent();
            InputDialogViewModel.Message = message;
            InputDialogViewModel.Message2 = message2;
            InputDialogViewModel.Caption = title;
            InputDialogViewModel.Input   = defaultResponse;
            InputDialogViewModel.Input2 = defaultResponse2;
            if (!string.IsNullOrEmpty(message2))
            {
                outp_message2.Visibility = Visibility.Visible;
                inp_input2.Visibility = Visibility.Visible;
            }
            DataContext = this;
        }

        public InputDialogViewModel InputDialogViewModel { get => _iDialogViewModel; set => _iDialogViewModel = value; }

        internal static (IDialogService.ServiceResult, string, string) Show(string message, string title, string defaultResponse = "",
            string message2 = "", string defaultResponse2 = "")
        {
            InputDialog inputDialog = new InputDialog(message, title, defaultResponse, message2, defaultResponse2);
            inputDialog.ShowDialog();
            return (inputDialog.Result, inputDialog._iDialogViewModel.Input, inputDialog._iDialogViewModel.Input2);
        }

        private IDialogService.ServiceResult Result { get; set; } = IDialogService.ServiceResult.None;

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            Result = IDialogService.ServiceResult.OK;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = IDialogService.ServiceResult.Cancel;
            Close();
        }
    }

    public class InputDialogViewModel : BindableBase
    {
        private string _message;
        private string _message2;
        private string _caption;
        private string _input;
        private string _input2;
        public InputDialogViewModel() { }
        public string Message { get => _message; set => SetProperty(ref _message, value); }
        public string Message2 { get => _message2; set => SetProperty(ref _message2, value); }
        public string Caption { get => _caption; set => SetProperty(ref _caption, value); }
        public string Input { get => _input; set => SetProperty(ref _input, value); }
        public string Input2 { get => _input2; set => SetProperty(ref _input2, value); }
    }
}
