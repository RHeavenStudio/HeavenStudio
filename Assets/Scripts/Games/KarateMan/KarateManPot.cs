using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NaughtyBezierCurves;

using HeavenStudio.Util;

namespace HeavenStudio.Games.Scripts_KarateMan
{
    public class KarateManPot : MonoBehaviour
    {
        public double startBeat;
        public ItemType type;
        public int path = 1;
        public string hitSfxOverride;

        public GameObject Shadow;
        private GameObject ShadowInstance;
        private SpriteRenderer shadowRenderer;


        //hit effects
        public GameObject HitMark;
        public ParticleSystem[] HitParticles;

        public string awakeAnim;
        FlyStatus status = FlyStatus.Fly;
        Color effectTint = Color.white;

        [SerializeField] SpriteRenderer[] cellRenderers;
        Material[] cellMaterials;
        public Color[] ItemBarrelMap;
        [SerializeField] Color[] ItemAlienMap;
        [SerializeField] Color[] ItemBombMap;
        [SerializeField] Color[] ItemCookingLidMap;

        public bool KickBarrelContent = false;
        public bool ShouldGlow = false;
        public int OnHitExpression = (int) KarateMan.KarateManFaces.Normal;

        public int comboId = -1;
        static int _lastCombo = -1;
        public static int LastCombo { get { return _lastCombo; } }
        public static int GetNewCombo() { _lastCombo++; return _lastCombo; }
        public static void ResetLastCombo() { _lastCombo = -1; }

        public enum ItemType {
            Pot,        // path 1
            Bulb,       // path 1
            Rock,       // path 1
            Ball,       // path 1
            Cooking,    // path 1
            Alien,      // path 1
            TacoBell,   // path 1
            Bomb,   // path 1

            KickBarrel, // path 1
            KickBomb,   // no path
            KickBall,   // no path

            ComboPot1,  // path 1
            ComboPot2,  // path 1
            ComboPot3,  // path 2
            ComboPot4,  // path 3
            ComboPot5,  // path 4
            ComboBarrel,// path 5

            CookingLid, //only used for hit
        }

        public enum FlyStatus {
            Fly,
            Hit,
            NG,
            HitWeak
        }

        //pot trajectory stuff
        public Transform[] HitPosition;
        public float[] HitPositionOffset;
        public Vector3[] StartPositionOffset;
        public float[] ItemSlipRt;
        public BezierCurve3D[] ItemCurves;
        public BezierCurve3D CurrentCurve;
        public float curveTargetBeat;

        public SpriteRenderer BulbLight;

        public void SetBulbColor(Color c) { 
            effectTint = c; 
            BulbLight.color = c;
        }

        float ProgressToHitPosition(float progress) {
            return progress + (HitPositionOffset[path] - 0.5f);
        }

        Vector3 ProgressToFlyPosition()
        {
            var cond = Conductor.instance;
            float progress = Mathf.Min(cond.GetPositionFromBeat(startBeat, 2f), 1f - ItemSlipRt[path]);
            float progressToHitPosition = ProgressToHitPosition(progress);

            Vector3 hitPosition = HitPosition[path].position;

            //https://www.desmos.com/calculator/ycn9v62i4f
            float offset = HitPositionOffset[path];
            float flyHeight = (progressToHitPosition*(progressToHitPosition-1f))/(offset*(offset-1f));
            float floorHeight = HitPosition[0].position.y;

            Vector3 startPosition = hitPosition + StartPositionOffset[path];
            Vector3 endPosition = hitPosition - StartPositionOffset[path];
            Vector3 flyPosition = new Vector3(
                Mathf.Lerp(startPosition.x, endPosition.x, progress),
                floorHeight + (HitPosition[path].position.y - floorHeight + (StartPositionOffset[path].y * (1 - Mathf.Min(cond.GetPositionFromBeat(startBeat, 1f), 1f)))) * flyHeight,
                Mathf.Lerp(startPosition.z, endPosition.z, progress)
            );

            if (progress >= 0.5f && flyPosition.y < HitPosition[0].position.y) {
                flyPosition.y = floorHeight;
            }
            return flyPosition;
        }

        void SetColourMapping()
        {
            Color alpha, bravo, delta;
            switch (type)
            {
                case ItemType.Alien:
                    alpha = ItemAlienMap[0];
                    bravo = ItemAlienMap[1];
                    delta = KarateMan.instance.ItemColor;
                    break;
                case ItemType.Bomb:
                case ItemType.KickBomb:
                    alpha = ItemBombMap[0];
                    bravo = ItemBombMap[1];
                    delta = KarateMan.instance.ItemColor;
                    break;
                case ItemType.KickBarrel:
                case ItemType.ComboBarrel:
                    alpha = ItemBarrelMap[0];
                    bravo = ItemBarrelMap[1];
                    delta = ItemBarrelMap[2];
                    break;
                case ItemType.Cooking:
                case ItemType.CookingLid:
                    alpha = ItemCookingLidMap[0];
                    bravo = ItemCookingLidMap[1];
                    delta = KarateMan.instance.ItemColor;
                    break;
                default:
                    alpha = bravo = delta = KarateMan.instance.ItemColor;
                    break;
            }
            for (int i = 0; i < cellRenderers.Length; i++) {
                SpriteRenderer r = cellRenderers[i];
                if (r.material != null)
                {
                    if (cellMaterials == null)
                    {
                        cellMaterials = new Material[cellRenderers.Length];
                        cellMaterials[i] = Instantiate(r.material);
                        r.material = cellMaterials[i];
                    }

                    r.material.SetColor("_ColorAlpha", alpha);
                    r.material.SetColor("_ColorBravo", bravo);
                    r.material.SetColor("_ColorDelta", delta);
                }
            }
        }


