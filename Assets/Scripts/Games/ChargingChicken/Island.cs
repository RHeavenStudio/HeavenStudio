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
        [SerializeField] public GameObject Helmet;
        [SerializeField] public GameObject StoneSplashEffect;
        [SerializeField] public ParticleSystem IslandCollapse;
        [SerializeField] public ParticleSystem IslandCollapseNg;
        [SerializeField] public ParticleSystem ChickenSplashEffect;
        [SerializeField] public ParticleSystem GrassL;
        [SerializeField] public ParticleSystem GrassR;

        [NonSerialized]public double journeySave = 0;
        [NonSerialized]public double journeyStart = 0;
        [NonSerialized]public double journeyEnd = 0;
        [NonSerialized]public double journeyBlastOffTime = 0;
        [NonSerialized]public double journeyLength = 0;
        [NonSerialized]public bool isMoving = false;

        [NonSerialized]public double respawnStart = 0;
        [NonSerialized]public double respawnEnd = 0;
        [NonSerialized]public bool isRespawning = false;

        [NonSerialized]public bool stonesExist = false;
        [NonSerialized]public float platformOffsetUnderChicken = -6f;
        [NonSerialized]public Vector3 particleOffset;
        [NonSerialized]public float stonePlatformFallOffset = 0;

        [NonSerialized]public float value1;
        [NonSerialized]public float speed1 = 0f;
        [NonSerialized]public float speed2 = 0f;

        [NonSerialized]public float grassState = 0;
        [NonSerialized]public bool grassFell = false;

        float previousPosition;

        [SerializeField] GameObject PlatformBase;

        StonePlatform[] stonePlatformJourney;

        private struct StonePlatform
        {
            public int stoneNumber;
            public GameObject thisPlatform;
            public bool hasFallen;
        }

        #endregion

        //global methods
        #region Global Methods

        private void Update()
        {
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

            if (grassState > 0.6 && IslandPos.localPosition.x < -1 && !grassFell)
            {
                GrassR.Play();
                SoundByte.PlayOneShotGame("chargingChicken/SE_CHIKEN_DOSHA", volume: 0.7f);
                grassFell = true;
            }
            if (grassState < -0.6 && IslandPos.localPosition.x < 2 && !grassFell)
            {
                GrassL.Play();
                SoundByte.PlayOneShotGame("chargingChicken/SE_CHIKEN_DOSHA", volume: 0.7f);
                grassFell = true;
            }
        }

        public void LateUpdate()
        {
            if (stonesExist)
            {
                StoneSplashCheck();
            }
        }

        public void Awake()
        {
            StartCoroutine(CalcVelocity());
            previousPosition = IslandPos.localPosition.x;
        }

        IEnumerator CalcVelocity()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (IslandPos.localPosition.x <= previousPosition) speed1 = -(IslandPos.localPosition.x - previousPosition) / Time.deltaTime;
                previousPosition = IslandPos.localPosition.x;
            }
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
            stonePlatformFallOffset = state;
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
                    grassFell = true;
                    IslandPos.localPosition = new Vector3(0, 0, 0);
                    CollapsedLandmass.localPosition = new Vector3(0, 0, 0);
                    if (stonePlatformJourney != null)
                    {
                        foreach (var a in stonePlatformJourney)
                        {
                            var stone = a.thisPlatform;

                            stone.transform.localPosition -= new Vector3(stonePlatformFallOffset, 0, 0);
                        }
                    }
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

        public void StoneSplashCheck(double offset = 0)
        {
            foreach (var a in stonePlatformJourney)
            {
                if (a.thisPlatform.transform.position.x < platformOffsetUnderChicken + offset && !a.hasFallen)
                {
                    var stone = a.thisPlatform;
                    var anim = stone.GetComponent<Animator>();
                    stonePlatformJourney[a.stoneNumber].hasFallen = true;
                    anim.Play("Fall", 0, 0);
                    anim.speed = (1f / Conductor.instance.pitchedSecPerBeat) * 0.3f;
                    SoundByte.PlayOneShotGame("chargingChicken/SE_CHIKEN_BLOCK_FALL_PITCH150", pitch: SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-150, 151), false), volume: 0.5f);
                    BeatAction.New(GameManager.instance, new List<BeatAction.Action>()
                    {
                        new BeatAction.Action(Conductor.instance.songPositionInBeatsAsDouble + 0.50, delegate { StoneSplash(stone); }),
                    });
                    break;
                }
            }
        }

        public void StoneSplash(GameObject a)
        {
            if (a.transform.position.x > (-7.5 + platformOffsetUnderChicken)) 
            {
                SoundByte.PlayOneShotGame("chargingChicken/SE_CHIKEN_BLOCK_FALL_WATER_PITCH400", pitch: SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-400, 401), false), volume: 0.5f);
            }
            GameObject splash = Instantiate(StoneSplashEffect, a.transform);
            splash.transform.localPosition -= new Vector3(particleOffset.x, particleOffset.y, particleOffset.z);
            splash.GetComponent<ParticleSystem>().Play();
        }

        public void ChickenFall()
        {
            var c = ChickenSplashEffect.transform.localPosition;
            ChickenSplashEffect.transform.localPosition = new Vector3(-IslandPos.localPosition.x + 2.5f, c.y, c.z);
            ChickenSplashEffect.Play();
        }

        //stone platform ported code

        public void SpawnStones(double beat, double length, bool tooLate)
        {
            stonePlatformJourney = new StonePlatform[(int)(length * 4)];

            for ( int i = 0; i < length * 4; i++ )
            {
                stonePlatformJourney[i].thisPlatform = Instantiate(PlatformBase, transform);
                stonePlatformJourney[i].stoneNumber = i;
                stonePlatformJourney[i].hasFallen = false;

                var a = stonePlatformJourney[i];

                var stone = a.thisPlatform;
                var anim = a.thisPlatform.GetComponent<Animator>();

                stone.SetActive(true);
                particleOffset = new Vector3(stone.transform.localPosition.x, stone.transform.localPosition.y, stone.transform.localPosition.z);
                stone.transform.localPosition = new Vector3((float)(((a.stoneNumber) * ChargingChicken.platformDistanceConstant) - (ChargingChicken.platformDistanceConstant / 2) + stonePlatformFallOffset) + stone.transform.localPosition.x, stone.transform.localPosition.y, 0);

                switch (i % 3)
                {
                    case 1: anim.DoScaledAnimation("Plat1", 0.5f, animLayer: 1); break;
                    case 2: anim.DoScaledAnimation("Plat2", 0.5f, animLayer: 1); break;
                }
                
                if (!tooLate)
                {
                    anim.DoScaledAnimation("Set", Conductor.instance.songPositionInBeatsAsDouble + ((double)a.stoneNumber / 64), 0.5f, animLayer: 0);
                    anim.speed = (1f / Conductor.instance.pitchedSecPerBeat) * 0.5f;
                }
            }

            BeatAction.New(GameManager.instance, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat - length - 1, delegate { stonesExist = true; }),
            });
        }

        #endregion
    }
}