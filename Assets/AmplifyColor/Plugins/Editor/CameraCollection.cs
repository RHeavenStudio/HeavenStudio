// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AmplifyColor
{
	public class CameraCollection
	{
		private string[] _cameraNames;
		private Camera _selectedCamera;
		private int _selectedIndex = -1;
		private string _selectedCameraName;
		private List<Camera> _camerasList;
		private List<string> _namesList;

		public string SelectedCameraName
		{
			get { return _selectedCameraName; }
		}

		public string[] CameraNames
		{
			get
			{
				return _cameraNames;
			}
		}

		public int SelectedIndex
		{
			get { return _selectedIndex; }
			set
			{
				_selectedIndex = value;
				SetupCamera();
			}
		}

		public Camera SelectedCamera
		{
			get { return _selectedCamera; }
		}

		private void SetupCamera()
		{
			try
			{
				_selectedCameraName = _camerasList[ _selectedIndex ].name;
			}
			catch ( Exception )
			{
				_selectedIndex = 0;
				_selectedCameraName = "";

				return;
			}
		}

		public void GenerateCameraList()
		{
			var cameras = Camera.allCameras;

			if ( cameras == null || cameras.Length == 0 )
			{
				_selectedCameraName = "No cameras were found in the scene";
				_selectedCamera = null;
				_selectedIndex = 0;
				_camerasList = new List<Camera>();
				_cameraNames = new[] { _selectedCameraName };
				_namesList = new List<string>( _cameraNames );

				return;
			}

			_camerasList = new List<Camera>();

			for ( int i = 0; i < cameras.Length; i++ )
			{
				//bool hidden = (cameras[i].hideFlags & HideFlags.HideAndDontSave) != 0 ||
				//    (cameras[i].hideFlags & HideFlags.HideInHierarchy) != 0 ||
				//    (cameras[i].hideFlags & HideFlags.HideInInspector) != 0;

				//if (!hidden)
				_camerasList.Add( cameras[ i ] );
			}

			_namesList = new List<string>();

			foreach ( var camera in _camerasList )
			{
				_namesList.Add( camera.name );
			}

			int index = _namesList.IndexOf( _selectedCameraName ?? "" );

			if ( index >= 0 )
			{
				_selectedIndex = index;
			}
			else
			{
				if ( Camera.main == null )
					index = 0;
				else
					index = _namesList.IndexOf( Camera.main.name );

				_selectedIndex = index >= 0 ? index : 0;
			}

			_cameraNames = _namesList.ToArray();
			_selectedCamera = _camerasList[ _selectedIndex ];
			_selectedCameraName = _selectedCamera.name;
		}

		public Texture2D GetCurrentEffectTexture()
		{
			var camera = SelectedCamera;

			if ( camera == null )
			{
				return null;
			}

            var component = ( MonoBehaviour ) camera.GetComponent<AmplifyColorBase>();

			if ( component != null )
			{
				Texture2D texture = null;
				try
				{
					var propInfo = component.GetType().GetField( "LutTexture" );
					texture = ( Texture2D ) propInfo.GetValue( component );
				}
				catch ( System.Exception )
				{
				}
				return texture;
			}

			return null;
		}
	}
}
