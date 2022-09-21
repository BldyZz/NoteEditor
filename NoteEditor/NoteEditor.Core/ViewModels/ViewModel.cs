using NoteEditor.Core.Data;
using NoteEditor.Core.Services;
using Prism.Events;
using Prism.Mvvm;

namespace NoteEditor.Core.ViewModels
{
    public class ViewModel : BindableBase
    {
        // ContainerTypes
        protected IEventAggregator _eventAggregator;
        protected IDialogService _dialogService;
        protected IEditor _editor;

        public ViewModel(IEventAggregator eventAggregator, IEditor editor, IDialogService dialogService)
        {
            _eventAggregator = eventAggregator;
            _editor          = editor;
            _dialogService   = dialogService;

            CreateCommands();
            SubscribeOnInitialized();
        }

        protected virtual void CreateCommands()
        {
        }

        protected virtual void SubscribeOnInitialized()
        {
        }

        public IEditor Editor
        {
            get => _editor;
            set => SetProperty(ref _editor, value);
        }
    }
}