        PlayerActionEvent OnHit;
        PlayerActionEvent OnHitWrongAction;
        void Awake()
        {
            switch (type)
            {
                case ItemType.ComboPot1:
                    OnHit =             KarateMan.instance.ScheduleInput(startBeat, 1f, InputType.STANDARD_ALT_DOWN, ComboStartJustOrNg, ComboStartThrough, ComboStartOut, CanCombo);
                    OnHitWrongAction =  KarateMan.instance.ScheduleUserInput(startBeat, 1f, InputType.STANDARD_DOWN | InputType.DIRECTION_DOWN, ComboStartWrongAction, ComboStartOut, ComboStartOut, CanHit);
                    path = 1;
                    break;
                case ItemType.ComboPot2:
                    path = 1;
                    BeatAction.New(this, new List<BeatAction.Action>() { new BeatAction.Action(startBeat + 1f, delegate { JoeComboSequence(); }) });
                    break;
                case ItemType.ComboPot3:
                    path = 2;
                    BeatAction.New(this, new List<BeatAction.Action>() { new BeatAction.Action(startBeat + 1f, delegate { JoeComboSequence(); }) });
                    break;
                case ItemType.ComboPot4:
                    path = 3;
                    //if the button isn't held anymore make Joe spin
                    BeatAction.New(this, new List<BeatAction.Action>() { new BeatAction.Action(startBeat + 1f, delegate { JoeComboSequence(); }) });
                    break;
                case ItemType.ComboPot5:
                    path = 4;
                    BeatAction.New(this, new List<BeatAction.Action>() { new BeatAction.Action(startBeat + 1f, delegate { JoeComboSequence(); }) });
                    break;
                case ItemType.ComboBarrel:
                    OnHit =             KarateMan.instance.ScheduleInput(startBeat, 1f, InputType.STANDARD_ALT_UP, ComboEndJustOrNg, ComboEndThrough, ComboEndOut, CanComboEnd);
                    path = 5;
                    break;
                case ItemType.KickBarrel:
                    OnHit =             KarateMan.instance.ScheduleInput(startBeat, 1f, InputType.STANDARD_DOWN | InputType.DIRECTION_DOWN, KickChargeJustOrNg, ItemThrough, ItemOut, CanCombo);
                    OnHitWrongAction =  KarateMan.instance.ScheduleUserInput(startBeat, 1f, InputType.STANDARD_ALT_DOWN, ItemWrongAction, ItemOut, ItemOut, CanCombo);
                    path = 1;
                    comboId = -1;
                    break;
                case ItemType.KickBomb:
                    OnHit =             KarateMan.instance.ScheduleInput(startBeat, 0.75f, InputType.STANDARD_UP | InputType.DIRECTION_UP, KickJustOrNg, KickThrough, KickOut, CanKick);
                    CurrentCurve = ItemCurves[6];
                    curveTargetBeat = 2 * 0.75f;
                    path = 1;
                    comboId = -1;
                    break;
                case ItemType.KickBall:
                    OnHit =             KarateMan.instance.ScheduleInput(startBeat, 0.75f, InputType.STANDARD_UP | InputType.DIRECTION_UP, KickJustOrNg, KickThrough, KickOut, CanKick);
                    CurrentCurve = ItemCurves[6];
                    curveTargetBeat = 2 * 0.75f;
                    path = 1;
                    comboId = -1;
                    break;
                case ItemType.CookingLid:
                    CurrentCurve = ItemCurves[9];
                    path = 1;
                    curveTargetBeat = 2f;
                    status = FlyStatus.Hit;
                    comboId = -1;
                    break;
                case ItemType.Bomb:
                    OnHit =             KarateMan.instance.ScheduleInput(startBeat, 1f, InputType.STANDARD_DOWN | InputType.DIRECTION_DOWN, ItemJustOrNg, ItemThrough, ItemOut, CanHit);
                    OnHitWrongAction =  KarateMan.instance.ScheduleUserInput(startBeat, 1f, InputType.STANDARD_ALT_DOWN, ItemWrongAction, ItemOut, ItemOut, CanHit);
                    path = 1;
                    comboId = -1;
                    break;
                default:
                    OnHit =             KarateMan.instance.ScheduleInput(startBeat, 1f, InputType.STANDARD_DOWN | InputType.DIRECTION_DOWN, ItemJustOrNg, ItemThrough, ItemOut, CanHit);
                    OnHitWrongAction =  KarateMan.instance.ScheduleUserInput(startBeat, 1f, InputType.STANDARD_ALT_DOWN, ItemWrongAction, ItemOut, ItemOut, CanHit);
                    path = 1;
                    comboId = -1;
                    break;
            }

            float floorHeight = HitPosition[0].position.y;
            if (CurrentCurve == null)
                transform.position = ProgressToFlyPosition();
            else
                transform.position = CurrentCurve.GetPoint(0f);

            transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (-360f * Time.deltaTime) + UnityEngine.Random.Range(0f, 360f));

            

            ShadowInstance = Instantiate(Shadow, KarateMan.instance.ItemHolder);
            shadowRenderer = ShadowInstance.GetComponent<SpriteRenderer>();
            shadowRenderer.color = KarateMan.instance.Joe.Shadows[0].color;
            ShadowInstance.SetActive(true);
            ShadowInstance.transform.position = new Vector3(transform.position.x, floorHeight - 0.5f, transform.position.z);
        }

