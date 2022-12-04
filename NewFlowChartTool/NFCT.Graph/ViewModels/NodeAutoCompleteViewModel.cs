using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using FlowChart.Type;
using FlowChartCommon;
using Prism.Mvvm;
using Type = System.Type;

namespace NFCT.Graph.ViewModels
{
    public class PromptItemViewModel : BindableBase
    {
        #region Static

        static PromptItemViewModel()
        {
            InactivatedBackGroundBrush = Application.Current.FindResource("HightlightBackGroundBrush") as SolidColorBrush ?? CanvasNodeResource.ErrorBrush;
            DefaultBackGroundBrush = Application.Current.FindResource("BackGroundBrush") as SolidColorBrush ?? CanvasNodeResource.ErrorBrush;
            SelectedBackGroundBrush = Application.Current.FindResource("NodeActionBackGround") as SolidColorBrush ?? CanvasNodeResource.ErrorBrush;

            UsedPrompts = new HashSet<string>();
        }

        private static Brush SelectedBackGroundBrush;
        private static Brush InactivatedBackGroundBrush;
        private static Brush DefaultBackGroundBrush;

        public static HashSet<string> UsedPrompts;

        #endregion

        public enum PromptType
        {
            Default = 0,
            Variable = 1,
            Event = 2,
            Action = 3,
            Method = 4,
            Property = 5,
        }

        public PromptItemViewModel(PromptType tp, string text)
        {
            Type = tp;
            _text = text;
        }

        public bool IsFunction { get; }
        public int ParameterCount { get; set; }
        public PromptType Type { get; private set; }

        private string _text;
        public string Text
        {
            get => IsFunction ? $"{_text}()" : _text;
            set => _text = value;
        }

        public string? Description { get; set; }

        public double Priority { get; set; }
        public double Weight;
        public int UseCount;

        public Brush BackGround => IsSelected ? 
            (NodeAutoCompleteViewModel.IsActivate ? SelectedBackGroundBrush : InactivatedBackGroundBrush) 
            : DefaultBackGroundBrush;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { SetProperty(ref _isSelected, value); RaisePropertyChanged(nameof(BackGround));}
        }
    }

