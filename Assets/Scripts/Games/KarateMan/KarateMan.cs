using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Util;

namespace HeavenStudio.Games
{
    using Scripts_KarateMan;

    public class KarateMan : Minigame
    {

        public enum HitType
        {
            Pot = 0,
            Rock = 2,
            Ball = 3,
            CookingPot = 6,
            Alien = 7,

            TacoBell = 999
        }

        public enum HitThree
        {
            HitTwo,
            HitThree,
            HitThreeAlt,
            HitFour
        }

        public enum LightBulbType
        {
            Normal,
            Blue,
            Yellow,
            Custom 
        }

        public enum BackgroundType
        {
            Yellow,
            Fuchsia,
            Blue,
            Red,
            Orange,
            Pink,
            Custom
        }

        public enum BackgroundFXType
        {
            None,
            Sunburst,
            Rings
        }

        public enum ShadowType
        {
            Tinted,
            Custom
        }

        public Color[] LightBulbColors;
        public Color[] BackgroundColors;
        public Color[] ShadowColors;
        public static Color ShadowBlendColor = new Color(195f / 255f, 48f / 255f, 2f / 255f);

        const float hitVoiceOffset = 0.042f;

        public GameObject Pot, Bomb;
        public KarateJoe KarateJoe;

        public List<Minigame.Eligible> EligibleCombos = new List<Minigame.Eligible>();

        public static KarateMan instance { get; set; }

        public Sprite[] ObjectSprites;
        public Sprite[] BarrelSprites;
        public Sprite[] CookingPotSprites;
        public Sprite[] OtherSprites;

        public List<BGSpriteC> BGSprites;
        public SpriteRenderer BGSprite;
        public SpriteRenderer BGFXSprite;

        public BackgroundType BGType = BackgroundType.Yellow;
        public BackgroundFXType BGFXType = BackgroundFXType.None;
        public Color BGColor;

        public ShadowType Shadow = ShadowType.Tinted;
        public Color ShadowColor = Color.black;

        private float newBeat;

        public GameEvent bop = new GameEvent();
        public GameEvent prepare = new GameEvent();

        private float bgBeat;

        public ParticleSystem potHitEffect;

        public GameObject comboRef;

        public GameObject HIT3Ref;

        public Sprite[] Numbers;


        [System.Serializable]
        public class BGSpriteC
        {
            public List<Sprite> Sprites;
        }

        private void Awake()
        {
            instance = this;
            BGType = 0;
            BGColor = BackgroundColors[0];
            Shadow = 0;
        }

        public override void OnGameSwitch(float beat)
        {
            base.OnGameSwitch(beat);
            SetBackgroundColor((int)BGType, (int)Shadow, BGColor, ShadowColor);
        }

        public void Combo(float beat)
        {
            comboRef.GetComponent<Animator>().enabled = true;
            comboRef.GetComponent<Animator>().Play("comboRef");
            Jukebox.PlayOneShotGame("karateman/barrelOutCombos");

            Shoot(beat, 0, true, "PotCombo1", 0, new Vector2(-0.94f, -2.904f));
            Shoot(beat + 0.25f, 0, true, "PotCombo2", 1, new Vector2(-0.94f, -2.904f));
            Shoot(beat + 0.5f, 0, true, "PotCombo3", 2, new Vector2(-0.776f, -3.162f));
            Shoot(beat + 0.75f, 0, true, "PotCombo4", 3, new Vector2(1.453f, -3.162f));
            Shoot(beat + 1f, 0, true, "PotCombo5", 4, new Vector2(0.124f, -3.123f));
            Shoot(beat + 1.5f, 4, true, "PotCombo6", 5, new Vector2(-1.333f, -2.995f));

            MultiSound.Play(new MultiSound.Sound[] 
            {
                new MultiSound.Sound("karateman/punchy1", beat + 1f), 
                new MultiSound.Sound("karateman/punchy2", beat + 1.25f), 
                new MultiSound.Sound("karateman/punchy3", beat + 1.5f), 
                new MultiSound.Sound("karateman/punchy4", beat + 1.75f), 
                new MultiSound.Sound("karateman/ko", beat + 2f), 
                new MultiSound.Sound("karateman/pow", beat + 2.5f) 
            });
        }

