namespace NoteEditor.Core.Services
{
    public interface IDialogService
    {
        public enum ServiceResult
        {
            None,
            OK,
            Yes,
            No,
            Cancel
        }
        public string FromFileDialog(string initialDir = "");
        public string FromSaveFileDialog(string initialDir);
        public ServiceResult YesNoDialog(string message, string title);
        public ServiceResult OkDialog(string message, string title);
        public (ServiceResult, string) FromInputDialog(string message, string title, string defaultResponse = "");
    }
}
