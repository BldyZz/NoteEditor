using Prism.Mvvm;
using System.Collections.Generic;

namespace NoteEditor.Core.Data
{
    public class FontSizes : BindableBase
    {
        private string _selectedSize;
        private static readonly int[] fontSizes = { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
        public FontSizes()
        {
            Sizes = new List<string>();
            foreach (var font in fontSizes)
            {
                Sizes.Add($"{font}");
            }
            SelectedSize = $"{fontSizes[4]}";
        }
        public List<string> Sizes { get; set; }
        public string SelectedSize 
        { 
            get => _selectedSize; 
            set => SetProperty(ref _selectedSize, value); 
        }
    }
}
