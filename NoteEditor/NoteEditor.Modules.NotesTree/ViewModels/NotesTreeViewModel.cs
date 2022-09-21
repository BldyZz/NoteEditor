using NoteEditor.Core.Data;
using NoteEditor.Core.Events;
using NoteEditor.Core.Services;
using NoteEditor.Core.ViewModels;
using NoteManager.Modules.NotesTree.Models;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NoteEditor.Modules.NotesTree.ViewModels
{
    public class NotesTreeViewModel : ViewModel
    {
        private ItemNode _root;
        private ObservableCollection<TreeViewItem> _wrappedTree; // Contains the root list of items wrapped around ItemNodes.
        private List<TreeViewItem> _flatList; // Contains all items wrapped around the ItemNodes to find an item faster.
        // AddBar for adding notes or filters
        private string _addBarVisible;
        private string _addBarMessage;
        private BarType _addBarType;
        private string _addBarTextField;

        private enum BarType 
        {
            None,
            Note, 
            Filter
        };


        public NotesTreeViewModel(IEventAggregator eventAggregator, IEditor editor, IDialogService dialogService)
            : base(eventAggregator, editor, dialogService)
        {
            AddBarVisible   = "Hidden";
            AddBarMessage   = "";
            _addBarType     = BarType.None;
            AddBarTextField = "";
            Root        = new ItemNode(editor.NotesDirectory);
            WrappedTree = new ObservableCollection<TreeViewItem>();
            FlatList    = new List<TreeViewItem>();
            FillTree();
        }

        // Command initialization
        protected override void CreateCommands()
        {
            AddFilter          = new DelegateCommand(ExecuteAddFilter);
            AddNote            = new DelegateCommand(ExecuteAddNote);
            AddBarOK           = new DelegateCommand(ExecuteAddBarOK);
            AddBarCancel       = new DelegateCommand(ExecuteAddBarCancel);
            Open               = new DelegateCommand(ExecuteOpen, CanExecuteOpen);
            Rename             = new DelegateCommand(ExecuteRename);
            Refresh            = new DelegateCommand(ExecuteRefresh);
            Delete             = new DelegateCommand(ExecuteDelete);
            Drag               = new DelegateCommand<MouseEventArgs>(ExecuteDrag);
            Drop               = new DelegateCommand<DragEventArgs>(ExecuteDrop);
            SortByName         = new DelegateCommand(() => { _root.SortBy(_root.ByName); FillTree(); });
            FindByCreationDate = new DelegateCommand(ExecuteFindByCreationDate);
            SortByCreationDate = new DelegateCommand(() => { _root.SortBy(_root.ByCreationDate); FillTree(); });
            SortByUpdatedDate  = new DelegateCommand(() => { _root.SortBy(_root.ByUpdatedDate); FillTree(); });
            InvertSorting      = new DelegateCommand(() => { _root.ReverseNodeOrder(); FillTree(); });
            OpenInExplorer     = new DelegateCommand(() => { Process.Start("explorer", _editor.NotesDirectory); });
        }
        // EventAggregator Subscriptions
        protected override void SubscribeOnInitialized()
        {
            _eventAggregator.GetEvent<AddNoteEvent>().Subscribe(AddNote.Execute);
            _eventAggregator.GetEvent<AddFilterEvent>().Subscribe(AddFilter.Execute);
            _eventAggregator.GetEvent<RefreshNodesEvent>().Subscribe(Refresh.Execute);
        }
        // Fills the lists(ViewModel) with TreeViewItems again, which are wrapped around the ItemNodes.
        private void FillTree()
        {
            WrappedTree.Clear();
            FlatList.Clear();
            WrapNodesRecursively(Root.Successors);
        }
        // Wraps the given list with TreeViewItems and adds to given parent.
        private void WrapNodesRecursively(IList<ItemNode> successors, TreeViewItem parent = null)
        {
            foreach(var s in successors)
            {
                TreeViewItem item = new TreeViewItem();
                Label nameLabel = new Label();
                nameLabel.Content = s.Name;
                item.Tag = s;
                Image image = null;
                // Adding Image to TreeViewItem.
                try
                {
                    image = new Image
                    {
                        Source = s.Type == ItemNode.NodeType.File ?
                        Imaging.CreateBitmapSourceFromHIcon(System.Drawing.Icon.ExtractAssociatedIcon(s.ItemPath).Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()) :
                        Application.Current.Resources["Filter"] as ImageSource,
                        Height = 20,
                        Width = 20
                    };
                }
                catch (Exception) // Likely if file or no associated icon wasn't found.
                {
                    image = new Image { Source = Application.Current.Resources["Note"] as ImageSource, Height = 20, Width = 20 };
                }
                // Adding additional information for the user.
                ToolTip tt = new ToolTip();
                tt.Content = $"Name:          {s.Name}\n" +
                             $"Creation Date: {s.CreationDate.ToString()}\n" +
                             $"Updated Date:  {s.UpdatedDate.ToString()}";
                item.ToolTip = tt;
                // Adding StackPanel for image and label containing the file/folder name
                StackPanel stack = new StackPanel();
                stack.Orientation = Orientation.Horizontal;
                stack.Children.Add(image);
                stack.Children.Add(nameLabel);
                item.Header = stack;
                // Is parent the root then add to the list - add to parent otherwise.
                if(parent == null) WrappedTree.Add(item);
                else parent.Items.Add(item);
                FlatList.Add(item);
                WrapNodesRecursively(s.Successors, item); // Wrap child recursively.
            }
        }
        // Returns a list with a date which is extracted out of the given string.
        private List<int> FilterOutDate(string date)
        {
            List<int> outDate = new List<int>();
            try
            {
                string format = "([0-9]{1,})";
                foreach (Match match in Regex.Matches(date, format))
                {
                    outDate.Add(int.Parse(match.Value));
                }
            }
            catch (Exception)
            {
                return new List<int>();
            }
            return outDate;
        }
        // Checks save - saves if desired and returns if the undertaking can go on with true.
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
                } else if (resultDialog == IDialogService.ServiceResult.Cancel)
                {
                    return false; // If yes no dialog was canceled, cancel the undertake.
                }
            }
            return true; // OK - Can do what ever changes the file target.
        }
        private void UnselectAll()
        {
            foreach(var item in _flatList) item.IsSelected = false;
        }
        private TreeViewItem GetSelectedItem()
        {
            foreach(var item in _flatList)
            {
                if (item.IsSelected) return item;
            }
            return null;
        }
        private TreeViewItem FindParentOf(TreeViewItem child)
        {
            if(child == null) return null;
            foreach(var item in _flatList)
            {
                if (item.Items.Contains(child)) return item;
            }
            return null;
        }
        private bool CheckIfParentsOpen(TreeViewItem child)
        {
            TreeViewItem parent = FindParentOf(child);
            bool areExpanded = true;
            while(parent != null)
            {
                areExpanded &= parent.IsExpanded;
                parent = FindParentOf(parent);
            }
            return areExpanded;
        }
        private void MoveFile(TreeViewItem droppedItem, TreeViewItem target)
        {
            var droppedNode = (droppedItem.Tag as ItemNode);
            if (droppedNode != null)
            {
                /**
                 * Gets the Parent of the drop target file adds it to
                 * the respectable position and removes it from the
                 * old one. Renews the Wrapper Tree.
                 */
                TreeViewItem parentDroppedItem = FindParentOf(droppedItem);
                TreeViewItem parentTarget = FindParentOf(target);
                if (droppedItem.Items.Contains(parentTarget) || // Can't move in itself
                    parentTarget == droppedItem || // Do not move if target is itself
                    parentDroppedItem == parentTarget // Do not move if target is in the same folder
                    ) return;

                // Get the parents of dropped item and target. No parent means Root.
                ItemNode toBeRemovedFrom = parentDroppedItem == null ? Root : (parentDroppedItem.Tag as ItemNode);
                ItemNode toBeAddedTo = parentTarget == null ? Root : (parentTarget.Tag as ItemNode);

                toBeRemovedFrom.Move(droppedNode, toBeAddedTo);
                FillTree();
            }
        }
        private void MoveFolder(TreeViewItem droppedItem, TreeViewItem target)
        {
            var droppedNode = (droppedItem.Tag as ItemNode);
            // Only moves if droppable exists.
            if (droppedNode != null)
            {
                TreeViewItem parent = FindParentOf(droppedItem);
                if (droppedItem.Items.Contains(target)) return; // Can't move in itself.
                                                                // Move from either parent directory or root.
                if (parent != null)
                    (parent.Tag as ItemNode).Move(droppedNode, target.Tag as ItemNode);
                else
                    Root.Move(droppedNode, target.Tag as ItemNode);

                FillTree();
            }
        }

        /// <summary>
        /// Command-Execute
        /// </summary>
        private void ExecuteAddFilter()
        {
            AddBarMessage = "Enter the name of the filter.";
            AddBarTextField = "New_Filter";
            _addBarType = BarType.Filter;
            AddBarVisible = "Visible";
        }
        private void ExecuteAddNote()
        {
            AddBarMessage = "Enter the name of the note.";
            AddBarTextField = "New_Note";
            _addBarType = BarType.Note;
            AddBarVisible = "Visible";
        }
        private void ExecuteOpen()
        {
            var item = GetSelectedItem();
            _editor.Open((item.Tag as ItemNode).ItemPath);
        }
        private void ExecuteRename()
        {
            var item = GetSelectedItem();
            if(item != null)
            {
                (IDialogService.ServiceResult result, string newName) = _dialogService.FromInputDialog("What new name should the item have?", "Rename", (item.Tag as ItemNode).Name);
                if (!(string.IsNullOrEmpty(newName) || result == IDialogService.ServiceResult.Cancel))
                {
                    (item.Tag as ItemNode).RenameItem((item.Tag as ItemNode), newName);
                    FillTree();
                }
            }
        }
        private void ExecuteRefresh()
        {
            Root = new ItemNode(_editor.NotesDirectory);
            FillTree();
        }
        private void ExecuteDelete()
        {
            var item = GetSelectedItem();
            if (item != null)
            {
                var resultDialog = _dialogService.YesNoDialog("Are you sure you want to delete this item with all of its content?", "Delete");
                if (resultDialog == IDialogService.ServiceResult.Yes)
                {
                    Root.DeleteItem(item.Tag as ItemNode);
                    FillTree();
                }
            }
        }
        private void ExecuteDrag(MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                var item = GetSelectedItem();
                if(item != null)
                    DragDrop.DoDragDrop(item, new DataObject(DataFormats.Serializable, item), DragDropEffects.Move);
            }
        }

        
        private void ExecuteDrop(DragEventArgs e)
        {
            Point targetPos = e.GetPosition(FlatList[0]);
            TreeViewItem droppedItem = e.Data.GetData(DataFormats.Serializable) as TreeViewItem;
            TreeViewItem target = FlatList[0];
            // Find the item which is the closest to the drop position on the y-axis.
            foreach (var item in _flatList) 
            {
                Point pos = e.GetPosition(item);
                if (Math.Abs(((Vector)pos).Y) < Math.Abs(((Vector)targetPos).Y) && CheckIfParentsOpen(item))
                {
                    target = item;
                    targetPos = pos;
                }
            }

            // Is the target itself or to far away do nothing
            if (droppedItem == target) return;

            if(Math.Abs(targetPos.Y) > 15.0f)
            {
                target = null;
                MoveFile(droppedItem, target);
            }

            else if ((target.Tag as ItemNode).IsFile())
            {
                MoveFile(droppedItem, target);
            } 
            else if ((target.Tag as ItemNode).IsFolder())
            {
                MoveFolder(droppedItem, target);
            }
        }

        private void ExecuteFindByCreationDate()
        {
            (IDialogService.ServiceResult result, string date) = _dialogService.FromInputDialog("What creation date has the item you are looking for? 'YYYY/MM/DD/HH/mm/SS'", "Find by creation date");
            if (string.IsNullOrEmpty(date) || date.Length < 4 || result == IDialogService.ServiceResult.Cancel) return;
            List<int> list = FilterOutDate(date);
            if(list.Count == 0) return;
            UnselectAll();
            foreach(var item in _flatList)
            {
                bool isEqual = true;
                DateTime itemDate = (item.Tag as ItemNode).CreationDate;
                int[] itemDateArr = new int[] { itemDate.Year, itemDate.Month, itemDate.Day, itemDate.Hour, itemDate.Minute, itemDate.Second };
                for(int i = 0; i < list.Count; i++) isEqual &= itemDateArr[i] == list[i];
                item.IsSelected = isEqual;
            }
        }

        private void AddFolder(string noteName)
        {
            var selected = GetSelectedItem();

            if (string.IsNullOrEmpty(noteName)) return;

            ItemNode target = null;
            if (selected == null) target = Root; // If no item was selected add to highest folder
            else
            {
                var node = (selected.Tag as ItemNode);
                if (node.IsFolder()) target = node; // Add to folder if folder is selected
                else // Add to the same path as selected item.
                {
                    var parent = FindParentOf(selected);
                    if (parent == null) // Already highest folder
                        target = Root;
                    else
                        target = (parent.Tag as ItemNode);
                }
            }
            target.CreateNode(noteName, ItemNode.NodeType.Folder);
            FillTree();
        }
        private void AddFile(string noteName)
        {
            var selected = GetSelectedItem();

            if (string.IsNullOrEmpty(noteName)) return;

            ItemNode target = null;

            if (selected == null) target = Root;
            else
            {
                var node = (selected.Tag as ItemNode);
                if (node.IsFolder()) target = node;
                else
                {
                    var parent = FindParentOf(selected);
                    if (parent == null)
                        target = Root;
                    else
                        target = (parent.Tag as ItemNode);
                }
            }
            target.CreateNode(noteName, ItemNode.NodeType.File);
            FillTree();
        }
        private void ExecuteAddBarOK()
        {
            if(_addBarType == BarType.Note)
            {
                AddFile(AddBarTextField);
            }
            else if(_addBarType == BarType.Filter)
            {
                AddFolder(AddBarTextField);
            }
            ExecuteAddBarCancel();
        }

        private void ExecuteAddBarCancel()
        {
            AddBarMessage = "";
            AddBarVisible = "Hidden";
            _addBarType = BarType.None;
            AddBarTextField = "";
        }

        /// <summary>
        /// Command-CanExecute
        /// </summary>
        private bool CanExecuteOpen()
        {
            var item = GetSelectedItem();
            if(item == null || _editor.OpenFile == (item.Tag as ItemNode).ItemPath || (item.Tag as ItemNode).IsFolder()) return false;
            return SaveCheck();
        }
        
        /// <summary>
        /// Commands
        /// </summary>
        public DelegateCommand<DragEventArgs> Drop { get; private set; }
        public DelegateCommand<MouseEventArgs> Drag { get; private set; }
        public DelegateCommand Refresh { get; private set; }
        public DelegateCommand Rename { get; private set; }
        public DelegateCommand Delete { get; private set; }
        public DelegateCommand Open { get; private set; }
        public DelegateCommand AddFilter { get; private set; }
        public DelegateCommand AddNote { get; private set; }
        public DelegateCommand AddBarOK { get; private set; }
        public DelegateCommand AddBarCancel { get; private set; }
        public DelegateCommand Remove { get; private set; }
        public DelegateCommand FindByCreationDate { get; private set; }
        public DelegateCommand SortByName { get; private set; }
        public DelegateCommand SortByCreationDate { get; private set; }
        public DelegateCommand SortByUpdatedDate { get; private set; }
        public DelegateCommand InvertSorting { get; private set; }
        public DelegateCommand OpenInExplorer { get; private set; }
        /// <summary>
        /// Getters/Setters
        /// </summary>
        public ItemNode Root { get => _root; set => SetProperty(ref _root, value); }
        public ObservableCollection<TreeViewItem> WrappedTree { get => _wrappedTree; set => SetProperty(ref _wrappedTree, value); }
        public List<TreeViewItem> FlatList { get => _flatList; set => SetProperty(ref _flatList, value); }
        public string AddBarVisible { get => _addBarVisible; set => SetProperty(ref _addBarVisible, value); }
        public string AddBarMessage { get => _addBarMessage; set => SetProperty(ref _addBarMessage, value); }
        public string AddBarTextField { get => _addBarTextField; set => SetProperty(ref _addBarTextField, value); }
    }
}
