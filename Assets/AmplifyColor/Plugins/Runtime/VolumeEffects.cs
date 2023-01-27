// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System;
using System.Linq;

namespace AmplifyColor
{
	[Serializable]
	public class VolumeEffectField
	{
		public string fieldName;
		public string fieldType;
		public System.Single valueSingle;
		public UnityEngine.Color valueColor;
		public System.Boolean valueBoolean;
		public UnityEngine.Vector2 valueVector2;
		public UnityEngine.Vector3 valueVector3;
		public UnityEngine.Vector4 valueVector4;

		public VolumeEffectField( string fieldName, string fieldType )
		{
			this.fieldName = fieldName;
			this.fieldType = fieldType;
		}

		public VolumeEffectField( FieldInfo pi, Component c )
			: this( pi.Name, pi.FieldType.FullName )
		{
			object val = pi.GetValue( c );
			UpdateValue( val );
		}

		public static bool IsValidType( string type )
		{
			switch ( type )
			{
				case "System.Single":
				case "System.Boolean":
				case "UnityEngine.Color":
				case "UnityEngine.Vector2":
				case "UnityEngine.Vector3":
				case "UnityEngine.Vector4":
					return true;
			}
			return false;
		}

		public void UpdateValue( object val )
		{
			switch ( fieldType )
			{
				case "System.Single": valueSingle = ( System.Single ) val; break;
				case "System.Boolean": valueBoolean = ( System.Boolean ) val; break;
				case "UnityEngine.Color": valueColor = ( UnityEngine.Color ) val; break;
				case "UnityEngine.Vector2": valueVector2 = ( UnityEngine.Vector2 ) val; break;
				case "UnityEngine.Vector3": valueVector3 = ( UnityEngine.Vector3 ) val; break;
				case "UnityEngine.Vector4": valueVector4 = ( UnityEngine.Vector4 ) val; break;
			}
		}
	}

	[Serializable]
	public class VolumeEffectComponent
	{
		public string componentName;
		public List<VolumeEffectField> fields;

		public VolumeEffectComponent( string name )
		{
			componentName = name;
			fields = new List<VolumeEffectField>();
		}

		public VolumeEffectField AddField( FieldInfo pi, Component c )
		{
			return AddField( pi, c, -1 );
		}

		public VolumeEffectField AddField( FieldInfo pi, Component c, int position )
		{
			VolumeEffectField field = VolumeEffectField.IsValidType( pi.FieldType.FullName ) ? new VolumeEffectField( pi, c ) : null;
			if ( field != null )
			{
				if ( position < 0 || position >= fields.Count )
					fields.Add( field );
				else
					fields.Insert( position, field );
			}
			return field;
		}

		public void RemoveEffectField( VolumeEffectField field )
		{
			fields.Remove( field );
		}

		public VolumeEffectComponent( Component c, VolumeEffectComponentFlags compFlags )
			: this( compFlags.componentName )
		{
			foreach ( VolumeEffectFieldFlags fieldFlags in compFlags.componentFields )
			{
				if ( !fieldFlags.blendFlag )
					continue;

			#if !UNITY_EDITOR && UNITY_METRO
				FieldInfo pi = c.GetType ().GetRuntimeField (fieldFlags.fieldName);
			#else
				FieldInfo pi = c.GetType().GetField( fieldFlags.fieldName );
			#endif
				VolumeEffectField field = VolumeEffectField.IsValidType( pi.FieldType.FullName ) ? new VolumeEffectField( pi, c ) : null;
				if ( field != null )
					fields.Add( field );
			}
		}

