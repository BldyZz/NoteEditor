using NoteEditor.Core.IO;
using Prism.Mvvm;
using System;
using System.IO;
using System.Windows.Documents;
using System.Windows.Media;

namespace NoteEditor.Core.Data
{
    public class Editor : BindableBase, IEditor
    {
        // Content
        private string _content;
        private TextSelection _selected;
        private FontFamilies _fontFamilies;
        private FontSizes _fontSizes;
        private Color? _fontColor;
        // Paths
        private string _root;
        private string _notesDirectory;
        private string _openFile;

        public Editor()
        {
            _root = Directory.GetCurrentDirectory();
            _notesDirectory = $"{_root}\\Notes";
            if (!Directory.Exists(_notesDirectory))
            {
                Directory.CreateDirectory(_notesDirectory);
            }
            OpenFile = string.Empty;
            Content = "";

            FontFamilies = new FontFamilies();
            FontSizes = new FontSizes();
        }


        /// <summary>
        /// Logic Methods
        /// </summary>
        private void ApplyOnSelection(System.Windows.DependencyProperty property, object value)
        {
            _selected.ApplyPropertyValue(property, value);
        }
        private object GetFromSelection(System.Windows.DependencyProperty property)
        {
            return _selected.GetPropertyValue(property);
        }
        private void ApplySizeOnSelection()
        {
            if(!string.IsNullOrEmpty(FontSizes.SelectedSize) && _selected != null)
            {
                if (Selection.IsEmpty) // If nothing is selected, add a space and change its properties
                {
                    Selection.Text = " ";
                    Selection.Select(Selection.Start, Selection.Start.GetPositionAtOffset(1));
                    Selection.ApplyPropertyValue(TextElement.FontSizeProperty, FontSizes.SelectedSize);
                } else
                {
                    Selection.ApplyPropertyValue(TextElement.FontSizeProperty, FontSizes.SelectedSize);
                }
                
            }
        }
        private void ApplyFamilyOnSelection()
        {
            if (!string.IsNullOrEmpty(FontFamilies.SelectedFamily.Source) && _selected != null)
            {
                
                if (Selection.IsEmpty) // If nothing is selected, add a space and change its properties
                {
                    Selection.Text = " ";
                    Selection.Select(Selection.Start, Selection.Start.GetPositionAtOffset(1));
                    Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, FontFamilies.SelectedFamily);
                }
                else
                {
                    Selection.ApplyPropertyValue(TextElement.FontFamilyProperty, FontFamilies.SelectedFamily);
                }
            }
        }
        private void ApplyColorOnSelection()
        {
            if (_fontColor.HasValue && _selected != null)
            {
                if (Selection.IsEmpty) // If nothing is selected, add a space and change its properties
                {
                    Selection.Text = " ";
                    Selection.Select(Selection.Start, Selection.Start.GetPositionAtOffset(1));
                    Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush((Color)_fontColor));
                }
                else
                {
                    Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush((Color)_fontColor));
                }
            }
        }
        // Trys to save the document and return the end result.
        public IEditor.SaveResult Save()
        {
            if (OpenFile == String.Empty) return IEditor.SaveResult.NoFileOpen;
            //if (!File.Exists(OpenFile)) File.Create(OpenFile);
            if (NoteLoader.Equal(OpenFile, ref _content)) return IEditor.SaveResult.Equivalent;
            NoteLoader.Save(OpenFile, ref _content);
            return IEditor.SaveResult.Success;
        }

        public bool ShouldSave()
        {
            // Is the note an open file and unequal with that file or
            // is no file open, but something is noted then return true -
            // returns false otherwise
            return !((NoteLoader.Equal(OpenFile, ref _content) && 
                    OpenFile != string.Empty) ||
                   (OpenFile == string.Empty && 
                   Content == string.Empty));
        }

        public IEditor.OpenResult Openable(string file)
        {
            if (file == OpenFile)   return IEditor.OpenResult.Equivalent; // to open would be redundant
            if (!File.Exists(file)) return IEditor.OpenResult.FailedToOpen; // File doesn't exist
            return IEditor.OpenResult.Success;
        }

        public void Open(string file)
        {
            Content = NoteLoader.Load(file);
            OpenFile = file;
        }

        /// <summary>
        /// Getters/Setters
        /// </summary>
        public FontFamilies FontFamilies { get => _fontFamilies; set { _fontFamilies = value; } }
        public FontFamily SelectedFamily 
        { 
            get => _fontFamilies.SelectedFamily; 
            set 
            { 
                _fontFamilies.SelectedFamily = value; 
                RaisePropertyChanged();
                ApplyFamilyOnSelection(); 
            } 
        }
        public Color? SelectedColor
        {
            get => _fontColor;
            set
            {
                SetProperty(ref _fontColor, value);
                ApplyColorOnSelection();
            }
        }
        public FontSizes FontSizes { get => _fontSizes; set { _fontSizes = value; } }
        public string SelectedSize 
        {
            get => _fontSizes.SelectedSize; 
            set { 
                _fontSizes.SelectedSize = value; 
                RaisePropertyChanged();
                ApplySizeOnSelection(); 
            }
        }
        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }
        public string OpenFile
        {
            get => _openFile;
            set => SetProperty(ref _openFile, value);
        }
        public string NotesDirectory
        {
            get => _notesDirectory;
            set => SetProperty(ref _notesDirectory, value);
        }
        public TextSelection Selection 
        { 
            get => _selected;
            set 
            {
                SetProperty(ref _selected, value);
                var size = GetFromSelection(TextElement.FontSizeProperty) as double?;
                _fontSizes.SelectedSize = size.ToString();
                _fontFamilies.SelectedFamily = GetFromSelection(TextElement.FontFamilyProperty) as FontFamily;
                var fontColor = GetFromSelection(TextElement.ForegroundProperty) as Brush;
                _fontColor = fontColor is SolidColorBrush ? (fontColor as SolidColorBrush).Color : null;
                RaisePropertyChanged(nameof(SelectedSize));
                RaisePropertyChanged(nameof(SelectedColor));
                RaisePropertyChanged(nameof(SelectedFamily));
            } 
        }
    }
}
