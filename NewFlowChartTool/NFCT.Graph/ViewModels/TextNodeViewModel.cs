using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FlowChart.AST;
using Prism.Mvvm;
using FlowChart.Core;
using FlowChart.Common;
using Prism.Regions;


namespace NFCT.Graph.ViewModels
{
    public class TextTokenViewModel : BindableBase
    {
        public TextTokenViewModel(string text)
        {
            Text = text;
        }
        public string Text { get; set; }
        public TextToken.TokenType Type { get; set; }
        private string? _tipText;
        public string? TipText
        {
            get => _tipText;
            set => SetProperty(ref _tipText, value);
        }
        public Brush Color => CanvasNodeResource.NodeTokenBrushes[(int)Type];
        public int FontSize => Type == TextToken.TokenType.Superscript ? 8 : 16;

        public void OnThemeSwitch()
        {
            RaisePropertyChanged(nameof(Color));
        }
    }

    public class NodeContent : BindableBase
    {
#if DEBUG
        public NodeContent()
        {

        }
#endif
        public NodeContent(BaseNodeViewModel baseNode)
        {
            BaseNode = baseNode;
        }

        public BaseNodeViewModel BaseNode { get; set; }

        public virtual void OnThemeSwitch()
        {
        }

        public virtual void OnParseEnd(ParseResult pr)
        {

        }
        public virtual void EnterEditingMode()
        {

        }

        public virtual void ExitEditingMode(NodeAutoCompleteViewModel acVm, bool save)
        {

        }

        public virtual void Ellipsis(bool ellipsis)
        {

        }
    }

    public class TextNodeViewModel : NodeContent
    {
#if DEBUG
        public TextNodeViewModel()
        {
            var graph = new FlowChart.Core.Graph("design");
            graph.AddNode(new StartNode());
            Node = new TextNode();
            Node.SetText("hello");
            BaseNode.Owner = new GraphPaneViewModel(graph);
            BaseNode.BgType = BaseNodeViewModel.NodeBgType.CONDITION;
            Tokens = new ObservableCollection<TextTokenViewModel>();
            Tokens.Add(new TextTokenViewModel("$a") { Type = TextToken.TokenType.Variable });
            Tokens.Add(new TextTokenViewModel(" = ") { Type = TextToken.TokenType.Default });
            Tokens.Add(new TextTokenViewModel("Func") { Type = TextToken.TokenType.Member });
            Tokens.Add(new TextTokenViewModel("(") { Type = TextToken.TokenType.Default });
            Tokens.Add(new TextTokenViewModel("123") { Type = TextToken.TokenType.Number });
            Tokens.Add(new TextTokenViewModel(", ") { Type = TextToken.TokenType.Default });
            Tokens.Add(new TextTokenViewModel("\"hello\"") { Type = TextToken.TokenType.String });
            Tokens.Add(new TextTokenViewModel(")") { Type = TextToken.TokenType.Default });
        }
#endif
        public new TextNode Node { get; set; }
        public string Text { get => Node.Text; }
        public ObservableCollection<TextTokenViewModel> Tokens { get; set; }
        private List<TextTokenViewModel> _allTokens;
        public GraphPaneViewModel Owner => BaseNode.Owner;
        public TextNodeViewModel(BaseNodeViewModel baseNode, TextNode node) :base(baseNode)
        {
            Node = node;
            _allTokens = new List<TextTokenViewModel>();
            Tokens = new ObservableCollection<TextTokenViewModel>();
            Tokens.Add(new TextTokenViewModel(Node.Text) {Type = TextToken.TokenType.Default});
        }

        public override void ExitEditingMode(NodeAutoCompleteViewModel acVm, bool save)
        {
            // avoid func is called twice: first called by Key.Esc, then called by LostFocus
            if (BaseNode.IsEditing == false)
                return;
            Logger.DBG($"[{nameof(TextNodeViewModel)}] ExitEditingMode");
            if (save && Node is TextNode textNode)
            {
                var inputText = acVm.Text;

                if (ControlNodeViewModel.MaybeControlNodeViewModel(inputText))
                {
                    var str = ControlNodeViewModel.ReplaceText(inputText, Owner);
                    if(str != null)
                        inputText = str;
                }

                Owner.ChangeNodeContentOperation(textNode, inputText);

                Logger.DBG($"node text is change to {Node.Text}");
                RaisePropertyChanged(nameof(Text));
                Owner.NeedLayout = true;
                Owner.Build();
            }

            BaseNode.IsEditing = false;
        }

