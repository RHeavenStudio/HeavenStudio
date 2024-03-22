using System.Collections;
using System.Collections.Generic;
using HeavenStudio;
using HeavenStudio.Games;
using HeavenStudio.Util;
using Jukebox;
using UnityEngine;

public class LakeScene : MonoBehaviour
{
    [SerializeField] public bool IsDummy = false;

    [SerializeField] public Animator FishAnimator;
    [SerializeField] public Animator BGAnimator;
    [SerializeField] public SpriteRenderer GradientBG;
    [SerializeField] public SpriteRenderer TopBG;
    [SerializeField] public SpriteRenderer BottomBG;
    [SerializeField] public SpriteRenderer[] FishSprites;

    [SerializeField] Color[] TopColors;
    [SerializeField] Color[] BottomColors;

    public RiqEntity Entity;
    public CatchOfTheDay Minigame;

    public int Setup(RiqEntity e, CatchOfTheDay minigame, int? lastLayout = null)
    {
        Debug.Log("Setting up lake...");
        Entity = e;
        Minigame = minigame;

        switch (e.datamodel)
        {
            case "catchOfTheDay/fish1":
                FishAnimator.DoScaledAnimationAsync("Fish1_Wait", 0.5f);
                minigame.ScheduleInput(e.beat, 2f, CatchOfTheDay.InputAction_BasicPress, Just, Through, Out);
                
                break;
            case "catchOfTheDay/fish2":
                FishAnimator.DoScaledAnimationAsync("Fish2_Wait", 0.5f);
                minigame.ScheduleInput(e.beat, 3f, CatchOfTheDay.InputAction_BasicPress, Just, Through, Out);
                break;
            case "catchOfTheDay/fish3":
                FishAnimator.DoScaledAnimationAsync("Fish3_Wait", 0.5f); // TODO whatif special anim
                minigame.ScheduleInput(e.beat, 4.5f, CatchOfTheDay.InputAction_BasicPress, Just, Through, Out);
                break;
            default:
                break;
        }

        int layout = e["layout"];
        if (layout == (int)CatchOfTheDay.FishLayout.Random)
        {
            List<int> layouts = new() { 0, 1, 2 };
            if (lastLayout is int ll)
                layouts.Remove(ll);
            layout = layouts[UnityEngine.Random.Range(0, layouts.Count)];
        }
        switch (layout)
        {
            case (int)CatchOfTheDay.FishLayout.LayoutC:
                BGAnimator.DoScaledAnimationAsync("LayoutC", 0.5f);
                break;
            case (int)CatchOfTheDay.FishLayout.LayoutB:
                BGAnimator.DoScaledAnimationAsync("LayoutB", 0.5f);
                break;
            case (int)CatchOfTheDay.FishLayout.LayoutA:
            default:
                BGAnimator.DoScaledAnimationAsync("LayoutA", 0.5f);
                break;
        }

        if (e["useCustomColor"])
        {
            SetBGColors(e["topColor"], e["bottomColor"]);
        }
        else
        {
            SetBGColors(TopColors[layout], BottomColors[layout]);
        }

        // TODO adjust position randomly

        return layout; // returning this so we can catalogue the most recent layout so we don't double up
    }
    public void SetBGColors(Color topColor, Color bottomColor)
    {
        Debug.Log(topColor);
        Debug.Log(bottomColor);

        GradientBG.color = topColor;
        TopBG.color = topColor;
        BottomBG.color = bottomColor;
        foreach (SpriteRenderer sprite in FishSprites)
        {
            sprite.color = bottomColor;
        }
    }

    public void Just(PlayerActionEvent caller, float state)
    {

    }
    public void Miss(PlayerActionEvent caller)
    {

    }
    public void Through(PlayerActionEvent caller)
    {

    }
    public void Out(PlayerActionEvent caller)
    {

    }
}
