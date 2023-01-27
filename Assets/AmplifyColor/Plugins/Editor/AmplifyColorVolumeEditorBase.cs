// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AmplifyColor
{
	public class VolumeEditorBase : EditorWindow
	{
		class NewLineContainer
		{
			public AmplifyColorBase camera;
			public string component;
			public string fieldName;
			public string fieldType;
			public object value;

			public NewLineContainer()
			{
			}

			public NewLineContainer( AmplifyColorBase camera )
			{
				this.camera = camera;
			}

			public NewLineContainer( AmplifyColorBase camera, string component )
			{
				this.camera = camera;
				this.component = component;
			}

			public NewLineContainer( AmplifyColorBase camera, string component, string fieldName, string fieldType, object value )
			{
				this.camera = camera;
				this.component = component;
				this.fieldName = fieldName;
				this.fieldType = fieldType;
				this.value = value;
			}

			public static string GenerateUniqueID( AmplifyColorBase camera, string component, string fieldName )
			{
				return camera.GetInstanceID().ToString() + "." + component + "." + fieldName;
			}

			public string GenerateUniqueID()
			{
				return ( ( camera != null ) ? camera.GetInstanceID().ToString() : "" ) + "." + component + "." + fieldName;
			}

			public void SetCamera( AmplifyColorBase camera )
			{
				this.camera = camera;
				component = "";
				fieldName = "";
				fieldType = "";
				value = null;
			}

			public void SetComponent( string component )
			{
				this.component = component;
				fieldName = "";
				fieldType = "";
				value = null;
			}
		}

		private Vector2 scrollPosition = new Vector2( 0, 0 );
		private GUIStyle removeButtonStyle;

		void OnGUI()
		{
			if ( removeButtonStyle == null )
			{
				removeButtonStyle = new GUIStyle( EditorStyles.miniButton );
				removeButtonStyle.border = new RectOffset( 0, 0, 0, 0 );
				removeButtonStyle.margin = new RectOffset( 0, 0, 1, 0 );
				removeButtonStyle.padding = new RectOffset( 0, 0, 0, 0 );
				removeButtonStyle.contentOffset = new Vector2( 0, 0 );
			}

			scrollPosition = GUILayout.BeginScrollView( scrollPosition );

			DrawVolumeList();

			GUILayout.EndScrollView();
		}

		const int iconMinWidth = 20, iconMaxWidth = 43;
		const int activeMinWidth = 10, activeMaxWidth = 40;
		const int visibleMinWidth = 10, visibleMaxWidth = 40;
		const int selectWidth = 16;
		const int nameMinWidth = 59, nameMaxWidth = 305;
		const int blendMinWidth = 20, blendMaxWidth = 94;
		const int priorityMinWidth = 20, priorityMaxWidth = 40;
		const int exposureMinWidth = 30, exposureMaxWidth = 50;
		const int lutWidth = 40;

		const int minIndentWidth = 0, maxIndentWidth = 8;
		const int minColumnWidth = 19, maxColumnWidth = 90;
		const int minValueWidth = 100, maxValueWidth = 200;

		private Texture2D volumeIcon = null;
		private string editName = "";
		private GameObject editObject = null;
		private Dictionary<string,NewLineContainer> copyLines = new Dictionary<string,NewLineContainer>();
		private Dictionary<object, List<NewLineContainer>> newLines;
		private bool dirtyVolumeFlags = false;

		public void OnEnable()
		{
			newLines = new Dictionary<object, List<NewLineContainer>>();
			removeButtonStyle = null;
			copyLines.Clear();

		#if UNITY_EDITOR
			AmplifyColorVolumeBase.Window = this;
		#endif
		}

		public void OnDisable()
		{
		#if UNITY_EDITOR
			AmplifyColorVolumeBase.Window = null;
		#endif
		}

		static string GetEditorPrefsKey( AmplifyColorVolumeBase volume )
		{
			return "AmplifyColor.VolumeEditor.Foldout." + volume.GetHashCode();
		}

		bool SetFoldout( AmplifyColorVolumeBase volume, bool state )
		{
			EditorPrefs.SetBool( GetEditorPrefsKey( volume ), state );
			return state;
		}

		bool GetFoldout( AmplifyColorVolumeBase volume )
		{
			string key = GetEditorPrefsKey( volume );
			if ( !EditorPrefs.HasKey( key ) )
				EditorPrefs.SetBool( key, false );
			return EditorPrefs.GetBool( key );
		}

		void DrawHeader( List<AmplifyColorVolumeBase> volumes )
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space( 10 );
			if ( volumes.Count > 0 )
			{
				EditorGUILayout.LabelField( "", GUILayout.MinWidth( iconMinWidth ), GUILayout.MaxWidth( iconMaxWidth - 8 ) );
				GUILayout.Space( 0 );
				EditorGUILayout.LabelField( "Active", EditorStyles.miniBoldLabel, GUILayout.MinWidth( activeMinWidth ), GUILayout.MaxWidth( activeMaxWidth ) );
				GUILayout.Space( 4 );
				EditorGUILayout.LabelField( "Visible", EditorStyles.miniBoldLabel, GUILayout.MinWidth( visibleMinWidth ), GUILayout.MaxWidth( visibleMaxWidth ) );
				GUILayout.Space( 3 );
				EditorGUILayout.LabelField( "", EditorStyles.miniBoldLabel, GUILayout.Width( selectWidth ) );
				GUILayout.Space( 10 );
				EditorGUILayout.LabelField( "Name", EditorStyles.miniBoldLabel, GUILayout.MinWidth( nameMinWidth ), GUILayout.MaxWidth( nameMaxWidth ) );
				GUILayout.Space( 8 );
				EditorGUILayout.LabelField( "Blend Time", EditorStyles.miniBoldLabel, GUILayout.MinWidth( blendMinWidth ), GUILayout.MaxWidth( blendMaxWidth ) );
				GUILayout.Space( 1 );
				EditorGUILayout.LabelField( "Exposure", EditorStyles.miniBoldLabel, GUILayout.MinWidth( exposureMinWidth ), GUILayout.MaxWidth( exposureMaxWidth ) );
				GUILayout.Space( 3 );
				EditorGUILayout.LabelField( "Priority", EditorStyles.miniBoldLabel, GUILayout.MinWidth( priorityMinWidth ), GUILayout.MaxWidth( priorityMaxWidth ) );
				GUILayout.Space( 1 );
				EditorGUILayout.LabelField( "LUT", EditorStyles.miniBoldLabel, GUILayout.Width( lutWidth ) );
			}
			else
			{
				EditorGUILayout.LabelField( "No Amplify Color Volumes were found in the scene.", EditorStyles.wordWrappedLabel );
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();
		}

		void DrawVolumeProperties( AmplifyColorVolumeBase volume )
		{
			GameObject obj = volume.gameObject;

			GUILayout.BeginHorizontal();
			GUILayout.Space( 10 );

			volumeIcon = ( volumeIcon == null ) ? Resources.Load( "volume-icon", typeof( Texture2D ) ) as Texture2D : volumeIcon;
			GUILayout.Label( "", GUILayout.MinWidth( iconMinWidth ), GUILayout.MaxWidth( iconMaxWidth ) );
			GUILayout.Space( -iconMinWidth * 2 );
			GUILayout.Label( volumeIcon, GUILayout.Width( 20 ), GUILayout.Height( 20 ) );
			GUILayout.Space( 16 );

			GUILayout.Space( 0 );

			bool active = obj.activeInHierarchy;
			bool keep = EditorGUILayout.Toggle( active, GUILayout.MinWidth( activeMinWidth ), GUILayout.MaxWidth( activeMaxWidth ) );
			if ( keep != active )
				obj.SetActive( keep );

			GUILayout.Space( 6 );

			volume.ShowInSceneView = EditorGUILayout.Toggle( volume.ShowInSceneView, GUILayout.MinWidth( visibleMinWidth ), GUILayout.MaxWidth( visibleMaxWidth ) );

			GUILayout.Space( 6 );

			GUI.skin.textField.fontSize = 10;
			GUI.skin.textField.alignment = TextAnchor.UpperCenter;
			if ( GUILayout.Button( ( Selection.activeObject == obj ) ? "●" : "", EditorStyles.textField, GUILayout.Width( 16 ), GUILayout.Height( 16 ) ) )
				Selection.activeObject = ( Selection.activeObject == obj ) ? null : obj;

			GUILayout.Space( 0 );

			GUI.skin.textField.fontSize = 11;
			GUI.skin.textField.alignment = TextAnchor.MiddleLeft;

			string instId = obj.GetInstanceID().ToString();
			GUI.SetNextControlName( instId );

			if ( editObject != obj )
				EditorGUILayout.TextField( obj.name, GUILayout.MinWidth( nameMinWidth ), GUILayout.MaxWidth( nameMaxWidth ) );
			else
				editName = EditorGUILayout.TextField( editName, GUILayout.MinWidth( nameMinWidth ), GUILayout.MaxWidth( nameMaxWidth ) );

			if ( GUI.GetNameOfFocusedControl() == instId )
			{
				if ( editObject != obj )
				{
					editName = obj.name;
					editObject = obj;
				}
			}
			if ( Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return && editObject == obj )
			{
				obj.name = editName;
				editName = "";
				editObject = null;
				Repaint();
			}

			GUILayout.Space( 3 );

			EditorGUIUtility.labelWidth = 5;
			volume.EnterBlendTime = EditorGUILayout.FloatField( " ", volume.EnterBlendTime, GUILayout.MinWidth( blendMinWidth ), GUILayout.MaxWidth( blendMaxWidth ) );

			GUILayout.Space( 3 );

			volume.Exposure = EditorGUILayout.FloatField( " ", volume.Exposure, GUILayout.MinWidth( exposureMinWidth ), GUILayout.MaxWidth( exposureMaxWidth ) );

			GUILayout.Space( 3 );

			volume.Priority = EditorGUILayout.IntField( " ", volume.Priority, GUILayout.MinWidth( priorityMinWidth ), GUILayout.MaxWidth( priorityMaxWidth ) );

			GUILayout.EndHorizontal();
		}

		void DrawVolumeEffects( AmplifyColorVolumeBase volume )
		{
			GUIStyle layerTitleStyle = new GUIStyle( EditorStyles.miniBoldLabel );
			layerTitleStyle.alignment = TextAnchor.MiddleLeft;
			layerTitleStyle.margin = new RectOffset( 0, 0, 0, 0 );

			GUIStyle foldoutTitleStyle = new GUIStyle( EditorStyles.foldout );
			foldoutTitleStyle.fontSize = 10;

			GUILayout.BeginHorizontal();
			GUILayout.Space( 10 );
			GUILayout.Label( "", GUILayout.MinWidth( iconMinWidth ), GUILayout.MaxWidth( iconMaxWidth - 10 ) );
			GUILayout.Space( 2 );

			GUILayout.BeginVertical();
			GUILayout.Space( 0 );

			GUILayout.BeginHorizontal();
			bool foldout = SetFoldout( volume, EditorGUILayout.Foldout( GetFoldout( volume ), "Blend Effects", foldoutTitleStyle ) );
			GUILayout.EndHorizontal();

			GUILayout.Space( 3 );

			if ( foldout )
				DrawVolumeEffectFields( volume );

			GUILayout.EndVertical();

			GUILayout.Space( 0 );
			GUILayout.EndHorizontal();
		}

		Vector4 DrawFixedVector4Field( Vector4 vec )
		{
			Vector3 xyz = new Vector3( vec.x, vec.y, vec.z );
			xyz = EditorGUILayout.Vector3Field( "", xyz, GUILayout.MinWidth( minValueWidth / 4 * 3 - 4 ), GUILayout.MaxWidth( maxValueWidth / 4 * 3 - 4 ), GUILayout.MaxHeight( 16 ) );
			EditorGUIUtility.labelWidth = 16;
			float w = EditorGUILayout.FloatField( "W", vec.w, GUILayout.MinWidth( minValueWidth / 4 ), GUILayout.MaxWidth( maxValueWidth / 4 ), GUILayout.MaxHeight( 16 ) );
			return new Vector4( xyz.x, xyz.y, xyz.z, w );
		}

		void DrawVolumeEffectFields( AmplifyColorVolumeBase volume )
		{
			List<AmplifyColor.VolumeEffect> effectsToDelete = new List<AmplifyColor.VolumeEffect>();
			float removeButtonSize = 16;

			List<NewLineContainer> volumeLines = null;
			if ( !( newLines.TryGetValue( volume, out volumeLines ) ) )
				volumeLines = newLines[ volume ] = new List<NewLineContainer>();

			GUIStyle minusStyle = new GUIStyle( ( GUIStyle ) "OL Minus" );
			GUIStyle plusStyle = new GUIStyle( ( GUIStyle ) "OL Plus" );
			minusStyle.margin.top = 2;
			plusStyle.margin.top = 2;

		#region CurrentEffectFields
			int fieldPosition = 0;
			foreach ( AmplifyColor.VolumeEffect effectVol in volume.EffectContainer.volumes )
			{
				if ( effectVol.gameObject == null )
					continue;

				AmplifyColorBase effect = effectVol.gameObject;
				List<AmplifyColor.VolumeEffectComponent> compsToDelete = new List<AmplifyColor.VolumeEffectComponent>();

				foreach ( AmplifyColor.VolumeEffectComponent comp in effectVol.components )
				{
					Component c = effect.GetComponent( comp.componentName );
					if ( c == null )
						continue;

					List<AmplifyColor.VolumeEffectField> fieldsToDelete = new List<AmplifyColor.VolumeEffectField>();
					List<KeyValuePair<string,int>> fieldsToAdd = new List<KeyValuePair<string,int>>();

					foreach ( AmplifyColor.VolumeEffectField field in comp.fields )
					{
						EditorGUILayout.BeginHorizontal();
						GUILayout.Label( "", GUILayout.MinWidth( minIndentWidth ), GUILayout.MaxWidth( maxIndentWidth ) );
						GUILayout.Space( 0 );

						if ( GUILayout.Button( "", minusStyle, GUILayout.MinWidth( 18 ), GUILayout.MaxWidth( 18 ), GUILayout.Height( 20 ) ) )
							fieldsToDelete.Add( field );

						Camera selectedCamera = EditorGUILayout.ObjectField( effect.GetComponent<Camera>(), typeof( Camera ), true, GUILayout.MinWidth( minColumnWidth * 1.5f ), GUILayout.MaxWidth( maxColumnWidth * 1.5f ) ) as Camera;
						AmplifyColorBase selectedEffect = ( selectedCamera != null ) ? selectedCamera.GetComponent<AmplifyColorBase>() : null;
						if ( selectedEffect != effect )
						{
							fieldsToDelete.Add( field );
							dirtyVolumeFlags = true;
							volumeLines.Add( new NewLineContainer( selectedEffect ) );
						}

						Component[] compArray = AmplifyColor.VolumeEffect.ListAcceptableComponents( effectVol.gameObject );
						List<string> compFlagsArray = compArray.Select( s => s.GetType().Name ).ToList();
						compFlagsArray.Remove( comp.componentName );
						string[] compNamesArray = new string[] { comp.componentName }.Concat( compFlagsArray ).ToArray();
						int selectedComponent = 0;
						selectedComponent = EditorGUILayout.Popup( selectedComponent, compNamesArray, GUILayout.MinWidth( minColumnWidth ), GUILayout.MaxWidth( maxColumnWidth ) );
						if ( selectedComponent != 0 )
						{
							volumeLines.Add( new NewLineContainer( effect, compNamesArray[ selectedComponent ] ) );
							fieldsToDelete.Add( field );
							dirtyVolumeFlags = true;
						}

						FieldInfo[] fieldArray = AmplifyColor.VolumeEffectComponent.ListAcceptableFields( c );
						string[] fieldFlagsArray = fieldArray.Select( s => s.Name ).ToArray();
						string[] fieldNamesArray = comp.GetFieldNames();
						fieldFlagsArray = fieldFlagsArray.Except( fieldNamesArray ).ToArray();

						List<string> names = new List<string>();
						names.Add( field.fieldName );
						names.AddRange( fieldFlagsArray );
						for ( int i = 0; i < names.Count; i++ )
						{
							if ( i == 0 )
								continue;

							FieldInfo fi = Array.Find( fieldArray, s => ( names[ i ] == s.Name ) );
							if ( fi != null )
								names[ i ] += " : " + fi.FieldType.Name;
						}

						int selectedField = 0;
						selectedField = EditorGUILayout.Popup( selectedField, names.ToArray(), GUILayout.MinWidth( minColumnWidth ), GUILayout.MaxWidth( maxColumnWidth ) );
						if ( selectedField != 0 )
						{
							fieldsToAdd.Add( new KeyValuePair<string,int>( fieldFlagsArray[ selectedField - 1 ], fieldPosition ) );
							fieldsToDelete.Add( field );
							dirtyVolumeFlags = true;
						}
						fieldPosition++;

						switch ( field.fieldType )
						{
							case "System.Single": field.valueSingle = EditorGUILayout.FloatField( field.valueSingle, GUILayout.MinWidth( minValueWidth ), GUILayout.MaxWidth( maxValueWidth ) ); break;
							case "System.Boolean": field.valueBoolean = EditorGUILayout.Toggle( "", field.valueBoolean, GUILayout.MinWidth( minValueWidth ), GUILayout.MaxWidth( maxValueWidth ) ); break;
							case "UnityEngine.Vector2": field.valueVector2 = EditorGUILayout.Vector2Field( "", field.valueVector2, GUILayout.MinWidth( minValueWidth ), GUILayout.MaxWidth( maxValueWidth ), GUILayout.MaxHeight( 16 ) ); break;
							case "UnityEngine.Vector3": field.valueVector3 = EditorGUILayout.Vector3Field( "", field.valueVector3, GUILayout.MinWidth( minValueWidth ), GUILayout.MaxWidth( maxValueWidth ), GUILayout.MaxHeight( 16 ) ); break;
							case "UnityEngine.Vector4": field.valueVector4 = DrawFixedVector4Field( field.valueVector4 ); break;
							case "UnityEngine.Color": field.valueColor = EditorGUILayout.ColorField( field.valueColor, GUILayout.MinWidth( minValueWidth ), GUILayout.MaxWidth( maxValueWidth ) ); break;
							default: EditorGUILayout.LabelField( field.fieldType, GUILayout.MinWidth( minValueWidth ), GUILayout.MaxWidth( maxValueWidth ) ); break;
						}

						// COPY TO CLIPBOARD
						string luid = NewLineContainer.GenerateUniqueID( effect, comp.componentName, field.fieldName );
						bool copied = copyLines.ContainsKey( luid );
						bool keep = GUILayout.Toggle( copied, "c", removeButtonStyle, GUILayout.Width( removeButtonSize ), GUILayout.Height( removeButtonSize ) );
						if ( keep != copied )
						{
							if ( keep )
							{
								object valueCopy = null;
								switch ( field.fieldType )
								{
									case "System.Single": valueCopy = field.valueSingle; break;
									case "System.Boolean": valueCopy = field.valueBoolean; break;
									case "UnityEngine.Vector2": valueCopy = field.valueVector2; break;
									case "UnityEngine.Vector3": valueCopy = field.valueVector3; break;
									case "UnityEngine.Vector4": valueCopy = field.valueVector4; break;
									case "UnityEngine.Color": valueCopy = field.valueColor; break;
								}
								copyLines.Add( luid, new NewLineContainer( effect, comp.componentName, field.fieldName, field.fieldType, valueCopy ) );
							}
							else
								copyLines.Remove( luid );

							//Debug.Log( "CopyComplete: " + luid + ", " + keep + ", " + volume.name );
						}

						EditorGUILayout.EndHorizontal();
						GUILayout.Space( 2 );
					}

					bool fieldRemoved = false;
					foreach ( AmplifyColor.VolumeEffectField field in fieldsToDelete )
					{
						comp.RemoveEffectField( field );
						fieldRemoved = true;
					}

					foreach ( KeyValuePair<string,int> pair in fieldsToAdd )
					{
						FieldInfo pi = c.GetType().GetField( pair.Key);
						if ( pi != null )
							comp.AddField( pi, c, pair.Value );
					}

					if ( fieldRemoved && comp.fields.Count <= 0 )
						compsToDelete.Add( comp );
				}

				bool compRemoved = false;
				foreach ( AmplifyColor.VolumeEffectComponent comp in compsToDelete )
				{
					effectVol.RemoveEffectComponent( comp );
					compRemoved = true;
				}

				if ( compRemoved && effectVol.components.Count <= 0 )
					effectsToDelete.Add( effectVol );
			}

			foreach ( AmplifyColor.VolumeEffect effectVol in effectsToDelete )
				volume.EffectContainer.RemoveVolumeEffect( effectVol );
		#endregion CurrentEffectFields

		#region NewLines
			List<NewLineContainer> linesToDelete = new List<NewLineContainer>();
			foreach ( NewLineContainer line in volumeLines )
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.Label( "", GUILayout.MinWidth( minIndentWidth ), GUILayout.MaxWidth( maxIndentWidth ) );
				GUILayout.Space( 0 );

				if ( GUILayout.Button( "", minusStyle, GUILayout.MinWidth( 18 ), GUILayout.MaxWidth( 18 ), GUILayout.Height( 20 ) ) )
					linesToDelete.Add( line );

				Camera selectedCamera  = EditorGUILayout.ObjectField( line.camera, typeof( Camera ), true, GUILayout.MinWidth( minColumnWidth * 1.5f ), GUILayout.MaxWidth( maxColumnWidth * 1.5f ) ) as Camera;
				AmplifyColorBase selectedEffect = ( selectedCamera != null ) ? selectedCamera.GetComponent<AmplifyColorBase>() : null;
				if ( selectedEffect != null && selectedEffect != line.camera )
					line.SetCamera( selectedEffect );

				AmplifyColor.VolumeEffect effectVol = null;
				if ( line.camera != null )
					effectVol = volume.EffectContainer.FindVolumeEffect( line.camera );

				if ( line.camera != null )
				{
					Component[] compArray = AmplifyColor.VolumeEffect.ListAcceptableComponents( line.camera );
					List<string> names = compArray.Select( s => s.GetType().Name ).ToList<string>();
					int popupSelected = names.IndexOf( line.component ) + 1;
					names.Insert( 0, "<Component>" );
					int selectedComponent = popupSelected;
					selectedComponent = EditorGUILayout.Popup( selectedComponent, names.ToArray(), GUILayout.MinWidth( minColumnWidth ), GUILayout.MaxWidth( maxColumnWidth ) );
					if ( selectedComponent != popupSelected )
						line.SetComponent( selectedComponent == 0 ? null : names[ selectedComponent ] );
				}
				else
				{
					GUI.enabled = false;
					EditorGUILayout.Popup( 0, new string[] { "<Component>" }, GUILayout.MaxWidth( maxColumnWidth ) );
					GUI.enabled = true;
				}

				Component c = ( line.camera == null ) ? null : line.camera.GetComponent( line.component );

				AmplifyColor.VolumeEffectComponent comp = null;
				if ( effectVol != null )
					comp = effectVol.FindEffectComponent( line.component );

				if ( c != null )
				{
					FieldInfo[] fieldArray = AmplifyColor.VolumeEffectComponent.ListAcceptableFields( c );
					string[] fieldFlagsArray = fieldArray.Select( s => s.Name ).ToArray();
					if ( comp != null )
					{
						string[] fieldNamesArray = comp.GetFieldNames();
						fieldFlagsArray = fieldFlagsArray.Except( fieldNamesArray ).ToArray();
					}

					List<string> names = fieldFlagsArray.ToList();
					for ( int i = 0; i < names.Count; i++ )
					{
						FieldInfo fi = Array.Find( fieldArray, s => ( names[ i ] == s.Name ) );
						if ( fi != null )
							names[ i ] += " : " + fi.FieldType.Name;
					}
					names.Insert( 0, "<Field>" );

					int selectedField = 0;
					selectedField = EditorGUILayout.Popup( selectedField, names.ToArray(), GUILayout.MinWidth( minColumnWidth ), GUILayout.MaxWidth( maxColumnWidth ) );
					if ( selectedField > 0 )
					{
						FieldInfo pi = c.GetType().GetField( fieldFlagsArray[ selectedField - 1 ] );
						if ( pi != null )
						{
							if ( effectVol == null )
								effectVol = volume.EffectContainer.AddJustColorEffect( line.camera );

							if ( comp == null )
								comp = effectVol.AddComponent( c, null );

							comp.AddField( pi, c );
							linesToDelete.Add( line );
							dirtyVolumeFlags = true;
						}
					}

					EditorGUILayout.LabelField( "", GUILayout.MinWidth( minValueWidth ), GUILayout.MaxWidth( maxValueWidth ) );
				}
				else
				{
					GUI.enabled = false;
					EditorGUILayout.Popup( 0, new string[] { "<Field>" }, GUILayout.MaxWidth( maxColumnWidth ) );
					EditorGUILayout.TextField( "", GUILayout.MinWidth( minValueWidth ), GUILayout.MaxWidth( maxValueWidth ) );
					GUI.enabled = true;
				}

				if ( line.camera != null )
				{
					string luid = NewLineContainer.GenerateUniqueID( line.camera, line.component, line.fieldName );
					bool copied = copyLines.ContainsKey( luid );
					bool keep = GUILayout.Toggle( copied, "c", removeButtonStyle, GUILayout.Width( removeButtonSize ), GUILayout.Height( removeButtonSize ) );
					if ( keep != copied )
					{
						if ( keep )
							copyLines.Add( luid, new NewLineContainer( line.camera, line.component, line.fieldName, line.fieldType, line.value ) );
						else
							copyLines.Remove( luid );

						//Debug.Log( "CopyIncomplete: " + luid + ", " + keep + ", " + volume.name );
					}
				}
				else
				{
					GUI.enabled = false;
					GUILayout.Button( "c", removeButtonStyle, GUILayout.Width( removeButtonSize ), GUILayout.Height( removeButtonSize ) );
					GUI.enabled = true;
				}

				EditorGUILayout.EndHorizontal();
				GUILayout.Space( 2 );
			}

			foreach ( NewLineContainer line in linesToDelete )
			{
				copyLines.Remove( line.GenerateUniqueID() );
				//Debug.Log( "Removed " + line.GenerateUniqueID() );
				volumeLines.Remove( line );
			}
		#endregion NewLines

		#region AddPaste
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label( "", GUILayout.MinWidth( minIndentWidth ), GUILayout.MaxWidth( maxIndentWidth ) );
			GUILayout.Space( 0 );

			bool add = GUILayout.Button( "", plusStyle, GUILayout.MinWidth( 18 ), GUILayout.MaxWidth( 18 ), GUILayout.Height( 20 ) );
			if ( add || GUILayout.Button( "Add New", GUILayout.MinWidth( minColumnWidth ), GUILayout.MaxWidth( maxColumnWidth ), GUILayout.Height( 20 ) ) )
				volumeLines.Add( new NewLineContainer() );

			GUI.enabled = ( copyLines.Count > 0 );
			if ( GUILayout.Button( "Paste", GUILayout.MinWidth( minColumnWidth ), GUILayout.MaxWidth( maxColumnWidth ), GUILayout.Height( 20 ) ) )
			{
				foreach ( var pair in copyLines )
				{
					NewLineContainer line = pair.Value;
					Component c = ( line.camera == null ) ? null : line.camera.GetComponent( line.component );
					FieldInfo pi = ( c != null ) ? c.GetType().GetField( line.fieldName ) : null;

					if ( pi == null )
						volumeLines.Add( new NewLineContainer( line.camera, line.component, line.fieldName, line.fieldType, line.value ) );
					else
					{
						AmplifyColor.VolumeEffect effectVol = volume.EffectContainer.FindVolumeEffect( line.camera );
						if ( effectVol == null )
							effectVol = volume.EffectContainer.AddJustColorEffect( line.camera );

						AmplifyColor.VolumeEffectComponent comp = effectVol.FindEffectComponent( line.component );
						if ( comp == null )
							comp = effectVol.AddComponent( c, null );

						AmplifyColor.VolumeEffectField field = comp.FindEffectField( line.fieldName );
						if ( field == null )
							field = comp.AddField( pi, c );
						else
							Debug.LogWarning( "[AmplifyColor] Blend Effect field already added to Volume " + volume.name + "." );

						field.UpdateValue( line.value );
					}
				}

				dirtyVolumeFlags = true;
			}
			GUI.enabled = true;

			EditorGUILayout.EndHorizontal();
			GUILayout.Space( 5 );
		#endregion AddPaste
		}

		void DrawLut( AmplifyColorVolumeBase volume )
		{
			GUILayout.Space( 0 );
			volume.LutTexture = ( Texture2D ) EditorGUILayout.ObjectField( volume.LutTexture, typeof( Texture2D ), false, GUILayout.Width( lutWidth ), GUILayout.Height( lutWidth ) );
			GUILayout.FlexibleSpace();
			GUILayout.Space( 5 );
		}

		void DrawLineSeparator()
		{
			GUILayout.BeginHorizontal();
			GUILayout.Space( 10 );
			GUILayout.Label( "", GUILayout.MinWidth( iconMinWidth ), GUILayout.MaxWidth( iconMaxWidth - 10 ) );
			GUILayout.Space( 4 );
			GUILayout.BeginVertical();
			GUILayout.Box( "", GUILayout.MaxWidth( 538 ), GUILayout.Height( 1 ) );
			GUILayout.EndVertical();
			GUILayout.Space( 5 );
			GUILayout.EndHorizontal();
		}

		private double searchDelayDuration = 0.25f;
		private DateTime searchDelayStart = DateTime.Now;
		private AmplifyColorVolumeBase[] volumeList;

		private void DrawVolumeList()
		{
			double searchDelayElapsed = ( DateTime.Now - searchDelayStart ).TotalSeconds;
			if ( volumeList == null || searchDelayElapsed > searchDelayDuration )
			{
				volumeList = Resources.FindObjectsOfTypeAll( typeof( AmplifyColorVolumeBase ) ) as AmplifyColorVolumeBase[];
				searchDelayStart = DateTime.Now;
			}

			volumeList = volumeList.Where( c => c != null ).ToArray();
			List<AmplifyColorVolumeBase> filtered = volumeList.OrderBy( o => o.name ).ToList<AmplifyColorVolumeBase>();

			GUILayout.BeginVertical();
			GUILayout.Space( 15 );

			DrawHeader( filtered );

			GUILayout.Space( 15 );

			dirtyVolumeFlags = false;

			foreach ( AmplifyColorVolumeBase volume in filtered )
			{
				GUILayout.BeginHorizontal();
				GUILayout.BeginVertical();

				DrawVolumeProperties( volume );

				EditorGUILayout.Separator();

				DrawVolumeEffects( volume );

				GUILayout.Space( 3 );
				GUILayout.EndVertical();

				DrawLut( volume );

				GUILayout.EndHorizontal();

				DrawLineSeparator();
			}

			if ( dirtyVolumeFlags )
			{
				GUI.FocusControl( "" );
				AmplifyColorBase[] effects = Resources.FindObjectsOfTypeAll( typeof( AmplifyColorBase ) ) as AmplifyColorBase[];
				AmplifyColorVolumeBase[] volumes = Resources.FindObjectsOfTypeAll( typeof( AmplifyColorVolumeBase ) ) as AmplifyColorVolumeBase[];
				AmplifyColor.VolumeEffectFlags.UpdateCamFlags( effects, volumes );
			}

			GUILayout.EndVertical();

			if ( GUI.changed )
				SceneView.RepaintAll();
		}

		[MenuItem( "GameObject/Amplify Color/Amplify Color Volume", false, 10 )]
		static void CreateColorVolume()
		{
			GameObject go = new GameObject( "Amplify Color Volume" );
			go.AddComponent<BoxCollider>().isTrigger = true;
			go.AddComponent( Type.GetType( "AmplifyColorVolume, Assembly-CSharp" ) );
			Selection.activeObject = go;
		}

		[MenuItem( "GameObject/Amplify Color/Amplify Color Volume 2D", false, 10 )]
	 	static void CreateColorVolume2D()
		{
			GameObject go = new GameObject( "Amplify Color Volume 2D" );
			go.AddComponent<BoxCollider2D>().isTrigger = true;
			go.AddComponent( Type.GetType( "AmplifyColorVolume2D, Assembly-CSharp" ) );
			Selection.activeObject = go;
		}
	}
}
