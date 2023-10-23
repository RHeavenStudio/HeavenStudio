using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio;
using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_KarateMan
{
    public class KarateManJoe : MonoBehaviour
    {
        public Animator anim;
        public Animator FaceAnim;
        public GameEvent bop = new GameEvent();
        public SpriteRenderer[] Shadows;

        public Color BombGlowTint;
        double bombGlowStart = double.MinValue;
        float bombGlowLength = 0f;
        float bombGlowIntensity = 0f;
        const float bombGlowRatio = 1f;

        double lastPunchTime = double.MinValue;
        double lastComboMissTime = double.MinValue;
        double lastUpperCutTime = double.MinValue;
        public bool inCombo = false;
        public bool lockedInCombo = false;
        public bool comboWaiting = false;
        int inComboId = -1;
        int shouldComboId = -1;
        public void SetComboId(int id) { inComboId = id; }
        public void SetShouldComboId(int id) { shouldComboId = id; }
        public int GetComboId() { return inComboId; }
        public int GetShouldComboId() { return shouldComboId; }

        public bool wantKick = false;
        public bool inKick = false;
        double lastChargeTime = double.MinValue;
        double unPrepareTime = double.MinValue;
        double noNuriJabTime = double.MinValue;
        bool canEmote = false;
        public int wantFace = 0;

        public bool inSpecial { get { return inCombo || lockedInCombo || 
            Conductor.instance.GetPositionFromBeat(lastChargeTime, 2.75f) <= 0.25f || inNuriLock; } }
        public bool inNuriLock { get { return (Conductor.instance.songPositionInBeatsAsDouble >= noNuriJabTime && Conductor.instance.songPositionInBeatsAsDouble < noNuriJabTime + 1f); } }

        private void Update()
        {
            var cond = Conductor.instance;

            if (cond.songPositionInBeatsAsDouble < bombGlowStart)
            {
                bombGlowIntensity = 1f;
            }
            else
            {
                float glowProg = cond.GetPositionFromBeat(bombGlowStart, bombGlowLength);
                bombGlowIntensity = 1f - glowProg;
                if (cond.songPositionInBeatsAsDouble >= bombGlowStart + bombGlowLength)
                {
                    bombGlowStart = double.MinValue;
                    bombGlowLength = 0f;
                }
            }
            UpdateJoeColour();

            if (canEmote && wantFace >= 0)
            {
                SetFaceExpressionForced(wantFace);
                if (wantFace == (int) KarateMan.KarateManFaces.Surprise) wantFace = -1;
            }

            if (cond.songPositionInBeatsAsDouble >= noNuriJabTime && cond.songPositionInBeatsAsDouble < noNuriJabTime + 1f)
            {
                anim.DoScaledAnimation("JabNoNuri", noNuriJabTime, 1f);
                bop.startBeat = noNuriJabTime + 1f;
            }
            else if (cond.songPositionInBeatsAsDouble >= noNuriJabTime + 1f && noNuriJabTime != double.MinValue)
            {
                bop.startBeat = noNuriJabTime + 1f;
                noNuriJabTime = double.MinValue;
            }

            if (unPrepareTime != double.MinValue && cond.songPositionInBeatsAsDouble >= unPrepareTime)
            {
                unPrepareTime = double.MinValue;
                anim.speed = 1f;
                anim.Play("Beat", -1, 0);
            }

            if (cond.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1, false) && cond.songPositionInBeatsAsDouble > bop.startBeat && cond.songPositionInBeatsAsDouble < bop.startBeat + bop.length && cond.songPositionInBeatsAsDouble >= unPrepareTime && !inCombo)
            {
                Bop();
            }

            if (inCombo && shouldComboId == -2)
            {
                float missProg = cond.GetPositionFromBeat(lastComboMissTime, 3f);
                if (missProg >= 0f && missProg < 1f)
                {
                    anim.DoScaledAnimation("LowKickMiss", lastComboMissTime, 3f);
                    bop.startBeat = lastComboMissTime + 3f;
                }
                else if (missProg >= 1f)
                {
                    anim.speed = 1f;
                    bop.startBeat = lastComboMissTime + 3f;
                    lastComboMissTime = double.MinValue;
                    inCombo = false;
                    inComboId = -1;
                    shouldComboId = -1;
                }
            }

            if (inKick)
            {
                float chargeProg = cond.GetPositionFromBeat(lastChargeTime, 2.75f);
                if (chargeProg >= 0f && chargeProg < 1f)
                {
                    anim.DoScaledAnimation("ManCharge", lastChargeTime, 2.75f);
                    bop.startBeat = lastChargeTime + 1.75f;
                }
                else if (chargeProg >= 1f)
                {
                    anim.speed = 1f;
                    bop.startBeat = lastChargeTime + 1.75f;
                    lastChargeTime = double.MinValue;
                    inKick = false;
                }
            }

            if (PlayerInput.Pressed(true) && !inSpecial)
            {
                if (!KarateMan.instance.IsExpectingInputNow(InputType.STANDARD_DOWN | InputType.DIRECTION_DOWN))
                {
                    Punch(1);
                    SoundByte.PlayOneShotGame("karateman/swingNoHit", forcePlay: true);
                }
            }
            
            if (PlayerInput.AltPressed() && KarateMan.IsComboEnable && !inSpecial)
            {
                if (!KarateMan.instance.IsExpectingInputNow(InputType.STANDARD_ALT_DOWN))
                {
                    //start a forced-fail combo sequence
                    ForceFailCombo(cond.songPositionInBeatsAsDouble);
                    KarateMan.instance.ScoreMiss(2);
                }
            }
            else if (PlayerInput.AltPressedUp())
            {
                if (!KarateMan.instance.IsExpectingInputNow(InputType.STANDARD_ALT_UP))
                {
                    if (inComboId != -1 && !lockedInCombo)
                    {
                        inComboId = -1;
                    }
                }
            }

            if ((!GameManager.instance.autoplay) && (PlayerInput.PressedUp(true) && !PlayerInput.Pressing(true)))
            {
                if (wantKick)
                {
                    //stopped holding, don't charge
                    wantKick = false;
                }
                else if (inKick && cond.GetPositionFromBeat(lastChargeTime, 2.75f) <= 0.5f && !KarateMan.instance.IsExpectingInputNow(InputType.STANDARD_UP | InputType.DIRECTION_UP))
                {
                    Kick(cond.songPositionInBeatsAsDouble);
                    SoundByte.PlayOneShotGame("karateman/swingKick", forcePlay: true);
                }
            }

        }

        public void Bop()
        {
            anim.speed = 1f;
            anim.Play("Beat", -1, 0);
            lastChargeTime = double.MinValue;
        }

        public bool Punch(int forceHand = 0)
        {
            if (GameManager.instance.currentGame != "karateman") return false;
            var cond = Conductor.instance;
            bool straight = false;

            anim.speed = 1f;
            unPrepareTime = double.MinValue;
            lastChargeTime = double.MinValue;
            inKick = false;

            switch (forceHand)
            {
                case 0:
                    if (cond.songPositionInBeatsAsDouble - lastPunchTime < 0.25f + (Minigame.JustLateTime() - 1f))
                    {
                        lastPunchTime = double.MinValue;
                        anim.DoScaledAnimationAsync("Straight", 0.5f);
                        straight = true;
                    }
                    else
                    {
                        lastPunchTime = cond.songPositionInBeatsAsDouble;
                        anim.DoScaledAnimationAsync("Jab", 0.5f);
                    }
                    break;
                case 1:
                    anim.DoScaledAnimationAsync("Jab", 0.5f);
                    break;
                case 2:
                    anim.DoScaledAnimationAsync("Straight", 0.5f);
                    straight = true;
                    break;
                case 3:
                    lastPunchTime = double.MinValue;
                    anim.DoNormalizedAnimation("JabNoNuri");
                    noNuriJabTime = cond.songPositionInBeatsAsDouble;
                    break;
            }
            bop.startBeat = cond.songPositionInBeatsAsDouble + 0.5f;
            return straight;    //returns what hand was used to punch the object
        }

        public void ComboSequence(int seq)
        {
            if (GameManager.instance.currentGame != "karateman") return;
            var cond = Conductor.instance;
            bop.startBeat = cond.songPositionInBeatsAsDouble + 1f;
            unPrepareTime = double.MinValue;
            switch (seq)
            {
                case 0:
                    anim.Play("LowJab", -1, 0);
                    break;
                case 1:
                    anim.Play("LowKick", -1, 0);
                    break;
                case 2:
                    anim.DoScaledAnimationAsync("BackHand", 0.5f);
                    comboWaiting = true;
                    break;
                case 3:
                    anim.DoScaledAnimationAsync("UpperCut", 0.5f);
                    lockedInCombo = false;
                    comboWaiting = false;
                    break;
                case 4:
                    anim.Play("ToReady", -1, 0);
                    bop.startBeat = cond.songPositionInBeatsAsDouble + 0.5f;
                    lockedInCombo = false;
                    comboWaiting = false;
                    break;
                default:
                    break;
            }
        }

        public void ComboMiss(double beat)
        {
            var cond = Conductor.instance;
            lastComboMissTime = beat;
            bop.startBeat = beat + 3f;
            unPrepareTime = double.MinValue;
            anim.DoNormalizedAnimation("LowKickMiss");
        }

        public void ForceFailCombo(double beat)
        {
            if (inCombo) return;
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { Punch(1); inCombo = true; inComboId = -1; shouldComboId = -1;}),
                new BeatAction.Action(beat + 0.25f, delegate { Punch(2); }),
                new BeatAction.Action(beat + 0.5f, delegate { ComboSequence(0); }),
                new BeatAction.Action(beat + 0.75f, delegate { shouldComboId = -2; ComboMiss(beat + 0.75f); }),
            });

            MultiSound.Play(new MultiSound.Sound[] 
            {
                new MultiSound.Sound("karateman/swingNoHit", beat), 
                new MultiSound.Sound("karateman/swingNoHit_Alt", beat + 0.25f), 
                new MultiSound.Sound("karateman/swingNoHit_Alt", beat + 0.5f), 
                new MultiSound.Sound("karateman/comboMiss", beat + 0.75f),  
            }, forcePlay: true);
        }

        public void StartKickCharge(double beat)
        {
            wantKick = true;
            unPrepareTime = double.MinValue;
            BeatAction.New(this, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat, delegate { 
                    if (wantKick)
                    {
                        wantKick = false;
                        inKick = true;
                        lastChargeTime = beat;
                        bop.startBeat = beat + 1.75f;
                    }
                })
            });
        }

        public void Kick(double beat)
        {
            if (!inKick) return;
            //play the kick animation and reset stance
            anim.speed = 1f;
            bop.startBeat = beat + 1f;
            unPrepareTime = double.MinValue;
            lastChargeTime = double.MinValue;
            inKick = false;

            anim.DoScaledAnimationAsync("ManKick", 0.5f);
        }

        public void MarkCanEmote()
        {
            canEmote = true;
        }

        public void MarkNoEmote()
        {
            canEmote = false;
        }

        public void UpdateJoeColour()
        {
            Color mainCol = KarateMan.instance.BodyColor;
            Color highlightCol = KarateMan.instance.HighlightColor;

            if (bombGlowIntensity > 0)
            {
                highlightCol = Color.LerpUnclamped(highlightCol, mainCol, bombGlowIntensity);
                mainCol = Color.LerpUnclamped(mainCol, BombGlowTint, bombGlowIntensity * bombGlowRatio);
            }

            KarateMan.instance.MappingMaterial.SetColor("_ColorAlpha", mainCol);
            KarateMan.instance.MappingMaterial.SetColor("_ColorBravo", new Color(1, 0, 0, 1));
            KarateMan.instance.MappingMaterial.SetColor("_ColorDelta", highlightCol);
        }

        public void Prepare(double beat, float length)
        {
            anim.speed = 0f;
            anim.Play("Beat", -1, 0);
            unPrepareTime = beat + length;
        }

        public void SetFaceExpressionForced(int face)
        {
            FaceAnim.DoScaledAnimationAsync("Face" + face.ToString("D2"));
        }

        public void SetFaceExpression(int face, bool ignoreCheck = false)
        {
            wantFace = face;
            if (canEmote || ignoreCheck)
                FaceAnim.DoScaledAnimationAsync("Face" + face.ToString("D2"));
        }

        public void ApplyBombGlow()
        {
            bombGlowStart = double.MaxValue;
            bombGlowLength = 0f;
            bombGlowIntensity = 1f;
        }

        public void RemoveBombGlow(double beat, float length = 0.5f)
        {
            if (double.IsNaN(bombGlowIntensity)) return;
            bombGlowStart = beat;
            bombGlowLength = length;
            bombGlowIntensity = 0f;
        }
    }
}