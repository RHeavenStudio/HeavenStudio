namespace Starpelly
{
	public static class OS
	{
		private static readonly OperatingSystem.IOperatingSystem _os;

		static OS()
		{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
			_os = new OperatingSystem.Windows();
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
			_os = new OperatingSystem.Linux();
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
			_os = new OperatingSystem.MacOS();
#endif
		}

		public static void ChangeWindowTitle(string newTitle)
		{
			_os.ChangeWindowTitle(newTitle);
		}
	}
}