		public void UpdateComponent( Component c, VolumeEffectComponentFlags compFlags )
		{
			foreach ( VolumeEffectFieldFlags fieldFlags in compFlags.componentFields )
			{
				if ( !fieldFlags.blendFlag )
					continue;

				if ( !fields.Exists( s => s.fieldName == fieldFlags.fieldName ) )
				{
				#if !UNITY_EDITOR && UNITY_METRO
					FieldInfo pi=c.GetType().GetRuntimeField(fieldFlags.fieldName);
				#else
					FieldInfo pi = c.GetType().GetField( fieldFlags.fieldName );
				#endif
					VolumeEffectField field = VolumeEffectField.IsValidType( pi.FieldType.FullName ) ? new VolumeEffectField( pi, c ) : null;
					if ( field != null )
						fields.Add( field );
				}
			}
		}

		public VolumeEffectField FindEffectField( string fieldName )
		{
			for ( int i = 0; i < fields.Count; i++ )
			{
				if ( fields[ i ].fieldName == fieldName )
					return fields[ i ];
			}
			return null;
		}

		public static FieldInfo[] ListAcceptableFields( Component c )
		{
			if ( c == null )
				return new FieldInfo[ 0 ];

		#if !UNITY_EDITOR && UNITY_METRO
			FieldInfo[] fields=c.GetType().GetRuntimeFields().ToArray();
		#else
			FieldInfo[] fields = c.GetType().GetFields();
		#endif

			return ( fields.Where( f => VolumeEffectField.IsValidType( f.FieldType.FullName ) ) ).ToArray();
		}

		public string[] GetFieldNames()
		{
			return ( from r in fields select r.fieldName ).ToArray();
		}
	}

	[Serializable]
	public class VolumeEffect
	{
		public AmplifyColorBase gameObject;
		public List<VolumeEffectComponent> components;

		public VolumeEffect( AmplifyColorBase effect )
		{
			gameObject = effect;
			components = new List<VolumeEffectComponent>();
		}

		public static VolumeEffect BlendValuesToVolumeEffect( VolumeEffectFlags flags, VolumeEffect volume1, VolumeEffect volume2, float blend )
		{
			VolumeEffect resultVolume = new VolumeEffect( volume1.gameObject );
			foreach ( VolumeEffectComponentFlags compFlags in flags.components )
			{
				if ( !compFlags.blendFlag )
					continue;

				VolumeEffectComponent ec1 = volume1.FindEffectComponent( compFlags.componentName );
				VolumeEffectComponent ec2 = volume2.FindEffectComponent( compFlags.componentName );
				if ( ec1 == null || ec2 == null )
					continue;

				VolumeEffectComponent resultComp = new VolumeEffectComponent( ec1.componentName );
				foreach ( VolumeEffectFieldFlags fieldFlags in compFlags.componentFields )
				{
					if ( fieldFlags.blendFlag )
					{
						VolumeEffectField ef1 = ec1.FindEffectField( fieldFlags.fieldName );
						VolumeEffectField ef2 = ec2.FindEffectField( fieldFlags.fieldName );
						if ( ef1 == null || ef2 == null )
							continue;

						VolumeEffectField resultField = new VolumeEffectField( ef1.fieldName, ef1.fieldType );

						switch ( resultField.fieldType )
						{
							case "System.Single": resultField.valueSingle = Mathf.Lerp( ef1.valueSingle, ef2.valueSingle, blend ); break;
							case "System.Boolean": resultField.valueBoolean = ef2.valueBoolean; break;
							case "UnityEngine.Vector2": resultField.valueVector2 = Vector2.Lerp( ef1.valueVector2, ef2.valueVector2, blend ); break;
							case "UnityEngine.Vector3": resultField.valueVector3 = Vector3.Lerp( ef1.valueVector3, ef2.valueVector3, blend ); break;
							case "UnityEngine.Vector4": resultField.valueVector4 = Vector4.Lerp( ef1.valueVector4, ef2.valueVector4, blend ); break;
							case "UnityEngine.Color": resultField.valueColor = Color.Lerp( ef1.valueColor, ef2.valueColor, blend ); break;
						}

						resultComp.fields.Add( resultField );
					}
				}
				resultVolume.components.Add( resultComp );
			}
			return resultVolume;
		}

