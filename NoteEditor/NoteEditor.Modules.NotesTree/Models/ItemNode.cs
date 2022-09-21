using System;
using System.Collections.Generic;
using System.IO;

namespace NoteManager.Modules.NotesTree.Models
{
    public class ItemNode
    {
        private string _name;
        private string _itemPath;
        private NodeType _type;
        private DateTime _creationDate;
        private DateTime _updatedDate;
        private readonly List<ItemNode> _successors;
        public enum NodeType
        {
            File,
            Folder
        }

        public ItemNode(string path)
        {
            FillNodeInformation(path);
            _successors = new List<ItemNode>();
            
            // this node can only have successors if it is a directory.
            if(Type == NodeType.Folder)
            {
                string[] files = Directory.GetFiles(path + '\\');
                string[] dirs = Directory.GetDirectories(path + '\\');
                foreach (string file in files) Successors.Add(new ItemNode(file));
                foreach (string dir in dirs)   Successors.Add(new ItemNode(dir));
            }
        }

        /// <summary>
        /// Creates a new successor if this is a directory and the successor doesn't already exist.
        /// </summary>
        /// <param name="name">Name of the new node.</param>
        /// <param name="type">Type of node</param>
        public void CreateNode(string name, NodeType type)
        {
            if (Type == NodeType.File) return;
            int existsCounter = 0;
            string path = $"{ItemPath}\\{name}";
            string existsCounterStr = "";
            string fileExtension = ".xml";
            if (type == NodeType.File)
            {
                while(File.Exists(path + existsCounterStr + fileExtension))
                {
                    existsCounter++;
                    existsCounterStr = $"({existsCounter})";
                }
                path = path + existsCounterStr + fileExtension;
                File.Create(path);
            } else
            {
                while (Directory.Exists(path + existsCounterStr))
                {
                    existsCounter++;
                    existsCounterStr = $"({existsCounter})";
                }
                path = path + existsCounterStr;
                Directory.CreateDirectory(path);
            }
            Successors.Add(new ItemNode(path));
        }
        
        /// <summary>
        /// Moves a node from this to a destination node.
        /// </summary>
        /// <param name="moveable">To be moved node.</param>
        /// <param name="dest">Destination which will contain the moveable as successor.</param>
        public void Move(ItemNode moveable, ItemNode dest)
        {
            if (!dest.Contains(moveable.Name))
            {
                Successors.Remove(moveable);
                dest.Successors.Add(moveable);

                string oldPath = ItemPath + '\\' + moveable.Name;
                string newPath = dest.ItemPath + '\\' + moveable.Name;

                if (moveable.Type == NodeType.Folder)
                {
                    Directory.Move(oldPath, newPath);
                }
                else if(moveable.Type == NodeType.File)
                {
                    File.Move(oldPath, newPath, false);
                }
                moveable.RenamePathRecursively(ItemPath, dest.ItemPath);
            }
        }
        
        /// <summary>
        /// Fills this node with information of the file/directory according to the given path.
        /// </summary>
        /// <param name="path">The path of which the information is aquired.</param>
        /// <exception cref="ArgumentNullException">If path is null or empty.</exception>
        public void FillNodeInformation(string path)
        {
            if (string.IsNullOrEmpty(path)) { throw new ArgumentNullException("path"); }
            ItemPath = path;
            Type = NoteEditor.Core.IO.NoteLoader.IsFolder(path) ? NodeType.Folder : NodeType.File;
            Name = Type == NodeType.Folder ? new DirectoryInfo(path).Name : Path.GetFileName(path);
            CreationDate = File.GetCreationTime(path);
            UpdatedDate = File.GetLastWriteTime(path);
        }

        /// <summary>
        /// Recursively removes given node from successor list and deletes it.
        /// </summary>
        /// <param name="node">The node to be removed and deleted</param>
        public void DeleteItem(ItemNode node)
        {
            if(node.Type == NodeType.Folder)
            {
                Directory.Delete(node.ItemPath, true);
            } else
            {
                File.Delete(node.ItemPath);
            }
            RemoveItem(node);
        }

        /// <summary>
        /// Removes given node from successor list recursively.
        /// </summary>
        /// <param name="node">The node to be removed.</param>
        public void RemoveItem(ItemNode node)
        {
            bool removed = Successors.Remove(node);
            if (!removed)
            {
                foreach(var item in Successors)
                {
                    item.RemoveItem(node);
                }
            }
        }

