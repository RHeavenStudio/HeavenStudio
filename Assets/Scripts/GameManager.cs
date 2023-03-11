using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Starpelly;
using Newtonsoft.Json;
using HeavenStudio.Games;
using HeavenStudio.Common;

namespace HeavenStudio
{
    public class GameManager : MonoBehaviour
    {
        [Header("Lists")]
        public DynamicBeatmap Beatmap = new DynamicBeatmap();
        [HideInInspector] public List<DynamicBeatmap.DynamicEntity> playerEntities = new List<DynamicBeatmap.DynamicEntity>();
        private List<GameObject> preloadedGames = new List<GameObject>();
        public List<GameObject> SoundObjects = new List<GameObject>();

        [Header("Components")]
        public string txt;
        public string ext;
        public Camera GameCamera, CursorCam, OverlayCamera, StaticCamera;
        public GameObject GameLetterbox;
        public CircleCursor CircleCursor;
        [HideInInspector] public GameObject GamesHolder;
        public Games.Global.Flash fade;
        public Games.Global.Filter filter;
        public GameObject textbox;

        [Header("Games")]
        public string currentGame;
        Coroutine currentGameSwitchIE;

        [Header("Properties")]
        public int currentEvent, currentTempoEvent, currentVolumeEvent, currentSectionEvent,
            currentPreEvent, currentPreSwitch, currentPreSequence;
        public float endBeat;
        public float startOffset;
        public bool playOnStart;
        public float startBeat;
        [NonSerialized] public GameObject currentGameO;
        public bool autoplay;
        public bool canInput = true;
        public DynamicBeatmap.ChartSection currentSection, nextSection;
        public float sectionProgress { get; private set; }

        public event Action<float> onBeatChanged;
        public event Action<DynamicBeatmap.ChartSection> onSectionChange;

        public int BeatmapEntities()
        {
            return Beatmap.entities.Count + Beatmap.tempoChanges.Count + Beatmap.volumeChanges.Count + Beatmap.beatmapSections.Count;
        }

        public static GameManager instance { get; private set; }
        private EventCaller eventCaller;

        // average input accuracy (msec)
        List<int> inputOffsetSamples = new List<int>();
        float averageInputOffset = 0;
        public float AvgInputOffset
        {
            get
            {
                return averageInputOffset;
            }
            set
            {
                inputOffsetSamples.Add((int)value);
                averageInputOffset = (float)inputOffsetSamples.Average();
            }
        }

        // input accuracy (%)
        double totalInputs = 0;
        double totalPlayerAccuracy = 0;
        public double PlayerAccuracy
        {
            get
            {
                if (totalInputs == 0) return 0;
                return totalPlayerAccuracy / totalInputs;
            }
        }
        bool skillStarCollected = false;

        private void Awake()
        {
            // autoplay = true;
            instance = this;
        }

        public void Init()
        {
            currentPreEvent= 0;
            currentPreSwitch = 0;
            currentPreSequence = 0;
 
            this.transform.localScale = new Vector3(30000000, 30000000);
            
            SpriteRenderer sp = this.gameObject.AddComponent<SpriteRenderer>();
            sp.enabled = false;
            sp.color = Color.black;
            sp.sprite = Resources.Load<Sprite>("Sprites/GeneralPurpose/Square");
            sp.sortingOrder = 30000;
            gameObject.layer = LayerMask.NameToLayer("Flash");

            GameObject fade = new GameObject("flash");
            this.fade = fade.AddComponent<Games.Global.Flash>();
            GameObject filter = new GameObject("filter");
            this.filter = filter.AddComponent<Games.Global.Filter>();


            GlobalGameManager.Init();

            eventCaller = this.gameObject.AddComponent<EventCaller>();
            eventCaller.GamesHolder = GamesHolder.transform;
            eventCaller.Init();
            Conductor.instance.SetBpm(Beatmap.bpm);
            Conductor.instance.SetVolume(Beatmap.musicVolume);
            Conductor.instance.firstBeatOffset = Beatmap.firstBeatOffset;

            // note: serialize this shit in the inspector //
                GameObject textbox = Instantiate(Resources.Load<GameObject>("Prefabs/Common/Textbox"));
                textbox.name = "Textbox";

                GameObject overlays = Instantiate(Resources.Load<GameObject>("Prefabs/Common/Overlays"));
                overlays.name = "Overlays";

                GameObject timingDisp = Instantiate(Resources.Load<GameObject>("Prefabs/Common/Overlays/TimingAccuracy"));
                timingDisp.name = "TimingDisplay";

                GameObject skillStarDisp = Instantiate(Resources.Load<GameObject>("Prefabs/Common/Overlays/SkillStar"));
                skillStarDisp.name = "SkillStar";

                GoForAPerfect.instance.Disable();
            /////
            

            if (txt != null && ext != null)
            {
                LoadRemix(txt, ext);
            }
            else
            {
                NewRemix();
            }

            SortEventsList();

            if (Beatmap.entities.Count >= 1)
            {
                SetCurrentGame(Beatmap.entities[0].datamodel.Split(0));
                SetGame(Beatmap.entities[0].datamodel.Split(0));
            }
            else
            {
                SetGame("noGame");
            }

            if (playOnStart)
            {
                Play(startBeat);
            }
        }

