using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Editor.Commands;

namespace HeavenStudio.Editor
{
    public class CommandManager : MonoBehaviour
    {
        public static CommandManager Instance { get; private set; }

        private Stack<ICommand> historyStack = new Stack<ICommand>();
        private Stack<ICommand> redoHistoryStack = new Stack<ICommand>();

        public int HistoryCount => historyStack.Count;

        private int maxItems = 128;

        private void Awake()
        {
            Instance = this;
        }

        public void AddCommand(ICommand command)
        {
            command.Execute();
            historyStack.Push(command);
            redoHistoryStack.Clear();
        }

        public void UndoCommand()
        {
            if (!CanUndo() || Conductor.instance.NotStopped()) return;

            if (historyStack.Count > 0)
            {
                redoHistoryStack.Push(historyStack.Peek());
                historyStack.Pop().Undo();
            }
        }

        public void RedoCommand()
        {
            if (!CanRedo() || Conductor.instance.NotStopped()) return;

            if (redoHistoryStack.Count > 0)
            {
                historyStack.Push(redoHistoryStack.Peek());
                redoHistoryStack.Pop().Execute();
            }
        }

        public void Clear()
        {
            historyStack.Clear();
            redoHistoryStack.Clear();
        }

        public bool CanUndo()
        {
            return historyStack.Count > 0;
        }
        public bool CanRedo()
        {
            return redoHistoryStack.Count > 0;
        }

    }
}