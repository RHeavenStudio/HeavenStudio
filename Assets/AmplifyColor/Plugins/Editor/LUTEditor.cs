// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System.IO;
using UnityEditor;
using UnityEngine;

namespace AmplifyColor
{
	public class LUTEditor : EditorWindow
	{
		public ToolSettings Settings { get { return ToolSettings.Instance; } }

		protected PhotoshopConnection Connection { get; set; }

		protected PhotoshopProtocol PhotoshopProtocol { get; set; }

		protected CameraCollection Cameras { get; set; }

		protected BackBufferHandler BackBufferHandler { get; set; }

		protected LUTWriter LUTWriter { get; set; }

		protected PhotoshopHandler PhotoshopHandler { get; set; }

		protected FileHandler FileHandler { get; set; }

		private bool lut_foldout = false;
		private bool conn_foldout = false;

		[MenuItem( "Window/Amplify Color/LUT Editor", false, 0 )]
		public static void Init()
		{
			GetWindow<LUTEditor>( false, "LUT Editor", true );
		}

		void OnEnable()
		{
			Connection = new PhotoshopConnection();

			PhotoshopProtocol = new PhotoshopProtocol( Connection );

			Cameras = new CameraCollection();

			BackBufferHandler = new BackBufferHandler( Cameras );

			LUTWriter = new LUTWriter();

			PhotoshopHandler = new PhotoshopHandler( PhotoshopProtocol, BackBufferHandler, LUTWriter, Settings, Cameras );

			FileHandler = new FileHandler( BackBufferHandler, LUTWriter, Settings, Cameras );

			Cameras.GenerateCameraList();
		}


		void OnDestroy()
		{
		}

		Vector2 _scrollPosition = new Vector2( 0, 0 );

		void OnGUI()
		{
			_scrollPosition = GUILayout.BeginScrollView( _scrollPosition );

			Settings.SelectedTab = EditorPrefs.GetInt( "AmplifyColor.SelectedTab", 3 );

			Settings.SelectedTab = GUILayout.Toolbar( Settings.SelectedTab, new[] { "Photoshop", "File", "Settings" } );

			EditorPrefs.SetInt( "AmplifyColor.SelectedTab", Settings.SelectedTab );

			switch ( Settings.SelectedTab )
			{
			case 0:
				ShowReadBackBufferGUI();
				ShowGUIPhotoshop();
				ShowGUITargetFile();
				ShowGUIStatusBar();
				ShowLUTChecklist();
				break;
			case 1:
				ShowReadBackBufferGUI();
				ShowGUIFile();
				ShowGUITargetFile();
				ShowGUIStatusBar();
				break;
			case 2:
				ShowGUISettings();
				ShowGUIStatusBar();
				ShowConnectionChecklist();
				break;
			}

			GUILayout.EndScrollView();
		}



		private void ShowLUTChecklist()
		{
			EditorGUILayout.Space();
			EditorGUILayout.Separator();
			EditorGUILayout.Space();

			lut_foldout = EditorGUILayout.Foldout( lut_foldout, "Help: Your LUT doesn't look right?" );
			if ( lut_foldout )
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space( 10 );
				GUILayout.BeginVertical();
				GUILayout.Label( "1) Go to your LUT import settings and set \"Texture Type\" to Advanced.", EditorStyles.wordWrappedLabel );
				GUILayout.Label( "2) Enable  \"Bypass sRGB Sampling\".", EditorStyles.wordWrappedLabel );
				GUILayout.Label( "3) Disable \"Generate Mip Maps\".", EditorStyles.wordWrappedLabel );
				GUILayout.Label( "4) Set \"Wrap Mode\" to Clamp.", EditorStyles.wordWrappedLabel );
				GUILayout.Label( "5) Set \"Filter Mode\" to Bilinear.", EditorStyles.wordWrappedLabel );
				GUILayout.Label( "6) Set \"Aniso Level\" to 0.", EditorStyles.wordWrappedLabel );
				GUILayout.Label( "7) Set \"Max Size\" to 1024.", EditorStyles.wordWrappedLabel );
				GUILayout.Label( "8) Set \"Format\" to Automatic Truecolor.", EditorStyles.wordWrappedLabel );
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
		}

		private void ShowConnectionChecklist()
		{
			EditorGUILayout.Space();
			EditorGUILayout.Separator();
			EditorGUILayout.Space();

			conn_foldout = EditorGUILayout.Foldout( conn_foldout, "Help: Having trouble connecting?" );
			if ( conn_foldout )
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space( 10 );
				GUILayout.BeginVertical();
				GUILayout.Label( "1) Use Photoshop CS 5.1 or above.", EditorStyles.wordWrappedLabel );
				GUILayout.Label( "2) Enable Remote access in Photoshop \"Edit/Remote Connections...\"", EditorStyles.wordWrappedLabel );
				GUILayout.Label( "3) Make sure both passwords match. Reset them just in case.", EditorStyles.wordWrappedLabel );
				GUILayout.Label( "4) Add Unity and Photoshop to firewall as exceptions.", EditorStyles.wordWrappedLabel );
				GUILayout.Label( "5) Set your active Build Target to Standalone.", EditorStyles.wordWrappedLabel );
				GUILayout.Label( "6) Set Host to target machine; usually 'localhost' or '127.0.0.1'.", EditorStyles.wordWrappedLabel );
				GUILayout.EndVertical();
				GUILayout.EndHorizontal();
			}
		}