        public void Shoot(float beat, int type, bool combo = false, string throwAnim = "", int comboIndex = 0, Vector2 endShadowPos = new Vector2(), UnityEngine.Color tint = default)
        {
            GameObject pot = Instantiate(Pot);
            pot.transform.parent = Pot.transform.parent;

            if (KarateJoe.instance.anim.IsAnimationNotPlaying())
                KarateJoe.instance.SetHead(0);

            Pot p = pot.GetComponent<Pot>();

            pot.SetActive(true);
            p.startBeat = beat;
            p.createBeat = beat;
            p.isThrown = true;
            p.type = type;

            if(type <= ObjectSprites.Length)
                p.Sprite.GetComponent<SpriteRenderer>().sprite = ObjectSprites[type];

            if (combo)
            {
                p.comboIndex = comboIndex;
                p.throwAnim = throwAnim;
                p.combo = true;
                KarateJoe.currentComboPots.Add(p);
                p.endShadowThrowPos = endShadowPos;
            }
            else
            {
                p.throwAnim = "PotThrow";

                string outSnd = "";
                switch (type)
                {
                    case 0:
                        if (Starpelly.Mathp.GetDecimalFromFloat(beat) == 0f)
                            outSnd = "karateman/objectOut";
                        else
                            outSnd = "karateman/offbeatObjectOut";
                        p.hitSnd = "karateman/potHit";
                        break;
                    case 1:
                        outSnd = "karateman/lightbulbOut";
                        p.hitSnd = "karateman/lightbulbHit";
                        if (tint != default && tint != Color.black) {
                            p.BulbLightSprite.SetActive(true);
                            p.BulbLightSprite.GetComponent<SpriteRenderer>().color = tint;
                        }
                        break;
                    case 2:
                        if (Starpelly.Mathp.GetDecimalFromFloat(beat) == 0f)
                            outSnd = "karateman/objectOut";
                        else
                            outSnd = "karateman/offbeatObjectOut";
                        p.hitSnd = "karateman/rockHit";
                        break;
                    case 3:
                        if (Starpelly.Mathp.GetDecimalFromFloat(beat) == 0f)
                            outSnd = "karateman/objectOut";
                        else
                            outSnd = "karateman/offbeatObjectOut";
                        p.hitSnd = "karateman/soccerHit";
                        break;
                    case 4:
                        p.kick = true;
                        outSnd = "karateman/barrelOutKicks";
                        p.hitSnd = "karateman/barrelBreak";

                        MultiSound.Play(new MultiSound.Sound[]
                        {
                            new MultiSound.Sound("karateman/punchKick1", beat + 1f),
                            new MultiSound.Sound("karateman/punchKick2", beat + 1.5f),
                            new MultiSound.Sound("karateman/punchKick3", beat + 1.75f),
                            new MultiSound.Sound("karateman/punchKick4", beat + 2.25f)
                        });
                        break;
                    case 6:
                        if (Starpelly.Mathp.GetDecimalFromFloat(beat) == 0f)
                            outSnd = "karateman/objectOut";
                        else
                            outSnd = "karateman/offbeatObjectOut";
                        p.hitSnd = "karateman/cookingPot";
                        break;
                    case 7:
                        if (Starpelly.Mathp.GetDecimalFromFloat(beat) == 0f)
                            outSnd = "karateman/objectOut";
                        else
                            outSnd = "karateman/offbeatObjectOut";
                        p.hitSnd = "karateman/alienHit";
                        break;
                    case 999:
                        p.Sprite.GetComponent<SpriteRenderer>().sprite = OtherSprites[0];
                        if (Starpelly.Mathp.GetDecimalFromFloat(beat) == 0f)
                            outSnd = "karateman/objectOut";
                        else
                            outSnd = "karateman/offbeatObjectOut";
                        p.hitSnd = "karateman/tacobell";
                        break;
                }

                p.endShadowThrowPos = new Vector2(-1.036f, -2.822f);

                Jukebox.PlayOneShotGame(outSnd);
            }
        }