        public void NewRemix()
        {
            Beatmap = new DynamicBeatmap();
            Beatmap.bpm = 120f;
            Beatmap.musicVolume = 100;
            Beatmap.firstBeatOffset = 0f;
            Conductor.instance.musicSource.clip = null;
        }

        public void LoadRemix(string json = "", string type = "riq", int version = 0)
        {

            if (json != "")
            {
                switch (type)
                {
                    case "tengoku":
                    case "rhmania":
                        Beatmap toConvert = JsonConvert.DeserializeObject<Beatmap>(json);
                        Beatmap = DynamicBeatmap.BeatmapConverter(toConvert);
                        break;
                    case "riq":
                        Beatmap = JsonConvert.DeserializeObject<DynamicBeatmap>(json);
                        Beatmap.PostProcess();
                        break;
                    default:
                        NewRemix();
                        break;
                }
            }
            else
            {
                NewRemix();
            }
            SortEventsList();
            Conductor.instance.SetBpm(Beatmap.bpm);
            Conductor.instance.SetVolume(Beatmap.musicVolume);
            Conductor.instance.firstBeatOffset = Beatmap.firstBeatOffset;
            Stop(0);
            SetCurrentEventToClosest(0);

            if (Beatmap.entities.Count >= 1)
            {
                SetCurrentGame(Beatmap.entities[0].datamodel.Split(0));
                SetGame(Beatmap.entities[0].datamodel.Split(0));
            }
            else
            {
                SetGame("noGame");
            }
        }

        public void ScoreInputAccuracy(double accuracy, bool late, double time, double weight = 1, bool doDisplay = true)
        {
            totalInputs += weight;
            totalPlayerAccuracy += accuracy * weight;

            if (accuracy < Minigame.rankOkThreshold && weight > 0)
            {
                SkillStarManager.instance.KillStar();
            }
            
            if (SkillStarManager.instance.IsEligible && !skillStarCollected && accuracy >= 1f)
            {
                if (SkillStarManager.instance.DoStarJust())
                    skillStarCollected = true;
            }

            // push the hit event to the timing display
            if (doDisplay)
                TimingAccuracyDisplay.instance.MakeAccuracyVfx(time, late);
        }

        public void SeekAheadAndPreload(double start, float seekTime = 8f)
        {
            //seek ahead to preload games that have assetbundles
            //check game switches first
            var gameSwitchs = Beatmap.entities.FindAll(c => c.datamodel.Split(1) == "switchGame");
            if (currentPreSwitch < gameSwitchs.Count && currentPreSwitch >= 0)
            {
                if (start + seekTime >= gameSwitchs[currentPreSwitch].beat)
                {
                    string gameName = gameSwitchs[currentPreSwitch].datamodel.Split(2);
                    var inf = GetGameInfo(gameName);
                    if (inf.usesAssetBundle && !inf.AssetsLoaded) 
                    {
                        Debug.Log($"ASYNC loading assetbundles for game {gameName}");
                        StartCoroutine(inf.LoadCommonAssetBundleAsync());
                        StartCoroutine(inf.LoadLocalizedAssetBundleAsync());
                    }
                    currentPreSwitch++;
                }
            }
            //then check game entities
            List<float> entities = Beatmap.entities.Select(c => c.beat).ToList();
            if (currentPreEvent < Beatmap.entities.Count && currentPreEvent >= 0)
            {
                if (start + seekTime >= entities[currentPreEvent])
                {
                    var entitiesAtSameBeat = Beatmap.entities.FindAll(c => c.beat == Beatmap.entities[currentPreEvent].beat && !EventCaller.FXOnlyGames().Contains(EventCaller.instance.GetMinigame(c.datamodel.Split('/')[0])));
                    SortEventsByPriority(entitiesAtSameBeat);
                    foreach (DynamicBeatmap.DynamicEntity entity in entitiesAtSameBeat)
                    {
                        string gameName = entity.datamodel.Split('/')[0];
                        var inf = GetGameInfo(gameName);
                        if (inf.usesAssetBundle && !inf.AssetsLoaded) 
                        {
                            Debug.Log($"ASYNC loading assetbundles for game {gameName}");
                            StartCoroutine(inf.LoadCommonAssetBundleAsync());
                            StartCoroutine(inf.LoadLocalizedAssetBundleAsync());
                        }
                        currentPreEvent++;
                    }
                }
            }
        }

