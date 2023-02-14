using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using FlowChart.Core;
using FlowChart.Misc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using FlowChart.Type;
using HL.Manager;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using NewFlowChartTool.ViewModels.TypeDialog;
using NFCT.Common;
using NFCT.Common.Events;
using Prism.Commands;
using Prism.Events;
using Sprache;
using Type = FlowChart.Type.Type;

namespace NewFlowChartTool.ViewModels
{
    class ParameterViewModel : BindableBase
    {
        public ParameterViewModel(Parameter param)
        {
            Model = param;
            Name = param.Name;
            Type = param.Type.Name;
            DefaultValue = param.Default;
        }
        public Parameter Model { get; set; }
        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        private string _type;
        public string Type { get => _type; set => SetProperty(ref _type, value); }
        private string? _description;
        public string? Description { get => _description; set => SetProperty(ref _description, value); }
        private string _defaultValue;
        public string DefaultValue { get => _defaultValue; set => SetProperty(ref _defaultValue, value); }
        public void UpdateToModel(Project p)
        {
            if(Model.Name != Name)
                Model.Name = Name;
            if(Model.Description != Description)
                Model.Description = Description;
            if (Model.Default != DefaultValue)
                Model.Default = DefaultValue;
            if (Model.Type.Name != Type)
            {
                var tp = p.GetType(Type);
                if (tp == null)
                {

                }
                else
                {
                    Model.Type = tp;
                }
            }
        }
    }
    class TypeMemberViewModel : BindableBase
    {
        public TypeMemberViewModel(Member member)
        {
            Model = member;
            _name = member.Name;
            _type = member.Type.Name;
            Description = member.Description;
        }

        public void SetModel(Member member)
        {
            Model = member;
            Name = member.Name;
            Type = member.Type.Name;
            Description = member.Description;
            OnParameterUpdate();
        }

        public Member Model { get; set; }
        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        private string _type;
        public string Type { get => _type; set => SetProperty(ref _type, value); }
        private string? _description;
        public string? Description { get => _description; set => SetProperty(ref _description, value); }

        public bool IsVariadic
        {
            get
            {
                if(Model is Method method)
                    return method.IsVariadic;
                return false;
            }

            set
            {
                if (Model is Method method)
                    method.IsVariadic = value;
            }
        }

        public string? Parameters
        {
            get
            {
                if (Model is Method method)
                    return string.Join(',', method.Parameters.ConvertAll(p => $"{p.Name} : {p.Type.Name}"));
                return null;
            }
        }

        public void OnParameterUpdate() { RaisePropertyChanged(nameof(Parameters));}

        public void UpdateToModel(Project p, Type type)
        {
            if (type.Name == "Event")
            {
                p.RenameEvent(Model.Name, Name);
                return;
            }
            if (Model.Name != Name)
            {
                type.RenameMember(Model.Name, Name);
            }

            if (Model.Description != Description)
                Model.Description = Description;
            if (Model.Type.Name != Type)
            {
                var tp = p.GetType(Type);
                if (tp == null)
                {

                }
                else
                {
                    Model.Type = tp;
                }
            }
        }
    }

    public class TypeViewModel : BindableBase
    {
        public TypeViewModel(FlowChart.Type.Type type)
        {
            Model = type;
        }
        public FlowChart.Type.Type Model { get; set; }

        public string Name => Model.Name;
        public string Abbr => Model.Abbr;
        public string? Description => Model.Description;

        public void OnNameChange()
        {
            RaisePropertyChanged(nameof(Name));
        }


    }
    internal class TypeDialogViewModel : BindableBase, IDialogAware
    {
        public static string NAME = "TypeDialog";
        public Project? CurrentProject;

