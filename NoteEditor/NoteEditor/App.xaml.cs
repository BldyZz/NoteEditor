using NoteEditor.Modules.Notes;
using NoteEditor.Modules.MenuBar;
using NoteEditor.Modules.NotesTree;
using NoteEditor.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using NoteEditor.Core.Data;
using NoteEditor.Core.Services;
using Prism.Regions;
using NoteEditor.Modules.Notes.Views;
using NoteEditor.Modules.MenuBar.Views;
using NoteEditor.Modules.NotesTree.Views;
using System;
namespace NoteEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {

        protected override void OnInitialized()
        {
            base.OnInitialized();
            var regionManager = Container.Resolve<IRegionManager>();
            if(regionManager == null) throw new ArgumentNullException(nameof(regionManager));
            // Register regions with views of projects
            regionManager.RegisterViewWithRegion("NotesRegion", typeof(NotesView));
            regionManager.RegisterViewWithRegion("NotesTreeRegion", typeof(NotesTreeView)); 
            regionManager.RegisterViewWithRegion("MenuBarRegion", typeof(MenuBarView));
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<NotesModule>();
            moduleCatalog.AddModule<MenuBarModule>();
            moduleCatalog.AddModule<NotesTreeModule>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register model types
            containerRegistry.RegisterInstance<IEditor>(new Editor());
            containerRegistry.RegisterInstance<IDialogService>(new DialogService());
        }

    }
}