        List<Beatmap.Entity> cuedVoices = new List<Beatmap.Entity>(); // "Hit" voices cued in advance are stored here so they aren't called multiple times in Update().
        private void Update()
        {
            if (Conductor.instance.ReportBeat(ref newBeat))
            {
                if (BGFXSprite.enabled)
                {
                    var type = (int)BGFXType - 1;
                    if (bgBeat % 2 == 0)
                    {
                        BGFXSprite.sprite = BGSprites[type].Sprites[0];
                    }
                    else
                    {
                        BGFXSprite.sprite = BGSprites[type].Sprites[1];
                    }
                    bgBeat++;
                }
            }

            if (Conductor.instance.ReportBeat(ref bop.lastReportedBeat, bop.startBeat % 1))
            {
                if (Conductor.instance.songPositionInBeats >= bop.startBeat && Conductor.instance.songPositionInBeats < bop.startBeat + bop.length)
                {
                    if (KarateJoe.anim.IsAnimationNotPlaying())
                        KarateJoe.anim.Play("Bop", 0, 0);
                }
            }

            if (prepare.length > 0)
            {
                if (Conductor.instance.songPositionInBeats >= prepare.startBeat && Conductor.instance.songPositionInBeats < prepare.startBeat + prepare.length)
                {
                    if (KarateJoe.anim.IsAnimationNotPlaying())
                    KarateJoe.AnimPlay("Prepare");
                }
                else
                {
                    KarateJoe.AnimPlay("Idle");
                    prepare.length = 0;
                }
            }

            if (!Conductor.instance.isPlaying)
                return;

            // Call "hit" voice slightly early to account for sound offset.
            var hitVoiceEvents = GameManager.instance.Beatmap.entities.FindAll(c => c.datamodel == "karateman/hit3" || c.datamodel == "karateman/hit4");
            for (int i = 0; i < hitVoiceEvents.Count; i++)
            {
                var hitEvent = hitVoiceEvents[i];
                var timeToEvent = hitEvent.beat - Conductor.instance.songPositionInBeats;
                if (timeToEvent <= 1f && timeToEvent > 0f && !cuedVoices.Contains(hitEvent))
                {
                    cuedVoices.Add(hitEvent);
                    var sound = "karateman/hit";
                    if (hitEvent.type == (int)KarateMan.HitThree.HitThreeAlt) sound += "Alt";
                    MultiSound.Play(new MultiSound.Sound[] { new MultiSound.Sound(sound, hitEvent.beat - hitVoiceOffset * Conductor.instance.songBpm / 60f) });
                }
            }
        }

        public void SetBackgroundFX(BackgroundFXType type)
        {
            BGFXType = type;

            if (BGFXType == BackgroundFXType.None)
            {
                BGFXSprite.enabled = false;
            }
            else
            {
                BGFXSprite.enabled = true;
                BGFXSprite.sprite = BGSprites[(int)type - 1].Sprites[0];
            }
        }

        public void SetBackgroundColor(int type, int shadowType, Color backgroundColor, Color shadowColor)
        {
            BGType = (BackgroundType)type;
            BGColor = backgroundColor;
            BGSprite.color = backgroundColor;
            Shadow = (ShadowType)shadowType;
            ShadowColor = shadowColor;
        }

        public void Bop(float beat, float length)
        {
            bop.length = length;
            bop.startBeat = beat;
        }

        public void Hit2(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] { new MultiSound.Sound("karateman/two", beat + 0.5f) });
        }

        public void Hit3(float beat, bool alt = false)
        {
            var sound = "karateman/three";
            if (alt) sound += "Alt";
            MultiSound.Play(new MultiSound.Sound[] { new MultiSound.Sound(sound, beat + 0.5f) });
            GameObject hit3 = Instantiate(HIT3Ref, this.transform);
            hit3.transform.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>().sprite = Numbers[2];
            BeatAction.New(hit3, new List<BeatAction.Action>() 
            { 
                new BeatAction.Action(beat + 0.5f, delegate { hit3.transform.GetChild(0).gameObject.SetActive(true); }),
                new BeatAction.Action(beat + 4.5f, delegate { Destroy(hit3); })
            });
        }

        public void Hit4(float beat)
        {
            MultiSound.Play(new MultiSound.Sound[] { new MultiSound.Sound("karateman/four", beat + 0.5f) });
            GameObject hit4 = Instantiate(HIT3Ref, this.transform);
            hit4.transform.GetChild(0).GetChild(1).GetComponent<SpriteRenderer>().sprite = Numbers[3];
            BeatAction.New(hit4, new List<BeatAction.Action>()
            {
                new BeatAction.Action(beat + 0.5f, delegate { hit4.transform.GetChild(0).gameObject.SetActive(true); }),
                new BeatAction.Action(beat + 4.5f, delegate { Destroy(hit4); })
            });
        }

        public void Prepare(float beat, float length)
        {
            prepare.startBeat = beat;
            prepare.length = length;
        }

        public void CreateBomb(Transform parent, Vector2 scale, ref GameObject shadow)
        {
            GameObject bomb = Instantiate(Bomb, parent);
            bomb.SetActive(true);
            bomb.transform.localScale = scale;
            shadow.transform.parent = bomb.transform;
            shadow.transform.SetAsLastSibling();
            bomb.GetComponent<Bomb>().shadow = shadow;
        }

        public Color GetShadowColor()
        {
            if(Shadow == ShadowType.Custom)
            {
                return ShadowColor;
            }
            else if(BGType < BackgroundType.Custom)
            {
                return ShadowColors[(int)BGType];
            }
            
            return Color.LerpUnclamped(BGColor, ShadowBlendColor, 0.45f);
        }
    }
}