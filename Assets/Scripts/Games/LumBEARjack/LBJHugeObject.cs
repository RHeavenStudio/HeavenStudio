using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using static HeavenStudio.Games.Scripts_AirRally.Cloud;

namespace HeavenStudio.Games.Scripts_LumBEARjack
{
    public class LBJHugeObject : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private SpriteRenderer _logSR;
        [SerializeField] private Sprite[] _logCutSprites = new Sprite[3];
        [SerializeField] private SpriteRenderer _freezerSR;
        [SerializeField] private Sprite[] _freezerCutSprites = new Sprite[3];
        [SerializeField] private SpriteRenderer _peachSR;
        [SerializeField] private Sprite[] _peachCutSprites = new Sprite[3];

        private LBJBear _bear;
        private LBJObjectRotate _rotateObject;
        private LumBEARjack.HugeType _type;
        private bool _right = true;
        private bool _zoom = true;
        private bool _baby = true;

        private PlayerActionEvent _initialAction;
        private PlayerActionEvent[] _soundsToDeleteIfMiss = new PlayerActionEvent[3];

        private double _rotationBeat;
        private double _rotationLength;

        private void Awake()
        {
            _logSR.gameObject.SetActive(false);
            _freezerSR.gameObject.SetActive(false);
            _peachSR.gameObject.SetActive(false);

            _rotateObject = GetComponent<LBJObjectRotate>();
        }

        public void Init(LBJBear bear, double beat, double length, LumBEARjack.HugeType type, bool right, bool zoom, bool baby, double startUpBeat = -1)
        {
            _bear = bear;
            _type = type;
            _right = right;
            _zoom = zoom;
            _baby = baby;

            switch (type)
            {
                case LumBEARjack.HugeType.log:
                    _logSR.gameObject.SetActive(true);
                    break;
                case LumBEARjack.HugeType.freezer:
                    _freezerSR.gameObject.SetActive(true);
                    break;
                case LumBEARjack.HugeType.peach:
                    _peachSR.gameObject.SetActive(true);
                    break;
                default:
                    break;
            }

            _rotationBeat = beat + (length / 6 * 2);
            _rotationLength = length / 6;

            if (startUpBeat <= beat + (length / 6 * 2)) _initialAction = LumBEARjack.instance.ScheduleInput(beat, length / 6 * 2, Minigame.InputAction_BasicPress, JustHit1, Miss, Blank);
            else
            {
                _rotationBeat = beat + (length / 6 * 3);
                SetObjectCutSprite(1);
            }

            if (startUpBeat <= beat + (length / 6 * 3)) _soundsToDeleteIfMiss[0] = LumBEARjack.instance.ScheduleInput(beat, length / 6 * 3, Minigame.InputAction_BasicPress, JustHit2, Miss, Blank);
            else
            {
                _rotationBeat = beat + (length / 6 * 4);
                SetObjectCutSprite(2);
            }

            if (startUpBeat <= beat + (length / 6 * 4)) _soundsToDeleteIfMiss[1] = LumBEARjack.instance.ScheduleInput(beat, length / 6 * 4, Minigame.InputAction_BasicPress, JustHit3, Miss, Blank);
            else
            {
                _rotationBeat = beat + (length / 6 * 5);
                SetObjectCutSprite(3);
            }

            _soundsToDeleteIfMiss[2] = LumBEARjack.instance.ScheduleInput(beat, length / 6 * 5, Minigame.InputAction_BasicPress, JustCut, Miss, Blank);
            Update();
        }

        private void Update()
        {
            if (PlayerInput.GetIsAction(Minigame.InputAction_BasicPress) && !LumBEARjack.instance.IsExpectingInputNow(Minigame.InputAction_BasicPress))
            {
                LumBEARjack.instance.ScoreMiss();
                Miss(null);
                if (_initialAction != null)
                {
                    _initialAction.Disable();
                    _initialAction.QueueDeletion();
                }
                return;
            }
            _rotateObject.Move(_rotationBeat, _rotationLength, _right);
        }