        public TypeDialogViewModel(IDialogService dlgService, IOutputMessage outputService)
        {
            Types = new ObservableCollection<TypeViewModel>();
            Members = new ObservableCollection<TypeMemberViewModel>();
            Parameters = new ObservableCollection<ParameterViewModel>();

            MethodDefineCodes = new List<string>();
            MethodCode = new TextDocument();

            _memberName = string.Empty;
            _memberType = string.Empty;

            _outputService = outputService;
            _dialogService = dlgService;
            EventHelper.Sub<ProjectCloseEvent, Project>(OnCloseProject, ThreadOption.UIThread);

            SelectedTypeChangeCommand = new DelegateCommand(OnSelectedTypeChange);
            SelectedMemberChangeCommand = new DelegateCommand(OnSelectedMemberChange);
            SelectedParamChangeCommand = new DelegateCommand(OnSelectedParamChange);
            AddNewTypeCommand = new DelegateCommand(AddNewType);
            RemoveTypeCommand = new DelegateCommand(RemoveType);
            RenameTypeCommand = new DelegateCommand(RenameType);
            AddNewPropertyCommand = new DelegateCommand(AddNewProperty);
            RemoveMemberCommand = new DelegateCommand(RemoveMember);
            AddNewMethodCommand = new DelegateCommand(AddNewMethod);

            AddNewParamCommand = new DelegateCommand(AddParameter);
            RemoveParamCommand = new DelegateCommand(RemoveParameter);
            ParamUpCommand = new DelegateCommand(delegate { MoveParameter(-1); });
            ParamDownCommand = new DelegateCommand(delegate { MoveParameter(1); });

            SaveCommand = new DelegateCommand(Save);

            EventHelper.Sub<NFCT.Common.Events.ThemeSwitchEvent, NFCT.Common.Theme>(OnThemeSwitch);
            OnThemeSwitch(Theme.Dark);
        }

        private readonly IOutputMessage _outputService;
        private readonly IDialogService _dialogService;
        void Output(string msg) { _outputService.Output(msg); }

        #region IDialogAware
        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            var project = parameters.GetValue<Project>("CurrentProject");
            if (project != CurrentProject)
            {
                Types.Clear();
                CurrentProject = project;
            }

            var typeDict = new Dictionary<Type, TypeViewModel>();
            TypeViewModel? dummyEventTypeVm = null;
            foreach (var typeViewModel in Types)
            {
                typeDict.Add(typeViewModel.Model, typeViewModel);
                if(typeViewModel.Name == "Event")
                    dummyEventTypeVm = typeViewModel;
            }

            foreach (var kv in CurrentProject.TypeDict)
            {
                var typeName = kv.Key;
                var type = kv.Value;

                if(!type.IsBuiltinType && !typeDict.ContainsKey(type))
                    Types.Add(new TypeViewModel(type));
            }

            // add all event as a Type
            if (dummyEventTypeVm == null)
            {
                dummyEventTypeVm = new TypeViewModel(new Type("Event") { IsBuiltinType = true });
                Types.Add(dummyEventTypeVm);
            }
        }

        public string Title => "Type Manager";
        public event Action<IDialogResult>? RequestClose;
        #endregion

        private TypeViewModel? _selectedType;
        public TypeViewModel? SelectedType
        {
            get => _selectedType;
            set { Update(); SetProperty(ref _selectedType, value); }
        }

        public ObservableCollection<TypeViewModel> Types { get; set; }
        private TypeMemberViewModel? _selectedMember;
        public TypeMemberViewModel? SelectedMember
        {
            get => _selectedMember;
            set { Update(); SetProperty(ref _selectedMember, value); }
        }

        private ParameterViewModel? _selectedParameter;
        public ParameterViewModel? SelectedParameter
        {
            get => _selectedParameter;
            set { Update(); SetProperty(ref _selectedParameter, value); }
        }

        private bool _isCodeStyle;
        public bool IsCodeStyle
        {
            get => _isCodeStyle;
            set { SetProperty(ref _isCodeStyle, value);
                GetMethodDefineCodes();
            }
        }

        public List<string> MethodDefineCodes;
        public TextDocument MethodCode { get; set; }

        private IHighlightingDefinition _highlightingDefinition;

        public IHighlightingDefinition HighlightingDefinition
        {
            get => _highlightingDefinition;
            set => SetProperty(ref _highlightingDefinition, value);
        }