        public void SeekAheadAndDoPreEvent(double start)
        {
            List<float> entities = Beatmap.entities.Select(c => c.beat).ToList();
            if (currentPreSequence < Beatmap.entities.Count && currentPreSequence >= 0)
            {
                var seekEntity = Beatmap.entities[currentPreSequence];

                float seekTime = EventCaller.instance.GetGameAction(
                    EventCaller.instance.GetMinigame(seekEntity.datamodel.Split(0)), seekEntity.datamodel.Split(1)).preFunctionLength;

                if (start + seekTime >= entities[currentPreSequence])
                {
                    float beat = seekEntity.beat;
                    var entitiesAtSameBeat = Beatmap.entities.FindAll(c => c.beat == seekEntity.beat);
                    SortEventsByPriority(entitiesAtSameBeat);
                    foreach (DynamicBeatmap.DynamicEntity entity in entitiesAtSameBeat)
                    {
                        currentPreSequence++;
                        string gameName = entity.datamodel.Split('/')[0];
                        var inf = GetGameInfo(gameName);
                        if (inf.usesAssetBundle && inf.AssetsLoaded && !inf.SequencesPreloaded) 
                        {
                            Debug.Log($"Preloading game {gameName}");
                            PreloadGameSequences(gameName);
                        }
                        else
                        {
                            eventCaller.CallPreEvent(entity);
                        }
                    }
                }
            }
        }

