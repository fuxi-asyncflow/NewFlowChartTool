using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace FlowChart.Core
{
    public interface IProjectFactory
    {
        void Create(Project project);
        void Save(Project project);
    }

    public class Project
    {
        public Project(IProjectFactory factory)
        {
            Factory = factory;
            Root = new Folder("");
            TypeDict = new Dictionary<string, Type.Type>();
            GraphDict = new Dictionary<string, Graph>();
        }

        public static Guid GenUUID()
        {
            return Guid.NewGuid();
        }

        #region PROPERTIES
        public string Path { get; set; }
        public Folder Root { get; set; }
        public Dictionary<string, Type.Type> TypeDict { get; set; }
        IProjectFactory Factory { get; set; }
        public Dictionary<string, Graph> GraphDict { get; set; }
        #endregion

        public bool Load()
        {
            Factory?.Create(this);
            return true;
        }

        public void AddType(Type.Type type)
        {
            TypeDict.Add(type.Name, type);
        }

        public Type.Type GetGlobalType()
        {
            return new Type.Type("Global");
        }

        public void AddGraph(Graph graph)
        {
            if (GraphDict.ContainsKey(graph.Path))
            {
                var originPath = graph.Path;
                graph.Path = originPath + "_dup";
                Console.WriteLine($"exist same path when add graph: {originPath} rename to {graph.Path}");
            }
            var paths = graph.Path.Split(".");
            graph.Name = paths.Last();
            var folder = Root;
            
            foreach(var path in paths.SkipLast(1))
            {
                folder = folder.GetOrCreateSubFolder(path);
                if(folder == null)
                {
                    break;
                }
            }
            if (folder == null)
                return;
            folder.AddChild(graph);
            Console.WriteLine($"graph path: {graph.Path}");
            
            GraphDict.Add(graph.Path, graph);
            graph.Project = this;
        }
    }
}
