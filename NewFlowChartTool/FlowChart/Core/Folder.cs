using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowChart.Core
{
    public class Folder : Item
    {
        public Folder(string name)
        : base(name)
        {
            Children = new List<Item>();
            Items = new Dictionary<string, Item>();
        }
        public Project Project { get; set; }
        public Type.Type? Type { get; set; }
        public List<Item> Children { get; set; }
        public Dictionary<string, Item> Items { get; set; }
        public Folder? GetOrCreateSubFolder(string subFolderName)
        {
            var child = Items.GetValueOrDefault(subFolderName);
            if(child == null)
            {
                var item = new Folder(subFolderName) {Project = Project};
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

        public void AddChild(Item item)
        {
            if(Items.ContainsKey(item.Name))
            {
                return;
            }

            Items.Add(item.Name, item);
            Children.Add(item);
        }

        public bool ContainsChild(string name)
        {
            return Items.ContainsKey(name);
        }
    }
}
