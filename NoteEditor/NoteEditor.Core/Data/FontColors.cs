using System;
using System.Collections.Generic;
using System.Windows.Media;
using Prism.Mvvm;

namespace NoteEditor.Core.Data
{
    [Obsolete("This class is deprecated.")]
    public class FontColors : BindableBase 
    {
        private Brush _selectedBrush;
        public FontColors()
        {
            Colors = new List<Brush>();
            
            foreach(var colorProperty in typeof(Brushes).GetProperties())
            {
                Colors.Add((Brush)colorProperty.GetValue(null));
            }
            SelectedBrush = Brushes.Black;
        }
        public List<Brush> Colors { get; set; }
        public Brush SelectedBrush { get => _selectedBrush; set => SetProperty(ref _selectedBrush, value); }

    }
}