        public ObservableCollection<TypeMemberViewModel> Members { get; set; }
        public DelegateCommand SelectedTypeChangeCommand { get; set; }
        public DelegateCommand SelectedMemberChangeCommand { get; set; }
        public DelegateCommand SelectedParamChangeCommand { get; set; }
        public DelegateCommand AddNewTypeCommand { get; set; }
        public DelegateCommand RemoveTypeCommand { get; set; }
        public DelegateCommand RenameTypeCommand { get; set; }
        public DelegateCommand AddNewPropertyCommand { get; set; }
        public DelegateCommand AddNewMethodCommand { get; set; }
        public DelegateCommand RemoveMemberCommand { get; set; }
        public DelegateCommand AddNewParamCommand { get; set; }
        public DelegateCommand RemoveParamCommand { get; set; }
        public DelegateCommand ParamUpCommand { get; set; }
        public DelegateCommand ParamDownCommand { get; set; }

        public DelegateCommand SaveCommand { get; set; }

        void OnCloseProject(Project project)
        {
            CurrentProject = null;
            Types.Clear();
        }

        void OnSelectedTypeChange()
        {
            if (SelectedType == null)
                return;
            Output($"type change to {SelectedType.Name}");
            Members.Clear();
            SelectedMember = null;
            if (SelectedType.Name == "Event")
            {
                ShowEvents();
            }
            else
            {
                foreach (var kv in SelectedType.Model.MemberDict)
                {
                    Members.Add(new TypeMemberViewModel(kv.Value));
                }
            }
        }

        void ShowEvents()
        {
            if (CurrentProject == null)
                return;
            Members.Clear();
            var events = CurrentProject.EventDict.Values.ToList();
            events.Sort((a, b) => a.EventId.CompareTo(b.EventId));
            events.RemoveAt(0);
            events.ForEach(ev =>
            {
                var method = new Method(ev.Name);
                method.Parameters = ev.Parameters;
                Members.Add(new TypeMemberViewModel(method));
            });
        }

        #region Type Editor

        //private string? _typeName;
        //public string? TypeName { get => _typeName; set => SetProperty(ref _typeName, value); }


        #endregion

        #region Member Editor

        private string _memberName;
        public string MemberName { get => _memberName; set => SetProperty(ref _memberName, value); }
        private string? _memberDescription;
        public string? MemberDescription { get => _memberDescription; set => SetProperty(ref _memberDescription, value); }
        private string _memberType;
        public string MemberType { get => _memberType; set => SetProperty(ref _memberType, value); }
        private int _memberKind;
        public int MemberKind { get => _memberKind; set => SetProperty(ref _memberKind, value); }
        private bool _isVariadicMethod;
        public bool IsVariadicMethod { get => _isVariadicMethod; set => SetProperty(ref _isVariadicMethod, value); }

        public ObservableCollection<ParameterViewModel> Parameters { get; set; }
        private string _paraName;
        public string ParaName { get => _paraName; set => SetProperty(ref _paraName, value); }
        private string? _paraDescription;
        public string? ParaDescription { get => _paraDescription; set => SetProperty(ref _paraDescription, value); }
        private string _paraType;
        public string ParaType { get => _paraType; set => SetProperty(ref _paraType, value); }
        private string? _paraValue;
        public string? ParaValue { get => _paraValue; set => SetProperty(ref _paraValue, value); }

        #endregion

        void OnSelectedMemberChange()
        {
            if (SelectedMember == null)
            {
                MemberName = string.Empty;
                MemberDescription = null;
                MemberType = string.Empty;
                Parameters.Clear();
                MemberKind = -1;
                return;
            }

            var m = SelectedMember;
            MemberName = m.Name;
            MemberDescription = m.Description;
            MemberType = m.Type;
            Parameters.Clear();
            if (m.Model is Method method)
            {
                MemberKind = 2;
                UpdateParameterView(method);

                IsVariadicMethod = method.IsVariadic;
            }
            else
            {
                MemberKind = 1;
            }

            GetMethodDefineCodes();
        }

        void OnSelectedParamChange()
        {
            if (SelectedParameter == null)
            {
                ParaName = string.Empty;
                ParaDescription = null;
                ParaValue = null;
                ParaType = string.Empty;
                return;
            }

            ParaName = SelectedParameter.Name;
            ParaDescription = SelectedParameter.Description;
            ParaType = SelectedParameter.Type;
            ParaValue = SelectedParameter.DefaultValue;
        }

