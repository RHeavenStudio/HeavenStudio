using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NaughtyBezierCurves;
using HeavenStudio.Util;
using UnityEngine.UIElements;

namespace HeavenStudio.Games.Scripts_SpaceSoccer
{
    public class Ball : SuperCurveObject
    {
        public enum State { None, Dispensing, Kicked, HighKicked, Toe };
        [Header("Components")]
        [HideInInspector] public Kicker kicker;
        [SerializeField] private GameObject holder;
        [SerializeField] private GameObject spriteHolder;
        [SerializeField] private GameObject kickFX;
        [Space(10)]
        //[SerializeField] private BezierCurve3D dispenseCurve;
        //[SerializeField] private BezierCurve3D kickCurve;
        //[SerializeField] private BezierCurve3D highKickCurve;
        //[SerializeField] private BezierCurve3D toeCurve;

        [Header("Properties")]
        public float startBeat;
        public State state;
        public float nextAnimBeat;
        public float highKickSwing = 0f;
        private float lastSpriteRot;
        public bool canKick;
        public bool waitKickRelease;
        private bool lastKickLeft;
        private SuperCurveObject.Path kickPath;
        private SuperCurveObject.Path dispensePath;
        private SuperCurveObject.Path highKickPath;
        private SuperCurveObject.Path toePath;
        //private float currentKickPathScale = 1;

        public void Init(Kicker kicker, float dispensedBeat)
        {
            this.kicker = kicker;
            kicker.ball = this;
            kicker.dispenserBeat = dispensedBeat;
            float currentBeat = Conductor.instance.songPositionInBeats;
            kickPath = SpaceSoccer.instance.GetPath("Kick");
            dispensePath = SpaceSoccer.instance.GetPath("Dispense");
            highKickPath = SpaceSoccer.instance.GetPath("HighKick");
            toePath = SpaceSoccer.instance.GetPath("Toe");
            //holder.transform.localPosition = kicker.transform.GetChild(0).position;

            if (currentBeat - dispensedBeat < 2f) //check if ball is currently being dispensed (should only be false if starting in the middle of the remix)
            {
                //Debug.Log("Dispensing");
                state = State.Dispensing;
                startBeat = dispensedBeat;
                nextAnimBeat = startBeat + GetAnimLength(State.Dispensing);
                kicker.kickTimes = 0;
                return;
            }

            var highKicks = GameManager.instance.Beatmap.entities.FindAll(c => c.datamodel == "spaceSoccer/high kick-toe!");
            int numHighKicks = 0;
            //determine what state the ball was in for the previous kick.
            for(int i = 0; i < highKicks.Count; i++)
            {
                if (highKicks[i].beat + highKicks[i].length <= currentBeat)
                {
                    numHighKicks++;
                    continue;
                }
                if (highKicks[i].beat > currentBeat)
                {
                    //Debug.Log("Setting state to kicked");
                    state = State.Kicked;
                    float relativeBeat = currentBeat - dispensedBeat;
                    startBeat = dispensedBeat + (int)(relativeBeat - 0.1); //this makes the startBeat be for the kick that is currently in progress, but it won't play the kicker's animation for that kick. the -0.1 makes it so that if playback is started right when the kicker kicks, it still plays the kicker's animation.
                    nextAnimBeat = startBeat + GetAnimLength(State.Kicked);
                    kicker.kickTimes = (int)(relativeBeat - 0.1) - numHighKicks - 1; //every high kick has 2 kicks in the same time a regular keep-up does 3 kicks.
                    break;
                }
                else
                {
                    highKickSwing = highKicks[i].swing;
                    if (highKickSwing == 0f)
                        highKickSwing = 0.5f;

                    if (highKicks[i].beat + GetAnimLength(State.HighKicked) > currentBeat)
                    {
                        //Debug.Log("Setting state to high kick");
                        state = State.HighKicked;
                        float relativeBeat = highKicks[i].beat - dispensedBeat;
                        startBeat = dispensedBeat + Mathf.Ceil(relativeBeat); //there is a chance this makes startBeat later than the current beat, but it shouldn't matter too much. It would only happen if the user places the high kicks incorrectly.
                        nextAnimBeat = startBeat + GetAnimLength(State.HighKicked);
                        kicker.kickTimes = Mathf.CeilToInt(relativeBeat) - numHighKicks - 1;
                        break;
                    }
                    else
                    {
                        //Debug.Log("Setting state to toe");
                        state = State.Toe;
                        float relativeBeat = Mathf.Ceil(highKicks[i].beat - dispensedBeat) + GetAnimLength(State.HighKicked); //there is a chance this makes startBeat later than the current beat, but it shouldn't matter too much. It would only happen if the user places the high kicks incorrectly.
                        startBeat = dispensedBeat + relativeBeat;
                        nextAnimBeat = startBeat + GetAnimLength(State.Toe);
                        kicker.kickTimes = (int)(relativeBeat - GetAnimLength(State.HighKicked)) - numHighKicks;
                        break;
                    }
                }
            }
            if(state == 0) //if the for loop didn't set the state, i.e. all the high kicks happen before the point we start at.
            {
                //Debug.Log("Defaulting to kicked state");
                state = State.Kicked;
                float relativeBeat = currentBeat - dispensedBeat;
                startBeat = dispensedBeat + (int)(relativeBeat - 0.1); //this makes the startBeat be for the kick that is currently in progress, but it won't play the kicker's animation for that kick. the -0.1 makes it so that if playback is started right when the kicker kicks, it still plays the kicker's animation.
                nextAnimBeat = startBeat + GetAnimLength(State.Kicked);
                kicker.kickTimes = (int)(relativeBeat - 0.1) - numHighKicks - 1;
            }
            Update(); //make sure the ball is in the right place
        }

