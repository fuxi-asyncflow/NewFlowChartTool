using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using FlowChart.Common;

namespace NFCT.Common.ViewModels
{
    public class UndoRedoCommand
    {
        public UndoRedoCommand(string description, Action redo, Action undo)
        {
            Description = description;
            Undo = undo;
            Redo = redo;
        }

        public string Description { get; set; }
        public Action Undo { get; set; }
        public Action Redo { get; set; }
        public override string ToString()
        {
            return Description;
        }
    }

    public class UndoRedoManagerViewModel : BindableBase
    {
        public UndoRedoManagerViewModel(Action<string>? output)
        {
            UndoCommands = new ObservableCollection<UndoRedoCommand>();
            RedoCommands = new ObservableCollection<UndoRedoCommand>();
            actions = () => { };
            reverts = new List<Action>();
            _output = output;
        }
        public ObservableCollection<UndoRedoCommand> UndoCommands { get; set; }
        public ObservableCollection<UndoRedoCommand> RedoCommands { get; set; }

        Action actions;
        List<Action> reverts;
        private string? _cmdName;

        public void Begin(string str)
        {
            _cmdName = str;
        }

        public void End()
        {
            Action revertAction = () => { };
            reverts.Reverse();
            reverts.ForEach(action => revertAction += action);
            UndoCommands.Add(new UndoRedoCommand(_cmdName, actions, revertAction));
            RedoCommands.Clear();

            actions = () => { };
            reverts.Clear();
            _cmdName = null;
        }

        public void AddAction(Action action, Action revert)
        {
            if (_cmdName == null)
                return;
            actions += action;
            reverts.Add(revert);
        }

        public void Undo()
        {
            int count = UndoCommands.Count;
            if (count == 0)
                return;
            var cmd = UndoCommands[count - 1];
            UndoCommands.RemoveAt(count - 1);
            RedoCommands.Add(cmd);
            _output?.Invoke($"[Undo] {cmd}");
            cmd.Undo();
        }

        public void Redo()
        {
            int count = RedoCommands.Count;
            if (count == 0)
                return;
            var cmd = RedoCommands[count - 1];
            RedoCommands.RemoveAt(count - 1);
            UndoCommands.Add(cmd);
            _output?.Invoke($"[Redo] {cmd}");
            cmd.Redo();
        }

        private Action<string>? _output;

    }
}
