using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Editor.Commands;

namespace HeavenStudio.Editor
{
    public class CommandManager : MonoBehaviour
    {
        private Stack<IAction> historyStack = new Stack<IAction>();
        private Stack<IAction> redoHistoryStack = new Stack<IAction>();

        int maxItems = 128;

        public bool canUndo()
        {
            return historyStack.Count > 0;
        }
        public bool canRedo()
        {
            return redoHistoryStack.Count > 0;
        }

        public static CommandManager instance { get; private set; }

        private void Awake()
        {
            instance = this;
        }

        public void Execute(IAction action)
        {
            action.Execute();
            historyStack.Push(action);
            redoHistoryStack.Clear();
        }

        public void Undo()
        {
            if (!canUndo() || Conductor.instance.NotStopped()) return;

            if (historyStack.Count > 0)
            {
                redoHistoryStack.Push(historyStack.Peek());
                historyStack.Pop().Undo();
            }
        }

        public void Redo()
        {
            if (!canRedo() || Conductor.instance.NotStopped()) return;

            if (redoHistoryStack.Count > 0)
            {
                historyStack.Push(redoHistoryStack.Peek());
                redoHistoryStack.Pop().Redo();
            }
        }

        // this is here as to not hog up memory, "max undos" basically
        private void EnsureCapacity()
        {
            if (maxItems > 0)
            {
            }
        }

        public void Clear()
        {
            historyStack.Clear();
            redoHistoryStack.Clear();
        }
    }
}