        public void Kick(bool player)
        {
            if (player)
            Jukebox.PlayOneShotGame("spaceSoccer/ballHit", -1, Jukebox.GetPitchFromCents(UnityEngine.Random.Range(-38, 39), false));

            lastSpriteRot = spriteHolder.transform.eulerAngles.z;

            SetState(State.Kicked);

            lastKickLeft = kicker.kickLeft;

            /*if (kicker.kickLeft)
            {
                kickCurve.transform.localScale = new Vector3(-1, 1);
                currentKickPathScale = -1;
            }
            else
            {
                kickCurve.transform.localScale = new Vector3(1, 1);
                currentKickPathScale = 1;
            }*/
            //kickCurve.KeyPoints[0].transform.position = holder.transform.position;
            //kickPath.positions[0].pos = holder.transform.position;

            HitFX();
        }

        public void HighKick()
        {
            lastSpriteRot = spriteHolder.transform.eulerAngles.z;

            SetState(State.HighKicked);

            //highKickCurve.KeyPoints[0].transform.position = holder.transform.position;
            highKickPath.positions[0].pos = holder.transform.position;


            HitFX();
        }

        public void Toe()
        {

            lastSpriteRot = spriteHolder.transform.eulerAngles.z;

            SetState(State.Toe);

            //toeCurve.KeyPoints[0].transform.position = holder.transform.position;
            toePath.positions[0].pos = holder.transform.position;
            if (lastKickLeft)
            {
                //toeCurve.KeyPoints[1].transform.localPosition = new Vector3(5.39f, 0);
                toePath.positions[1].pos = new Vector3(5.39f, 0);
            }
            else
            {
                //toeCurve.KeyPoints[1].transform.localPosition = new Vector3(6.49f, 0);
                toePath.positions[1].pos = new Vector3(6.49f, 0);
            }


            HitFX();
        }

