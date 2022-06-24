#if UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
using System.Diagnostics;

namespace Starpelly.OperatingSystem
{
	public class Linux : IOperatingSystem
	{
		public void ChangeWindowTitle(string newTitle)
		{
			var pid = Process.GetCurrentProcess().Id;
			var args = $"search --all --pid {pid} --class '.' set_window --name \"{newTitle}\"";
			Process.Start("xdotool", args);
		}
	}
}
#endif
