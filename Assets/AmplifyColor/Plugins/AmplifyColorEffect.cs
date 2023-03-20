// Amplify Color - Advanced Color Grading for Unity
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using UnityEngine;

#if UNITY_5_6_OR_NEWER
[ImageEffectAllowedInSceneView]
#endif
[ImageEffectTransformsToLDR]
[ExecuteInEditMode]
[AddComponentMenu( "Image Effects/Amplify Color" )]
sealed public class AmplifyColorEffect : AmplifyColorBase
{
}