        private void Update()
        {
            float beat = Conductor.instance.songPositionInBeats;
            switch (state) //handle animations
            {
                case State.None: //the only time any ball should ever have this state is if it's the unused offscreen ball (which is the only reason this state exists)
                    {
                        gameObject.SetActive(false);
                        break;
                    }
                case State.Dispensing:
                    {
                        float normalizedBeatAnim = Conductor.instance.GetPositionFromBeat(startBeat, 2.35f);

                        //dispenseCurve.KeyPoints[0].transform.position = new Vector3(kicker.transform.GetChild(0).position.x - 6f, kicker.transform.GetChild(0).position.y - 6f);
                        //dispenseCurve.KeyPoints[1].transform.position = new Vector3(kicker.transform.GetChild(0).position.x - 1f, kicker.transform.GetChild(0).position.y - 6f);

                        dispensePath.positions[0].pos = new Vector3(kicker.transform.GetChild(0).position.x - 6f, kicker.transform.GetChild(0).position.y - 6f, kicker.transform.GetChild(0).position.z);
                        dispensePath.positions[1].pos = new Vector3(kicker.transform.GetChild(0).position.x - 1f, kicker.transform.GetChild(0).position.y - 6f, kicker.transform.GetChild(0).position.z);

                        //holder.transform.localPosition = dispenseCurve.GetPoint(normalizedBeatAnim);
                        holder.transform.localPosition = GetPathPositionFromBeat(dispensePath, Mathf.Max(beat, startBeat), out float height, startBeat);
                        spriteHolder.transform.eulerAngles = new Vector3(0, 0, Mathf.Lerp(0f, -1440f, normalizedBeatAnim));
                        break;
                    }
                case State.Kicked:
                    {
                        float normalizedBeatAnim = Conductor.instance.GetPositionFromBeat(startBeat, 1.5f);

                        if (!lastKickLeft)
                        {
                            //kickCurve.KeyPoints[1].transform.position = new Vector3(kicker.transform.GetChild(0).position.x + 0.5f, kicker.transform.GetChild(0).position.y - 6f);
                            kickPath.positions[1].pos = new Vector3(kicker.transform.GetChild(0).position.x + 0.5f, kicker.transform.GetChild(0).position.y - 6f, kicker.transform.GetChild(0).position.z);
                            kickPath.positions[0].pos = new Vector3(kicker.transform.GetChild(0).position.x - 2f, kicker.transform.GetChild(0).position.y - 0.5f, kicker.transform.GetChild(0).position.z);
                            spriteHolder.transform.eulerAngles = new Vector3(0, 0, Mathf.Lerp(lastSpriteRot, lastSpriteRot - 360f, normalizedBeatAnim));
                        }
                        else
                        {
                            //kickCurve.KeyPoints[1].transform.position = new Vector3(kicker.transform.GetChild(0).position.x - 2.5f, kicker.transform.GetChild(0).position.y - 6f);
                            kickPath.positions[1].pos = new Vector3(kicker.transform.GetChild(0).position.x - 2f, kicker.transform.GetChild(0).position.y - 6f, kicker.transform.GetChild(0).position.z);
                            kickPath.positions[0].pos = new Vector3(kicker.transform.GetChild(0).position.x + 0.5f, kicker.transform.GetChild(0).position.y - 0.5f, kicker.transform.GetChild(0).position.z);
                            spriteHolder.transform.eulerAngles = new Vector3(0, 0, Mathf.Lerp(lastSpriteRot, lastSpriteRot + 360f, normalizedBeatAnim));
                        }

                        //holder.transform.localPosition = kickCurve.GetPoint(normalizedBeatAnim);
                        holder.transform.localPosition = GetPathPositionFromBeat(kickPath, Mathf.Max(beat, startBeat), out float height, startBeat);
                        break;
                    }
                case State.HighKicked:
                    {
                        float normalizedBeatAnim = Conductor.instance.GetPositionFromBeat(startBeat, GetAnimLength(State.HighKicked) + 0.3f);
                        highKickPath.positions[0].duration = GetAnimLength(State.HighKicked) + 0.3f;

                        //highKickCurve.KeyPoints[1].transform.position = new Vector3(kicker.transform.GetChild(0).position.x - 3.5f, kicker.transform.GetChild(0).position.y - 6f);
                        highKickPath.positions[1].pos = new Vector3(kicker.transform.GetChild(0).position.x - 3.5f, kicker.transform.GetChild(0).position.y - 6f, kicker.transform.GetChild(0).position.z);

                        //holder.transform.localPosition = highKickCurve.GetPoint(normalizedBeatAnim);
                        holder.transform.localPosition = GetPathPositionFromBeat(highKickPath, Mathf.Max(beat, startBeat), out float height, startBeat);
                        spriteHolder.transform.eulerAngles = new Vector3(0, 0, Mathf.Lerp(lastSpriteRot, lastSpriteRot + 360f, normalizedBeatAnim));
                        break;
                    }
                case State.Toe:
                    {
                        float normalizedBeatAnim = Conductor.instance.GetPositionFromBeat(startBeat, GetAnimLength(State.Toe) + 0.35f);
                        toePath.positions[0].duration = GetAnimLength(State.Toe) + 0.35f;

                        if (!lastKickLeft)
                        {
                            //toeCurve.KeyPoints[1].transform.position = new Vector3(kicker.transform.GetChild(0).position.x + 0.5f, kicker.transform.GetChild(0).position.y - 6f);
                            toePath.positions[1].pos = new Vector3(kicker.transform.GetChild(0).position.x + 0.5f, kicker.transform.GetChild(0).position.y - 6f, kicker.transform.GetChild(0).position.z);
                        }
                        else
                        {
                            //toeCurve.KeyPoints[1].transform.position = new Vector3(kicker.transform.GetChild(0).position.x - 1.0f, kicker.transform.GetChild(0).position.y - 6f);
                            toePath.positions[1].pos = new Vector3(kicker.transform.GetChild(0).position.x - 1.0f, kicker.transform.GetChild(0).position.y - 6f, kicker.transform.GetChild(0).position.z);
                        }

                        //holder.transform.localPosition = toeCurve.GetPoint(normalizedBeatAnim);
                        holder.transform.localPosition = GetPathPositionFromBeat(toePath, Mathf.Max(beat, startBeat), out float height, startBeat);
                        spriteHolder.transform.eulerAngles = new Vector3(0, 0, Mathf.Lerp(lastSpriteRot, -860f, normalizedBeatAnim));
                        break;
                    }
            }
        }

        private void HitFX()
        {
            GameObject kickfx = Instantiate(kickFX.gameObject, SpaceSoccer.instance.transform);
            kickfx.SetActive(true);
            kickfx.transform.position = holder.transform.position;
        }

        private void SetState(State newState)
        {
            state = newState;
            startBeat = nextAnimBeat;
            nextAnimBeat += GetAnimLength(newState);
        }

        public float GetAnimLength(State anim)
        {
            switch(anim)
            {
                case State.Dispensing:
                    return 2f;
                case State.Kicked:
                    return 1f;
                case State.HighKicked:
                    return 2f - highKickSwing;
                case State.Toe:
                    return 2f - (1f - highKickSwing);
                default:
                    Debug.LogError("Ball has invalid state. State number: " + (int)anim);
                    return 0f;
            }
        }
    }
}