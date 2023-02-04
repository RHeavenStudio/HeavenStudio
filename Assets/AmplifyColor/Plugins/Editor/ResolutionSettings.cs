// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

namespace AmplifyColor
{
	public class ResolutionSettings
	{
		private readonly string[] _resolutionOptions = new[]
		{
			//"Game Window",
			"640x480",
			"800x600",
			"1024x768",
			"1280x720",
			"1680x1050",
			"1920x1080",
			"Custom",
		};

		private int _selectedResolution = 3;
		private int _targetWidth = 1280;
		private int _targetHeight = 720;

		public int TargetWidth
		{
			get { return _targetWidth; }
			set
			{
				if ( IsCustom )
					_targetWidth = value;
			}
		}

		public int TargetHeight
		{
			get { return _targetHeight; }
			set
			{
				if ( IsCustom )
					_targetHeight = value;
			}
		}

		public int SelectedResolution
		{
			get { return _selectedResolution; }
			set
			{
				_selectedResolution = value;

				SetResolution();
			}
		}

		public bool IsCustom
		{
			get { return SelectedResolution == ( _resolutionOptions.Length - 1 ); }
		}

		public string[] ResolutionOptions
		{
			get { return _resolutionOptions; }
		}

		private void SetResolution()
		{
			switch ( _selectedResolution )
			{
			case 0:
				_targetWidth = 640;
				_targetHeight = 480;
				break;
			case 1:
				_targetWidth = 800;
				_targetHeight = 600;
				break;
			case 2:
				_targetWidth = 1024;
				_targetHeight = 768;
				break;
			case 3:
				_targetWidth = 1280;
				_targetHeight = 720;
				break;
			case 4:
				_targetWidth = 1680;
				_targetHeight = 1050;
				break;
			case 5:
				_targetWidth = 1920;
				_targetHeight = 1080;
				break;
			}
		}
	}
}
