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

public class AmplifyColorEffectEditorBase : Editor
{
	SerializedProperty tonemapper;
	SerializedProperty exposure;
	SerializedProperty linearWhitePoint;
	SerializedProperty useDithering;

	SerializedProperty qualityLevel;
	SerializedProperty blendAmount;
	SerializedProperty lutTexture;
	SerializedProperty lutBlendTexture;
	SerializedProperty maskTexture;
	SerializedProperty useDepthMask;
	SerializedProperty depthMaskCurve;

	SerializedProperty useVolumes;
	SerializedProperty exitVolumeBlendTime;
	SerializedProperty triggerVolumeProxy;
	SerializedProperty volumeCollisionMask;

	void OnEnable()
	{
		tonemapper = serializedObject.FindProperty( "Tonemapper" );
		exposure = serializedObject.FindProperty( "Exposure" );
		linearWhitePoint = serializedObject.FindProperty( "LinearWhitePoint" );
		useDithering = serializedObject.FindProperty( "ApplyDithering" );

		qualityLevel = serializedObject.FindProperty( "QualityLevel" );
		blendAmount = serializedObject.FindProperty( "BlendAmount" );
		lutTexture = serializedObject.FindProperty( "LutTexture" );
		lutBlendTexture = serializedObject.FindProperty( "LutBlendTexture" );
		maskTexture = serializedObject.FindProperty( "MaskTexture" );
		useDepthMask = serializedObject.FindProperty( "UseDepthMask" );
		depthMaskCurve = serializedObject.FindProperty( "DepthMaskCurve" );

		useVolumes = serializedObject.FindProperty( "UseVolumes" );
		exitVolumeBlendTime = serializedObject.FindProperty( "ExitVolumeBlendTime" );
		triggerVolumeProxy = serializedObject.FindProperty( "TriggerVolumeProxy" );
		volumeCollisionMask = serializedObject.FindProperty( "VolumeCollisionMask" );

		if ( !Application.isPlaying )
		{
			AmplifyColorBase effect = target as AmplifyColorBase;

			bool needsNewID = string.IsNullOrEmpty( effect.SharedInstanceID );
			if ( !needsNewID )
				needsNewID = FindClone( effect );

			if ( needsNewID )
			{
				effect.NewSharedInstanceID();
				EditorUtility.SetDirty( target );
			}
		}
	}

	bool FindClone( AmplifyColorBase effect )
	{
	#if UNITY_2018_3_OR_NEWER
		GameObject effectPrefab = PrefabUtility.GetCorrespondingObjectFromSource( effect.gameObject ) as GameObject;
		PrefabAssetType effectPrefabType = PrefabUtility.GetPrefabAssetType( effect.gameObject );
		PrefabInstanceStatus effectPrefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus( effect.gameObject );
		bool effectIsPrefab = ( effectPrefabType != PrefabAssetType.NotAPrefab && effectPrefabInstanceStatus == PrefabInstanceStatus.NotAPrefab );
	#else
	#if UNITY_2018_2_OR_NEWER
		GameObject effectPrefab = PrefabUtility.GetCorrespondingObjectFromSource( effect.gameObject ) as GameObject;
	#else
		GameObject effectPrefab = PrefabUtility.GetPrefabParent( effect.gameObject ) as GameObject;
	#endif
		PrefabType effectPrefabType = PrefabUtility.GetPrefabType( effect.gameObject );
		bool effectIsPrefab = ( effectPrefabType != PrefabType.None && effectPrefabType != PrefabType.PrefabInstance );
	#endif
		bool effectHasPrefab = ( effectPrefab != null );

		AmplifyColorBase[] all = Resources.FindObjectsOfTypeAll( typeof( AmplifyColorBase ) ) as AmplifyColorBase[];
		bool foundClone = false;

		foreach ( AmplifyColorBase other in all )
		{
			if ( other == effect || other.SharedInstanceID != effect.SharedInstanceID )
			{
				// skip: same effect or already have different ids
				continue;
			}

		#if UNITY_2018_3_OR_NEWER
			GameObject otherPrefab = PrefabUtility.GetCorrespondingObjectFromSource( other.gameObject ) as GameObject;
			PrefabAssetType otherPrefabType = PrefabUtility.GetPrefabAssetType( other.gameObject );
			PrefabInstanceStatus otherPrefabInstanceStatus = PrefabUtility.GetPrefabInstanceStatus( other.gameObject );
			bool otherIsPrefab = ( otherPrefabType != PrefabAssetType.NotAPrefab && otherPrefabInstanceStatus == PrefabInstanceStatus.NotAPrefab );
		#else
		#if UNITY_2018_2_OR_NEWER
			GameObject otherPrefab = PrefabUtility.GetCorrespondingObjectFromSource( other.gameObject ) as GameObject;
		#else
			GameObject otherPrefab = PrefabUtility.GetPrefabParent( other.gameObject ) as GameObject;
		#endif
			PrefabType otherPrefabType = PrefabUtility.GetPrefabType( other.gameObject );
			bool otherIsPrefab = ( otherPrefabType != PrefabType.None && otherPrefabType != PrefabType.PrefabInstance );
		#endif
			bool otherHasPrefab = ( otherPrefab != null );

			if ( otherIsPrefab && effectHasPrefab && effectPrefab == other.gameObject )
			{
				// skip: other is a prefab and effect's prefab is other
				continue;
			}

			if ( effectIsPrefab && otherHasPrefab && otherPrefab == effect.gameObject )
			{
				// skip: effect is a prefab and other's prefab is effect
				continue;
			}

			if ( !effectIsPrefab && !otherIsPrefab && effectHasPrefab && otherHasPrefab && effectPrefab == otherPrefab )
			{
				// skip: both aren't prefabs and both have the same parent prefab
				continue;
			}

			foundClone = true;
		}

		return foundClone;
	}

