using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FlowChart.AST;
using FlowChart.Core;
using FlowChart.Common;

namespace NFCT.Graph.ViewModels
{
    internal class ControlNodeViewModel : NodeContent
    {
        static ControlNodeViewModel()
        {
            ControlFuncNames = new HashSet<string>()
            {
                "stopnode", "stopflow", "gotonode", "waitall", "repeat"
            };
            
        }
        public ControlNodeViewModel(BaseNodeViewModel baseNode) : base(baseNode)
        {
            ParamNodes = new ObservableCollection<BaseNodeViewModel>();
        }

        public string EditingText { get; set; }

        public static HashSet<string> ControlFuncNames;

        private GraphPaneViewModel Owner => BaseNode.Owner;

        public string? ControlFuncName { get; set; }
        public ObservableCollection<BaseNodeViewModel> ParamNodes { get; set; }

        public static bool MaybeControlNodeViewModel(string text)
        {
            var left = text.IndexOf('(');
            var right = text.IndexOf(')');
            if (left == -1 || right == -1 || left > right)
            {
                return false;
            }

            var startText = text.Substring(0, left).Trim();
            if (!ControlFuncNames.Contains(startText))
            {
                return false;
            }

            return true;
        }

        public override void EnterEditingMode()
        {
            var paramStr = string.Join(", ", ParamNodes.ToList().ConvertAll(node => node.Id.ToString()));
            EditingText = $"{ControlFuncName}({paramStr})";
            //BaseNode.EnterEditingMode();
        }

        public override void ExitEditingMode(NodeAutoCompleteViewModel acVm, bool save)
        {
            // avoid func is called twice: first called by Key.Esc, then called by LostFocus
            if (BaseNode.IsEditing == false)
                return;
            Logger.DBG($"[{nameof(ControlNodeViewModel)}] ExitEditingMode");
            if (save && BaseNode.Node is TextNode textNode)
            {
                var inputText = acVm.Text;
                if (MaybeControlNodeViewModel(inputText) && ParseText(inputText)) 
                {
                    var paramStr = string.Join(", ", ParamNodes.ToList().ConvertAll(node => $"\"{node.Node.Uid.ToString()}\""));
                    textNode.Text = $"{ControlFuncName}({paramStr})";
                }
                else
                    textNode.Text = acVm.Text;
                Logger.DBG($"node text is change to {textNode.Text}");
                //RaisePropertyChanged(nameof(Text));
                Owner.NeedLayout = true;
                Owner.Build();
            }

            BaseNode.IsEditing = false;
        }

        public void HandleRepeatNode(ParseResult pr)
        {
            var Node = BaseNode.Node;
            var graph = Node.OwnerGraph;
            var subNodes = graph.FindSubGraph(Node);

            var content = Node.Content ?? new GenerateContent();
            content.Type = GenerateContent.ContentType.CONTROL;
            content.Contents.Clear();
            content.Contents.Add("repeat");

            subNodes.ForEach(node =>
            {
                content.Contents.Add(node.Uid.ToString());
            });
        }

        public override void OnParseEnd(ParseResult pr)
        {
            var Node = BaseNode.Node;
            if (Node is not TextNode textNode)
                return;
            if (!MaybeControlNodeViewModel(textNode.Text))
            {
                var textNodeVm = new TextNodeViewModel(BaseNode, textNode);
                BaseNode.ReplaceContent(textNodeVm);
                textNodeVm.OnParseEnd(pr);
                return;
            }

            if (ControlFuncName == "repeat")
            {
                HandleRepeatNode(pr);
            }

            if (pr.IsError)
                BaseNode.BgType = BaseNodeViewModel.NodeBgType.ERROR;
            else if (ControlFuncName == "waitall")
                BaseNode.BgType = BaseNodeViewModel.NodeBgType.WAIT;
            else
                BaseNode.BgType = BaseNodeViewModel.NodeBgType.ACTION;
            RaisePropertyChanged(nameof(BaseNode.BgType));
        }

        public static string? ReplaceText(string text, GraphPaneViewModel graphVm)
        {
            var left = text.IndexOf('(');
            var right = text.IndexOf(')');
            if (left == -1 || right == -1 || left > right)
            {
                //TODO change node type to TextNode
                return null;
            }

            var startText = text.Substring(0, left).Trim();
            if (!ControlFuncNames.Contains(startText))
            {
                //TODO change node type to TextNode
                return null;
            }

            var funcName = startText;
            var paramStr = text.Substring(left + 1, right - left - 1).Trim();
            var paramList = paramStr.Split(',').ToList().ConvertAll(s => s.Trim());
            var ParamNodes = new List<BaseNodeViewModel>();
            paramList.ForEach(idStr =>
            {
                int id;
                if (Int32.TryParse(idStr, out id))
                {
                    if (id < graphVm.Nodes.Count)
                        ParamNodes.Add(graphVm.Nodes[id]);
                    else
                        Logger.WARN($"cannot find node with id `{id}`");

                }
                else
                {
                    Node? node = null;
                    if (idStr.StartsWith('"'))
                        idStr = idStr.Trim('"');
                    if (Guid.TryParse(idStr, out Guid uid))
                    {
                        node = graphVm.Graph.GetNode(uid);
                    }

                    if (node == null)
                        Logger.WARN($"cannot find node with uid `{idStr}`");
                    else
                    {
                        ParamNodes.Add(graphVm.NodeDict[node]);
                    }
                }
            });
            var str =  string.Join(", ", ParamNodes.ToList().ConvertAll(node => $"\"{node.Node.Uid.ToString()}\""));
            return $"{funcName}({str})";
        }

        public bool ParseText(string? text)
        {
            text ??= ((TextNode)(BaseNode.Node)).Text;
            var left = text.IndexOf('(');
            var right = text.IndexOf(')');
            if (left == -1 || right == -1 || left > right)
            {
                //TODO change node type to TextNode
                return false;
            }

            var startText = text.Substring(0, left).Trim();
            if (!ControlFuncNames.Contains(startText))
            {
                //TODO change node type to TextNode
                return false;
            }

            ControlFuncName = startText;
            var paramStr = text.Substring(left + 1, right - left - 1).Trim();
            var paramList = paramStr.Split(',').ToList().ConvertAll(s => s.Trim());
            ParamNodes.Clear();
            paramList.ForEach(idStr =>
            {
                int id;
                if (Int32.TryParse(idStr, out id))
                {
                    if (id < Owner.Nodes.Count)
                        ParamNodes.Add(Owner.Nodes[id]);
                    else
                        Logger.WARN($"cannot find node with id `{id}`");

                }
                else
                {
                    Node? node = null;
                    if (idStr.StartsWith('"'))
                        idStr = idStr.Trim('"');
                    if (Guid.TryParse(idStr, out Guid uid))
                    {
                        node = Owner.Graph.GetNode(uid);
                    }
                    
                    if (node == null)
                        Logger.WARN($"cannot find node with uid `{idStr}`");
                    else
                    {
                        ParamNodes.Add(Owner.NodeDict[node]);
                    }
                }
            });
            return true;
        }
    }
}
