using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class LogoAnimator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
         Animator animator = gameObject.GetComponent<Animator>();
         animator.Play("Beat");
    }

    void Update()
    {
        if (HeavenStudio.MenuConductor.instance.songPositionInBeats % 1f == 0f)
        {
            //animator.Play("Beat");
            Debug.Log("Beat");
        }
    }
}