		public VolumeEffectComponent AddComponent( Component c, VolumeEffectComponentFlags compFlags )
		{
			if ( compFlags == null )
			{
				VolumeEffectComponent created = new VolumeEffectComponent( c.GetType() + "" );
				components.Add( created );
				return created;
			}
			else
			{
				VolumeEffectComponent component;
				if ( ( component = FindEffectComponent( c.GetType() + "" ) ) != null )
				{
					component.UpdateComponent( c, compFlags );
					return component;
				}
				else
				{
					VolumeEffectComponent created = new VolumeEffectComponent( c, compFlags );
					components.Add( created );
					return created;
				}
			}
		}
		public void RemoveEffectComponent( VolumeEffectComponent comp )
		{
			components.Remove( comp );
		}

		public void UpdateVolume()
		{
			if ( gameObject == null )
				return;

			VolumeEffectFlags effectFlags = gameObject.EffectFlags;
			foreach ( VolumeEffectComponentFlags compFlags in effectFlags.components )
			{
				if ( !compFlags.blendFlag )
					continue;

				Component c = gameObject.GetComponent( compFlags.componentName );
				if ( c != null )
					AddComponent( c, compFlags );
			}
		}

		public void SetValues( AmplifyColorBase targetColor )
		{
			VolumeEffectFlags effectFlags = targetColor.EffectFlags;
			GameObject go = targetColor.gameObject;
			foreach ( VolumeEffectComponentFlags compFlags in effectFlags.components )
			{
				if ( !compFlags.blendFlag )
					continue;

				Component c = go.GetComponent( compFlags.componentName );
				VolumeEffectComponent effectComp = FindEffectComponent( compFlags.componentName );
				if ( c == null || effectComp == null )
					continue;

				foreach ( VolumeEffectFieldFlags fieldFlags in compFlags.componentFields )
				{
					if ( !fieldFlags.blendFlag )
						continue;

				#if !UNITY_EDITOR && UNITY_METRO
					FieldInfo fi = c.GetType().GetRuntimeField(fieldFlags.fieldName);
				#else
					FieldInfo fi = c.GetType().GetField( fieldFlags.fieldName );
				#endif
					VolumeEffectField effectField = effectComp.FindEffectField( fieldFlags.fieldName );
					if ( fi == null || effectField == null )
						continue;

					switch ( fi.FieldType.FullName )
					{
						case "System.Single": fi.SetValue( c, effectField.valueSingle ); break;
						case "System.Boolean": fi.SetValue( c, effectField.valueBoolean ); break;
						case "UnityEngine.Vector2": fi.SetValue( c, effectField.valueVector2 ); break;
						case "UnityEngine.Vector3": fi.SetValue( c, effectField.valueVector3 ); break;
						case "UnityEngine.Vector4": fi.SetValue( c, effectField.valueVector4 ); break;
						case "UnityEngine.Color": fi.SetValue( c, effectField.valueColor ); break;
					}
				}
			}
		}