        string? GetNewTypeName()
        {
            if (CurrentProject == null)
                return null;
            const string NEW_TYPE_NAME = "NewType";
            var name = NEW_TYPE_NAME;
            if (CurrentProject.GetType(name) == null)
                return name;
            for (int i = 0; i < 100; i++)
            {
                name = $"{NEW_TYPE_NAME}_{i}";
                if (CurrentProject.GetType(name) == null)
                    return name;
            }
            return null;
        }

        void AddNewType()
        {
            var newTypeName = GetNewTypeName();
            if (newTypeName == null)
                return;
            var newType = new Type(newTypeName);
            var newTypeVm = new TypeViewModel(newType);
            Types.Add(newTypeVm);
            SelectedType = newTypeVm;
            OnSelectedTypeChange();
        }

        void RemoveType()
        {
            if (SelectedType == null || CurrentProject == null)
                return;
            var typeName = SelectedType.Name;
            if (typeName == BuiltinTypes.GlobalType.Name
                || typeName == "Event")
                return;
            var result = MessageBox.Show($"this operation will remove type `{typeName}`", "Remove Type",MessageBoxButton.OKCancel);
            if (result != MessageBoxResult.OK)
                return;
            
            CurrentProject.RemoveType(typeName);
            Types.Remove(SelectedType);
            SelectedType = null;
        }

        void RenameType()
        {
            if (SelectedType == null || CurrentProject == null)
                return;
            var typeName = SelectedType.Name;
            if (typeName == BuiltinTypes.GlobalType.Name
                || typeName == "Event")
                return;

            _dialogService.Show(InputDialogViewModel.NAME, result =>
            {
                if (result.Result != ButtonResult.OK)
                    return;
                var newName = result.Parameters.GetValue<string>("Value");
                CurrentProject.RenameType(typeName, newName);
                SelectedType.OnNameChange();
            });
        }

        string? GetNewMemberName(string defaultNewName)
        {
            if (SelectedType == null)
                return null;
            
            var name = defaultNewName;
            if (Members.All(m => m.Name != name))
                return name;
            for (int i = 0; i < 100; i++)
            {
                name = $"{defaultNewName}_{i}";
                if (Members.All(m => m.Name != name))
                    return name;
            }
            return null;
        }

        void AddNewProperty()
        {
            if (SelectedType == null)
                return;
            var name = GetNewMemberName("NewProperty");
            if (name == null)
                return;
            var prop = new Property(name);
            var memberVm = new TypeMemberViewModel(prop);
            Debug.Assert(SelectedType.Model.AddMember(prop));
            Members.Add(memberVm);
            SelectedMember = memberVm;
        }

        void AddNewMethod()
        {
            if (SelectedType == null)
                return;
            if (SelectedType.Name == "Event")
            {
                AddNewEvent();
                return;
            }
            var name = GetNewMemberName("NewMethod");
            if (name == null)
                return;
            var method = new Method(name);
            var memberVm = new TypeMemberViewModel(method);
            Debug.Assert(SelectedType.Model.AddMember(method));
            Members.Add(memberVm);
            SelectedMember = memberVm;
        }

        void AddNewEvent()
        {
            if (CurrentProject == null)
                return;
            var newEventName = "NewEvent";
            if (CurrentProject.GetEvent(newEventName) != null)
            {
                for (int i = 0; i < 100; i++)
                {
                    newEventName = $"NewEvent_{i}";
                    if (CurrentProject.GetEvent(newEventName) == null)
                        break;
                }
            }

            var ev = new EventType(newEventName) { EventId = -1 };
            CurrentProject.AddEvent(ev);

            var method = new Method(ev.Name);
            method.Parameters = ev.Parameters;
            var memberVm = new TypeMemberViewModel(method);
            Members.Add(memberVm);
            SelectedMember = memberVm;
        }

        void RemoveMember()
        {
            if (SelectedType == null)
                return;
            if (SelectedMember == null)
                return;
            if (SelectedType.Model.RemoveMember(SelectedMember.Name))
            {
                Members.Remove(SelectedMember);
                SelectedMember = null;
            }
        }