        void Start()
        {
            Animator mobjAnim = GetComponent<Animator>();
            mobjAnim.Play(awakeAnim, -1, 0);
            SetColourMapping();
        }

        void Update()
        {
            var cond = Conductor.instance;
            float floorHeight = HitPosition[0].position.y;
            switch (status)
            {
                case FlyStatus.Fly:
                    float prog = cond.GetPositionFromBeat(startBeat, 2f);
                    if (CurrentCurve == null)
                        transform.position = ProgressToFlyPosition();
                    else
                    {
                        prog = cond.GetPositionFromBeat(startBeat, curveTargetBeat);
                        transform.position = CurrentCurve.GetPoint(Mathf.Min(prog, 1f));
                    }
                    
                    if (type == ItemType.Bomb && cond.songPositionInBeatsAsDouble >= startBeat + 2f)
                    {
                        ParticleSystem p = Instantiate(HitParticles[7], transform.position, Quaternion.identity, KarateMan.instance.ItemHolder);
                        p.Play();

                        Destroy(ShadowInstance.gameObject);
                        Destroy(gameObject);
                        return;
                    }
                    else if (prog >= 2f || (ItemKickable() && prog >= 1f)) {
                        if (type == ItemType.KickBomb)
                        {
                            ParticleSystem p = Instantiate(HitParticles[7], ItemCurves[6].GetPoint(1f), Quaternion.identity, KarateMan.instance.ItemHolder);
                            p.Play();
                        }
                        Destroy(ShadowInstance.gameObject);
                        Destroy(gameObject);
                        return;
                    }
                    else if (CurrentCurve == null && prog < 1f - ItemSlipRt[path]) {
                        transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (125f * Time.deltaTime * (1/cond.pitchedSecPerBeat)));
                    }
                    else if (CurrentCurve != null && prog < 1f)
                    {
                        transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (-90f * Time.deltaTime * (1/cond.pitchedSecPerBeat)));
                    }
                    break;
                case FlyStatus.Hit:
                    prog = cond.GetPositionFromBeat(startBeat, curveTargetBeat);
                    if (type == ItemType.Bomb && cond.songPositionInBeatsAsDouble >= startBeat + curveTargetBeat)
                    {
                        ParticleSystem p = Instantiate(HitParticles[7], CurrentCurve.GetPoint(1f), Quaternion.identity, KarateMan.instance.ItemHolder);
                        p.Play();

                        Destroy(ShadowInstance.gameObject);
                        Destroy(gameObject);

                        SoundByte.PlayOneShotGame("karateman/bombBreak", volume: 0.25f);
                        return;
                    }
                    else if (cond.songPositionInBeatsAsDouble >= startBeat + Mathf.Max(2f, curveTargetBeat) || CurrentCurve == null) {

                        if (type == ItemType.KickBomb)
                        {
                            ParticleSystem p = Instantiate(HitParticles[6], ItemCurves[7].GetPoint(1f), Quaternion.identity, KarateMan.instance.ItemHolder);
                            p.Play();
                        }
                        else if (type == ItemType.KickBall && cond.songPositionInBeatsAsDouble < startBeat + curveTargetBeat + 1f)
                            return;
                        
                        Destroy(ShadowInstance.gameObject);
                        Destroy(gameObject);
                        return;
                    }
                    else {
                        if (prog <= 1f)
                        {
                            transform.position = CurrentCurve.GetPoint(prog);
                            transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (-270f * Time.deltaTime * (1/cond.pitchedSecPerBeat)));
                        }
                        else
                        {
                            transform.position = CurrentCurve.GetPoint(1f);
                        }
                    }
                    break;
                case FlyStatus.NG:
                    prog = cond.GetPositionFromBeat(startBeat, curveTargetBeat);

                    if (type == ItemType.Bomb && cond.songPositionInBeatsAsDouble >= startBeat + curveTargetBeat)
                    {
                        KarateMan.instance.Joe.RemoveBombGlow(startBeat + curveTargetBeat, 1f);
                        ParticleSystem p = Instantiate(HitParticles[7], CurrentCurve.GetPoint(1f), Quaternion.identity, KarateMan.instance.ItemHolder);
                        p.Play();

                        Destroy(ShadowInstance.gameObject);
                        Destroy(gameObject);
                        return;
                    }
                    else if (cond.songPositionInBeatsAsDouble >= startBeat + Mathf.Max(2f, curveTargetBeat) || (ItemKickable() && prog >= 1f) || CurrentCurve == null) {
                        if (type == ItemType.KickBomb)
                        {
                            ParticleSystem p = Instantiate(HitParticles[7], ItemCurves[8].GetPoint(1f), Quaternion.identity, KarateMan.instance.ItemHolder);
                            p.Play();
                        }
                        Destroy(ShadowInstance.gameObject);
                        Destroy(gameObject);
                        return;
                    }
                    else {
                        if (prog <= 1f)
                        {
                            transform.position = CurrentCurve.GetPoint(prog);
                            transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + (-90f * Time.deltaTime * (1/cond.pitchedSecPerBeat)));
                        }
                        else
                        {
                            transform.position = CurrentCurve.GetPoint(1f);
                        }
                    }
                    break;
                case FlyStatus.HitWeak:
                    prog = cond.GetPositionFromBeat(startBeat, 1f);

                    Vector3 pos = new Vector3(HitPosition[1].position.x + 0.25f, HitPosition[0].position.y, HitPosition[1].position.z);
                    if (type == ItemType.Bomb && cond.songPositionInBeatsAsDouble >= startBeat + 1f)
                    {
                        KarateMan.instance.Joe.RemoveBombGlow(startBeat + 1f, 1f);

                        ParticleSystem p = Instantiate(HitParticles[7], pos, Quaternion.identity, KarateMan.instance.ItemHolder);
                        p.Play();

                        Destroy(ShadowInstance.gameObject);
                        Destroy(gameObject);
                        return;
                    }
                    else if (cond.songPositionInBeatsAsDouble >= startBeat + 3f)
                    {
                        Destroy(ShadowInstance.gameObject);
                        Destroy(gameObject);
                        return;
                    }
                    if (prog <= 1f)
                    {
                        pos.y = EasingFunction.EaseInCubic(HitPosition[1].position.y, HitPosition[0].position.y, prog);
                    }
                    transform.position = pos;
                    break;
            }
            ShadowInstance.transform.position = new Vector3(transform.position.x, floorHeight - 0.5f, transform.position.z);
            shadowRenderer.color = KarateMan.instance.Joe.Shadows[0].color;
            SetColourMapping();
        }

        void CreateHitMark(bool useLocalPos = false)
        {
            GameObject hitMark = Instantiate(HitMark, KarateMan.instance.ItemHolder);
            if (useLocalPos)
                hitMark.transform.localPosition = transform.position;
            else
                hitMark.transform.position = HitPosition[path].position;
            
            hitMark.SetActive(true);
        }

        //handles hitsound and particles
        void ItemHitEffect(bool straight = false)
        {
            ParticleSystem p;
            CreateHitMark(ItemKickable());
            KarateMan game = KarateMan.instance;
            switch (type)
            {
                case ItemType.Bulb:
                    CurrentCurve = ItemCurves[straight ? 1 : 0];
                    curveTargetBeat = straight ? 1f : 1.5f;
                    SoundByte.PlayOneShotGame(hitSfxOverride ?? "karateman/lightbulbHit", forcePlay: true);
                    p = Instantiate(HitParticles[5], HitPosition[1].position, Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f)), game.ItemHolder);

                    if (effectTint.a == 0)
                        break;

                    //change gradient to match own colour
                    var col = p.colorOverLifetime;
                    col.enabled = true;
                    Gradient grad = new Gradient();
                    grad.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 0.25f), new GradientColorKey(effectTint, 0.5f), new GradientColorKey(effectTint, 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 0.5f), new GradientAlphaKey(0.0f, 1.0f) });
                    col.color = grad;

                    var col2 = p.gameObject.transform.GetChild(0).GetComponent<ParticleSystem>().colorOverLifetime;
                    col2.enabled = true;
                    Gradient grad2 = new Gradient();
                    grad2.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(effectTint, 0.5f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) });
                    col2.color = grad2;

                    p.Play();
                    break;
                case ItemType.Rock:
                    CurrentCurve = ItemCurves[1];
                    curveTargetBeat = 1f;
                    SoundByte.PlayOneShotGame("karateman/rockHit", forcePlay: true);
                    p = Instantiate(HitParticles[4], HitPosition[1].position, Quaternion.identity, game.ItemHolder);
                    p.Play();

                    if (game.IsNoriActive && game.NoriPerformance >= 1f)
                        SoundByte.PlayOneShotGame("karateman/rockHit_fullNori", forcePlay: true);
                    break;
                case ItemType.Ball:
                    CurrentCurve = ItemCurves[1];
                    curveTargetBeat = 1f;
                    SoundByte.PlayOneShotGame("karateman/soccerHit", forcePlay: true);
                    p = Instantiate(HitParticles[1], HitPosition[1].position, Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f)), game.ItemHolder);
                    p.Play();
                    break;
                case ItemType.Cooking:
                    CurrentCurve = ItemCurves[1];
                    curveTargetBeat = 1f;
                    SoundByte.PlayOneShotGame("karateman/cookingPot", forcePlay: true);
                    p = Instantiate(HitParticles[1], HitPosition[1].position, Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f)), game.ItemHolder);
                    p.Play();
                    game.CreateItemInstance(startBeat + 1f, "Item09", 0, ItemType.CookingLid);
                    GetComponent<Animator>().Play("Item08", -1, 0);

                    if (game.IsNoriActive && game.NoriPerformance >= 1f)
                        SoundByte.PlayOneShotGame("karateman/rockHit_fullNori", forcePlay: true);
                    break;
                case ItemType.Alien:
                    CurrentCurve = ItemCurves[1];
                    curveTargetBeat = 1f;
                    SoundByte.PlayOneShotGame("karateman/alienHit", forcePlay: true);
                    p = Instantiate(HitParticles[1], HitPosition[1].position, Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f)), game.ItemHolder);
                    p.Play();

                    if (game.IsNoriActive && game.NoriPerformance >= 1f)
                        SoundByte.PlayOneShotGame("karateman/rockHit_fullNori", forcePlay: true);
                    break;
                case ItemType.Bomb:
                    CurrentCurve = ItemCurves[1];
                    curveTargetBeat = 1f;
                    SoundByte.PlayOneShotGame("karateman/bombHit", forcePlay: true);
                    p = Instantiate(HitParticles[2], HitPosition[1].position, Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f)), game.ItemHolder);
                    p.Play();
                    game.Joe.RemoveBombGlow(startBeat + 1f, 1f);

                    if (game.IsNoriActive && game.NoriPerformance >= 1f)
                        SoundByte.PlayOneShotGame("karateman/rockHit_fullNori", forcePlay: true);
                    break;
                case ItemType.TacoBell:
                    CurrentCurve = ItemCurves[1];
                    curveTargetBeat = 1f;
                    SoundByte.PlayOneShotGame("karateman/rockHit", forcePlay: true);
                    SoundByte.PlayOneShotGame("karateman/tacobell", forcePlay: true);
                    p = Instantiate(HitParticles[1], HitPosition[1].position, Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f)), game.ItemHolder);
                    p.Play();

                    if (game.IsNoriActive && game.NoriPerformance >= 1f)
                        SoundByte.PlayOneShotGame("karateman/rockHit_fullNori", forcePlay: true);
                    break;
                case ItemType.ComboPot1:
                    CurrentCurve = ItemCurves[straight ? 1 : 0];
                    curveTargetBeat = 1.5f;
                    SoundByte.PlayOneShotGame("karateman/comboHit1", forcePlay: true);
                    p = Instantiate(HitParticles[1], HitPosition[1].position, Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f)), game.ItemHolder);
                    p.Play();
                    break;
                case ItemType.ComboPot2:
                    CurrentCurve = ItemCurves[0];
                    curveTargetBeat = 1.5f;
                    SoundByte.PlayOneShotGame("karateman/comboHit1", forcePlay: true);
                    p = Instantiate(HitParticles[1], HitPosition[1].position, Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f)), game.ItemHolder);
                    p.Play();
                    break;
                case ItemType.ComboPot3:
                    CurrentCurve = ItemCurves[2];
                    curveTargetBeat = 1f;
                    SoundByte.PlayOneShotGame("karateman/comboHit2", forcePlay: true);
                    p = Instantiate(HitParticles[1], HitPosition[2].position, Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f)), game.ItemHolder);
                    p.Play();
                    break;
                case ItemType.ComboPot4:
                    CurrentCurve = ItemCurves[3];
                    curveTargetBeat = 1f;
                    SoundByte.PlayOneShotGame("karateman/comboHit3", forcePlay: true);
                    p = Instantiate(HitParticles[1], HitPosition[3].position, Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f)), game.ItemHolder);
                    p.Play();
                    break;
                case ItemType.ComboPot5:
                    CurrentCurve = ItemCurves[4];
                    curveTargetBeat = 1f;
                    SoundByte.PlayOneShotGame("karateman/comboHit3", forcePlay: true);
                    p = Instantiate(HitParticles[1], HitPosition[4].position, Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f)), game.ItemHolder);
                    p.Play();
                    break;
                case ItemType.ComboBarrel:
                    SoundByte.PlayOneShotGame("karateman/comboHit4", forcePlay: true);
                    p = Instantiate(HitParticles[0], HitPosition[5].position, Quaternion.identity, game.ItemHolder);
                    p.Play();
                    p = Instantiate(HitParticles[1], HitPosition[5].position, Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f)), game.ItemHolder);
                    p.Play();
                    break;
                case ItemType.KickBarrel:
                    if (KickBarrelContent) {
                        game.CreateItemInstance(startBeat + 1f, "Item03", OnHitExpression, ItemType.KickBall);
                    } else {
                        if (ShouldGlow) game.Joe.ApplyBombGlow();
                        game.CreateItemInstance(startBeat + 1f, "Item04", OnHitExpression, ItemType.KickBomb);
                    }
                    SoundByte.PlayOneShotGame("karateman/barrelBreak", forcePlay: true);
                    p = Instantiate(HitParticles[0], HitPosition[1].position, Quaternion.Euler(0, 0, -5f), game.ItemHolder);
                    p.Play();
                    p = Instantiate(HitParticles[1], HitPosition[1].position, Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f)), game.ItemHolder);
                    p.Play();

                    break;
                case ItemType.KickBomb:
                    SoundByte.PlayOneShotGame("karateman/bombKick", forcePlay: true);
                    p = Instantiate(HitParticles[2], ItemCurves[6].GetPoint(0.5f), Quaternion.identity, game.ItemHolder);
                    p.Play();

                    game.Joe.RemoveBombGlow(startBeat + 0.75f);
                    break;
                case ItemType.KickBall:
                    SoundByte.PlayOneShotGame("karateman/bombKick", forcePlay: true);
                    p = Instantiate(HitParticles[1], ItemCurves[6].GetPoint(0.5f), Quaternion.identity, game.ItemHolder);
                    p.Play();
                    break;
                default:
                    CurrentCurve = ItemCurves[straight ? 1 : 0];
                    curveTargetBeat = straight ? 1f : 1.5f;
                    if (game.IsNoriActive && game.NoriPerformance < 0.6f)
                    {
                        SoundByte.PlayOneShotGame("karateman/potHit_lowNori", forcePlay: true);
                        SoundByte.PlayOneShotGame("karateman/potHit", volume: 0.66f, forcePlay: true);
                    }
                    else
                        SoundByte.PlayOneShotGame("karateman/potHit", forcePlay: true);
                    p = Instantiate(HitParticles[3], HitPosition[1].position, Quaternion.identity, game.ItemHolder);
                    p.Play();

                    break;
            }

            startBeat = Conductor.instance.songPositionInBeatsAsDouble;
            status = FlyStatus.Hit;
        }

        int ItemPunchHand()
        {
            switch (type)
            {
                case ItemType.Rock:
                case ItemType.Ball:
                case ItemType.Cooking:
                case ItemType.Alien:
                case ItemType.TacoBell:
                case ItemType.Bomb:
                case ItemType.KickBarrel:
                    return 2;
                default:
                    return 0;
            }
        }

        bool ItemKickable()
        {
            switch (type)
            {
                case ItemType.KickBomb:
                case ItemType.KickBall:
                    return true;
                default:
                    return false;
            }
        }

        bool ItemNeedNori()
        {
            switch (type)
            {
                case ItemType.Rock:
                case ItemType.Cooking:
                case ItemType.Alien:
                case ItemType.Bomb:
                case ItemType.TacoBell:
                    return true;
                default:
                    return false;
            }
        }

        void JoeComboSequence()
        {
            if (GameManager.instance.currentGame != "karateman") return;
            var joe = KarateMan.instance.Joe;
            if (joe.GetShouldComboId() != comboId || !joe.inCombo) return;
            switch (type)
            {
                case ItemType.ComboPot2:
                    joe.Punch(2);
                    if (joe.GetComboId() != comboId)
                        SoundByte.PlayOneShotGame("karateman/swingNoHit_Alt", forcePlay: true);
                    else
                    {
                        ItemHitEffect();
                    }
                    break;
                case ItemType.ComboPot3:
                    joe.ComboSequence(0);
                    if (joe.GetComboId() != comboId) {}
                    else
                    {
                        ItemHitEffect();
                    }
                    break;
                case ItemType.ComboPot4:
                    //if the button isn't held anymore make Joe spin
                    if (joe.GetComboId() != comboId) {
                        joe.ComboMiss(startBeat + 1f);
                        SoundByte.PlayOneShotGame("karateman/comboMiss", forcePlay: true);
                        joe.SetShouldComboId(-2);
                    }
                    else
                    {
                        joe.ComboSequence(1);
                        joe.lockedInCombo = true;
                        ItemHitEffect();
                    }
                    break;
                case ItemType.ComboPot5:
                    joe.ComboSequence(2);
                    if (joe.GetComboId() != comboId) {}
                    else
                    {
                        ItemHitEffect();
                    }
                    break;
                default:
                    break;
            }
        }

        void DoHitExpression(double offset)
        {
            if (OnHitExpression == (int) KarateMan.KarateManFaces.Normal)
                return;
            var joe = KarateMan.instance.Joe;
            BeatAction.New(joe, new List<BeatAction.Action>()
            {
                new BeatAction.Action(offset, delegate {
                    joe.SetFaceExpression(OnHitExpression);
                }),
                new BeatAction.Action(offset + 2f, delegate {
                    joe.SetFaceExpression((int) KarateMan.KarateManFaces.Normal);
                })
            });
        }

        public bool CanHit()
        {
            var joe = KarateMan.instance.Joe;
            return status == FlyStatus.Fly && !(joe.inCombo || joe.inNuriLock);
        }

        public void ItemJustOrNg(PlayerActionEvent caller, float state)
        {
            if (GameManager.instance.currentGame != "karateman") return;
            var joe = KarateMan.instance.Joe;
            if (state <= -1f || state >= 1f) {
                bool straight = joe.Punch(ItemPunchHand());
                startBeat = Conductor.instance.songPositionInBeatsAsDouble;
                CurrentCurve = ItemCurves[6];
                curveTargetBeat = 1f;
                SoundByte.PlayOneShot("miss");
                status = FlyStatus.NG;

                joe.SetFaceExpression((int) KarateMan.KarateManFaces.Sad);
                BeatAction.New(joe, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(startBeat + 2f, delegate {
                        joe.SetFaceExpression((int) KarateMan.KarateManFaces.Normal);
                    }),
                });

                KarateMan.instance.Nori.DoNG();
            }
            else {
                if (KarateMan.instance.IsNoriActive)
                {
                    if (ItemNeedNori() && KarateMan.instance.NoriPerformance < 0.6f)
                    {
                        CreateHitMark(false);
                        startBeat = Conductor.instance.songPositionInBeatsAsDouble;
                        status = FlyStatus.HitWeak;
                        SoundByte.PlayOneShotGame("karateman/hitNoNori", forcePlay: true);
                        joe.Punch(3);
                        transform.rotation = Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z - 30f);
                        KarateMan.instance.Nori.DoNG();
                        return;
                    }
                }
                bool straight = joe.Punch(ItemPunchHand());
                DoHitExpression(startBeat + 1f);
                ItemHitEffect(straight);
                status = FlyStatus.Hit;
                KarateMan.instance.Nori.DoHit();
            }
        }

        public void ItemWrongAction(PlayerActionEvent caller, float state)
        { 
            if (GameManager.instance.currentGame != "karateman") return;
            if (!KarateMan.IsComboEnable) return;
            //hitting a normal object with the alt input
            //WHEN SCORING THIS IS A MISS
            var joe = KarateMan.instance.Joe;
            joe.ForceFailCombo(Conductor.instance.songPositionInBeatsAsDouble);
            if (state <= -1f || state >= 1f) {
                startBeat = Conductor.instance.songPositionInBeatsAsDouble;
                CurrentCurve = ItemCurves[6];
                curveTargetBeat = 1f;
                SoundByte.PlayOneShot("miss");
                status = FlyStatus.NG;
            }
            else {
                ItemHitEffect();
            }

            BeatAction.New(joe, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat + 4f, delegate {
                    joe.SetFaceExpression((int) KarateMan.KarateManFaces.Sad);
                }),
                new BeatAction.Action(startBeat + 6f, delegate {
                    joe.SetFaceExpression((int) KarateMan.KarateManFaces.Normal);
                }),
            });
            KarateMan.instance.Nori.DoThrough();
        }

        public void ItemOut(PlayerActionEvent caller) {}

        public void ItemThrough(PlayerActionEvent caller)
        {
            var joe = KarateMan.instance.Joe;
            if (GameManager.instance.currentGame != "karateman") return;
            if (status != FlyStatus.Fly || gameObject == null) return;
            BeatAction.New(joe, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat + 2f, delegate { 
                    joe.SetFaceExpression((int) KarateMan.KarateManFaces.Surprise);
                    SoundByte.PlayOneShotGame("karateman/karate_through", forcePlay: true);
                }),
                new BeatAction.Action(startBeat + 5f, delegate {
                    if (joe.wantFace == -1)
                        joe.SetFaceExpression((int) KarateMan.KarateManFaces.Normal);
                }),
            });
            KarateMan.instance.Nori.DoThrough();
            OnHit.CanHit(false);
            OnHitWrongAction.CanHit(false);
        }

        public bool CanCombo()
        {
            var joe = KarateMan.instance.Joe;
            return status == FlyStatus.Fly && !(joe.inKick || joe.wantKick || joe.inCombo || joe.inNuriLock);
        }

        public void ComboStartJustOrNg(PlayerActionEvent caller, float state)
        {
            if (GameManager.instance.currentGame != "karateman") return;
            var joe = KarateMan.instance.Joe;
            joe.inCombo = true;
            joe.Punch(1);
            joe.SetComboId(comboId);
            joe.SetShouldComboId(comboId);
            if (state <= -1f || state >= 1f) {
                startBeat = Conductor.instance.songPositionInBeatsAsDouble;
                CurrentCurve = ItemCurves[6];
                curveTargetBeat = 1f;
                SoundByte.PlayOneShot("miss");
                status = FlyStatus.NG;

                KarateMan.instance.Nori.DoNG();
            }
            else {
                ItemHitEffect();
                KarateMan.instance.Nori.DoHit();
                }
        }

        public void ComboStartOut(PlayerActionEvent caller) {}
        public void ComboStartThrough(PlayerActionEvent caller) 
        {
            var joe = KarateMan.instance.Joe;
            if (GameManager.instance.currentGame != "karateman") return;
            if (status != FlyStatus.Fly || gameObject == null) return;
            BeatAction.New(joe, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat + 2f, delegate {
                    joe.SetFaceExpression((int) KarateMan.KarateManFaces.Surprise);
                    SoundByte.PlayOneShotGame("karateman/karate_through", forcePlay: true);
                }),
                new BeatAction.Action(startBeat + 5f, delegate {
                    if (joe.wantFace == -1)
                        joe.SetFaceExpression((int) KarateMan.KarateManFaces.Normal);
                }),
            });
            KarateMan.instance.Nori.DoThrough();
            OnHit.CanHit(false);
            OnHitWrongAction.CanHit(false);
        }

        public void ComboStartWrongAction(PlayerActionEvent caller, float state)
        { 
            if (GameManager.instance.currentGame != "karateman") return;
            //hitting a combo start with the normal input
            //WHEN SCORING THIS IS A MISS
            var joe = KarateMan.instance.Joe;
            bool straight = joe.Punch(ItemPunchHand());
            if (state <= -1f || state >= 1f) {
                startBeat = Conductor.instance.songPositionInBeatsAsDouble;
                CurrentCurve = ItemCurves[6];
                curveTargetBeat = 1f;
                SoundByte.PlayOneShot("miss");
                status = FlyStatus.NG;
            }
            else {
                ItemHitEffect(straight);
            }
            KarateMan.instance.Nori.DoThrough();
        }

        public bool CanComboEnd()
        {
            var joe = KarateMan.instance.Joe;
            return status == FlyStatus.Fly && joe.inCombo && joe.GetComboId() == comboId;
        }

        public void ComboEndJustOrNg(PlayerActionEvent caller, float state)
        {
            if (GameManager.instance.currentGame != "karateman") return;
            var joe = KarateMan.instance.Joe;
            joe.inCombo = false;
            joe.SetComboId(-1);
            joe.SetShouldComboId(-1);
            joe.ComboSequence(3);
            if (state <= -1f || state >= 1f) {
                startBeat = Conductor.instance.songPositionInBeatsAsDouble;
                CurrentCurve = ItemCurves[5];
                curveTargetBeat = 1f;
                SoundByte.PlayOneShot("miss");
                status = FlyStatus.NG;

                BeatAction.New(joe, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(startBeat + 2f, delegate {
                        joe.SetFaceExpression((int) KarateMan.KarateManFaces.Sad);
                    }),
                    new BeatAction.Action(startBeat + 6f, delegate {
                        joe.SetFaceExpression((int) KarateMan.KarateManFaces.Normal);
                    }),
                });

                KarateMan.instance.Nori.DoNG();
            }
            else {
                DoHitExpression(startBeat + 1.5f);
                ItemHitEffect();
                KarateMan.instance.Nori.DoHit();
            }
        }

        public void ComboEndOut(PlayerActionEvent caller) {
            if (GameManager.instance.currentGame != "karateman") return;
            var joe = KarateMan.instance.Joe;
            if (status == FlyStatus.Fly && joe.inCombo && joe.GetComboId() == comboId && joe.comboWaiting)
            {
                joe.inCombo = false;
                joe.SetComboId(-1);
                joe.SetShouldComboId(-1);
                joe.ComboSequence(3);
                SoundByte.PlayOneShotGame("karateman/swingKick", forcePlay: true);
            }
            OnHit.CanHit(false);
        }

        public void ComboEndThrough(PlayerActionEvent caller) 
        {
            if (GameManager.instance.currentGame != "karateman") return;
            if (status != FlyStatus.Fly || gameObject == null) return;
            var joe = KarateMan.instance.Joe;
            if (joe.GetComboId() != comboId || !joe.inCombo)
            {
                BeatAction.New(joe, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(startBeat + 2f, delegate { 
                        joe.SetFaceExpression((int) KarateMan.KarateManFaces.Surprise);
                    }),
                    new BeatAction.Action(startBeat + 5f, delegate {
                        if (joe.wantFace == -1)
                            joe.SetFaceExpression((int) KarateMan.KarateManFaces.Normal);
                    }),
                });
            }
            else
            {
                joe.SetFaceExpression((int) KarateMan.KarateManFaces.VerySad);
                BeatAction.New(joe, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(startBeat + 1.5f, delegate { 
                        joe.inCombo = false;
                        joe.SetComboId(-1);
                        joe.SetShouldComboId(-1);
                        joe.ComboSequence(4);
                    }),
                    new BeatAction.Action(startBeat + 2f, delegate { 
                        joe.SetFaceExpression((int) KarateMan.KarateManFaces.VerySad);
                    }),
                    new BeatAction.Action(startBeat + 5f, delegate { 
                        joe.SetFaceExpression((int) KarateMan.KarateManFaces.Normal);
                    })
                });
            }
            KarateMan.instance.Nori.DoThrough();
            OnHit.CanHit(false);
        }

        public void KickChargeJustOrNg(PlayerActionEvent caller, float state)
        {
            if (GameManager.instance.currentGame != "karateman") return;
            var joe = KarateMan.instance.Joe;

            joe.Punch(ItemPunchHand());
            if (state <= -1f || state >= 1f) {
                startBeat = Conductor.instance.songPositionInBeatsAsDouble;
                CurrentCurve = ItemCurves[6];
                curveTargetBeat = 1f;
                SoundByte.PlayOneShot("miss");
                status = FlyStatus.NG;

                joe.SetFaceExpression((int) KarateMan.KarateManFaces.Sad);
                BeatAction.New(joe, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(startBeat + 2f, delegate {
                        joe.SetFaceExpression((int) KarateMan.KarateManFaces.Normal);
                    }),
                });
                KarateMan.instance.Nori.DoNG();
            }
            else {
                joe.StartKickCharge(startBeat + 1.25f);
                ItemHitEffect();
                status = FlyStatus.Hit;
                KarateMan.instance.Nori.DoHit();
            }
        }

        public void KickChargeOut(PlayerActionEvent caller) {}

        public void KickChargeThrough(PlayerActionEvent caller)
        {
            var joe = KarateMan.instance.Joe;
            if (GameManager.instance.currentGame != "karateman") return;
            if (status != FlyStatus.Fly || gameObject == null) return;
            BeatAction.New(joe, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat + 2f, delegate {
                    joe.SetFaceExpression((int) KarateMan.KarateManFaces.Surprise);
                    SoundByte.PlayOneShotGame("karateman/karate_through", forcePlay: true);
                }),
                new BeatAction.Action(startBeat + 5f, delegate {
                    if (joe.wantFace == -1)
                        joe.SetFaceExpression((int) KarateMan.KarateManFaces.Normal);
                }),
            });
            KarateMan.instance.Nori.DoThrough();
            OnHit.CanHit(false);
            OnHitWrongAction.CanHit(false);
        }

        public bool CanKick()
        {
            var joe = KarateMan.instance.Joe;
            return status == FlyStatus.Fly && joe.inKick;
        }

        public void KickJustOrNg(PlayerActionEvent caller, float state)
        {
            if (GameManager.instance.currentGame != "karateman") return;
            var joe = KarateMan.instance.Joe;

            joe.Kick(Conductor.instance.songPositionInBeatsAsDouble);
            if (state <= -1f || state >= 1f) {
                startBeat = Conductor.instance.songPositionInBeatsAsDouble;
                CurrentCurve = ItemCurves[8];
                curveTargetBeat = 1f;
                SoundByte.PlayOneShot("miss");
                status = FlyStatus.NG;

                BeatAction.New(joe, new List<BeatAction.Action>()
                {
                    new BeatAction.Action(startBeat + 1.25f, delegate {
                        joe.SetFaceExpression((int) KarateMan.KarateManFaces.Sad);
                    }),
                    new BeatAction.Action(startBeat + 4.25f, delegate {
                        joe.SetFaceExpression((int) KarateMan.KarateManFaces.Normal);
                    }),
                });

                KarateMan.instance.Nori.DoNG();
                if (type == ItemType.KickBomb)
                    joe.RemoveBombGlow(startBeat + 0.75f);
            }
            else {
                DoHitExpression(startBeat + 2f);
                ItemHitEffect();
                status = FlyStatus.Hit;
                CurrentCurve = ItemCurves[7];
                startBeat = Conductor.instance.songPositionInBeatsAsDouble;
                curveTargetBeat = 3f;
                KarateMan.instance.Nori.DoHit();
            }
        }

        public void KickOut(PlayerActionEvent caller) {}

        public void KickThrough(PlayerActionEvent caller)
        {
            if (GameManager.instance.currentGame != "karateman") return;
            if (status != FlyStatus.Fly || gameObject == null) return;
            BeatAction.New(KarateMan.instance.Joe, new List<BeatAction.Action>()
            {
                new BeatAction.Action(startBeat + 2f, delegate { 
                    KarateMan.instance.Joe.SetFaceExpression((int) KarateMan.KarateManFaces.VerySad);
                }),
                new BeatAction.Action(startBeat + 4f, delegate { 
                    KarateMan.instance.Joe.SetFaceExpression((int) KarateMan.KarateManFaces.Normal);
                })
            });
            KarateMan.instance.Nori.DoThrough();
            OnHit.CanHit(false);
            if (type == ItemType.KickBomb)
                KarateMan.instance.Joe.RemoveBombGlow(startBeat + 0.75f * 2, 1.5f);
        }
    }
}