        public override string ToString()
        {
            return $"[{nameof(TextNodeViewModel)}] {Text}";
        }

        public override void OnThemeSwitch()
        {
            RaisePropertyChanged(nameof(BaseNode.BgType));
            //TODO some graph theme take effect after close and open again
            foreach (var textTokenViewModel in Tokens)
            {
                textTokenViewModel.OnThemeSwitch();
            }
        }

        public override void OnParseEnd(ParseResult pr)
        {
            if (ControlNodeViewModel.MaybeControlNodeViewModel(Text))
            {
                var controlNodeVm = new ControlNodeViewModel(BaseNode);
                controlNodeVm.ParseText(Node.Text);
                BaseNode.ReplaceContent(controlNodeVm);
                if (controlNodeVm.ControlFuncName == "repeat")
                {
                    controlNodeVm.HandleRepeatNode(pr);
                }
                return;
            }

            if (pr.Tokens != null)
            {
                var project = Owner.Graph.Project;
                Tokens.Clear();
                _allTokens.Clear();
                pr.Tokens.ForEach(token =>
                {
                    if (token.End > Text.Length)
                    {
                        if(Text.Length != 0)
                            Logger.WARN($"invalid token {token.Start}:{token.End} in node {Text} ");
                    }
                    else
                    {
                        var tk = new TextTokenViewModel(Text.Substring(token.Start, token.End - token.Start))
                        {
                            Type = token.Type
                        };
                        _allTokens.Add(tk);
                        Tokens.Add(tk);

                        // add superscript
                        var refData = project.GetRefData(tk.Text);
                        if (refData != null && !string.IsNullOrEmpty(refData.Abbr))
                        {
                            var refTk = new TextTokenViewModel(refData.Abbr) { Type = TextToken.TokenType.Superscript };
                            _allTokens.Add(refTk);
                            Tokens.Add(refTk);
                            if (!string.IsNullOrEmpty(refData.Description))
                                tk.TipText = refData.Description;
                            Logger.DBG($"token {refData.Content} ref to {refData.Abbr} {refData.Description}");
                        }
                    }
                });
            }

            BaseNode.ErrorMessage = pr.IsError ? pr.ErrorMessage : null;
            BaseNode.WarningMessage = pr.IsWarning ? pr.WarningMessage : null;
            Owner.NeedLayout = true;
        }

        public override void Ellipsis(bool ellipsis)
        {
            const int MAX_LENGTH = 20;
            var queue = new Queue<TextTokenViewModel>();
            var stack = new Stack<TextTokenViewModel>();
            int length = 0;
            if (ellipsis == true && Text.Length > MAX_LENGTH)
            {
                for (int i = 0; i < _allTokens.Count; i++)
                {
                    int index = i >> 1;
                    if (i % 2 == 0)
                    {
                        var token = _allTokens[index];
                        length += token.Text.Length;
                        if (length > MAX_LENGTH)
                            break;
                        queue.Enqueue(token);
                        
                    }
                    else
                    {
                        var token = _allTokens[_allTokens.Count - 1 - index];
                        length += token.Text.Length;
                        if (length > MAX_LENGTH)
                            break;
                        stack.Push(token);
                    }
                }
                Tokens.Clear();
                foreach (var token in queue)
                {
                    Tokens.Add(token);
                }
                Tokens.Add(new TextTokenViewModel(" ... "));
                foreach (var token in stack)
                {
                    Tokens.Add(token);
                }
            }
            else
            {
                Tokens.Clear();
                _allTokens.ForEach(Tokens.Add);
            }
        }
    }

    public class StartNodeViewModel : NodeContent
    {
        public new StartNode Node { get; set; }
        public StartNodeViewModel(BaseNodeViewModel baseNode, StartNode node) : base(baseNode)
        {
            Node = node;
        }
    }

}