		private void ShowGUITargetFile()
		{
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			EditorGUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();
			var lutpath = EditorGUILayout.TextField( "Save Preset to File", LUTWriter.TexturePath );

			if ( GUILayout.Button( "Browse" ) )
			{
				var path = EditorUtility.SaveFilePanelInProject( "Save as", Path.GetFileName( LUTWriter.TexturePath ), "png", "Please enter a file name to save the texture to" );

				if ( !string.IsNullOrEmpty( path ) )
				{
					lutpath = path;
				}
			}
			EditorGUILayout.EndHorizontal();
			LUTWriter.Overwrite = EditorGUILayout.Toggle( "Overwrite Existing File", LUTWriter.Overwrite );

			//var lutobject = (Texture2D)EditorGUILayout.ObjectField("Texture", LUTWriter.TextureObject, typeof(Texture2D), false);

			//LUTWriter.Update(lutpath, lutobject);
			LUTWriter.TexturePath = lutpath;

			EditorGUILayout.EndVertical();
		}

		private void ShowGUIStatusBar()
		{
			EditorGUILayout.Space();
			EditorGUILayout.Separator();
			EditorGUILayout.Space();
			EditorGUILayout.LabelField( "Status", Settings.Message ?? "", EditorStyles.wordWrappedLabel );
		}

		private void ShowGUIFile()
		{
			EditorGUILayout.Space();

			if ( GUILayout.Button( "Save reference file" ) )
			{
				FileHandler.SaveToFile();
			}

			if ( GUILayout.Button( "Load graded file" ) )
			{
				FileHandler.ReadFromFile();
			}

			EditorGUILayout.BeginHorizontal();

			Settings.FilePath = EditorGUILayout.TextField( Settings.FilePath ?? "" );

			if ( GUILayout.Button( "Reload" ) )
			{
				FileHandler.Reload();
			}

			EditorGUILayout.EndHorizontal();
		}

		private void ShowGUIPhotoshop()
		{
			EditorGUILayout.Space();

			if ( GUILayout.Button( "Send screenshot to Photoshop" ) )
			{
				PhotoshopHandler.SendToPhotoshop();
			}
			if ( GUILayout.Button( "Read preset from Photoshop" ) )
			{
				PhotoshopHandler.ReadFromPhotoshopTools();
			}
		}

		private void ShowReadBackBufferGUI()
		{
			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField( "", "Screenshot settings" );

			EditorGUILayout.Space();

			Cameras.GenerateCameraList();
			Cameras.SelectedIndex = EditorGUILayout.Popup( "Camera", Cameras.SelectedIndex, Cameras.CameraNames );

			EditorGUILayout.Space();

			ShowGUIResolution();

			Settings.AddLut = EditorGUILayout.Toggle( "Add LUT", Settings.AddLut );
			Settings.ApplyHDRControl = EditorGUILayout.Toggle( "Apply HDR Control", Settings.ApplyHDRControl );
			Settings.ApplyColorGrading = EditorGUILayout.Toggle( "Apply Color Grading", Settings.ApplyColorGrading );

			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
		}

		private void ShowGUIResolution()
		{
			Settings.Resolution.SelectedResolution = EditorPrefs.GetInt( "AmplifyColor.SelectedResolution", 3 );

			Settings.Resolution.SelectedResolution = EditorGUILayout.Popup( "Resolution", Settings.Resolution.SelectedResolution, Settings.Resolution.ResolutionOptions );

			EditorPrefs.SetInt( "AmplifyColor.SelectedResolution", Settings.Resolution.SelectedResolution );

			if ( Settings.Resolution.IsCustom )
			{
				Settings.Resolution.TargetWidth = EditorGUILayout.IntField( "Width", Settings.Resolution.TargetWidth );
				Settings.Resolution.TargetHeight = EditorGUILayout.IntField( "Height", Settings.Resolution.TargetHeight );
			}

			EditorGUILayout.Space();
		}

		private void ShowGUISettings()
		{
			EditorGUILayout.LabelField( "", "Setup Photoshop connection" );

			EditorGUILayout.Space();

			Settings.Host = EditorPrefs.GetString( "AmplifyColor.NetworkHost", "localhost" );
			Settings.Password = EditorPrefs.GetString( "AmplifyColor.NetworkPassword", "password" );
			bool showPassword = EditorPrefs.GetBool( "AmplifyColor.ShowPassword", false );

			Settings.Host = EditorGUILayout.TextField( "Host", Settings.Host );
			if ( showPassword )
				Settings.Password = EditorGUILayout.TextField( "Password", Settings.Password );
			else
				Settings.Password = EditorGUILayout.PasswordField( "Password", Settings.Password );

			showPassword = EditorGUILayout.Toggle( "Show Password", showPassword );

			EditorPrefs.SetString( "AmplifyColor.NetworkHost", Settings.Host );
			EditorPrefs.SetString( "AmplifyColor.NetworkPassword", Settings.Password );
			EditorPrefs.SetBool( "AmplifyColor.ShowPassword", showPassword );

			EditorGUILayout.Separator();

			if ( GUILayout.Button( "Test Connection" ) )
			{
				Connection.TestConnection();
			}

			//EditorGUILayout.Space();
			//EditorGUILayout.Space();
			//EditorGUILayout.LabelField("", "LUT Settings");
			//var size = EditorGUILayout.IntSlider("Size", Settings.LUT.Size, 0, 64);
			//var columns = EditorGUILayout.IntField("Columns", Settings.LUT.Columns);
			//var rows = EditorGUILayout.IntField("Rows", Settings.LUT.Rows);

			//Settings.LUT.Update(size, columns, rows);

			//bool toggle = true;
		}
	}
}
