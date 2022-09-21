using System.Windows.Documents;
using System.Windows.Media;

namespace NoteEditor.Core.Data
{
    public interface IEditor
    {
        /// <summary>
        /// Enums
        /// </summary>
        public enum SaveResult
        {
            Success,
            Equivalent,
            NoFileOpen,
        }
        public enum OpenResult
        {
            Success,
            Equivalent,
            FailedToOpen
        }
        /// <summary>
        /// Methods
        /// </summary>
        /// <returns></returns>
        public SaveResult Save();
        public bool ShouldSave();
        public OpenResult Openable(string file);
        public void Open(string file);
        /// <summary>
        /// Getters/Setters
        /// </summary>
        public FontFamilies FontFamilies { get; set; }
        public FontFamily SelectedFamily { get; set; }
        public Color? SelectedColor { get; set; }
        public FontSizes FontSizes { get; set; }
        public string SelectedSize { get; set; }
        public string Content { get; set; }
        public string OpenFile { get; set; }
        public string NotesDirectory { get; set; }
        public TextSelection Selection { get; set; }
    }
}
