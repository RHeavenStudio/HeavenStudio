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

public class AmplifyColorVolumeEditor : AmplifyColor.VolumeEditorBase
{
	[MenuItem( "Window/Amplify Color/Volume Editor", false, 1 )]
	public static void Init()
	{
		GetWindow<AmplifyColorVolumeEditor>( false, "Volume Editor", true );
	}
}
