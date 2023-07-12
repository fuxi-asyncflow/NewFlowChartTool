using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Common;
using FlowChart.Type;

namespace FlowChart.Core
{
    public class Folder : TreeItem
    {
        public Folder(string name)
        : base(name)
        {
            Children = new List<TreeItem>();
            Items = new Dictionary<string, TreeItem>();
            LocalSubGraphs = new List<SubGraphMethod>();
            LocalSubGraphDict = new Dictionary<string, SubGraphMethod>();
        }
        
        public List<TreeItem> Children { get; set; }
        public Dictionary<string, TreeItem> Items { get; set; }
        public List<SubGraphMethod> LocalSubGraphs { get; set; }
        public Dictionary<string, SubGraphMethod> LocalSubGraphDict { get; set; }

        public bool IsRootFolder => Project.Root == Parent;

        #region extra info

        public bool HasExtraInfo => _extra != null && _extra.Count > 0;

        public SortedDictionary<string, string>? _extra; // extra information, may be used for plugins

        public string? GetExtraProp(string name)
        {
            string? v = null;
            _extra?.TryGetValue(name, out v);
            return v;
        }

        public void SetExtraProp(string name, string value)
        {
            if (_extra == null)
                _extra = new SortedDictionary<string, string>();
            _extra[name] = value;
        }

        #endregion

        public void AddChild(TreeItem item)
        {
            if(Items.ContainsKey(item.Name))
            {
                Logger.ERR($"item exist when try to add item `{item.Path}` to folder `{Path}`");
                return;
            }

            item.Parent = this;
            Items.Add(item.Name, item);
            Children.Add(item);
            if (item is Graph graph && graph.IsLocalSubGraph)
            {
                AddLocalSubGraph(graph);
            }
        }

        public void AddLocalSubGraph(Graph graph)
        {
            if (LocalSubGraphDict.ContainsKey(graph.Name))
            {
                var m = LocalSubGraphDict[graph.Name];
                if (m.RelativeGraph == graph)
                    return;
                else
                {
                    LocalSubGraphDict.Remove(graph.Name);
                    LocalSubGraphs.Remove(m);
                }
            }

            var method = graph.ToMethod();
            LocalSubGraphs.Add(method);
            LocalSubGraphDict.Add(graph.Name, method);
        }

        public bool RemoveLocalSubGraph(Graph graph)
        {
            if (LocalSubGraphDict.ContainsKey(graph.Name))
            {
                var method = LocalSubGraphDict[graph.Name];
                LocalSubGraphDict.Remove(graph.Name);
                LocalSubGraphs.Remove(method);
                return true;
            }

            return false;
        }

        public bool RenameLocalSubGraph(Graph graph, string oldName)
        {
            var method = FindSubGraphMethod(oldName);
            if (method != null && method.RelativeGraph == graph)
            {
                method.Name = graph.Name;
                method.Update();
                LocalSubGraphDict.Remove(oldName);
                LocalSubGraphDict.Add(graph.Name, method);
                return true;
            }
            else
            {
                Logger.WARN($"rename local subgraph failed: cannot find method in folder with name: {oldName}");
                return false;
            }
        }

        public SubGraphMethod? FindSubGraphMethod(string name)
        {
            SubGraphMethod? method = null;
            LocalSubGraphDict.TryGetValue(name, out method);
            return method;
        }

        public bool ContainsChild(string name)
        {
            return Items.ContainsKey(name);
        }

        public string GetUnusedName(string name)
        {
            var originName = name;
            int idx = 1;
            while (Items.ContainsKey(name))
            {
                name = $"{originName}_{idx++}";
            }
            return name;
        }

        public override void Rename(string newName)
        {
            var parentPath = Parent.JoinPath();
            //Debug.Assert(Path == $"{parentPath}.{Name}");
            var oldPath = Path;
            Name = newName;
            var newPath = IsRootFolder ? Name : $"{parentPath}.{Name}";
            Path = newPath;
            Project?.RenameItem(this, oldPath, newPath);
            RenameChildren(newPath);

            RaiseRenameEvent(newName);
        }

        private void RenameChildren(string parentPath)
        {
            foreach (var treeItem in Children)
            {
                var path = $"{parentPath}.{treeItem.Name}";
                if (treeItem is Folder folder)
                {
                    var oldPath = folder.Path;
                    folder.Path = path;
                    Project?.RenameItem(folder, oldPath, path);
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
