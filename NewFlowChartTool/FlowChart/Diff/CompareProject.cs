using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Core;

namespace FlowChart.Diff
{
    public enum DiffState
    {
        NoChange = 0,
        Modify = 1,
        Add = 2,
        Remove = 3,
        Rename = 4
    }

    public class DiffNode
    {
        public DiffState State { get; set; }
        public Node? NewNode;
        public Node? OldNode;
    }

    public class DiffGraph
    {
        public DiffGraph()
        {
            Nodes = new List<DiffNode>();
        }
        public DiffState State { get; set; }
        public Graph? OldGraph;
        public Graph? NewGraph;
        public Graph ResultGraph;
        public List<DiffNode> Nodes;
    }

    public class CompareProject
    {
        public CompareProject(Project oldProject, Project newProject)
        {
            DiffResult = new List<DiffGraph>();
            OldProject = oldProject;
            NewProject = newProject;
        }

        public void Compare()
        {
            DiffResult.Clear();
            var oldDict = new Dictionary<Guid, Graph>();
            foreach (var graph in OldProject.GraphDict.Values)
            {
                oldDict.Add(graph.Uid, graph);
            }
            var newDict = new Dictionary<Guid, Graph>();
            foreach (var graph in NewProject.GraphDict.Values)
            {
                newDict.Add(graph.Uid, graph);
            }

            foreach (var kv in oldDict)
            {
                var uid = kv.Key;
                var oldGraph = kv.Value;
                if (newDict.ContainsKey(uid))
                {
                    var newGraph = newDict[uid];
                    newDict.Remove(uid);
                    var resultGraph = CompareGraph(oldGraph, newGraph);
                    if (resultGraph != null)
                    {
                        DiffResult.Add(resultGraph);
                    }
                }
                else
                {
                    DiffResult.Add(new DiffGraph()
                    {
                        OldGraph = kv.Value,
                        NewGraph = null,
                        ResultGraph = kv.Value,
                        State = DiffState.Remove
                    });
                }
            }

            foreach (var graph in newDict.Values)
            {
                DiffResult.Add(new DiffGraph()
                {
                    OldGraph = null,
                    NewGraph = graph,
                    ResultGraph = graph,
                    State = DiffState.Add
                });
            }
        }
        DiffGraph? CompareGraph(Graph oldGraph, Graph newGraph)
        {
            var nodes = new List<DiffNode>();
            var oldNodesDict = new Dictionary<Guid, Node>(oldGraph.NodeDict);
            var newNodesDict = new Dictionary<Guid, Node>(newGraph.NodeDict);
            foreach (var kv in oldNodesDict)
            {
                var uid = kv.Key;
                var oldNode = kv.Value;
                if (newNodesDict.ContainsKey(uid))
                {
                    var newNode = newNodesDict[uid];
                    newNodesDict.Remove(uid);
                    if (!CompareNode(oldNode, newNode))
                    {
                        nodes.Add(new DiffNode()
                        {
                            OldNode = oldNode,
                            NewNode = newNode,
                            State = DiffState.Modify
                        });
                    }
                }
                else
                {
                    nodes.Add(new DiffNode() {OldNode = oldNode, NewNode = null, State = DiffState.Remove});
                }
            }

            foreach (var kv in newNodesDict)
            {
                nodes.Add(new DiffNode() {OldNode = null, NewNode = kv.Value, State = DiffState.Add});
            }
            if(nodes.Count == 0)
                return null;
            return new DiffGraph() { OldGraph = oldGraph, NewGraph = newGraph, Nodes = nodes, State = DiffState.Modify};
        }

        bool CompareNode(Node oldNode, Node newNode)
        {
            var oldType = oldNode.GetType();
            if (oldType != newNode.GetType())
                return false;
            return oldNode.Equals(newNode);
        }

        public List<DiffGraph> DiffResult;

        public Project OldProject;
        public Project NewProject;
        public Project MergeProject;

        public void Print()
        {
            Console.WriteLine("==== diff result: ");
            if (DiffResult.Count == 0)
                Console.WriteLine("no change");
            foreach (var diffGraph in DiffResult)
            {
                if (diffGraph.State == DiffState.Remove)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    if(diffGraph.OldGraph != null)
                        Console.WriteLine($"--- {diffGraph.OldGraph.Path}, {diffGraph.OldGraph.Uid}");
                }
                else if (diffGraph.State == DiffState.Add)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    if (diffGraph.NewGraph != null)
                        Console.WriteLine($"+++ {diffGraph.NewGraph.Path}, {diffGraph.NewGraph.Uid}");
                }
                else if (diffGraph.State == DiffState.Modify)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    if (diffGraph.NewGraph != null)
                        Console.WriteLine($"=== {diffGraph.NewGraph.Path}, {diffGraph.NewGraph.Uid}");
                    foreach (var node in diffGraph.Nodes)
                    {
                        if (node.State == DiffState.Remove)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            if (node.OldNode != null)
                                Console.WriteLine($"--- {node.OldNode}, {node.OldNode.Uid}");
                        }
                        else if (node.State == DiffState.Add)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            if (node.NewNode != null)
                                Console.WriteLine($"+++ {node.NewNode}, {node.NewNode.Uid}");
                        }
                        else if (node.State == DiffState.Modify)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"*** {node.OldNode} -> {node.NewNode} {node.OldNode.Uid}");
                        }
                    }
                }
            }
        }
    }
}
