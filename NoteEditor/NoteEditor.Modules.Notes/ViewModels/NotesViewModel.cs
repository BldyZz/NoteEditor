using NoteEditor.Core.Data;
using NoteEditor.Core.Events;
using NoteEditor.Core.Services;
using NoteEditor.Core.ViewModels;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;

namespace NoteEditor.Modules.Notes.ViewModels
{
    public class NotesViewModel : ViewModel
    {
        // Editor content
        private Xceed.Wpf.Toolkit.RichTextBox _richTextBox;
        // Sub-Windows
        private string _searchBarVisible;
        private string _searchTextField;
        private int    _searchIndex;
        private string _lastSearch;
        private bool _lastUseRegEx;
        private bool _useRegEx;
        private List<TextRange> _results;
        private string _searchResultsFound;

        public NotesViewModel(IEventAggregator eventAggregator, IEditor editor, IDialogService dialogService) 
            : base(eventAggregator, editor, dialogService)
        {
            SearchBarVisible = "Hidden";
            SearchTextField = "";
            _searchIndex = 0;
            _lastSearch = "";
            _lastUseRegEx = false;
            UseRegEx = false;
            SearchResultsFound = "";
        }

        protected override void CreateCommands()
        {
            Save              = new DelegateCommand(ExecuteSave);
            SelectionChanged  = new DelegateCommand<RoutedEventArgs>(ExecuteSelectionChanged);
            Search            = new DelegateCommand(ExecuteSearch);
            OpenSearch        = new DelegateCommand(ExecuteOpenSearch);
            CloseSearch       = new DelegateCommand(ExecuteCloseSearch);
        }

        protected override void SubscribeOnInitialized()
        {
            _eventAggregator.GetEvent<SearchEvent>().Subscribe(OpenSearch.Execute);
        }

        private static List<TextRange> Find(TextRange range, string searchPattern, bool useRegEx = false, RegexOptions regexOptions = RegexOptions.None)
        {
            MatchCollection matches = new Regex(useRegEx ? searchPattern : Regex.Escape(searchPattern), regexOptions).Matches(range.Text);
            TextPointer subBegin = null;
            List<TextRange> results = new List<TextRange>();
            foreach (Match match in matches)
            {
                TextRange result = null;
                if(subBegin == null)
                {
                    subBegin = range.Start.GetPositionAtOffset(match.Index);
                }
                while (subBegin != range.End)
                {
                    var b = subBegin;
                    var e = subBegin.GetPositionAtOffset(match.Length);
                    if (b == null || e == null) break;
                    result = new TextRange(b, e);
                    if (result.Text == match.Value) break;
                    else result = null;
                    subBegin = subBegin.GetPositionAtOffset(1);
                }
                if(result != null)
                {
                    subBegin = result.End;
                    results.Add(result);
                }
            }
            return results;
        }

        /// <summary>
        /// Commands
        /// </summary>
        public DelegateCommand Save { get; private set; }
        public  DelegateCommand<RoutedEventArgs> SelectionChanged { get; private set; }
        public DelegateCommand Search { get; private set; }
        public DelegateCommand OpenSearch { get; private set; }
        public DelegateCommand CloseSearch { get; private set; }
        //public DelegateCommand<DragEventArgs> Drop { get; set; }
        //public DelegateCommand<DragEventArgs> DragEnter { get; set; }

        /// <summary>
        /// Command-Execute
        /// </summary>
        void ExecuteSave()
        {
            System.Windows.Input.Keyboard.ClearFocus();
            var result = _editor.Save();
            if(result == IEditor.SaveResult.NoFileOpen)
            {
                string file = _dialogService.FromSaveFileDialog(_editor.NotesDirectory);
                _editor.OpenFile = file;
                result = _editor.Save();
                if (result == IEditor.SaveResult.Success) _eventAggregator.GetEvent<RefreshNodesEvent>().Publish();
            }
        }
        void ExecuteSelectionChanged(RoutedEventArgs e)
        {
            _richTextBox = (e.Source as Xceed.Wpf.Toolkit.RichTextBox);
            _editor.Selection = _richTextBox.Selection;
        }
        void ExecuteSearch()
        {
            RegexOptions options = RegexOptions.Compiled | RegexOptions.IgnoreCase;
            // Checks if RegEx is used. If true test the validity of entered pattern.
            if (UseRegEx)
            {
                try
                {
                    _ = new Regex(SearchTextField).Match("");
                }
                catch (Exception)
                {
                    _dialogService.OkDialog("An error occurred with the entered pattern.", "Search...");
                    return;
                }
            }
            // Don't search again if input is the same
            if (_lastSearch == SearchTextField && _lastUseRegEx == UseRegEx) 
            {
                _searchIndex++;
            }
            else // Search
            {
                var range = new TextRange(_richTextBox.Document.ContentStart, _richTextBox.Document.ContentEnd);
                _results = Find(range, SearchTextField, UseRegEx, options);
                _searchIndex = 0;
            }

            if(_searchIndex >= _results.Count()) // Last finding passed.
            {
                _dialogService.OkDialog($"Cannot find more instances of '{SearchTextField}'.\rReturns to the first occurence.", "Search...");
                _searchIndex = -1;
                _lastSearch = SearchTextField;
            } else // Goto next result.
            {
                var result = _results[_searchIndex];
                _richTextBox.Selection.Select(result.Start, result.End);
                SearchResultsFound = $"Result: {_searchIndex + 1}/{_results.Count}";
                _richTextBox.Focus();
                // scroll to occurrence
                //_richTextBox.ScrollToVerticalOffset(_richTextBox.Selection.Start.GetCharacterRect(LogicalDirection.Forward).BottomLeft.Y);
                var characterRect = _richTextBox.Selection.Start.GetCharacterRect(LogicalDirection.Forward);
                _richTextBox.ScrollToHorizontalOffset(_richTextBox.HorizontalOffset + characterRect.Left - _richTextBox.ActualWidth / 2d);
                _richTextBox.ScrollToVerticalOffset(_richTextBox.VerticalOffset + characterRect.Top - _richTextBox.ActualHeight / 2d);
                _lastSearch = SearchTextField;
                _lastUseRegEx = UseRegEx;
            }
        }

        private void ExecuteOpenSearch()
        {
            SearchTextField = _editor.Selection.Text;
            SearchBarVisible = "Visible";
        }

        private void ExecuteCloseSearch()
        {
            SearchBarVisible = "Hidden";
            _lastSearch = "";
            SearchResultsFound = "";
        }

        /// <summary>
        /// Command-CanExecute
        /// </summary>

        /// <summary>
        /// Getters/Setters
        /// </summary>
        public string SearchBarVisible
        {
            get => _searchBarVisible;
            set => SetProperty(ref _searchBarVisible, value);
        }
        public string SearchTextField
        {
            get => _searchTextField;
            set => SetProperty(ref _searchTextField, value);
        }
        public bool UseRegEx
        {
            get => _useRegEx;
            set => SetProperty(ref _useRegEx, value);
        }
        public string SearchResultsFound
        {
            get => _searchResultsFound;
            set => SetProperty(ref _searchResultsFound, value);
        }
    }
}