        private void Update()
        {
            PlayerInput.UpdateInputControllers();

            if (BeatmapEntities() < 1) //bruh really you forgot to ckeck tempo changes
                return;
            if (!Conductor.instance.isPlaying)
                return;
            Conductor cond = Conductor.instance;
            
            List<float> entities = Beatmap.entities.Select(c => c.beat).ToList();

            List<float> tempoChanges = Beatmap.tempoChanges.Select(c => c.beat).ToList();
            if (currentTempoEvent < Beatmap.tempoChanges.Count && currentTempoEvent >= 0)
            {
                if (cond.songPositionInBeatsAsDouble >= tempoChanges[currentTempoEvent])
                {
                    cond.SetBpm(Beatmap.tempoChanges[currentTempoEvent].tempo);
                    currentTempoEvent++;
                }
            }

            List<float> volumeChanges = Beatmap.volumeChanges.Select(c => c.beat).ToList();
            if (currentVolumeEvent < Beatmap.volumeChanges.Count && currentVolumeEvent >= 0)
            {
                if (cond.songPositionInBeatsAsDouble >= volumeChanges[currentVolumeEvent])
                {
                    cond.SetVolume(Beatmap.volumeChanges[currentVolumeEvent].volume);
                    currentVolumeEvent++;
                }
            }

            List<float> chartSections = Beatmap.beatmapSections.Select(c => c.beat).ToList();
            if (currentSectionEvent < Beatmap.beatmapSections.Count && currentSectionEvent >= 0)
            {
                if (cond.songPositionInBeatsAsDouble >= chartSections[currentSectionEvent])
                {
                    Debug.Log("Section " + Beatmap.beatmapSections[currentSectionEvent].sectionName + " started");
                    currentSection = Beatmap.beatmapSections[currentSectionEvent];
                    currentSectionEvent++;
                    if (currentSectionEvent < Beatmap.beatmapSections.Count)
                        nextSection = Beatmap.beatmapSections[currentSectionEvent];
                    else
                        nextSection = null;
                    onSectionChange?.Invoke(currentSection);
                }
            }

            float seekTime = 8f;
            //seek ahead to preload games that have assetbundles
            SeekAheadAndPreload(cond.songPositionInBeatsAsDouble, seekTime);

            SeekAheadAndDoPreEvent(Conductor.instance.songPositionInBeatsAsDouble);

            if (currentEvent < Beatmap.entities.Count && currentEvent >= 0)
            {
                if (cond.songPositionInBeatsAsDouble >= entities[currentEvent])
                {
                    // allows for multiple events on the same beat to be executed on the same frame, so no more 1-frame delay
                    var entitiesAtSameBeat = Beatmap.entities.FindAll(c => c.beat == Beatmap.entities[currentEvent].beat && !EventCaller.FXOnlyGames().Contains(EventCaller.instance.GetMinigame(c.datamodel.Split('/')[0])));
                    var fxEntities = Beatmap.entities.FindAll(c => c.beat == Beatmap.entities[currentEvent].beat && EventCaller.FXOnlyGames().Contains(EventCaller.instance.GetMinigame(c.datamodel.Split('/')[0])));

                    SortEventsByPriority(fxEntities);
                    SortEventsByPriority(entitiesAtSameBeat);

                    // FX entities should ALWAYS execute before gameplay entities
                    for (int i = 0; i < fxEntities.Count; i++)
                    {
                        eventCaller.CallEvent(fxEntities[i], true);
                        currentEvent++;
                    }

                    foreach (DynamicBeatmap.DynamicEntity entity in entitiesAtSameBeat)
                    {
                        // if game isn't loaded, preload game so whatever event that would be called will still run outside if needed
                        if (entity.datamodel.Split('/')[0] != currentGame)
                        {
                            eventCaller.CallEvent(entity, false);
                        }
                        else
                        {
                            eventCaller.CallEvent(entity, true);
                        }

                        // Thank you to @shshwdr for bring this to my attention
                        currentEvent++;
                    }
                }
            }

            if (currentSection == null)
            {
                sectionProgress = 0;
            }
            else
            {
                float currectSectionStart = (float)cond.GetSongPosFromBeat(currentSection.beat);

                if (nextSection == null) 
                    sectionProgress = (cond.songPosition - currectSectionStart) / ((float)cond.GetSongPosFromBeat(endBeat) - currectSectionStart);
                else
                    sectionProgress = (cond.songPosition - currectSectionStart) / ((float)cond.GetSongPosFromBeat(nextSection.beat) - currectSectionStart);
            }
        }

        private void LateUpdate() {
            OverlaysManager.instance.TogleOverlaysVisibility(Editor.Editor.instance == null || Editor.Editor.instance.fullscreen || ((PersistentDataManager.gameSettings.overlaysInEditor) && (!Editor.Editor.instance.fullscreen)) || HeavenStudio.Editor.GameSettings.InPreview);
        }

        public void ToggleInputs(bool inputs)
        {
            canInput = inputs;
        }

        #region Play Events

        public void Play(float beat)
        {
            canInput = true;
            inputOffsetSamples.Clear();
            averageInputOffset = 0;

            totalInputs = 0;
            totalPlayerAccuracy = 0;

            TimingAccuracyDisplay.instance.ResetArrow();
            SkillStarManager.instance.Reset();
            skillStarCollected = false;

            GoForAPerfect.instance.perfect = true;
            GoForAPerfect.instance.Disable();

            SectionMedalsManager.instance.Reset();

            StartCoroutine(PlayCo(beat));
            onBeatChanged?.Invoke(beat);
        }

        private IEnumerator PlayCo(float beat)
        {
            yield return null;
            bool paused = Conductor.instance.isPaused;

            Conductor.instance.SetBpm(Beatmap.bpm);
            Conductor.instance.SetVolume(Beatmap.musicVolume);
            Conductor.instance.firstBeatOffset = Beatmap.firstBeatOffset;

            Conductor.instance.Play(beat);
            if (!paused)
            {
                SetCurrentEventToClosest(beat);
            }

            KillAllSounds();

            Minigame miniGame = currentGameO.GetComponent<Minigame>();
            if (miniGame != null)
                miniGame.OnPlay(beat);
        }

        public void Pause()
        {
            Conductor.instance.Pause();
            KillAllSounds();
        }

