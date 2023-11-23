using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessingVFX : MonoBehaviour
{
    private PostProcessVolume _volume;

    // events


    private void Awake()
    {
        _volume = GetComponent<PostProcessVolume>();
    }


}