/*
                  |--光标位置 
    ->|-      -|<- prehold  
abc + test(a,b).ob+xyz
             ->| |<--replacepart 
      |-querycmd-|
---leftcmd ------|---rightcmd
*/
    public class NodeAutoCompleteViewModel : BindableBase
    {
        public NodeAutoCompleteViewModel()
        {
            Prompts = new ObservableCollection<PromptItemViewModel>();
            _promptList = new List<PromptItemViewModel>();

            Prompts.Add(new PromptItemViewModel(PromptItemViewModel.PromptType.Default, "foo"));
            Prompts.Add(new PromptItemViewModel(PromptItemViewModel.PromptType.Default, "bar"));
            Prompts.Add(new PromptItemViewModel(PromptItemViewModel.PromptType.Default, "abc"));
            Prompts.Add(new PromptItemViewModel(PromptItemViewModel.PromptType.Default, "123"));
        }

        public static bool IsActivate { get; set; }
        public BaseNodeViewModel? Node { get; set; }
        private string _text;
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value, nameof(Text));
        }
        private GraphPaneViewModel? GraphVm => Node?.Owner;

        private List<PromptItemViewModel> _promptList;
        public ObservableCollection<PromptItemViewModel> Prompts { get; set; }

        #region funcitons

        private string _prehold;    // 智能提示上词时，左侧不会被替换的部分
        private string _replace;    // 智能提示上词时，会被替换的部分
        private string _posthold;
        private FlowChart.Type.Type? _ownerType;

        private void _splitInputString(string leftCmd, string rightCmd)
        {
            int leftLength = leftCmd.Length;
            _prehold = null;
            int breakChar;  // 记录分隔符号
            for (int i = leftLength - 1; i >= 0; i--)
            {
                char c = leftCmd[i];
                if (!(char.IsLetterOrDigit(c) || c == '_'))
                {
                    breakChar = c;
                    if (c == '$') i--;
                    _prehold = leftCmd.Substring(0, i + 1);
                    _replace = leftCmd.Substring(i + 1);
                    // 如果分隔符是. 那么要对.之前的进行类型判断
                    if (breakChar == '.')
                    {
                        string ownerString = GetOwnerString(leftCmd.Substring(0, i));
                        _ownerType = GetOwnerType(ownerString);
                        //Logger.DBG(string.Format("owner type is {0} {1}, left {2}", ownerString, _ownerType, leftCmd));
                    }
                    else
                    {
                        _ownerType = BuiltinTypes.GlobalType;
                    }

                    break;
                }
            }

            if (_prehold == null)
            {
                _prehold = "";
                _replace = leftCmd;
                _ownerType = BuiltinTypes.GlobalType;
            }
            _posthold = rightCmd;
            //Logger.DBG(string.Format("l&r {0} | {1}", leftCmd, rightCmd));
            //Logger.DBG(string.Format("p&r {0} | {1}", _prehold, _replace));
        }

        private string GetOwnerString(string leftCmd)
        {
            int leftLength = leftCmd.Length;
            int rightParenthesisCount = 0;
            bool inParentheses = false;
            List<char> charList = new List<char>();
            for (int i = leftLength - 1; i >= 0; i--)
            {
                char c = leftCmd[i];
                if (c == ')')
                {
                    rightParenthesisCount++;
                    if (!inParentheses)
                    {
                        charList.Add(c);
                        inParentheses = true;
                    }
                }
                else if (c == '(')
                {
                    if (rightParenthesisCount == 0) break;
                    rightParenthesisCount--;
                    if (rightParenthesisCount == 0) inParentheses = false;
                }
                else if (",=><+-*/".Contains(c) && !inParentheses)
                {
                    break;
                }
                if (!inParentheses)
                {
                    charList.Add(c);
                }
            }
            charList.Reverse();
            string ownerStr = new string(charList.ToArray());
            return ownerStr;
        }

        private FlowChart.Type.Type? GetOwnerType(string ownerStr)
        {
            if (GraphVm == null)
                return null;
            var project = GraphVm.Graph.Project;
            var builder = project.Builder;
            return builder.GetTextType(GraphVm.Graph, ownerStr);
        }

        #endregion

        #region prepare promotions
        public Method? OutsideFuncInfo { get; set; }
        public int ParameterPos { get; set; }

        public void PreparePromptList(string leftCmd, string rightCmd)
        {
            _splitInputString(leftCmd, rightCmd);
            
            // 首先判断是否在函数内部
            int leftPos = GetUnmatchLeftParenthesesPos(leftCmd);
            if (leftPos > 0)
            {
                // 获得函数信息 函数信息包括调用者类型，函数名称
                // 先获得函数名
                int i = leftPos - 1;
                List<char> charList = new List<char>();
                string funcName = "";
                for (; i >= 0; i--)
                {
                    char c = leftCmd[i];
                    if (char.IsLetterOrDigit(c) || c == '_')
                    {
                        charList.Add(c);
                    }
                    else
                    {
                        break;
                    }
                }
                charList.Reverse();
                funcName = new string(charList.ToArray());
                // 然后获得函数所属的对象的类型
                var type = BuiltinTypes.GlobalType;
                if (i > 0 && leftCmd[i] == '.')
                {
                    string ownerString = GetOwnerString(leftCmd.Substring(0, i));
                    type = GetOwnerType(ownerString);
                }
                Logger.DBG(string.Format("inside function {0} of type {1}", funcName, type));

                OutsideFuncInfo = null;

                if (type != null)
                {
                    OutsideFuncInfo = type.FindMember(funcName) as Method;
                }
                if (OutsideFuncInfo == null && type == BuiltinTypes.GlobalType && GraphVm != null) //TODO
                {
                    type = GraphVm.Graph.Type;
                    OutsideFuncInfo = type.FindMember(funcName) as Method;
                }
               
                // 如果接口参数获得成功
                if (OutsideFuncInfo != null)
                {
                    ParameterPos = ParseParametersInCmd(leftCmd.Substring(leftPos + 1));
                }

            }

            // 下面就要准备提示词的内容
            _promptList.Clear();
            var graph = GraphVm.Graph;
            if (_ownerType != null)
            {
                if (_ownerType is EventType) //前面是事件
                {
                    AddPromptEventParameters(_ownerType.Name);
                }
                else if (_ownerType == BuiltinTypes.GlobalType)
                {
                    // 所有的内容都添加
                    AddPromptEvents(graph.Type);
                    AddPromptVariables();
                    AddPromptCategoryMembers(BuiltinTypes.GlobalType);
                    AddPromptCategoryMembers(graph.Type);
                    
                    AddPromptFunctionParameters(OutsideFuncInfo);

                }
                else
                {
                    AddPromptEvents(_ownerType, true); // 对象后面也可以.Onxx
                    if (AddPromptCategoryMembers(_ownerType) == false) // 如果加载类型接口失败，那么加载所有接口
                    {
                        AddPromptCategoryMembers(BuiltinTypes.GlobalType);
                        AddPromptCategoryMembers(GraphVm.Graph.Type);
                    }
                }
            }
            SortPromptList(_replace);

        }

        private int ParseParametersInCmd(string paraStr)
        {
            int paraPos = 0;
            if (string.IsNullOrEmpty(paraStr)) return 0;
            int leftParenthesisCount = 0;
            int length = paraStr.Length;
            bool inParentheses = false;
            for (int i = 0; i < length; i++)
            {
                char c = paraStr[i];
                if (c == '(' || c == '{')
                {
                    leftParenthesisCount++;
                    if (!inParentheses)
                    {
                        inParentheses = true;
                    }
                }
                else if (c == ')' || c == '}')
                {
                    // 碰到孤单的 ) 表示参数字符串结束了
                    if (leftParenthesisCount == 0)
                    {
                        break;
                    }
                    leftParenthesisCount--;
                    if (leftParenthesisCount == 0)
                    {
                        inParentheses = false;
                    }
                }
                else if (c == ',' && !inParentheses)
                {
                    paraPos++;
                }
            }
            return paraPos;
        }

        private int GetUnmatchLeftParenthesesPos(string leftCmd)
        {
            if (string.IsNullOrEmpty(leftCmd)) return -1;
            int rightParenthesisCount = 0;
            int length = leftCmd.Length;
            for (int i = length - 1; i >= 0; i--)
            {
                char c = leftCmd[i];
                if (c == ')')
                {
                    rightParenthesisCount++;
                }
                else if (c == '(')
                {
                    if (rightParenthesisCount == 0)
                        return i;
                    else
                    {
                        rightParenthesisCount--;
                    }
                }
            }
            return -1;
        }

        private bool AddPromptCategoryMembers(FlowChart.Type.Type? type)
        {
            if (type == null) return false;
            //if (type.StartsWith("Table<") && type.Length > 7)
            //{
            //    type = type.Substring(6, type.Length - 7);
            //}
            var prompts = new List<PromptItemViewModel>();

            var handledCategories = new HashSet<FlowChart.Type.Type>();
            Action<FlowChart.Type.Type> _addPromptMember = null;
            _addPromptMember = c =>
            {
                if (!handledCategories.Contains(c))
                {
                    handledCategories.Add(c);

                    c.MemberDict.Values.ToList().ForEach(member =>
                    {
                        string typeStr = "V";
                        var promptType = PromptItemViewModel.PromptType.Default;
                        if (member is FlowChart.Type.Property)
                            promptType = PromptItemViewModel.PromptType.Property;
                        else if (member is Method method)
                        {
                            promptType = method.Parameters.Count == 0 ? PromptItemViewModel.PromptType.Action : PromptItemViewModel.PromptType.Method;
                        }
                        
                        prompts.Add(new PromptItemViewModel(promptType, member.Name)
                        {
                            Description = member.Description,
                            //IsFunction = !(member is Property),
                            //ParameterCount = member.Parameters == null ? 0 : member.Parameters.Count,
                            //UseCount = member.UseCount
                        });
                    });

                    c.BaseTypes.ForEach(p =>
                    {
                        _addPromptMember(p);
                    });
                }
            };

            _addPromptMember(type);
            _promptList.AddRange(prompts);

            return true;
        }

        private void AddPromptEvents(FlowChart.Type.Type type, bool startWithOn = false)
        {
            // 实现继承功能
            var types = new HashSet<FlowChart.Type.Type>();
            Action<FlowChart.Type.Type>? _addParentType = null;
            _addParentType = c =>
            {
                if (!types.Contains(c))
                {
                    types.Add(c);
                    c.BaseTypes.ForEach(p =>
                    {
                        _addParentType?.Invoke(p);
                    });
                }
            };
            var ca = GraphVm.Graph.Type;
            _addParentType(ca);

            var prompts = new List<PromptItemViewModel>();
            var project = GraphVm.Graph.Project;
            project.EventDict.Values.ToList().ForEach(ev =>
            {
                // Event可能包含Type多种类型,此时任一类型匹配都加入到对应类型的提示中,
                // Type为空则是当前图类型ownerEvent事件
                prompts.Add(new PromptItemViewModel(PromptItemViewModel.PromptType.Event, ev.Name) { Description = ev.Description});
                //if (member is Event)
                //{
                //    var curEvent = member as Event;
                //    bool ownerEvent = curEvent.Types.Count == 0;
                //    // ownerEvent的判断放在前面，对于逆水寒，可以短路后面的操作
                //    if (ownerEvent || types.Contains(member.Type) ||
                //        curEvent.Types.Exists(t => types.Contains(t)))
                //    {
                //        if (!startWithOn)
                //        {
                //            prompts.Add(new Prompt("E") { Text = member.Name });
                //        }
                //        prompts.Add(new Prompt("E") { Text = "On" + member.Name });
                //    }
                //}
            });
            _promptList.AddRange(prompts);
        }

        private void AddPromptVariables()
        {
            var prompts = new List<PromptItemViewModel>();
            //Unit.Unit.Parameters.ForEach(p => prompts.Add(new Prompt("V"){Text = "$" + p.Name}));
            foreach (var variableViewModel in GraphVm.VariablesPanel.Variables)
            {
                prompts.Add(new PromptItemViewModel(PromptItemViewModel.PromptType.Variable, "$" + variableViewModel.Name)
                    {Description = variableViewModel.Description});
            }
            _promptList.AddRange(prompts);
        }

        private void AddPromptEventParameters(string eventName)
        {
            var prompts = new List<PromptItemViewModel>();
            var eventType = GraphVm.Graph.Project.GetEvent(eventName);
            
            if (eventType != null)
            {
                eventType.Parameters.ForEach(p => prompts.Add(
                    new PromptItemViewModel(PromptItemViewModel.PromptType.Default, p.Name) { Description = p.Description }));
            }
            _promptList.AddRange(prompts);
        }

        // 将函数的参数名以 xx = 的形式添加到候选词列表中
        private void AddPromptFunctionParameters(FlowChart.Type.Method? func)
        {
            if (func == null)
                return;
            var prompts = new List<PromptItemViewModel>();

            if (func.Parameters.Count > 0)
            {
                func.Parameters.ForEach(p => prompts.Add(
                    new PromptItemViewModel(PromptItemViewModel.PromptType.Default, $"{p.Name} = ") 
                        { Description = p.Description, Priority = 10 }));
            }
            _promptList.AddRange(prompts);
        }

        private void AddPromptEnum(string enumType)
        {
            //var prompts = new List<PromptItemViewModel>();

            //var enumTypeList = MainViewModel.Inst.ActiveProject.EnumTypeList;
            //foreach (var enumTypeObj in enumTypeList)
            //{
            //    if (enumTypeObj.Name == enumType)
            //    {
            //        enumTypeObj.Values.ForEach(v => prompts.Add(new Prompt()
            //        {
            //            Text = v.Value,
            //            Description = v.Description,
            //            IsSingleLine = true,
            //            Priority = 5
            //        }));
            //    }
            //}
            //Prompts.AddRange(prompts);
        }

        #endregion

        public void SortPromptList(string head)
        {
            //Console.WriteLine("size: " + _promptList.Count);
            head = head.ToLower();
            // 计数匹配度
            foreach (var prompt in _promptList)
            {
                string name = prompt.Text.ToLower();
                int matchCount = 0; // 两字符串匹配的字符数量

                int nameLen = name.Length;
                int headLen = head.Length;
                int len = nameLen < headLen ? nameLen : headLen;
                for (int i = 0; i < len; i++)
                {
                    if (name[i] == head[i])
                    {
                        matchCount++;
                    }
                    else
                    {
                        break;
                    }
                }


                int headIndex = name.IndexOf(head);
                if (headIndex == -1) headIndex = 100;

                prompt.Weight = matchCount - headIndex;
                if (PromptItemViewModel.UsedPrompts.Contains(name))
                {
                    prompt.Priority = 20;
                }
            }

            _promptList.Sort((a, b) =>
            {
                // 注意 weight越大，排序时应该放在前面，所以要加个-号
                int weightCompare = a.Priority.CompareTo(b.Priority);
                if (weightCompare != 0) return -weightCompare;

                weightCompare = a.Weight.CompareTo(b.Weight);
                if (weightCompare != 0) return -weightCompare;

                weightCompare = a.UseCount.CompareTo(b.UseCount);
                if (weightCompare != 0) return -weightCompare;

                return String.Compare(a.Text, b.Text, StringComparison.OrdinalIgnoreCase);
            });
            bool dashed = false;
            //_promptList.ForEach(p => Prompts.Add(p));
            // 输入字符串长度小于等于1时，输出排序后的全部结果
            if (head.Length <= 1)
            {
                Prompts.Clear();
                _promptList.ForEach(p => Prompts.Add(p));
            }
            // 长度大于1时，只输出匹配度高的结果
            else
            {
                var addList = new List<PromptItemViewModel>();
                _promptList.ForEach(p =>
                {
                    if (p.Weight > -50)
                    {
                        addList.Add(p);
                    }
                    else // 特殊情况
                    {
                        // 在输入变量时，所有的变量都要列在提示框里面
                        if (head.StartsWith("$") && p.Text.StartsWith("$"))
                        {
                            addList.Add(p);
                        }
                    }

                    // p.IsDashed = p.IsSelected;
                    // if (p.IsDashed) Console.WriteLine("Dashed");
                    // if (!dashed && p.IsDashed)
                    //     dashed = true;
                });
                if (addList.Count > 0)
                {
                    Prompts.Clear();
                    addList.ForEach(p => Prompts.Add(p));
                }
            }
            // 如果默认提示匹配结果较差，比如输入的是接口的中文描述，则请求根据描述搜索接口
            //AddSmartPrompts(head);

            // if (Prompts.Count > 0 && !dashed)
            // {
            //     Prompts[0].IsDashed = true;
            //     //Prompts[0].IsSelected = true;
            // }
            //Logger.DBG("prompt count " + Prompts.Count);
        }

        public string ApplyPrompt(PromptItemViewModel prompt, ref int cursorPos)
        {
            string replaceResult = _prehold + prompt.Text;
            cursorPos = replaceResult.Length;
            if (prompt.IsFunction && prompt.ParameterCount > 0)
            {
                cursorPos--;
            }
            return replaceResult + _posthold;
        }




    }
}
