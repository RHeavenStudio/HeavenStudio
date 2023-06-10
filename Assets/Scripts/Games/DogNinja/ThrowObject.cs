using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using NaughtyBezierCurves;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_DogNinja
{
    public class ThrowObject : MonoBehaviour
    {
        public double startBeat;
        public int type;
        public bool fromLeft;
        public bool shouldSfx = true;
        public int direction;
        public string sfxNum;
        
        private Vector3 objPos;
        private bool isActive = true;
        private double barelyTime;
        
        [Header("Animators")]
        Animator DogAnim;

        [Header("References")]
        public BezierCurve3D curve;
        [SerializeField] BezierCurve3D barelyCurve;
        [SerializeField] BezierCurve3D BarelyLeftCurve;
        [SerializeField] BezierCurve3D BarelyRightCurve;
        [SerializeField] GameObject HalvesLeftBase;
        [SerializeField] GameObject HalvesRightBase;
        [SerializeField] Transform ObjectParent;
        public Sprite[] objectLeftHalves;
        public Sprite[] objectRightHalves;

        private DogNinja game;
        
        private void Awake()
        {
            game = DogNinja.instance;
            DogAnim = game.DogAnim;
        }

        private void Start()
        {
            barelyCurve = fromLeft ? BarelyRightCurve : BarelyLeftCurve;
            
            game.ScheduleInput(startBeat, 1f, InputType.STANDARD_DOWN | InputType.DIRECTION_DOWN, Hit, Miss, Out);
        }

        private void Update()
        {
            float flyPos = Conductor.instance.GetPositionFromBeat(startBeat, 1f)+1.1f;
            float flyPosBarely = Conductor.instance.GetPositionFromBeat(barelyTime, 1f)+1f;
            if (isActive) {
                flyPos *= 0.31f;
                transform.position = curve.GetPoint(flyPos);
                objPos = curve.GetPoint(flyPos);
                // destroy object when it's off-screen
                if (flyPos > 1f) {
                    GameObject.Destroy(gameObject);
                }
            } else {
                flyPosBarely *= 0.3f;
                transform.position = barelyCurve.GetPoint(flyPosBarely) + objPos;
                float rot = fromLeft ? 200f : -200f;
                transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (rot * Time.deltaTime));
                if (flyPosBarely > 1f) {
                    GameObject.Destroy(gameObject);
                }
            }

            if ((!Conductor.instance.isPlaying && !Conductor.instance.isPaused) 
                || GameManager.instance.currentGame != "dogNinja") {
                GameObject.Destroy(gameObject);
            }
        }

        private void SuccessSlice() 
        {
            string slice = "Slice" + direction switch
            {
                0 => "Left",
                1 => "Right",
                2 => "Both",
            };

            DogAnim.DoScaledAnimationAsync(slice, 0.5f);
            if (shouldSfx) SoundByte.PlayOneShotGame(sfxNum+"2");

            game.WhichLeftHalf.sprite = objectLeftHalves[type-1];
            game.WhichRightHalf.sprite = objectRightHalves[type-1];

            SpawnHalves LeftHalf = Instantiate(HalvesLeftBase, game.gameObject.transform).GetComponent<SpawnHalves>();
            LeftHalf.startBeat = startBeat;
            LeftHalf.lefty = fromLeft;
            LeftHalf.objPos = objPos;

            SpawnHalves RightHalf = Instantiate(HalvesRightBase, game.gameObject.transform).GetComponent<SpawnHalves>();
            RightHalf.startBeat = startBeat;
            RightHalf.lefty = fromLeft;
            RightHalf.objPos = objPos;

            GameObject.Destroy(gameObject);
        }

        private void JustSlice()
        {
            isActive = false;
            barelyTime = Conductor.instance.songPositionInBeatsAsDouble;

            string barely = "Barely" + direction switch
            {
                0 => "Left",
                1 => "Right",
                2 => "Both",
                _ => "Both",
            };

            DogAnim.DoScaledAnimationAsync(barely, 0.5f);
            SoundByte.PlayOneShotGame("dogNinja/barely");
        }

        private void Hit(PlayerActionEvent caller, float state)
        {
            game.DogAnim.SetBool("needPrepare", false);
            if (state >= 1f || state <= -1f) JustSlice();
                else SuccessSlice();
        }

        private void Miss(PlayerActionEvent caller)
        {
            if (!DogAnim.GetBool("needPrepare")) return;
            DogAnim.DoScaledAnimationAsync("UnPrepare", 0.5f);
            DogAnim.SetBool("needPrepare", false);
        }

        private void Out(PlayerActionEvent caller) 
        {
            DogAnim.SetBool("needPrepare", false);
        }
    }
}
