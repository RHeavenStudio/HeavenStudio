// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEditor;

namespace AmplifyColor
{
	public class ToolSettings
	{
		private static readonly ToolSettings _instance = new ToolSettings();
		public bool AddLut = true;
		public bool ApplyHDRControl = true;
		public bool ApplyColorGrading = false;
		public string FilePath = "";

		public string Host = "localhost";
		public LUTSettings LUT = new LUTSettings();

		public string Message = string.Empty;
		public string Password = "password";

		public ResolutionSettings Resolution = new ResolutionSettings();

		public int SelectedTab;

		private ToolSettings()
		{
			if ( !EditorPrefs.HasKey( "AmplifyColor.NetworkHost" ) )
				EditorPrefs.SetString( "AmplifyColor.NetworkHost", Host );
			else
				Host = EditorPrefs.GetString( "AmplifyColor.NetworkHost", "localhost" );

			if ( !EditorPrefs.HasKey( "AmplifyColor.NetworkPassword" ) )
				EditorPrefs.SetString( "AmplifyColor.NetworkPassword", Password );
			else
				Password = EditorPrefs.GetString( "AmplifyColor.NetworkPassword", "password" );

			if ( !EditorPrefs.HasKey( "AmplifyColor.SelectedTab" ) )
				EditorPrefs.SetInt( "AmplifyColor.SelectedTab", SelectedTab );
			else
				SelectedTab = EditorPrefs.GetInt( "AmplifyColor.SelectedTab", 0 );
		}

		public static ToolSettings Instance
		{
			get { return _instance; }
		}
	}
}