        void AddParameter()
        {
            if (SelectedMember is not { Model: Method method })
                return;
            var p = new Parameter("NewArg") { Type = BuiltinTypes.AnyType };
            method.Parameters.Add(p);
            var paraVm = new ParameterViewModel(p);
            Parameters.Add(paraVm);
            SelectedParameter = paraVm;
        }

        void UpdateParameterView(Method method)
        {
            Parameters.Clear();
            method.Parameters.ForEach(p =>
            {
                var pvm = new ParameterViewModel(p)
                {
                    Description = p.Description,
                    DefaultValue = p.Default
                };
                Parameters.Add(pvm);
            });
        }

        void RemoveParameter()
        {
            if (SelectedType == null || SelectedMember == null || SelectedParameter == null)
                return;
            if (SelectedMember.Model is not Method method)
                return;
            var p = SelectedParameter.Model;
            method.Parameters.Remove(p);

            UpdateParameterView(method);
            
        }

        void MoveParameter(int direction) // -1 - up, 1 - down
        {
            if (SelectedType == null || SelectedMember == null || SelectedParameter == null)
                return;
            if (SelectedMember.Model is not Method method)
                return;
            
            var parameters = method.Parameters;
            var idx = parameters.IndexOf(SelectedParameter.Model);
            if (idx < 0)
                return;
            var newIdx = idx + direction;
            if (direction > 0)
            {
                if (newIdx >= parameters.Count)
                    return;
            }
            else
            {
                if (newIdx < 0)
                    return;
            }
            (parameters[newIdx], parameters[idx]) = (parameters[idx], parameters[newIdx]);
            UpdateParameterView(method);
        }

        void GetMethodDefineCodes()
        {
            if (!IsCodeStyle || SelectedMember == null )
            {
                return;
            }

            MethodDefineCodes.Clear();
            if (SelectedMember.Model is Method method)
            {
                if (method.Description != null)
                {
                    MethodDefineCodes.Add($"// {method.Description}");
                }
                if(method.Parameters.Count == 0)
                    MethodDefineCodes.Add($"{method.Type.Name} {method.Name}();");
                else
                {
                    MethodDefineCodes.Add($"{method.Type.Name} {method.Name}(");
                    var last = method.Parameters.Last();
                    foreach (var para in method.Parameters)
                    {
                        var comma = para == last ? "" : ",";
                        var line = $"  {para.Type.Name} {para.Name}";
                        if (para.Default != null)
                        {
                            line += $" = {para.Default}";
                        }

                        line += comma;
                        if (para.Description != null)
                            line += $" // {para.Description}";
                        MethodDefineCodes.Add(line);
                    }
                    MethodDefineCodes.Add(");");
                }

                MethodCode.Text = string.Join('\n', MethodDefineCodes);
            }
        }

