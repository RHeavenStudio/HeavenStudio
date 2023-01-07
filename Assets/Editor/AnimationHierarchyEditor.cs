
#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
	
public class AnimationHierarchyEditor : EditorWindow {
	private static int columnWidth = 300;
	
	private Animator animatorObject;
	private List<AnimationClip> animationClips;
	private ArrayList pathsKeys;
	private Hashtable paths;

	Dictionary<string, string> tempPathOverrides;

	private Vector2 scrollPos = Vector2.zero;
	
	[MenuItem("Window/Animation Hierarchy Editor")]
	static void ShowWindow() {
		EditorWindow.GetWindow<AnimationHierarchyEditor>();
	}


	public AnimationHierarchyEditor(){
		animationClips = new List<AnimationClip>();
		tempPathOverrides = new Dictionary<string, string>();
	}
	
	void OnSelectionChange() {
		if (Selection.objects.Length > 1 )
		{
			Debug.Log ("Length? " + Selection.objects.Length);
			animationClips.Clear();
			foreach ( Object o in Selection.objects )
			{
				if ( o is AnimationClip ) animationClips.Add((AnimationClip)o);
			}
		}
		else if (Selection.activeObject is AnimationClip) {
			animationClips.Clear();
			animationClips.Add((AnimationClip)Selection.activeObject);
			FillModel();
		} else {
			animationClips.Clear();
		}
		
		this.Repaint();
	}

	private string sOriginalRoot = "Root";
	private string sNewRoot = "SomeNewObject/Root";

