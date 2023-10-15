using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Editor.Commands
{
    public interface IAction
    {
        void Execute();
        void Undo();
        void Redo();
    }
}