namespace HeavenStudio.Editor.Commands
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}