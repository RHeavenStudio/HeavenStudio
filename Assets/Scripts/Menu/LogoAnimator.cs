using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class LogoAnimator : MonoBehaviour
{
    public float currentBeatRem = 0;
    public float lastBeatRem = 0;
    public bool beatSide = false;   // false = right, true = left

    // Start is called before the first frame update
    void Start()
    {
         
    }

    void Update()
    {
        Animator animator = gameObject.GetComponent<Animator>();

        currentBeatRem = HeavenStudio.MenuConductor.instance.songPositionInBeats % 1;

        if (currentBeatRem < lastBeatRem & beatSide == false)
        {
            animator.Play("BeatR");
            beatSide = true;
            Debug.Log("Beat");
            //animator.Play("idle");
        }
        else if (currentBeatRem < lastBeatRem & beatSide == true)
        {
            animator.Play("BeatL");
            beatSide = false;
            Debug.Log("BeatL");
            //animator.Play("idle");
        }

        lastBeatRem = HeavenStudio.MenuConductor.instance.songPositionInBeats % 1;
    }
}