        private void SetObjectCutSprite(int step)
        {
            switch (_type)
            {
                case LumBEARjack.HugeType.log:
                    _logSR.sprite = _logCutSprites[step - 1];
                    break;
                case LumBEARjack.HugeType.freezer:
                    _freezerSR.sprite = _freezerCutSprites[step - 1];
                    break;
                case LumBEARjack.HugeType.peach:
                    _peachSR.sprite = _peachCutSprites[step - 1];
                    break;
                default:
                    break;
            }
        }

        private void JustHit1(PlayerActionEvent caller, float state)
        {
            JustHit(caller, state, 1);
        }

        private void JustHit2(PlayerActionEvent caller, float state)
        {
            JustHit(caller, state, 2);
        }

        private void JustHit3(PlayerActionEvent caller, float state)
        {
            JustHit(caller, state, 3);
        }

        private void JustHit(PlayerActionEvent caller, float state, int step)
        {
            if (state >= 1f || state <= -1f)
            {
                _bear.SwingWhiff(false);
                Miss(caller);
                return;
            }

            SoundByte.PlayOneShotGame("lumbearjack/" + (_type == LumBEARjack.HugeType.peach ? "peach" : "hit") + "Voice" + (Random.Range(1, 3) == 1 ? "A" : "B"));
            SoundByte.PlayOneShotGame("lumbearjack/baseHit");

            LumBEARjack.instance.DoHugeObjectEffect(_type, true);

            string hitSound = _type switch
            {
                LumBEARjack.HugeType.log => $"hugeLogHit{step}",
                LumBEARjack.HugeType.freezer => $"freezerHit{step}",
                LumBEARjack.HugeType.peach => $"peachHit{step}",
                _ => throw new System.NotImplementedException(),
            };
            SoundByte.PlayOneShotGame("lumbearjack/" + hitSound);
            _bear.CutMid(_type == LumBEARjack.HugeType.freezer);
            _rotationBeat = _soundsToDeleteIfMiss[step - 1].startBeat + _soundsToDeleteIfMiss[step - 1].timer;

            SetObjectCutSprite(step);
        }

        private void JustCut(PlayerActionEvent caller, float state)
        {
            if (state >= 1f || state <= -1f)
            {
                _bear.SwingWhiff(false);
                Miss(caller);
                return;
            }

            SoundByte.PlayOneShotGame("lumbearjack/" + (_type == LumBEARjack.HugeType.peach ? "peachCutVoice" : "hugeLogCutVoice"));

            LumBEARjack.instance.DoHugeObjectEffect(_type, false);
            if (_type == LumBEARjack.HugeType.peach && _baby) LumBEARjack.instance.ActivateBaby(caller.startBeat + caller.timer, (float)_rotationLength); 

            string cutSound = _type switch
            {
                LumBEARjack.HugeType.log => "hugeLogCut",
                LumBEARjack.HugeType.freezer => "freezerCut",
                LumBEARjack.HugeType.peach => "peachCut",
                _ => throw new System.NotImplementedException(),
            };
            SoundByte.PlayOneShotGame("lumbearjack/" + cutSound);
            _bear.Cut(caller.startBeat + caller.timer, false, false, _zoom);
            Destroy(gameObject);
        }

        private void Miss(PlayerActionEvent caller)
        {
            SoundByte.PlayOneShot("miss");
            foreach (var s in _soundsToDeleteIfMiss)
            {
                if (s == null) continue;
                s.Disable();
                s.QueueDeletion();
            }
            SpriteRenderer sr = _type switch
            {
                LumBEARjack.HugeType.log => _logSR,
                LumBEARjack.HugeType.freezer => _freezerSR,
                LumBEARjack.HugeType.peach => _peachSR,
                _ => throw new System.NotImplementedException(),
            };
            LumBEARjack.instance.ActivateMissEffect(sr.transform, sr);
            Destroy(gameObject);
        }

        private void Blank(PlayerActionEvent caller) { }
    }
}