		public void BlendValues( AmplifyColorBase targetColor, VolumeEffect other, float blendAmount )
		{
			VolumeEffectFlags effectFlags = targetColor.EffectFlags;
			GameObject go = targetColor.gameObject;

			for ( int comp = 0; comp < effectFlags.components.Count; comp++ )
			{
				VolumeEffectComponentFlags compFlags = effectFlags.components[ comp ];
				if ( !compFlags.blendFlag )
					continue;

				Component c = go.GetComponent( compFlags.componentName );
				VolumeEffectComponent effectComp = FindEffectComponent( compFlags.componentName );
				VolumeEffectComponent effectCompOther = other.FindEffectComponent( compFlags.componentName );
				if ( c == null || effectComp == null || effectCompOther == null )
					continue;

				for ( int i = 0; i < compFlags.componentFields.Count; i++ )
				{
					VolumeEffectFieldFlags fieldFlags = compFlags.componentFields[ i ];
					if ( !fieldFlags.blendFlag )
						continue;

				#if !UNITY_EDITOR && UNITY_METRO
					FieldInfo fi = c.GetType().GetRuntimeField(fieldFlags.fieldName);
				#else
					FieldInfo fi = c.GetType().GetField( fieldFlags.fieldName );
				#endif
					VolumeEffectField effectField = effectComp.FindEffectField( fieldFlags.fieldName );
					VolumeEffectField effectFieldOther = effectCompOther.FindEffectField( fieldFlags.fieldName );

					if ( fi == null || effectField == null || effectFieldOther == null )
						continue;

					switch ( fi.FieldType.FullName )
					{
						case "System.Single": fi.SetValue( c, Mathf.Lerp( effectField.valueSingle, effectFieldOther.valueSingle, blendAmount ) ); break;
						case "System.Boolean": fi.SetValue( c, effectFieldOther.valueBoolean ); break;
						case "UnityEngine.Vector2": fi.SetValue( c, Vector2.Lerp( effectField.valueVector2, effectFieldOther.valueVector2, blendAmount ) ); break;
						case "UnityEngine.Vector3": fi.SetValue( c, Vector3.Lerp( effectField.valueVector3, effectFieldOther.valueVector3, blendAmount ) ); break;
						case "UnityEngine.Vector4": fi.SetValue( c, Vector4.Lerp( effectField.valueVector4, effectFieldOther.valueVector4, blendAmount ) ); break;
						case "UnityEngine.Color": fi.SetValue( c, Color.Lerp( effectField.valueColor, effectFieldOther.valueColor, blendAmount ) ); break;
					}
				}
			}
		}

		public VolumeEffectComponent FindEffectComponent( string compName )
		{
			for ( int i = 0; i < components.Count; i++ )
			{
				if ( components[ i ].componentName == compName )
					return components[ i ];
			}
			return null;
		}

		public static Component[] ListAcceptableComponents( AmplifyColorBase go )
		{
			if ( go == null )
				return new Component[ 0 ];

			Component[] comps = go.GetComponents( typeof( Component ) );
			return ( comps.Where( comp => comp != null && ( !( ( comp.GetType() + "" ).StartsWith( "UnityEngine." ) || comp.GetType() == typeof( AmplifyColorBase ) ) ) ) ).ToArray();
		}

		public string[] GetComponentNames()
		{
			return ( from r in components select r.componentName ).ToArray();
		}
	}

	[Serializable]
	public class VolumeEffectContainer
	{
		public List<VolumeEffect> volumes;

		public VolumeEffectContainer()
		{
			volumes = new List<VolumeEffect>();
		}

		public void AddColorEffect( AmplifyColorBase colorEffect )
		{
			VolumeEffect volume;
			if ( ( volume = FindVolumeEffect( colorEffect ) ) != null )
				volume.UpdateVolume();
			else
			{
				volume = new VolumeEffect( colorEffect );
				volumes.Add( volume );
				volume.UpdateVolume();
			}
		}

		public VolumeEffect AddJustColorEffect( AmplifyColorBase colorEffect )
		{
			VolumeEffect created = new VolumeEffect( colorEffect );
			volumes.Add( created );
			return created;
		}

		public VolumeEffect FindVolumeEffect( AmplifyColorBase colorEffect )
		{
			// find by reference first
			for ( int i = 0; i < volumes.Count; i++ )
			{
				if ( volumes[ i ].gameObject == colorEffect )
					return volumes[ i ];
			}

			// in case reference fails, find by instance id (e.g. instantiated prefabs)
			for ( int i = 0; i < volumes.Count; i++ )
			{
				if ( volumes[ i ].gameObject != null && volumes[ i ].gameObject.SharedInstanceID == colorEffect.SharedInstanceID )
					return volumes[ i ];
			}

			// not found
			return null;
		}

		public void RemoveVolumeEffect( VolumeEffect volume )
		{
			volumes.Remove( volume );
		}

		public AmplifyColorBase[] GetStoredEffects()
		{
			return ( from r in volumes select r.gameObject ).ToArray();
		}
	}
}



