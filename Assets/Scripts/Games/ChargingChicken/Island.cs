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
        [SerializeField] public Animator PlatformAnim;
        [SerializeField] public Transform IslandPos;
        [SerializeField] public Transform CollapsedLandmass;
        [SerializeField] public GameObject BigLandmass;
        [SerializeField] public GameObject SmallLandmass;
        [SerializeField] public GameObject FullLandmass;
        [SerializeField] public GameObject StonePlatform;
        [SerializeField] public GameObject Platform1;
        [SerializeField] public GameObject Platform2;
        [SerializeField] public GameObject Platform3;
        [SerializeField] public GameObject Helmet;
        [SerializeField] public ParticleSystem IslandCollapse;
        [SerializeField] public ParticleSystem IslandCollapseNg;
        [SerializeField] public ParticleSystem StoneSplashEffect;
        [SerializeField] public ParticleSystem ChickenSplashEffect;

        [NonSerialized]public double journeySave = 0;
        [NonSerialized]public double journeyStart = 0;
        [NonSerialized]public double journeyEnd = 0;
        [NonSerialized]public double journeyBlastOffTime = 0;
        [NonSerialized]public double journeyLength = 0;
        [NonSerialized]public bool isMoving = false;

        [NonSerialized]public double respawnStart = 0;
        [NonSerialized]public double respawnEnd = 0;
        [NonSerialized]public bool isRespawning = false;

        [NonSerialized]public bool isStonePlatform = false;
        [NonSerialized]public bool canFall = false;
        [NonSerialized]public bool isFalling = false;

        [NonSerialized]public bool isBeingSet = false;

        [NonSerialized]public float value1 = 0f;
        [NonSerialized]public float speed1 = 0f;
        [NonSerialized]public float speed2 = 0f;

        #endregion

        //global methods
        #region Global Methods

        private void Update()
        {
            float previousPosition = IslandPos.localPosition.x;

            if (isMoving)
            {
                value1 = (Conductor.instance.GetPositionFromBeat(journeyBlastOffTime, journeyLength));
                float newX1 = Util.EasingFunction.EaseOutCubic((float)journeyStart, (float)journeyEnd, value1);
                IslandPos.localPosition = new Vector3(newX1, 0, 0);
            }
            if (value1 >= 1)
            {
                isMoving = false;
            }
            if (respawnStart < Conductor.instance.songPositionInBeatsAsDouble && isRespawning)
            {
                float value2 = (Conductor.instance.GetPositionFromBeat(respawnStart, respawnEnd - respawnStart));
                float newX2 = Util.EasingFunction.Linear((float)journeyStart - (float)journeySave, (float)journeyEnd, 1 - value2);
                IslandPos.localPosition = new Vector3(newX2, 0, 0);
            }
            if (canFall && IslandPos.localPosition.x < -0.5)
            {
                PlatformAnim.Play("Fall", -1, 0);
                PlatformAnim.speed = (1f / Conductor.instance.pitchedSecPerBeat) * 0.3f;
                SoundByte.PlayOneShotGame("chargingChicken/SE_CHIKEN_BLOCK_FALL_PITCH150", pitch: SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-150, 151), false), volume: 0.5f);
                BeatAction.New(GameManager.instance, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(Conductor.instance.songPositionInBeatsAsDouble + 0.50, delegate { StoneSplash(); }),
                    new BeatAction.Action(Conductor.instance.songPositionInBeatsAsDouble + 3.00, delegate { Destroy(gameObject); }),
                });
                canFall = false;
            }

            float currentPosition = IslandPos.localPosition.x;
            speed1 = (previousPosition - currentPosition) / Time.deltaTime;
        }

        #endregion

        //island methods
        #region Island Methods

        public void ChargerArmCountIn(double beat, double lateness)
        {
            BeatAction.New(GameManager.instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - 4, delegate { if (lateness > 3) ChargerAnim.DoScaledAnimationAsync("Prep1", 0.5f); }),
                new BeatAction.Action(beat - 3, delegate { if (lateness > 2) ChargerAnim.DoScaledAnimationAsync("Prep2", 0.5f); }),
                new BeatAction.Action(beat - 2, delegate { if (lateness > 1) ChargerAnim.DoScaledAnimationAsync("Prep3", 0.5f); }),
                new BeatAction.Action(beat - 1, delegate { if (lateness > 0) ChargerAnim.DoScaledAnimationAsync("Prep4", 0.5f); }),
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

        public void SetUpCollapse(double collapseTime)
        {
            //collapse island (successful)
            BeatAction.New(GameManager.instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(collapseTime, delegate { 
                    SoundByte.PlayOneShotGame("chargingChicken/SE_CHIKEN_LAND_RESET", volume: 0.7f);
                    BigLandmass.SetActive(false);
                    SmallLandmass.SetActive(true);
                    IslandCollapse.Play();
                }),
            });
        }

        public void CollapseUnderPlayer()
        {
            SoundByte.PlayOneShotGame("chargingChicken/SE_CHIKEN_LAND_RESET", volume: 0.7f);
            SmallLandmass.SetActive(false);
            IslandCollapseNg.Play();
        }

        #endregion

        //stone platform methods
        #region Stone Platform Methods

        public void BecomeStonePlatform(int offset)
        {
            isStonePlatform = true;
            canFall = true;

            BigLandmass.SetActive(false);
            FullLandmass.SetActive(false);
            StonePlatform.SetActive(true);

            switch (offset % 3) {
                case 0: Platform1.SetActive(true); break;
                case 1: Platform2.SetActive(true); break;
                case 2: Platform3.SetActive(true); break;
            }

        }

        public void StoneFall(int offset, bool tooLate)
        {
            if (tooLate) return;
            PlatformAnim.DoScaledAnimation("Set", Conductor.instance.songPositionInBeatsAsDouble/*  + ((double)offset / 64) */, 0.5f);
            PlatformAnim.speed = (1f / Conductor.instance.pitchedSecPerBeat) * 0.5f;
        }

        public void StoneSplash()
        {
            if (IslandPos.localPosition.x > -8) 
            {
                SoundByte.PlayOneShotGame("chargingChicken/SE_CHIKEN_BLOCK_FALL_WATER_PITCH400", pitch: SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-400, 401), false), volume: 0.5f);
                StoneSplashEffect.Play();
            }
        }

        public void ThisIsNotMoving()
        {
            isMoving = false;
        }

        public void ChickenFall()
        {
            var c = ChickenSplashEffect.transform.localPosition;
            ChickenSplashEffect.transform.localPosition = new Vector3(-IslandPos.localPosition.x + 1.5f, c.y, c.z);
            ChickenSplashEffect.Play();
        }

        #endregion
    }
}