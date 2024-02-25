using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_ChargingChicken
{
    public class Island : MonoBehaviour
    {
        //definitions
        #region Definitions

        [SerializeField] public Animator ChargerAnim;
        [SerializeField] public Animator FakeChickenAnim;
        [SerializeField] public Transform IslandPos;
        [SerializeField] public Transform CollapsedLandmass;
        [SerializeField] public GameObject BigLandmass;
        [SerializeField] public GameObject SmallLandmass;

        [NonSerialized]public double journeySave = 0;
        [NonSerialized]public double journeyStart = 0;
        [NonSerialized]public double journeyEnd = 0;
        [NonSerialized]public double journeyBlastOffTime = 0;
        [NonSerialized]public double journeyLength = 0;
        [NonSerialized]public bool isMoving = false;

        [NonSerialized]public double respawnStart = 0;
        [NonSerialized]public double respawnEnd = 0;
        [NonSerialized]public bool isRespawning = false;

        #endregion

        //global methods
        #region Global Methods

        private void Update()
        {
            if (isMoving)
            {
                float value1 = (Conductor.instance.GetPositionFromBeat(journeyBlastOffTime, journeyLength));
                float newX1 = Util.EasingFunction.EaseOutCubic((float)journeyStart, (float)journeyEnd, value1);
                IslandPos.localPosition = new Vector3(newX1, 0, 0);
            }
            if (respawnStart < Conductor.instance.songPositionInBeatsAsDouble && isRespawning)
            {
                float value2 = (Conductor.instance.GetPositionFromBeat(respawnStart, respawnEnd - respawnStart));
                float newX2 = Util.EasingFunction.Linear((float)journeyStart - (float)journeySave, (float)journeyEnd, 1 - value2);
                IslandPos.localPosition = new Vector3(newX2, 0, 0);
            }
        }

        #endregion

        //island methods
        #region Island Methods

        public void ChargerArmCountIn(double beat)
        {
            BeatAction.New(GameManager.instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 4, delegate { this.ChargerAnim.DoScaledAnimationAsync("Prep1", 0.5f); }),
                new BeatAction.Action(beat - 3, delegate { this.ChargerAnim.DoScaledAnimationAsync("Prep2", 0.5f); }),
                new BeatAction.Action(beat - 2, delegate { this.ChargerAnim.DoScaledAnimationAsync("Prep3", 0.5f); }),
                new BeatAction.Action(beat - 1, delegate { this.ChargerAnim.DoScaledAnimationAsync("Prep4", 0.5f); }),
            });
        }

        public void ChargingAnimation()
        {
            ChargerAnim.DoScaledAnimationAsync("Pump", 0.5f);
        }

        public void BlastoffAnimation()
        {
            ChargerAnim.DoScaledAnimationAsync("Idle", 0.5f);
        }

        public void PositionIsland(float state)
        {
            CollapsedLandmass.localPosition = new Vector3(state, 0, 0);
        }

        public void CollapseUnderPlayer()
        {
            SoundByte.PlayOneShotGame("chargingChicken/complete");
            SmallLandmass.SetActive(false);
        }

        #endregion
    }
}