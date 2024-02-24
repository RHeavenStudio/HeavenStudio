using System;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_SpaceSoccer
{
    public class Ball : SuperCurveObject
    {
        public enum State { None, Dispensing, Kicked, HighKicked, Toe };
        [Header("Components")]
        [HideInInspector] public Kicker kicker;
        [SerializeField] private GameObject holder;
        [SerializeField] private GameObject spriteHolder;
        [Space(10)]
        //[SerializeField] private BezierCurve3D dispenseCurve;
        //[SerializeField] private BezierCurve3D kickCurve;
        //[SerializeField] private BezierCurve3D highKickCurve;
        //[SerializeField] private BezierCurve3D toeCurve;

        [Header("Properties")]
        public double startBeat;
        public State state;
        public double nextAnimBeat;
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

        protected override void UpdateLastRealPos()
        {
            lastRealPos = transform.localPosition;
        }
        public void Init(Kicker kicker, double dispensedBeat)
        {
            this.kicker = kicker;
            kicker.ball = this;
            kicker.dispenserBeat = dispensedBeat;
            double currentBeat = Conductor.instance.songPositionInBeatsAsDouble;
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

            var highKicks = GameManager.instance.Beatmap.Entities.FindAll(c => c.datamodel == "spaceSoccer/high kick-toe!");
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
                    double relativeBeat = currentBeat - dispensedBeat;
                    startBeat = dispensedBeat + (int)(relativeBeat - 0.1); //this makes the startBeat be for the kick that is currently in progress, but it won't play the kicker's animation for that kick. the -0.1 makes it so that if playback is started right when the kicker kicks, it still plays the kicker's animation.
                    nextAnimBeat = startBeat + GetAnimLength(State.Kicked);
                    kicker.kickTimes = (int)(relativeBeat - 0.1) - numHighKicks - 1; //every high kick has 2 kicks in the same time a regular keep-up does 3 kicks.
                    break;
                }
                else
                {
                    highKickSwing = 0.5f;

                    if (highKicks[i].beat + GetAnimLength(State.HighKicked) > currentBeat)
                    {
                        //Debug.Log("Setting state to high kick");
                        state = State.HighKicked;
                        double relativeBeat = highKicks[i].beat - dispensedBeat;
                        startBeat = dispensedBeat + Math.Ceiling(relativeBeat); //there is a chance this makes startBeat later than the current beat, but it shouldn't matter too much. It would only happen if the user places the high kicks incorrectly.
                        nextAnimBeat = startBeat + GetAnimLength(State.HighKicked);
                        kicker.kickTimes = (int)Math.Ceiling(relativeBeat) - numHighKicks - 1;
                        break;
                    }
                    else
                    {
                        //Debug.Log("Setting state to toe");
                        state = State.Toe;
                        double relativeBeat = Math.Ceiling(highKicks[i].beat - dispensedBeat) + GetAnimLength(State.HighKicked); //there is a chance this makes startBeat later than the current beat, but it shouldn't matter too much. It would only happen if the user places the high kicks incorrectly.
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
                double relativeBeat = currentBeat - dispensedBeat;
                startBeat = dispensedBeat + (int)(relativeBeat - 0.1); //this makes the startBeat be for the kick that is currently in progress, but it won't play the kicker's animation for that kick. the -0.1 makes it so that if playback is started right when the kicker kicks, it still plays the kicker's animation.
                nextAnimBeat = startBeat + GetAnimLength(State.Kicked);
                kicker.kickTimes = (int)(relativeBeat - 0.1) - numHighKicks - 1;
            }
            Update(); //make sure the ball is in the right place
        }

        public void Kick(bool player)
        {
            if (player)
            SoundByte.PlayOneShotGame("spaceSoccer/ballHit", -1, SoundByte.GetPitchFromCents(UnityEngine.Random.Range(-38, 39), false));

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
            UpdateLastRealPos();
        }

        public void HighKick()
        {
            lastSpriteRot = spriteHolder.transform.eulerAngles.z;

            SetState(State.HighKicked);

            //highKickCurve.KeyPoints[0].transform.position = holder.transform.position;
            //highKickPath.positions[0].pos = holder.transform.position;
            UpdateLastRealPos();
        }

        public void Toe()
        {

            lastSpriteRot = spriteHolder.transform.eulerAngles.z;

            SetState(State.Toe);

            //toeCurve.KeyPoints[0].transform.position = holder.transform.position;
            //toePath.positions[0].pos = holder.transform.position;
            UpdateLastRealPos();
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
        }

        private void Update()
        {
            double beat = Conductor.instance.songPositionInBeatsAsDouble;
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
                        //holder.transform.localPosition = dispenseCurve.GetPoint(normalizedBeatAnim);
                        holder.transform.localPosition = GetPathPositionFromBeat(dispensePath, Math.Max(beat, startBeat), out double height, startBeat);
                        spriteHolder.transform.eulerAngles = new Vector3(0, 0, Mathf.Lerp(0f, -1440f, normalizedBeatAnim));
                        break;
                    }
                case State.Kicked:
                    {
                        float normalizedBeatAnim = Conductor.instance.GetPositionFromBeat(startBeat, 1.5f);

                        if (!lastKickLeft)
                        {
                            //kickCurve.KeyPoints[1].transform.position = new Vector3(kicker.transform.GetChild(0).position.x + 0.5f, kicker.transform.GetChild(0).position.y - 6f);
                            kickPath.positions[1].pos = new Vector3(0, -6f);
                            spriteHolder.transform.eulerAngles = new Vector3(0, 0, Mathf.Lerp(lastSpriteRot, lastSpriteRot - 360f, normalizedBeatAnim));
                        }
                        else
                        {
                            //kickCurve.KeyPoints[1].transform.position = new Vector3(kicker.transform.GetChild(0).position.x - 2.5f, kicker.transform.GetChild(0).position.y - 6f);
                            kickPath.positions[1].pos = new Vector3(-2.5f, -6f);
                            spriteHolder.transform.eulerAngles = new Vector3(0, 0, Mathf.Lerp(lastSpriteRot, lastSpriteRot + 360f, normalizedBeatAnim));
                        }

                        //holder.transform.localPosition = kickCurve.GetPoint(normalizedBeatAnim);
                        holder.transform.localPosition = GetPathPositionFromBeat(kickPath, Math.Max(beat, startBeat), out double height, startBeat);
                        break;
                    }
                case State.HighKicked:
                    {
                        float normalizedBeatAnim = Conductor.instance.GetPositionFromBeat(startBeat, GetAnimLength(State.HighKicked) + 0.3f);
                        highKickPath.positions[0].duration = GetAnimLength(State.HighKicked) + 0.3f;

                        //highKickCurve.KeyPoints[1].transform.position = new Vector3(kicker.transform.GetChild(0).position.x - 3.5f, kicker.transform.GetChild(0).position.y - 6f);

                        //holder.transform.localPosition = highKickCurve.GetPoint(normalizedBeatAnim);
                        holder.transform.localPosition = GetPathPositionFromBeat(highKickPath, Math.Max(beat, startBeat), out double height, startBeat);
                        spriteHolder.transform.eulerAngles = new Vector3(0, 0, Mathf.Lerp(lastSpriteRot, lastSpriteRot + 360f, normalizedBeatAnim));
                        break;
                    }
                case State.Toe:
                    {
                        //float normalizedBeatAnim = Conductor.instance.GetPositionFromBeat(startBeat, GetAnimLength(State.Toe) + 0.35f);
                        toePath.positions[0].duration = GetAnimLength(State.Toe) + 0.35f;

                        if (!lastKickLeft)
                        {
                            //toeCurve.KeyPoints[1].transform.position = new Vector3(kicker.transform.GetChild(0).position.x + 0.5f, kicker.transform.GetChild(0).position.y - 6f);
                            toePath.positions[1].pos = new Vector3(-1.5f, -6f);
                        }
                        else
                        {
                            //toeCurve.KeyPoints[1].transform.position = new Vector3(kicker.transform.GetChild(0).position.x - 1.0f, kicker.transform.GetChild(0).position.y - 6f);
                            toePath.positions[1].pos = new Vector3(-0.5f, -6f);
                        }

                        //holder.transform.localPosition = toeCurve.GetPoint(normalizedBeatAnim);
                        holder.transform.localPosition = GetPathPositionFromBeat(toePath, Math.Max(beat, startBeat), out double height, startBeat);
                        break;
                    }
            }
            holder.transform.position = new Vector3(holder.transform.position.x, holder.transform.position.y, kicker.transform.GetChild(0).position.z);
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