// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;

[ExecuteInEditMode]
[AddComponentMenu( "" )]
public class AmplifyColorVolumeBase : MonoBehaviour
{
	public Texture2D LutTexture;
	public float Exposure = 1.0f;
	public float EnterBlendTime = 1.0f;
	public int Priority = 0;
	public bool ShowInSceneView = true;

	[HideInInspector] public AmplifyColor.VolumeEffectContainer EffectContainer = new AmplifyColor.VolumeEffectContainer();

#if UNITY_EDITOR
	public static EditorWindow Window;

	void OnEnable()
	{
		if ( Window != null )
			Window.Repaint();
	}

	void OnDestroy()
	{
		if ( Window != null )
			Window.Repaint();
	}
#endif

	void OnDrawGizmos()
	{
		if ( ShowInSceneView )
		{
			BoxCollider box = GetComponent<BoxCollider>();
			BoxCollider2D box2d = GetComponent<BoxCollider2D>();

			if ( box != null || box2d != null )
			{
				Vector3 center, size;
				if ( box != null )
				{
					center = box.center;
					size = box.size;
				}
				else
				{
				#if UNITY_5_6_OR_NEWER
					center = box2d.offset;
				#else
					center = box2d.center;
				#endif
					size = box2d.size;
				}

				Gizmos.color = Color.green;
				Gizmos.matrix = transform.localToWorldMatrix;
				Gizmos.DrawWireCube( center, size );
			}
		}
	}

	void OnDrawGizmosSelected()
	{
		BoxCollider box = GetComponent<BoxCollider>();
		BoxCollider2D box2d = GetComponent<BoxCollider2D>();
		if ( box != null || box2d != null )
		{
			Color col = Color.green;
			col.a = 0.2f;
			Gizmos.color = col;
			Gizmos.matrix = transform.localToWorldMatrix;

			Vector3 center, size;
			if ( box != null )
			{
				center = box.center;
				size = box.size;
			}
			else
			{
			#if UNITY_5_6_OR_NEWER
				center = box2d.offset;
			#else
				center = box2d.center;
			#endif
				size = box2d.size;
			}
			Gizmos.DrawCube( center, size );
		}
	}
}
