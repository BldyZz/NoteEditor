using System;
using Xceed.Wpf.Toolkit;
namespace NoteEditor.Core.Services
{
    

    public class DialogService : IDialogService
    {
        
        public string FromFileDialog(string initialDir = "")
        {
            var fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.DefaultExt = "xml";
            fileDialog.Filter = "xml files (*.xml)|*.xml";
            fileDialog.Multiselect = false;
            if (!string.IsNullOrEmpty(initialDir))
            {
                fileDialog.InitialDirectory = initialDir;
            }
            bool? result = fileDialog.ShowDialog();
            return (result == false || result == null) ? string.Empty : fileDialog.FileName;
        }
        public string FromSaveFileDialog(string initialDir = "")
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.DefaultExt = "xml";
            saveFileDialog.Filter = "xml files (*.xml)|*.xml";
            if (!string.IsNullOrEmpty(initialDir))
            {
                saveFileDialog.InitialDirectory = initialDir;
            }
            bool? result = saveFileDialog.ShowDialog();
            return (result == false || result == null) ? string.Empty : saveFileDialog.FileName;
        }

        public IDialogService.ServiceResult YesNoDialog(string message, string title)
        {
            try
            {
                var result = MessageBox.Show(message, title, System.Windows.MessageBoxButton.YesNoCancel);
                return MessageBoxResultToResult(result);
            }
            catch (Exception)
            {
                return IDialogService.ServiceResult.Cancel;
            }
        }

        public IDialogService.ServiceResult OkDialog(string message, string title)
        {
            return MessageBoxResultToResult(MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK));
        }

        public (IDialogService.ServiceResult, string) FromInputDialog(string message, string title, string defaultResponse = "")
        {
            return (IDialogService.ServiceResult.OK, Microsoft.VisualBasic.Interaction.InputBox(message, title, defaultResponse));
        }

        private IDialogService.ServiceResult MessageBoxResultToResult(System.Windows.MessageBoxResult result)
        {
            switch (result)
            {
                case System.Windows.MessageBoxResult.OK:
                    return IDialogService.ServiceResult.OK;
                case System.Windows.MessageBoxResult.Yes:
                    return IDialogService.ServiceResult.Yes;
                case System.Windows.MessageBoxResult.No:
                    return IDialogService.ServiceResult.No;
                case System.Windows.MessageBoxResult.Cancel:
                    return IDialogService.ServiceResult.Cancel;
                default: return IDialogService.ServiceResult.Cancel;
            }
        }
    }
}