        public void Stop(float beat)
        {
            Conductor.instance.Stop(beat);
            SetCurrentEventToClosest(beat);
            onBeatChanged?.Invoke(beat);
            KillAllSounds();

            // I feel like I should standardize the names
            SkillStarManager.instance.KillStar();
            TimingAccuracyDisplay.instance.StopStarFlash();
            GoForAPerfect.instance.Disable();
            SectionMedalsManager.instance.OnRemixEnd();

            Debug.Log($"Average input offset for playthrough: {averageInputOffset}ms");
            Debug.Log($"Accuracy for playthrough: {(PlayerAccuracy * 100) : 0.00}");

            if (playOnStart)
            {
                Play(0);
            }
        }

        public void KillAllSounds()
        {
            for (int i = 0; i < SoundObjects.Count; i++)
                Destroy(SoundObjects[i].gameObject);
            
            SoundObjects.Clear();
        }

        #endregion

        #region List Functions

        public void SortEventsList()
        {
            Beatmap.entities.Sort((x, y) => x.beat.CompareTo(y.beat));
            Beatmap.tempoChanges.Sort((x, y) => x.beat.CompareTo(y.beat));
            Beatmap.volumeChanges.Sort((x, y) => x.beat.CompareTo(y.beat));
        }

        void SortEventsByPriority(List<DynamicBeatmap.DynamicEntity> entities)
        {
            entities.Sort((x, y) => {
                Minigames.Minigame xGame = EventCaller.instance.GetMinigame(x.datamodel.Split(0));
                Minigames.GameAction xAction = EventCaller.instance.GetGameAction(xGame, x.datamodel.Split(1));
                Minigames.Minigame yGame = EventCaller.instance.GetMinigame(y.datamodel.Split(0));
                Minigames.GameAction yAction = EventCaller.instance.GetGameAction(yGame, y.datamodel.Split(1));

                return yAction.priority.CompareTo(xAction.priority);
            });

        }

        public void SetCurrentEventToClosest(float beat)
        {
            SortEventsList();
            onBeatChanged?.Invoke(beat);
            if (Beatmap.entities.Count > 0)
            {
                List<float> entities = Beatmap.entities.Select(c => c.beat).ToList();

                currentEvent = entities.IndexOf(Mathp.GetClosestInList(entities, beat));
                currentPreEvent = entities.IndexOf(Mathp.GetClosestInList(entities, beat));
                currentPreSequence = entities.IndexOf(Mathp.GetClosestInList(entities, beat));

                var gameSwitchs = Beatmap.entities.FindAll(c => c.datamodel.Split(1) == "switchGame");

                string newGame = Beatmap.entities[currentEvent].datamodel.Split(0);

                if (gameSwitchs.Count > 0)
                {
                    int index = gameSwitchs.FindIndex(c => c.beat == Mathp.GetClosestInList(gameSwitchs.Select(c => c.beat).ToList(), beat));
                    currentPreSwitch = index;
                    var closestGameSwitch = gameSwitchs[index];
                    if (closestGameSwitch.beat <= beat)
                    {
                        newGame = closestGameSwitch.datamodel.Split(2);
                    }
                    else if (closestGameSwitch.beat > beat)
                    {
                        if (index == 0)
                        {
                            newGame = Beatmap.entities[0].datamodel.Split(0);
                        }
                        else
                        {
                            if (index - 1 >= 0)
                            {
                                newGame = gameSwitchs[index - 1].datamodel.Split(2);
                            }
                            else
                            {
                                newGame = Beatmap.entities[Beatmap.entities.IndexOf(closestGameSwitch) - 1].datamodel.Split(0);
                            }
                        }
                    }
                    // newGame = gameSwitchs[gameSwitchs.IndexOf(gameSwitchs.Find(c => c.beat == Mathp.GetClosestInList(gameSwitchs.Select(c => c.beat).ToList(), beat)))].datamodel.Split(2);
                }

                if (!GetGameInfo(newGame).fxOnly)
                {
                    SetGame(newGame);
                }

                List<DynamicBeatmap.DynamicEntity> allEnds = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "end" });
                if (allEnds.Count > 0)
                    endBeat = allEnds.Select(c => c.beat).Min();
                else
                    endBeat = Conductor.instance.SongLengthInBeats();
            }
            else
            {
                SetGame("noGame");
                endBeat = Conductor.instance.SongLengthInBeats();
            }

            if (Beatmap.tempoChanges.Count > 0)
            {
                currentTempoEvent = 0;
                List<float> tempoChanges = Beatmap.tempoChanges.Select(c => c.beat).ToList();

                //for tempo changes, just go over all of em until the last one we pass
                for (int t = 0; t < tempoChanges.Count; t++)
                {
                    // Debug.Log("checking tempo event " + t + " against beat " + beat + "( tc beat " + tempoChanges[t] + ")");
                    if (tempoChanges[t] > beat)
                    {
                        break;
                    }
                    currentTempoEvent = t;
                }
                // Debug.Log("currentTempoEvent is now " + currentTempoEvent);
            }

