// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;

namespace AmplifyColor
{
	[Serializable]
	public class VolumeEffectFieldFlags
	{
		public string fieldName;
		public string fieldType;
		public bool blendFlag = false;

		public VolumeEffectFieldFlags( FieldInfo pi )
		{
			fieldName = pi.Name;
			fieldType = pi.FieldType.FullName;
		}

		public VolumeEffectFieldFlags( VolumeEffectField field )
		{
			fieldName = field.fieldName;
			fieldType = field.fieldType;
			blendFlag = true; // why?...
		}
	}

	[Serializable]
	public class VolumeEffectComponentFlags
	{
		public string componentName;
		public List<VolumeEffectFieldFlags> componentFields;
		public bool blendFlag = false;

		public VolumeEffectComponentFlags( string name )
		{
			componentName = name;
			componentFields = new List<VolumeEffectFieldFlags>();
		}

		public VolumeEffectComponentFlags( VolumeEffectComponent comp )
			: this( comp.componentName )
		{
			blendFlag = true;
			foreach ( VolumeEffectField field in comp.fields )
			{
				if ( VolumeEffectField.IsValidType( field.fieldType ) )
					componentFields.Add( new VolumeEffectFieldFlags( field ) );
			}
		}

		public VolumeEffectComponentFlags( Component c )
			: this( c.GetType() + "" )
		{
		#if !UNITY_EDITOR && UNITY_METRO
			FieldInfo[] fields=c.GetType().GetRuntimeFields().ToArray();
		#else
			FieldInfo[] fields = c.GetType().GetFields();
		#endif
			foreach ( FieldInfo pi in fields )
			{
				if ( VolumeEffectField.IsValidType( pi.FieldType.FullName ) )
					componentFields.Add( new VolumeEffectFieldFlags( pi ) );
			}

		}

		public void UpdateComponentFlags( VolumeEffectComponent comp )
		{
			foreach ( VolumeEffectField field in comp.fields )
			{
				if ( componentFields.Find( s => s.fieldName == field.fieldName ) == null && VolumeEffectField.IsValidType( field.fieldType ) )
					componentFields.Add( new VolumeEffectFieldFlags( field ) );
			}
		}

		public void UpdateComponentFlags( Component c )
		{
		#if !UNITY_EDITOR && UNITY_METRO
			FieldInfo[] fields=c.GetType().GetRuntimeFields().ToArray();
		#else
			FieldInfo[] fields = c.GetType().GetFields();
		#endif
			foreach ( FieldInfo pi in fields )
			{
				if ( !componentFields.Exists( s => s.fieldName == pi.Name ) && VolumeEffectField.IsValidType( pi.FieldType.FullName ) )
					componentFields.Add( new VolumeEffectFieldFlags( pi ) );
			}
		}

		public string[] GetFieldNames()
		{
			return ( from r in componentFields where r.blendFlag select r.fieldName ).ToArray();
		}
	}

	[Serializable]
	public class VolumeEffectFlags
	{
		public List<VolumeEffectComponentFlags> components;

		public VolumeEffectFlags()
		{
			components = new List<VolumeEffectComponentFlags>();
		}

		public void AddComponent( Component c )
		{
			VolumeEffectComponentFlags componentFlags;
			if ( ( componentFlags = components.Find( s => s.componentName == c.GetType() + "" ) ) != null )
				componentFlags.UpdateComponentFlags( c );
			else
				components.Add( new VolumeEffectComponentFlags( c ) );
		}

		public void UpdateFlags( VolumeEffect effectVol )
		{
			foreach ( VolumeEffectComponent comp in effectVol.components )
			{
				VolumeEffectComponentFlags compFlags = null;
				if ( ( compFlags = components.Find( s => s.componentName == comp.componentName ) ) == null )
					components.Add( new VolumeEffectComponentFlags( comp ) );
				else
					compFlags.UpdateComponentFlags( comp );
			}
		}

		public static void UpdateCamFlags( AmplifyColorBase[] effects, AmplifyColorVolumeBase[] volumes )
		{
			foreach ( AmplifyColorBase effect in effects )
			{
				effect.EffectFlags = new VolumeEffectFlags();
				foreach ( AmplifyColorVolumeBase volume in volumes )
				{
					VolumeEffect effectVolume = volume.EffectContainer.FindVolumeEffect( effect );
					if ( effectVolume != null )
						effect.EffectFlags.UpdateFlags( effectVolume );
				}
			}
		}

		public VolumeEffect GenerateEffectData( AmplifyColorBase go )
		{
			VolumeEffect result = new VolumeEffect( go );
			foreach ( VolumeEffectComponentFlags compFlags in components )
			{
				if ( !compFlags.blendFlag )
					continue;

				Component c = go.GetComponent( compFlags.componentName );
				if ( c != null )
					result.AddComponent( c, compFlags );
			}
			return result;
		}

		public VolumeEffectComponentFlags FindComponentFlags( string compName )
		{
			for ( int i = 0; i < components.Count; i++ )
			{
				if ( components[ i ].componentName == compName )
					return components[ i ];
			}
			return null;
		}

		public string[] GetComponentNames()
		{
			return ( from r in components where r.blendFlag select r.componentName ).ToArray();
		}
	}
}
