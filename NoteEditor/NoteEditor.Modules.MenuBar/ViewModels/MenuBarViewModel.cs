using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using NoteEditor.Core.Events;
using System;
using NoteEditor.Core.Data;
using NoteEditor.Core.Services;
using NoteEditor.Core.ViewModels;

namespace NoteEditor.Modules.MenuBar.ViewModels
{
    public class MenuBarViewModel : ViewModel
    {
        public MenuBarViewModel(IEventAggregator eventAggregator, IEditor editor, DialogService dialogService)
            : base(eventAggregator, editor, dialogService)
        {
        }

        protected override void CreateCommands()
        {
            AddFilter = new DelegateCommand(() => { _eventAggregator.GetEvent<AddFilterEvent>().Publish(); });
            AddNote = new DelegateCommand(() => { _eventAggregator.GetEvent<AddNoteEvent>().Publish(); });
            Save = new DelegateCommand(ExecuteSave);
            Open = new DelegateCommand(ExecuteOpen);
            Exit = new DelegateCommand(ExecuteExit);
            Search = new DelegateCommand(ExecuteSearch);
        }
        protected override void SubscribeOnInitialized()
        {
            _eventAggregator.GetEvent<ExitEvent>().Subscribe(ExecuteExit);
        }
        private bool SaveCheck()
        {
            if (_editor.ShouldSave())
            {
                var resultDialog = _dialogService.YesNoDialog("Do you want to save your changes?", "Save");
                if (resultDialog == IDialogService.ServiceResult.Yes)
                {
                    var resultSave = _editor.Save(); // Try to save
                    string file = "...";
                    if (resultSave == IEditor.SaveResult.NoFileOpen) // Lets the user save if file isn't an open file(new file)
                    {
                        file = _dialogService.FromSaveFileDialog(_editor.NotesDirectory);
                        _editor.OpenFile = file;
                        resultSave = _editor.Save(); // saves again.
                    }
                    if (file == string.Empty) return false; // If from save file dialog was canceled, cancel the undertake.
                }
                else if (resultDialog == IDialogService.ServiceResult.Cancel)
                {
                    return false; // If yes no dialog was canceled, cancel the undertake.
                }
            }
            return true; // OK - Can do what ever changes the file target.
        }
        /// <summary>
        /// Commands
        /// </summary>
        public DelegateCommand AddNote { get; private set; }
        public DelegateCommand AddFilter { get; private set; }
        public DelegateCommand Save { get; private set; }
        public DelegateCommand Open { get; private set; }
        public DelegateCommand Exit { get; private set; }
        public DelegateCommand Search { get; private set; }
        /// <summary>
        /// Command-Execute
        /// </summary>
        private void ExecuteExit()
        {
            if (!SaveCheck()) return;
            Environment.Exit(0);
        }
        private void ExecuteSave()
        {
            if (!SaveCheck()) return;
            var result = _editor.Save();
            if (result == IEditor.SaveResult.NoFileOpen)
            {
                string file = _dialogService.FromSaveFileDialog(_editor.NotesDirectory);
                _editor.OpenFile = file;
                result = _editor.Save();
            }
        }
        private void ExecuteOpen() 
        {
            if (!SaveCheck()) return; // Canceled Save 
            string file = _dialogService.FromFileDialog();
            if(file != string.Empty)
            {
                _editor.Open(file);
            }
            // Canceled Open
        }
        private void ExecuteSearch()
        {
            _eventAggregator.GetEvent<SearchEvent>().Publish();
        }
        /// <summary>
        /// Command-CanExecute
        /// </summary>

        /// <summary>
        /// Getters/Setters
        /// </summary>
        public int MenuHeight { get; set; } = 20;
    }
}
