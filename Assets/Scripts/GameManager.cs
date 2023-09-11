using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

using Starpelly;
using Jukebox;
using HeavenStudio.Util;
using HeavenStudio.Games;
using HeavenStudio.Common;

namespace HeavenStudio
{
    public class GameManager : MonoBehaviour
    {
        const int SoundPoolSizeMin = 32;
        const int SoundPoolSizeMax = 32;

        [Header("Lists")]
        [NonSerialized] public RiqBeatmap Beatmap = new();
        private List<GameObject> preloadedGames = new();
        [NonSerialized] public ObjectPool<Sound> SoundObjects;

        [Header("Components")]
        [NonSerialized] public Camera GameCamera, CursorCam, OverlayCamera, StaticCamera;
        [NonSerialized] public CircleCursor CircleCursor;
        [NonSerialized] public GameObject GamesHolder;
        [NonSerialized] public Games.Global.Flash fade;
        [NonSerialized] public Games.Global.Filter filter;

        [Header("Games")]
        [NonSerialized] public string currentGame;
        Coroutine currentGameSwitchIE;

        [Header("Properties")]
        [NonSerialized] public string txt = null;
        [NonSerialized] public string ext = null;

        [NonSerialized]
        public int currentEvent, currentTempoEvent, currentVolumeEvent, currentSectionEvent,
            currentPreEvent, currentPreSwitch, currentPreSequence;
        [NonSerialized] public double endBeat;
        [NonSerialized] public float startOffset;
        [NonSerialized] public bool playOnStart;
        [NonSerialized] public double startBeat;
        [NonSerialized] public GameObject currentGameO;
        [NonSerialized] public bool autoplay;
        [NonSerialized] public bool canInput = true;
        [NonSerialized] public RiqEntity currentSection, nextSection;
        public double sectionProgress { get; private set; }

        bool AudioLoadDone;
        bool ChartLoadError;

        List<double> eventBeats, tempoBeats, volumeBeats, sectionBeats;

        public event Action<double> onBeatChanged;
        public event Action<RiqEntity> onSectionChange;