	void OnGUI() {
		if (Event.current.type == EventType.ValidateCommand) {
			switch (Event.current.commandName) {
			case "UndoRedoPerformed":
				FillModel();
				break;
			}
		}
		
		if (animationClips.Count > 0 ) {
			scrollPos = GUILayout.BeginScrollView(scrollPos, GUIStyle.none);
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Referenced Animator (Root):", GUILayout.Width(columnWidth));

			animatorObject = ((Animator)EditorGUILayout.ObjectField(
				animatorObject,
				typeof(Animator),
				true,
				GUILayout.Width(columnWidth))
							  );
			

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Animation Clip:", GUILayout.Width(columnWidth));

			if ( animationClips.Count == 1 )
			{
				animationClips[0] = ((AnimationClip)EditorGUILayout.ObjectField(
					animationClips[0],
					typeof(AnimationClip),
					true,
					GUILayout.Width(columnWidth))
								  );
			}		   
			else
			{
				GUILayout.Label("Multiple Anim Clips: " + animationClips.Count, GUILayout.Width(columnWidth));
			}
			EditorGUILayout.EndHorizontal();

			GUILayout.Space(20);

			EditorGUILayout.BeginHorizontal();

			sOriginalRoot = EditorGUILayout.TextField(sOriginalRoot, GUILayout.Width(columnWidth));
			sNewRoot = EditorGUILayout.TextField(sNewRoot, GUILayout.Width(columnWidth));
			if (GUILayout.Button("Replace Root")) {
				Debug.Log("O: "+sOriginalRoot+ " N: "+sNewRoot);
				ReplaceRoot(sOriginalRoot, sNewRoot);
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("Reference path:", GUILayout.Width(columnWidth));
			GUILayout.Label("Animated properties:", GUILayout.Width(columnWidth*0.5f));
			GUILayout.Label("(Count)", GUILayout.Width(60));
			GUILayout.Label("Object:", GUILayout.Width(columnWidth));
			EditorGUILayout.EndHorizontal();
			
			if (paths != null) 
			{
				foreach (string path in pathsKeys) 
				{
					GUICreatePathItem(path);
				}
			}
			
			GUILayout.Space(40);
			GUILayout.EndScrollView();
		} else {
			GUILayout.Label("Please select an Animation Clip");
		}
	}


	void GUICreatePathItem(string path) {
		string newPath = path;
		GameObject obj = FindObjectInRoot(path);
		GameObject newObj;
		ArrayList properties = (ArrayList)paths[path];

		string pathOverride = path;

		if ( tempPathOverrides.ContainsKey(path) ) pathOverride = tempPathOverrides[path];
		
		EditorGUILayout.BeginHorizontal();
		
		pathOverride = EditorGUILayout.TextField(pathOverride, GUILayout.Width(columnWidth));
		if ( pathOverride != path ) tempPathOverrides[path] = pathOverride;

		if (GUILayout.Button("Change", GUILayout.Width(60))) {
			newPath = pathOverride;
			tempPathOverrides.Remove(path);
		}
		
		EditorGUILayout.LabelField(
			properties != null ? properties.Count.ToString() : "0",
			GUILayout.Width(60)
			);
		
		Color standardColor = GUI.color;
		
		if (obj != null) {
			GUI.color = Color.green;
		} else {
			GUI.color = Color.red;
		}
		
		newObj = (GameObject)EditorGUILayout.ObjectField(
			obj,
			typeof(GameObject),
			true,
			GUILayout.Width(columnWidth)
			);
		
		GUI.color = standardColor;
		
		EditorGUILayout.EndHorizontal();
		
		try {
			if (obj != newObj) {
				UpdatePath(path, ChildPath(newObj));
			}
			
			if (newPath != path) {
				UpdatePath(path, newPath);
			}
		} catch (UnityException ex) {
			Debug.LogError(ex.Message);
		}
	}
	
	void OnInspectorUpdate() {
		this.Repaint();
	}
	
	void FillModel() {
		paths = new Hashtable();
		pathsKeys = new ArrayList();

		foreach ( AnimationClip animationClip in animationClips )
		{
			FillModelWithCurves(AnimationUtility.GetCurveBindings(animationClip));
			FillModelWithCurves(AnimationUtility.GetObjectReferenceCurveBindings(animationClip));
		}
	}
	
	private void FillModelWithCurves(EditorCurveBinding[] curves) {
		foreach (EditorCurveBinding curveData in curves) {
			string key = curveData.path;
			
			if (paths.ContainsKey(key)) {
				((ArrayList)paths[key]).Add(curveData);
			} else {
				ArrayList newProperties = new ArrayList();
				newProperties.Add(curveData);
				paths.Add(key, newProperties);
				pathsKeys.Add(key);
			}
		}
	}

	string sReplacementOldRoot;
	string sReplacementNewRoot;


	void ReplaceRoot(string oldRoot, string newRoot)
	{
		float fProgress = 0.0f;
		sReplacementOldRoot = oldRoot;
		sReplacementNewRoot = newRoot;

		AssetDatabase.StartAssetEditing();
		
		for ( int iCurrentClip = 0; iCurrentClip < animationClips.Count; iCurrentClip++ )
		{
			AnimationClip animationClip =  animationClips[iCurrentClip];
			Undo.RecordObject(animationClip, "Animation Hierarchy Root Change");
			
			for ( int iCurrentPath = 0; iCurrentPath < pathsKeys.Count; iCurrentPath ++)
			{
				string path = pathsKeys[iCurrentPath] as string;
				ArrayList curves = (ArrayList)paths[path];

				for (int i = 0; i < curves.Count; i++) 
				{
					EditorCurveBinding binding = (EditorCurveBinding)curves[i];

					if ( path.Contains(sReplacementOldRoot) )
					{
						if ( !path.Contains(sReplacementNewRoot) )
						{
							string sNewPath = Regex.Replace(path, "^"+sReplacementOldRoot, sReplacementNewRoot );												

							AnimationCurve curve = AnimationUtility.GetEditorCurve(animationClip, binding);
							if ( curve != null )
							{
								AnimationUtility.SetEditorCurve(animationClip, binding, null);				
								binding.path = sNewPath;
								AnimationUtility.SetEditorCurve(animationClip, binding, curve);
							}
							else
							{
								ObjectReferenceKeyframe[] objectReferenceCurve = AnimationUtility.GetObjectReferenceCurve(animationClip, binding);
								AnimationUtility.SetObjectReferenceCurve(animationClip, binding, null);
								binding.path = sNewPath;
								AnimationUtility.SetObjectReferenceCurve(animationClip, binding, objectReferenceCurve);
							}
						}
					}
				}
				
				// Update the progress meter
				float fChunk = 1f / animationClips.Count;
				fProgress = (iCurrentClip * fChunk) + fChunk * ((float) iCurrentPath / (float) pathsKeys.Count);				
				
				EditorUtility.DisplayProgressBar(
					"Animation Hierarchy Progress", 
					"How far along the animation editing has progressed.",
					fProgress);
			}

		}
		AssetDatabase.StopAssetEditing();
		EditorUtility.ClearProgressBar();
		
		FillModel();
		this.Repaint();
	}
	
	void UpdatePath(string oldPath, string newPath) 
	{
		if (paths[newPath] != null) {
			throw new UnityException("Path " + newPath + " already exists in that animation!");
		}
		AssetDatabase.StartAssetEditing();
		for ( int iCurrentClip = 0; iCurrentClip < animationClips.Count; iCurrentClip++ )
		{
			AnimationClip animationClip =  animationClips[iCurrentClip];
			Undo.RecordObject(animationClip, "Animation Hierarchy Change");
			
			//recreating all curves one by one
			//to maintain proper order in the editor - 
			//slower than just removing old curve
			//and adding a corrected one, but it's more
			//user-friendly
			for ( int iCurrentPath = 0; iCurrentPath < pathsKeys.Count; iCurrentPath ++)
			{
				string path = pathsKeys[iCurrentPath] as string;
				ArrayList curves = (ArrayList)paths[path];
				
				for (int i = 0; i < curves.Count; i++) {
					EditorCurveBinding binding = (EditorCurveBinding)curves[i];
					AnimationCurve curve = AnimationUtility.GetEditorCurve(animationClip, binding);
					ObjectReferenceKeyframe[] objectReferenceCurve = AnimationUtility.GetObjectReferenceCurve(animationClip, binding);


						if ( curve != null )
							AnimationUtility.SetEditorCurve(animationClip, binding, null);
						else
							AnimationUtility.SetObjectReferenceCurve(animationClip, binding, null);

						if (path == oldPath) 
							binding.path = newPath;

						if ( curve != null )
							AnimationUtility.SetEditorCurve(animationClip, binding, curve);
						else
							AnimationUtility.SetObjectReferenceCurve(animationClip, binding, objectReferenceCurve);

					float fChunk = 1f / animationClips.Count;
					float fProgress = (iCurrentClip * fChunk) + fChunk * ((float) iCurrentPath / (float) pathsKeys.Count);				
					
					EditorUtility.DisplayProgressBar(
						"Animation Hierarchy Progress", 
						"How far along the animation editing has progressed.",
						fProgress);
				}
			}
		}
		AssetDatabase.StopAssetEditing();
		EditorUtility.ClearProgressBar();
		FillModel();
		this.Repaint();
	}
	
	GameObject FindObjectInRoot(string path) {
		if (animatorObject == null) {
			return null;
		}
		
		Transform child = animatorObject.transform.Find(path);
		
		if (child != null) {
			return child.gameObject;
		} else {
			return null;
		}
	}
	
	string ChildPath(GameObject obj, bool sep = false) {
		if (animatorObject == null) {
			throw new UnityException("Please assign Referenced Animator (Root) first!");
		}
		
		if (obj == animatorObject.gameObject) {
			return "";
		} else {
			if (obj.transform.parent == null) {
				throw new UnityException("Object must belong to " + animatorObject.ToString() + "!");
			} else {
				return ChildPath(obj.transform.parent.gameObject, true) + obj.name + (sep ? "/" : "");
			}
		}
	}
}

#endif