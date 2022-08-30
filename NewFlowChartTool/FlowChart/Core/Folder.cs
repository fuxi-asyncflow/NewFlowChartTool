using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Core
{
    public class Folder : TreeItem
    {
        public Folder(string name)
        : base(name)
        {
            Children = new List<TreeItem>();
            Items = new Dictionary<string, TreeItem>();
        }
        
        public List<TreeItem> Children { get; set; }
        public Dictionary<string, TreeItem> Items { get; set; }
        public Folder? GetOrCreateSubFolder(string subFolderName)
        {
            var child = Items.GetValueOrDefault(subFolderName);
            if(child == null)
            {
                var item = new Folder(subFolderName) {Project = Project, Type = Type};
                AddChild(item);
                return item;

            }
            else if(child is Folder)
            {
                return (Folder)child;
            }
            else
            {
                return null;
            }
        }

        public void AddChild(TreeItem item)
        {
            if(Items.ContainsKey(item.Name))
            {
                return;
            }

            item.Parent = this;
            Items.Add(item.Name, item);
            Children.Add(item);
        }

        public bool ContainsChild(string name)
        {
            return Items.ContainsKey(name);
        }

        public override void Rename(string newName)
        {
            var parentPath = Parent.JoinPath();
            //Debug.Assert(Path == $"{parentPath}.{Name}");

            Name = newName;
            var newPath = $"{parentPath}.{Name}";
            RenameChildren(newPath);

            RaiseRenameEvent(newName);
        }

        private void RenameChildren(string parentPath)
        {
            foreach (var treeItem in Children)
            {
                var path = $"{parentPath}.{Name}";
                if (treeItem is Folder folder)
                {
                    folder.RenameChildren(path);
                }
                else if (treeItem is Graph graph)
                {
                    graph.SetPath(path);
                }
            }
        }
    }
}