        public int BeatmapEntities()
        {
            return Beatmap.Entities.Count + Beatmap.TempoChanges.Count + Beatmap.VolumeChanges.Count + Beatmap.SectionMarkers.Count;
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

        // cleared sections
        List<bool> clearedSections = new List<bool>();
        public bool ClearedSection
        {
            set
            {
                clearedSections.Add(value);
            }
        }

        private void Awake()
        {
            // autoplay = true;
            instance = this;
        }

        public void Init(bool preLoaded = false)
        {
            AudioLoadDone = false;
            ChartLoadError = false;
            currentPreEvent = 0;
            currentPreSwitch = 0;
            currentPreSequence = 0;

            GameObject filter = new GameObject("filter");
            this.filter = filter.AddComponent<Games.Global.Filter>();

            eventCaller = this.gameObject.AddComponent<EventCaller>();
            eventCaller.GamesHolder = GamesHolder.transform;
            eventCaller.Init();
            Conductor.instance.SetBpm(120f);
            Conductor.instance.SetVolume(100f);
            Conductor.instance.firstBeatOffset = Beatmap.data.offset;

            // note: serialize this shit in the inspector //
            GameObject textbox = Instantiate(Resources.Load<GameObject>("Prefabs/Common/Textbox"));
            textbox.name = "Textbox";

            GameObject timingDisp = Instantiate(Resources.Load<GameObject>("Prefabs/Common/Overlays/TimingAccuracy"));
            timingDisp.name = "TimingDisplay";

            GameObject skillStarDisp = Instantiate(Resources.Load<GameObject>("Prefabs/Common/Overlays/SkillStar"));
            skillStarDisp.name = "SkillStar";

            GameObject overlays = Instantiate(Resources.Load<GameObject>("Prefabs/Common/Overlays"));
            overlays.name = "Overlays";

            GoForAPerfect.instance.Disable();
            /////

            SoundObjects = new ObjectPool<Sound>(CreatePooledSound, OnTakePooledSound, OnReturnPooledSound, OnDestroyPooledSound, true, SoundPoolSizeMin, SoundPoolSizeMax);


            if (preLoaded)
            {
                LoadRemix(false);
            }
            else
            {
                RiqFileHandler.ClearCache();
                NewRemix();
            }

            SortEventsList();

            if (Beatmap.Entities.Count >= 1)
            {
                SetCurrentGame(Beatmap.Entities[0].datamodel.Split(0));
                SetGame(Beatmap.Entities[0].datamodel.Split(0));
            }
            else
            {
                SetGame("noGame");
            }

            if (playOnStart)
            {
                StartCoroutine(WaitReadyAndPlayCo(startBeat));
            }
        }

        Sound CreatePooledSound()
        {
            GameObject oneShot = new GameObject($"Pooled Scheduled Sound");
            oneShot.transform.SetParent(transform);

            AudioSource audioSource = oneShot.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            Sound snd = oneShot.AddComponent<Sound>();

            oneShot.SetActive(false);

            return snd;
        }

        // Called when an item is returned to the pool using Release
        void OnReturnPooledSound(Sound snd)
        {
            snd.Stop();
        }

        // Called when an item is taken from the pool using Get
        void OnTakePooledSound(Sound snd)
        {
            snd.gameObject.SetActive(true);
        }

        // If the pool capacity is reached then any items returned will be destroyed.
        void OnDestroyPooledSound(Sound snd)
        {
            snd.Stop();
            Destroy(snd.gameObject);
        }

        public void NewRemix()
        {
            AudioLoadDone = false;
            Beatmap = new("1", "HeavenStudio");
            Beatmap.data.properties = Minigames.propertiesModel;
            Beatmap.AddNewTempoChange(0, 120f);
            Beatmap.AddNewVolumeChange(0, 100f);
            Beatmap.data.offset = 0f;
            Conductor.instance.musicSource.clip = null;
            RiqFileHandler.WriteRiq(Beatmap);
            AudioLoadDone = true;
        }

        public IEnumerator LoadMusic()
        {
            ChartLoadError = false;
            IEnumerator load = RiqFileHandler.LoadSong();
            while (true)
            {
                object current = load.Current;
                try
                {
                    if (load.MoveNext() == false)
                    {
                        break;
                    }
                    current = load.Current;
                }
                catch (System.IO.FileNotFoundException f)
                {
                    Debug.LogWarning("chart has no music: " + f.Message);
                    Conductor.instance.musicSource.clip = null;
                    AudioLoadDone = true;
                    yield break;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load music: {e.Message}");
                    GlobalGameManager.ShowErrorMessage("Error Loading Music", e.Message + "\n\n" + e.StackTrace);
                    AudioLoadDone = true;
                    ChartLoadError = true;
                    yield break;
                }
                yield return current;
            }
            Conductor.instance.musicSource.clip = RiqFileHandler.StreamedAudioClip;
            AudioLoadDone = true;
        }

        public void LoadRemix(bool editor = false)
        {
            AudioLoadDone = false;
            ChartLoadError = false;
            try
            {
                Beatmap = RiqFileHandler.ReadRiq();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load remix: {e.Message}");
                GlobalGameManager.ShowErrorMessage("Error Loading RIQ", e.Message + "\n\n" + e.StackTrace);
                ChartLoadError = true;
                return;
            }
            if (!editor)
                StartCoroutine(LoadMusic());
            SortEventsList();
            Conductor.instance.SetBpm(Beatmap.TempoChanges[0]["tempo"]);
            Conductor.instance.SetVolume(Beatmap.VolumeChanges[0]["volume"]);
            Conductor.instance.firstBeatOffset = Beatmap.data.offset;
            if (!playOnStart)
            {
                Stop(0);
            }
            SetCurrentEventToClosest(0);

            if (Beatmap.Entities.Count >= 1)
            {
                SetCurrentGame(Beatmap.Entities[0].datamodel.Split(0));
                SetGame(Beatmap.Entities[0].datamodel.Split(0));
            }
            else
            {
                SetGame("noGame");
            }

            if (editor)
            {
                Debug.Log(Beatmap.data.riqOrigin);
                if (Beatmap.data.riqOrigin != "HeavenStudio")
                {
                    string origin = Beatmap.data.riqOrigin?.DisplayName() ?? "Unknown Origin";
                    GlobalGameManager.ShowErrorMessage("Warning",
                        $"This chart came from\n<alpha=#AA>{origin}</color>\nand uses content not included in Heaven Studio.\n\n<color=\"yellow\">You may be able to edit this chart in Heaven Studio to be used in its original program.</color>");
                }
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
            List<RiqEntity> entitiesAtSameBeat = ListPool<RiqEntity>.Get();
            List<RiqEntity> gameSwitchs = ListPool<RiqEntity>.Get();
            Minigames.Minigame inf;

            //seek ahead to preload games that have assetbundles
            //check game switches first
            foreach (RiqEntity entity in Beatmap.Entities)
            {
                if (entity.datamodel.Split(1) == "switchGame")
                {
                    gameSwitchs.Add(entity);
                }
            }
            if (currentPreSwitch < gameSwitchs.Count && currentPreSwitch >= 0)
            {
                if (start + seekTime >= gameSwitchs[currentPreSwitch].beat)
                {
                    string gameName = gameSwitchs[currentPreSwitch].datamodel.Split(2);
                    inf = GetGameInfo(gameName);
                    if (inf != null && inf.usesAssetBundle && !inf.AssetsLoaded)
                    {
                        Debug.Log($"ASYNC loading assetbundles for game {gameName}");
                        StartCoroutine(inf.LoadCommonAssetBundleAsync());
                        StartCoroutine(inf.LoadLocalizedAssetBundleAsync());
                    }
                    currentPreSwitch++;
                }
            }
            //then check game entities
            if (currentPreEvent < Beatmap.Entities.Count && currentPreEvent >= 0)
            {
                if (start + seekTime >= eventBeats[currentPreEvent])
                {
                    foreach (RiqEntity entity in Beatmap.Entities)
                    {
                        if (entity.beat == Beatmap.Entities[currentPreEvent].beat && !EventCaller.FXOnlyGames().Contains(eventCaller.GetMinigame(entity.datamodel.Split('/')[0])))
                        {
                            entitiesAtSameBeat.Add(entity);
                        }
                    }
                    SortEventsByPriority(entitiesAtSameBeat);
                    foreach (RiqEntity entity in entitiesAtSameBeat)
                    {
                        string gameName = entity.datamodel.Split('/')[0];
                        inf = GetGameInfo(gameName);
                        if (inf != null && inf.usesAssetBundle && !inf.AssetsLoaded)
                        {
                            Debug.Log($"ASYNC loading assetbundles for game {gameName}");
                            StartCoroutine(inf.LoadCommonAssetBundleAsync());
                            StartCoroutine(inf.LoadLocalizedAssetBundleAsync());
                        }
                        currentPreEvent++;
                    }
                }
            }

            ListPool<RiqEntity>.Release(entitiesAtSameBeat);
            ListPool<RiqEntity>.Release(gameSwitchs);
        }

        public void SeekAheadAndDoPreEvent(double start)
        {
            if (currentPreSequence < Beatmap.Entities.Count && currentPreSequence >= 0)
            {
                List<RiqEntity> entitiesAtSameBeat = ListPool<RiqEntity>.Get();
                RiqEntity seekEntity = Beatmap.Entities[currentPreSequence];

                foreach (RiqEntity entity in Beatmap.Entities)
                {
                    if (entity.beat == seekEntity.beat)
                    {
                        entitiesAtSameBeat.Add(entity);
                    }
                }
                SortEventsByPriority(entitiesAtSameBeat);

                float seekTime = eventCaller.GetGameAction(
                    eventCaller.GetMinigame(seekEntity.datamodel.Split(0)), seekEntity.datamodel.Split(1)).preFunctionLength;

                if (start + seekTime >= eventBeats[currentPreSequence])
                {
                    foreach (RiqEntity entity in entitiesAtSameBeat)
                    {
                        currentPreSequence++;
                        string gameName = entity.datamodel.Split('/')[0];
                        var inf = GetGameInfo(gameName);
                        if (inf != null && inf.usesAssetBundle && inf.AssetsLoaded && !inf.SequencesPreloaded)
                        {
                            Debug.Log($"Preloading game {gameName}");
                            PreloadGameSequences(gameName);
                        }
                        eventCaller.CallPreEvent(entity);
                    }
                }
                ListPool<RiqEntity>.Release(entitiesAtSameBeat);
            }
        }

        private void Update()
        {
            if (BeatmapEntities() < 1) //bruh really you forgot to ckeck tempo changes
                return;
            if (!Conductor.instance.isPlaying)
                return;
            Conductor cond = Conductor.instance;

            if (currentTempoEvent < Beatmap.TempoChanges.Count && currentTempoEvent >= 0)
            {
                if (cond.songPositionInBeatsAsDouble >= tempoBeats[currentTempoEvent])
                {
                    cond.SetBpm(Beatmap.TempoChanges[currentTempoEvent]["tempo"]);
                    currentTempoEvent++;
                }
            }

            if (currentVolumeEvent < Beatmap.VolumeChanges.Count && currentVolumeEvent >= 0)
            {
                if (cond.songPositionInBeatsAsDouble >= volumeBeats[currentVolumeEvent])
                {
                    cond.SetVolume(Beatmap.VolumeChanges[currentVolumeEvent]["volume"]);
                    currentVolumeEvent++;
                }
            }

            if (currentSectionEvent < Beatmap.SectionMarkers.Count && currentSectionEvent >= 0)
            {
                if (cond.songPositionInBeatsAsDouble >= sectionBeats[currentSectionEvent])
                {
                    Debug.Log("Section " + Beatmap.SectionMarkers[currentSectionEvent]["sectionName"] + " started");
                    currentSection = Beatmap.SectionMarkers[currentSectionEvent];
                    currentSectionEvent++;
                    if (currentSectionEvent < Beatmap.SectionMarkers.Count)
                        nextSection = Beatmap.SectionMarkers[currentSectionEvent];
                    else
                        nextSection = default(RiqEntity);
                    onSectionChange?.Invoke(currentSection);
                }
            }

            float seekTime = 8f;
            //seek ahead to preload games that have assetbundles
            SeekAheadAndPreload(cond.songPositionInBeatsAsDouble, seekTime);

            SeekAheadAndDoPreEvent(Conductor.instance.songPositionInBeatsAsDouble);

            if (currentEvent < Beatmap.Entities.Count && currentEvent >= 0)
            {
                if (cond.songPositionInBeatsAsDouble >= eventBeats[currentEvent])
                {
                    List<RiqEntity> entitiesAtSameBeat = ListPool<RiqEntity>.Get();
                    List<RiqEntity> fxEntities = ListPool<RiqEntity>.Get();

                    // allows for multiple events on the same beat to be executed on the same frame, so no more 1-frame delay
                    using (PooledObject<List<RiqEntity>> pool = ListPool<RiqEntity>.Get(out List<RiqEntity> currentBeatEntities))
                    {
                        currentBeatEntities = Beatmap.Entities.FindAll(c => c.beat == Beatmap.Entities[currentEvent].beat);
                        foreach (RiqEntity entity in currentBeatEntities)
                        {
                            if (EventCaller.FXOnlyGames().Contains(eventCaller.GetMinigame(entity.datamodel.Split('/')[0])))
                            {
                                fxEntities.Add(entity);
                            }
                            else
                            {
                                entitiesAtSameBeat.Add(entity);
                            }
                        }
                    }

                    SortEventsByPriority(fxEntities);
                    SortEventsByPriority(entitiesAtSameBeat);

                    // FX entities should ALWAYS execute before gameplay entities
                    foreach (RiqEntity entity in fxEntities)
                    {
                        eventCaller.CallEvent(entity, true);
                        currentEvent++;
                    }

                    foreach (RiqEntity entity in entitiesAtSameBeat)
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

                    ListPool<RiqEntity>.Release(entitiesAtSameBeat);
                    ListPool<RiqEntity>.Release(fxEntities);
                }
            }

            if (currentSection == null)
            {
                sectionProgress = 0;
            }
            else
            {
                double currectSectionStart = cond.GetSongPosFromBeat(currentSection.beat);

                if (nextSection == null)
                    sectionProgress = (cond.songPosition - currectSectionStart) / (cond.GetSongPosFromBeat(endBeat) - currectSectionStart);
                else
                    sectionProgress = (cond.songPosition - currectSectionStart) / (cond.GetSongPosFromBeat(nextSection.beat) - currectSectionStart);
            }
        }

        private void LateUpdate()
        {
            OverlaysManager.instance.TogleOverlaysVisibility(Editor.Editor.instance == null || Editor.Editor.instance.fullscreen || ((PersistentDataManager.gameSettings.overlaysInEditor) && (!Editor.Editor.instance.fullscreen)) || HeavenStudio.Editor.GameSettings.InPreview);
        }

        public void ToggleInputs(bool inputs)
        {
            canInput = inputs;
        }

        #region Play Events

        public void Play(double beat, float delay = 0f)
        {
            bool paused = Conductor.instance.isPaused;
            Debug.Log("Playing at " + beat);
            canInput = true;
            if (!paused)
            {
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
                clearedSections.Clear();
            }

            StartCoroutine(PlayCo(beat, delay));
            onBeatChanged?.Invoke(beat);
        }

        private IEnumerator PlayCo(double beat, float delay = 0f)
        {
            yield return new WaitForSeconds(delay);
            bool paused = Conductor.instance.isPaused;

            Conductor.instance.Play(beat);
            if (paused)
            {
                Util.SoundByte.UnpauseOneShots();
            }
            else
            {
                Conductor.instance.SetBpm(Beatmap.TempoChanges[0]["tempo"]);
                Conductor.instance.SetVolume(Beatmap.VolumeChanges[0]["volume"]);
                Conductor.instance.firstBeatOffset = Beatmap.data.offset;
                SetCurrentEventToClosest(beat);
                KillAllSounds();
            }

            Minigame miniGame = currentGameO.GetComponent<Minigame>();
            if (miniGame != null)
                miniGame.OnPlay(beat);
        }

        public void Pause()
        {
            Conductor.instance.Pause();
            Util.SoundByte.PauseOneShots();
            canInput = false;
        }

        public void Stop(double beat, bool restart = false, float restartDelay = 0f)
        {
            Minigame miniGame = currentGameO.GetComponent<Minigame>();
            if (miniGame != null)
                miniGame.OnStop(beat);

            Conductor.instance.Stop(beat);
            SetCurrentEventToClosest(beat);
            onBeatChanged?.Invoke(beat);

            // I feel like I should standardize the names
            SkillStarManager.instance.KillStar();
            TimingAccuracyDisplay.instance.StopStarFlash();
            GoForAPerfect.instance.Disable();
            SectionMedalsManager.instance.OnRemixEnd();

            // pass this data to rating screen + stats
            Debug.Log($"== Playthrough statistics of {Beatmap["remixtitle"]} (played at {System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}) ==");
            Debug.Log($"Average input offset for playthrough: {averageInputOffset}ms");
            Debug.Log($"Accuracy for playthrough: {(PlayerAccuracy * 100): 0.00}");
            Debug.Log($"Cleared {clearedSections.FindAll(c => c).Count} sections out of {Beatmap.SectionMarkers.Count}");
            if (SkillStarManager.instance.IsCollected)
                Debug.Log($"Skill Star collected");
            else
                Debug.Log($"Skill Star not collected");
            if (GoForAPerfect.instance.perfect)
                Debug.Log($"Perfect Clear!");

            KillAllSounds();
            if (playOnStart || restart)
            {
                Play(0, restartDelay);
            }
            // when rating screen gets added playOnStart will instead move to that scene
        }

        private IEnumerator WaitReadyAndPlayCo(double beat)
        {
            WaitUntil yieldOverlays = new WaitUntil(() => OverlaysManager.OverlaysReady);
            WaitUntil yieldBeatmap = new WaitUntil(() => Beatmap != null && Beatmap.Entities.Count > 0);
            WaitUntil yieldAudio = new WaitUntil(() => AudioLoadDone || (ChartLoadError && !GlobalGameManager.IsShowingDialog));

            // wait for overlays to be ready
            yield return yieldOverlays;
            // wait for first game to be loaded
            yield return yieldBeatmap;
            //wait for audio clip to be loaded
            yield return yieldAudio;

            SkillStarManager.instance.KillStar();
            TimingAccuracyDisplay.instance.StopStarFlash();
            GoForAPerfect.instance.Disable();
            SectionMedalsManager.instance?.OnRemixEnd();

            GlobalGameManager.UpdateDiscordStatus(Beatmap["remixtitle"], false, true);

            Play(beat, 1f);
            yield break;
        }

        public void KillAllSounds()
        {
            Debug.Log("Killing all sounds");
            SoundObjects.Clear();
            Util.SoundByte.KillOneShots();
        }

        #endregion

        #region List Functions

        public void SortEventsList()
        {
            Beatmap.Entities.Sort((x, y) => x.beat.CompareTo(y.beat));
            Beatmap.TempoChanges.Sort((x, y) => x.beat.CompareTo(y.beat));
            Beatmap.VolumeChanges.Sort((x, y) => x.beat.CompareTo(y.beat));
            Beatmap.SectionMarkers.Sort((x, y) => x.beat.CompareTo(y.beat));

            eventBeats = Beatmap.Entities.Select(c => c.beat).ToList();
            tempoBeats = Beatmap.TempoChanges.Select(c => c.beat).ToList();
            volumeBeats = Beatmap.VolumeChanges.Select(c => c.beat).ToList();
            sectionBeats = Beatmap.SectionMarkers.Select(c => c.beat).ToList();
        }

        void SortEventsByPriority(List<RiqEntity> entities)
        {
            entities.Sort((x, y) =>
            {
                Minigames.Minigame xGame = eventCaller.GetMinigame(x.datamodel.Split(0));
                Minigames.GameAction xAction = eventCaller.GetGameAction(xGame, x.datamodel.Split(1));
                Minigames.Minigame yGame = eventCaller.GetMinigame(y.datamodel.Split(0));
                Minigames.GameAction yAction = eventCaller.GetGameAction(yGame, y.datamodel.Split(1));

                return yAction.priority.CompareTo(xAction.priority);
            });

        }

        public static double GetClosestInList(List<double> list, double compareTo)
        {
            if (list.Count > 0)
                return list.Aggregate((x, y) => Math.Abs(x - compareTo) < Math.Abs(y - compareTo) ? x : y);
            else
                return double.MinValue;
        }

        public static int GetIndexAfter(List<double> list, double compareTo)
        {
            list.Sort();
            if (list.Count > 0)
            {
                foreach (double item in list)
                {
                    if (item >= compareTo)
                    {
                        return Math.Max(list.IndexOf(item), 0);
                    }
                }
                return list.Count;
            }
            return 0;
        }

        public static int GetIndexBefore(List<double> list, double compareTo)
        {
            list.Sort();
            if (list.Count > 0)
            {
                foreach (double item in list)
                {
                    if (item >= compareTo)
                    {
                        return Math.Max(list.IndexOf(item) - 1, 0);
                    }
                }
                return list.Count - 1;
            }
            return 0;
        }

        public void SetCurrentEventToClosest(double beat)
        {
            SortEventsList();
            onBeatChanged?.Invoke(beat);
            if (Beatmap.Entities.Count > 0)
            {
                currentEvent = GetIndexAfter(eventBeats, beat);
                currentPreEvent = GetIndexAfter(eventBeats, beat);
                currentPreSequence = GetIndexAfter(eventBeats, beat);

                var gameSwitchs = Beatmap.Entities.FindAll(c => c.datamodel.Split(1) == "switchGame");

                string newGame = Beatmap.Entities[Math.Min(currentEvent, eventBeats.Count - 1)].datamodel.Split(0);

                if (gameSwitchs.Count > 0)
                {
                    int index = GetIndexBefore(gameSwitchs.Select(c => c.beat).ToList(), beat);
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
                            newGame = Beatmap.Entities[0].datamodel.Split(0);
                        }
                        else
                        {
                            if (index - 1 >= 0)
                            {
                                newGame = gameSwitchs[index - 1].datamodel.Split(2);
                            }
                            else
                            {
                                newGame = Beatmap.Entities[Beatmap.Entities.IndexOf(closestGameSwitch) - 1].datamodel.Split(0);
                            }
                        }
                    }
                    // newGame = gameSwitchs[gameSwitchs.IndexOf(gameSwitchs.Find(c => c.beat == Mathp.GetClosestInList(gameSwitchs.Select(c => c.beat).ToList(), beat)))].datamodel.Split(2);
                }

                if (!GetGameInfo(newGame).fxOnly)
                {
                    SetGame(newGame);
                }

                List<RiqEntity> allEnds = EventCaller.GetAllInGameManagerList("gameManager", new string[] { "end" });
                if (allEnds.Count > 0)
                    endBeat = allEnds.Select(c => c.beat).Min();
                else
                    endBeat = Conductor.instance.SongLengthInBeatsAsDouble();
            }
            else
            {
                SetGame("noGame");
                endBeat = Conductor.instance.SongLengthInBeatsAsDouble();
            }

            if (Beatmap.TempoChanges.Count > 0)
            {
                currentTempoEvent = 0;

                //for tempo changes, just go over all of em until the last one we pass
                for (int t = 0; t < tempoBeats.Count; t++)
                {
                    // Debug.Log("checking tempo event " + t + " against beat " + beat + "( tc beat " + tempoChanges[t] + ")");
                    if (tempoBeats[t] > beat)
                    {
                        break;
                    }
                    currentTempoEvent = t;
                }
                // Debug.Log("currentTempoEvent is now " + currentTempoEvent);
            }

            if (Beatmap.VolumeChanges.Count > 0)
            {
                currentVolumeEvent = 0;

                for (int t = 0; t < volumeBeats.Count; t++)
                {
                    if (volumeBeats[t] > beat)
                    {
                        break;
                    }
                    currentVolumeEvent = t;
                }
            }

            currentSection = default(RiqEntity);
            nextSection = default(RiqEntity);
            if (Beatmap.SectionMarkers.Count > 0)
            {
                currentSectionEvent = 0;

                for (int t = 0; t < sectionBeats.Count; t++)
                {
                    if (sectionBeats[t] > beat)
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

        public void SwitchGame(string game, double beat, bool flash)
        {
            if (game != currentGame)
            {
                if (currentGameSwitchIE != null)
                    StopCoroutine(currentGameSwitchIE);
                currentGameSwitchIE = StartCoroutine(SwitchGameIE(game, beat, flash));
            }
        }

        IEnumerator SwitchGameIE(string game, double beat, bool flash)
        {
            if (flash)
            {
                HeavenStudio.StaticCamera.instance.ToggleCanvasVisibility(false);
            }

            SetGame(game, false);

            Minigame miniGame = currentGameO.GetComponent<Minigame>();
            if (miniGame != null)
                miniGame.OnGameSwitch(beat);

            while (beat + 0.25 > Conductor.instance.songPositionInBeats)
            {
                if (!Conductor.instance.isPlaying)
                {
                    HeavenStudio.StaticCamera.instance.ToggleCanvasVisibility(true);
                    SetAmbientGlowToCurrentMinigameColor();
                    StopCoroutine(currentGameSwitchIE);
                }
                yield return null;
            }

            HeavenStudio.StaticCamera.instance.ToggleCanvasVisibility(true);
            SetAmbientGlowToCurrentMinigameColor();
        }

        private void SetGame(string game, bool useMinigameColor = true)
        {
            Destroy(currentGameO);

            currentGameO = Instantiate(GetGame(game));
            currentGameO.transform.parent = eventCaller.GamesHolder.transform;
            currentGameO.name = game;

            SetCurrentGame(game, useMinigameColor);

            ResetCamera();
        }

        public void PreloadGameSequences(string game)
        {
            var gameInfo = GetGameInfo(game);
            //load the games' sound sequences
            // TODO: this blocks the main thread, and sound sequences sould be stored in a ScriptableObject
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
                    var gameInfos = Beatmap.Entities
                        .Select(x => x.datamodel.Split(0))
                        .Select(x => GetGameInfo(x))
                        .Where(x => x != null)
                        .Where(x => !x.fxOnly)
                        .Select(x => x.LoadableName);
                    name = gameInfos.FirstOrDefault() ?? "noGame";
                }
                else
                {
                    if (gameInfo.usesAssetBundle)
                    {
                        //game is packed in an assetbundle, load from that instead
                        // this is fucked!! figure out a way to make this async
                        return gameInfo.GetCommonAssetBundle().LoadAsset<GameObject>(name);
                    }
                    name = gameInfo.LoadableName;
                }
            }
            return Resources.Load<GameObject>($"Games/{name}");
        }

        public Minigames.Minigame GetGameInfo(string name)
        {
            return eventCaller.minigames.Find(c => c.name == name);
        }

        public void SetCurrentGame(string game, bool useMinigameColor = true)
        {
            currentGame = game;
            if (GetGameInfo(currentGame) != null)
            {
                CircleCursor.InnerCircle.GetComponent<SpriteRenderer>().color = Colors.Hex2RGB(GetGameInfo(currentGame).color);
                if (useMinigameColor) HeavenStudio.StaticCamera.instance.SetAmbientGlowColour(Colors.Hex2RGB(GetGameInfo(currentGame).color), true);
                //else HeavenStudio.StaticCamera.instance.SetAmbientGlowColour(HeavenStudio.GameCamera.currentColor, false);
                else HeavenStudio.StaticCamera.instance.SetAmbientGlowColour(Color.black, false);
            }
            else
            {
                CircleCursor.InnerCircle.GetComponent<SpriteRenderer>().color = Color.white;
                //HeavenStudio.StaticCamera.instance.SetAmbientGlowColour(HeavenStudio.GameCamera.currentColor, false);
                HeavenStudio.StaticCamera.instance.SetAmbientGlowColour(Color.black, false);
            }
        }

        private void SetAmbientGlowToCurrentMinigameColor()
        {
            if (GetGameInfo(currentGame) != null)
                HeavenStudio.StaticCamera.instance.SetAmbientGlowColour(Colors.Hex2RGB(GetGameInfo(currentGame).color), true);
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