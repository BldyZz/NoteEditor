using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace NoteEditor.Core.Data
{
    public class FontFamilies : BindableBase
    {
        private FontFamily _selectedFamily;
        public FontFamilies()
        {
            Families = new List<FontFamily>();
            foreach (FontFamily font in Fonts.SystemFontFamilies)
            {
                Families.Add(font);
            }
            SelectedFamily = Families[0];
        }
        public List<FontFamily> Families { get; set; }
        public FontFamily SelectedFamily { 
            get => _selectedFamily; 
            set => SetProperty(ref _selectedFamily, value); 
        }
    }

}