        /// <summary>
        /// Renames the given node, also in memory by the given name.
        /// </summary>
        /// <param name="item">Node to be renamed.</param>
        /// <param name="newName">New node name.</param>
        public void RenameItem(ItemNode item, string newName)
        {
            var path = Path.GetDirectoryName(item.ItemPath);
            string newPathName = path + '\\' + newName;
            if (item.Type == NodeType.Folder)
            {
                Directory.Move(path + '\\' + item.Name, path + '\\' + newName);
            }
            else
            {
                File.Move(item.ItemPath, newPathName);
            }
            item.ItemPath = newPathName;
            item.Name = newName;
            foreach(var node in Successors)
            {
                node.RenamePathRecursively(node.ItemPath, path + '\\' + item.Name + '\\' + node.Name);
            }
        }

        public void RenamePathRecursively(string oldPath, string newPath)
        {
            ItemPath = ItemPath.Replace(oldPath, newPath);
            foreach(var item in Successors)
            {
                item.RenamePathRecursively(oldPath, newPath);
            }
        }

        /// <summary>
        /// Reverses the order of all nodes recursively.
        /// </summary>
        public void ReverseNodeOrder()
        {
            _successors.Reverse();
            foreach (var s in _successors) s.ReverseNodeOrder();
        }

        /// <summary>
        /// Sorts all nodes under this node by the given comparision.
        /// </summary>
        /// <param name="comparision">Method on which nodes should be sorted by recursively.</param>
        public void SortBy(Comparison<ItemNode> comparision)
        {
            _successors.Sort(comparision);
            foreach (var s in Successors)
            {
                s.SortBy(comparision);
            }
        }
        /// <summary>
        /// Converts passed DateTime into an int array with the order Day(0)..Second(5)
        /// </summary>
        private static Func<DateTime, int[]> DateTimeToArray = delegate (DateTime date)  
        {
            return new int[] { date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second };
        };
        public Comparison<ItemNode> ByCreationDate = delegate (ItemNode left, ItemNode right)
        {
            int[] leftDate = DateTimeToArray(left.CreationDate);
            int[] rightDate = DateTimeToArray(right.CreationDate);
            for(var i = 0; i < leftDate.Length; i++)
            {
                if (leftDate[i] > rightDate[i]) return  1; // left is bigger than right
                if (leftDate[i] < rightDate[i]) return -1; // left is smaller than right
            }
            return 0; // left and right are equal
        };
        public Comparison<ItemNode> ByUpdatedDate = delegate (ItemNode left, ItemNode right)
        {
            int[] leftDate = DateTimeToArray(left.UpdatedDate);
            int[] rightDate = DateTimeToArray(right.UpdatedDate);
            for (var i = 0; i < leftDate.Length; i++)
            {
                if (leftDate[i] > rightDate[i]) return 1; // left is bigger than right
                if (leftDate[i] < rightDate[i]) return -1; // left is smaller than right
            }
            return 0; // left and right are equal
        };
        public Comparison<ItemNode> ByName = delegate (ItemNode left, ItemNode right) 
        {
            for(var i = 0; i < left.Name.Length; i++)
            {
                if (i > right.Name.Length-1)      return  1; // left wins, because it has more characters
                if (char.ToLower(left.Name[i]) > char.ToLower(right.Name[i])) return  1; // left is bigger than right
                if (char.ToLower(left.Name[i]) < char.ToLower(right.Name[i])) return -1; // left is smaller than right
            }
            if(left.Name.Length < right.Name.Length) return -1; // right wins, because it has more characters
            return 0; // left and right are equal
        };

        /// <summary>
        /// Returns if the given item name is given in this successor list.
        /// </summary>
        /// <param name="itemName">Name of the item.</param>
        /// <returns>True if successor name is in list - false otherwise.</returns>
        public bool Contains(string itemName)
        {
            bool contains = false;
            foreach(var item in Successors)
            {
                contains |= item.Name == itemName;
            }
            return contains;
        }
        /// <summary>
        /// Getters/Setters
        /// </summary>
        public string Name { get => _name; set => _name = value; }
        public string ItemPath { get => _itemPath; set => _itemPath = value; }
        public NodeType Type { get => _type; set => _type = value; }
        public DateTime CreationDate { get => _creationDate; set => _creationDate = value; }
        public DateTime UpdatedDate { get => _updatedDate; set => _updatedDate = value; }
        public IList<ItemNode> Successors => _successors;
        public bool IsFile() => Type == NodeType.File;
        public bool IsFolder() => Type == NodeType.Folder;
    }
}