	void ToggleContextTitle( SerializedProperty prop, string title )
	{
		GUILayout.Space( 5 );
		GUILayout.BeginHorizontal();
		prop.boolValue = GUILayout.Toggle( prop.boolValue, "", GUILayout.Width( 15 ) );
		GUILayout.BeginVertical();
		GUILayout.Space( 3 );
		GUILayout.Label( title );
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		Camera ownerCamera = ( target as AmplifyColorBase ).GetComponent<Camera>();

		GUILayout.BeginVertical();

		if ( ownerCamera != null )
		{
			GUILayout.Space( 12 );
			GUILayout.Label( "HDR Control", EditorStyles.boldLabel );
			GUILayout.Space( -4 );
			EditorGUILayout.PropertyField( tonemapper );
			EditorGUILayout.PropertyField( exposure );
			GUI.enabled = ( tonemapper.enumValueIndex == ( int ) AmplifyColor.Tonemapping.FilmicHable );
			EditorGUILayout.PropertyField( linearWhitePoint );
			GUI.enabled = true;
			linearWhitePoint.floatValue = Mathf.Max( 0, linearWhitePoint.floatValue );
			EditorGUILayout.PropertyField( useDithering );

			GUILayout.Space( 4 );
		}
		else
			GUILayout.Space( 12 );

		GUILayout.Label( "Color Grading", EditorStyles.boldLabel );
		GUILayout.Space( -4 );
		EditorGUILayout.PropertyField( qualityLevel );
		EditorGUILayout.PropertyField( blendAmount );
		EditorGUILayout.PropertyField( lutTexture );
		EditorGUILayout.PropertyField( lutBlendTexture );
		EditorGUILayout.PropertyField( maskTexture );
		EditorGUILayout.PropertyField( useDepthMask );
		EditorGUILayout.PropertyField( depthMaskCurve );

		GUILayout.Space( 4 );
		GUILayout.Label( "Effect Volumes", EditorStyles.boldLabel );
		GUILayout.Space( -4 );
		EditorGUILayout.PropertyField( useVolumes );
		EditorGUILayout.PropertyField( exitVolumeBlendTime );
		EditorGUILayout.PropertyField( triggerVolumeProxy );
		EditorGUILayout.PropertyField( volumeCollisionMask );

	#if UNITY_5_6_OR_NEWER
		bool hdr = ownerCamera.allowHDR;
	#else
		bool hdr = ownerCamera.hdr;
	#endif

		if ( ownerCamera != null && ( tonemapper.enumValueIndex != ( int ) AmplifyColor.Tonemapping.Disabled || exposure.floatValue != 1.0f ||
			linearWhitePoint.floatValue != 11.2f || useDithering.boolValue ) && !hdr )
		{
			GUILayout.Space( 4 );
			EditorGUILayout.HelpBox( "HDR Control requires Camera HDR to be enabled", MessageType.Warning );
		}

		GUILayout.Space( 4 );
		GUILayout.EndHorizontal();

		serializedObject.ApplyModifiedProperties();
	}
}