        void OnThemeSwitch(Theme theme)
        {
            void ApplyToDynamicResource(ComponentResourceKey key, Color? newColor)
            {
                if (Application.Current.Resources[key] == null || newColor == null)
                    return;

                // Re-coloring works with SolidColorBrushs linked as DynamicResource
                if (Application.Current.Resources[key] is SolidColorBrush)
                {
                    //backupDynResources.Add(resourceName);

                    var newColorBrush = new SolidColorBrush((Color)newColor);
                    newColorBrush.Freeze();

                    Application.Current.Resources[key] = newColorBrush;
                }
            }

            if (theme == Theme.Dark)
            {
                ThemedHighlightingManager.Instance.SetCurrentTheme("VS2019_Dark");
            }
            else
            {
                ThemedHighlightingManager.Instance.SetCurrentTheme("Light");
            }

            HighlightingDefinition = ThemedHighlightingManager.Instance.GetDefinitionByExtension(".cs");

            var hltheme = ThemedHighlightingManager.Instance.CurrentTheme.HlTheme;
            if (hltheme == null)
                return;

            //foreach (var item in hltheme.GlobalStyles)
            //{
            //    switch (item.TypeName)
            //    {
            //        case "DefaultStyle":
            //            ApplyToDynamicResource(NFCT.Themes.ResourceKeys.EditorBackground, item.backgroundcolor);
            //            ApplyToDynamicResource(NFCT.Themes.ResourceKeys.EditorForeground, item.foregroundcolor);
            //            break;

            //        case "CurrentLineBackground":
            //            ApplyToDynamicResource(NFCT.Themes.ResourceKeys.EditorCurrentLineBackgroundBrushKey, item.backgroundcolor);
            //            ApplyToDynamicResource(NFCT.Themes.ResourceKeys.EditorCurrentLineBorderBrushKey, item.bordercolor);
            //            break;

            //        case "LineNumbersForeground":
            //            ApplyToDynamicResource(NFCT.Themes.ResourceKeys.EditorLineNumbersForeground, item.foregroundcolor);
            //            break;

            //        case "Selection":
            //            ApplyToDynamicResource(NFCT.Themes.ResourceKeys.EditorSelectionBrush, item.backgroundcolor);
            //            ApplyToDynamicResource(NFCT.Themes.ResourceKeys.EditorSelectionBorder, item.bordercolor);
            //            break;

            //        case "Hyperlink":
            //            ApplyToDynamicResource(NFCT.Themes.ResourceKeys.EditorLinkTextBackgroundBrush, item.backgroundcolor);
            //            ApplyToDynamicResource(NFCT.Themes.ResourceKeys.EditorLinkTextForegroundBrush, item.foregroundcolor);
            //            break;

            //        case "NonPrintableCharacter":
            //            ApplyToDynamicResource(NFCT.Themes.ResourceKeys.EditorNonPrintableCharacterBrush, item.foregroundcolor);
            //            break;

            //        default:
            //            throw new System.ArgumentOutOfRangeException("GlobalStyle named '{0}' is not supported.", item.TypeName);
            //    }
            //}

            if (HighlightingDefinition != null)
            {
                // Reset property for currently select highlighting definition
                HighlightingDefinition = ThemedHighlightingManager.Instance.GetDefinition(HighlightingDefinition.Name);

                if (HighlightingDefinition != null)
                    return;
            }

        }

        void Update()
        {
            if (CurrentProject == null)
                return;

            if (IsCodeStyle && SelectedMember != null && SelectedType != null)
            {
                var code = MethodCode.Text;
                var methodResult = MethodStringParser.Method.Parse(code);
                var m = methodResult.ToMember(CurrentProject);
                if (m != null)
                {
                    var originModel = SelectedMember.Model;
                    SelectedType.Model.RemoveMember(originModel.Name);
                    SelectedMember.SetModel(m);
                    SelectedType.Model.AddMember(m);
                    OnSelectedMemberChange();
                }
            }

            // save param
            if (SelectedParameter != null)
            {
                if (SelectedMember != null  && SelectedMember.Model.SaveToFile)
                {
                    SelectedParameter.Name = ParaName;
                    SelectedParameter.Description = ParaDescription;
                    SelectedParameter.DefaultValue = ParaValue;
                    SelectedParameter.Type = ParaType;
                    SelectedParameter.UpdateToModel(CurrentProject);
                    SelectedMember?.OnParameterUpdate();
                }
            }

            // save member
            if (SelectedMember != null)
            {
                if (SelectedMember.Model.SaveToFile)
                {
                    SelectedMember.Name = MemberName;
                    SelectedMember.Description = MemberDescription;
                    SelectedMember.Type = MemberType;
                    SelectedMember.IsVariadic = IsVariadicMethod;
                    if (SelectedType != null)
                        SelectedMember.UpdateToModel(CurrentProject, SelectedType.Model);
                }
            }

            //// type rename
            //if (SelectedType != null && !string.IsNullOrEmpty(TypeName))
            //{
            //    if (SelectedType.Name == "Global" || SelectedType.Name == "Event")
            //    {
            //        // output message
            //    }
            //    else if (SelectedType.Name != TypeName)
            //    {
            //        CurrentProject.RenameType(SelectedType.Name, TypeName);
            //        SelectedType.OnNameChange();
            //    }
            //}

            // save type
            foreach (var typeVm in Types)
            {
                if(CurrentProject.GetType(typeVm.Name) != null)
                    continue;
                CurrentProject.AddType(typeVm.Model);
                Output($"type {typeVm.Name} successfully added");
            }
        }

        void Save()
        {
            Update();
            CurrentProject?.Save();
        }
    }


}