            if (Beatmap.volumeChanges.Count > 0)
            {
                currentVolumeEvent = 0;
                List<float> volumeChanges = Beatmap.volumeChanges.Select(c => c.beat).ToList();

                for (int t = 0; t < volumeChanges.Count; t++)
                {
                    if (volumeChanges[t] > beat)
                    {
                        break;
                    }
                    currentVolumeEvent = t;
                }
            }

            currentSection = null;
            nextSection = null;
            if (Beatmap.beatmapSections.Count > 0)
            {
                currentSectionEvent = 0;
                List<float> beatmapSections = Beatmap.beatmapSections.Select(c => c.beat).ToList();

                for (int t = 0; t < beatmapSections.Count; t++)
                {
                    if (beatmapSections[t] > beat)
                    {
                        break;
                    }
                    currentSectionEvent = t;
                }
            }
            onSectionChange?.Invoke(currentSection);

            SeekAheadAndPreload(beat);
        }

        #endregion

        public void SwitchGame(string game, float beat)
        {
            if (game != currentGame)
            {
                if (currentGameSwitchIE != null)
                    StopCoroutine(currentGameSwitchIE);
                currentGameSwitchIE = StartCoroutine(SwitchGameIE(game, beat));
            }
        }

        IEnumerator SwitchGameIE(string game, float beat)
        {
            this.GetComponent<SpriteRenderer>().enabled = true;

            SetGame(game);

            Minigame miniGame = currentGameO.GetComponent<Minigame>();
            if (miniGame != null)
                miniGame.OnGameSwitch(beat);

            yield return new WaitForSeconds(0.1f);

            this.GetComponent<SpriteRenderer>().enabled = false;
        }

        private void SetGame(string game)
        {
            Destroy(currentGameO);

            currentGameO = Instantiate(GetGame(game));
            currentGameO.transform.parent = eventCaller.GamesHolder.transform;
            currentGameO.name = game;

            SetCurrentGame(game);

            ResetCamera();
        }

        public void PreloadGameSequences(string game)
        {
            var gameInfo = GetGameInfo(game);
            //load the games' sound sequences
            if (gameInfo != null && gameInfo.LoadedSoundSequences == null)
                gameInfo.LoadedSoundSequences = GetGame(game).GetComponent<Minigame>().SoundSequences;
        }

        public GameObject GetGame(string name)
        {
            var gameInfo = GetGameInfo(name);
            if (gameInfo != null)
            {
                if (gameInfo.fxOnly)
                {
                    var gameEntities = Beatmap.entities.FindAll(c => {
                            var gameName = c.datamodel.Split(0);
                            var newGameInfo = GetGameInfo(gameName);
                            if (newGameInfo == null)
                                return false;
                            else
                                return !newGameInfo.fxOnly;
                        }).ToList();
                    if (gameEntities.Count != 0)
                        name = gameEntities[0].datamodel.Split(0);
                    else
                        name = "noGame";
                }
                else
                {
                    if (gameInfo.usesAssetBundle)
                    {
                        //game is packed in an assetbundle, load from that instead
                        return gameInfo.GetCommonAssetBundle().LoadAsset<GameObject>(name);
                    }
                }
            }
            return Resources.Load<GameObject>($"Games/{name}");
        }

        public Minigames.Minigame GetGameInfo(string name)
        {
            return EventCaller.instance.minigames.Find(c => c.name == name);
        }

        public void SetCurrentGame(string game)
        {
            currentGame = game;
            if (GetGameInfo(currentGame) != null) CircleCursor.InnerCircle.GetComponent<SpriteRenderer>().color = Colors.Hex2RGB(GetGameInfo(currentGame).color);
            else
                CircleCursor.InnerCircle.GetComponent<SpriteRenderer>().color = Color.white;
        }

        private bool SongPosLessThanClipLength(float t)
        {
            if (Conductor.instance.musicSource.clip != null)
                return Conductor.instance.GetSongPosFromBeat(t) < Conductor.instance.musicSource.clip.length;
            else
                return true;
        }

        public void ResetCamera()
        {
            HeavenStudio.GameCamera.ResetAdditionalTransforms();
        }
    }
}