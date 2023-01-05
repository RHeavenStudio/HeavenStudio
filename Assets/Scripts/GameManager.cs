using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Starpelly;
using Newtonsoft.Json;
using HeavenStudio.Games;

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
        public TextAsset txt;
        public Camera GameCamera, CursorCam, OverlayCamera;
        public GameObject GameLetterbox;
        public CircleCursor CircleCursor;
        [HideInInspector] public GameObject GamesHolder;
        public Games.Global.Flash fade;
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
        public float sectionProgress { get { 
            if (currentSection == null) return 0;
            if (nextSection == null) return (Conductor.instance.songPositionInBeats - currentSection.beat) / (endBeat - currentSection.beat); 
            return (Conductor.instance.songPositionInBeats - currentSection.beat) / (nextSection.beat - currentSection.beat); 
        }}

        public event Action<float> onBeatChanged;
        public event Action<DynamicBeatmap.ChartSection> onSectionChange;

        public int BeatmapEntities()
        {
            return Beatmap.entities.Count + Beatmap.tempoChanges.Count + Beatmap.volumeChanges.Count + Beatmap.beatmapSections.Count;
        }

        public static GameManager instance { get; private set; }
        private EventCaller eventCaller;

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

            GameObject fade = new GameObject();
            this.fade = fade.AddComponent<Games.Global.Flash>();

            if (txt != null)
            {
                string json = txt.text;
                Beatmap = JsonConvert.DeserializeObject<DynamicBeatmap>(json);
            }
            else
            {
                NewRemix();
            }

            SortEventsList();

            GlobalGameManager.Init();

            eventCaller = this.gameObject.AddComponent<EventCaller>();
            eventCaller.GamesHolder = GamesHolder.transform;
            eventCaller.Init();
            Conductor.instance.SetBpm(Beatmap.bpm);
            Conductor.instance.SetVolume(Beatmap.musicVolume);
            Conductor.instance.firstBeatOffset = Beatmap.firstBeatOffset;

            GameObject textbox = Instantiate(Resources.Load<GameObject>("Prefabs/Common/Textbox"));
            textbox.name = "Textbox";

            if (playOnStart)
            {
                Play(startBeat);
            }

            // SetCurrentGame(eventCaller.GamesHolder.transform.GetComponentsInChildren<Transform>()[1].name);

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

        public void SeekAheadAndPreload(float start, float seekTime = 8f)
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
                        Debug.Log("ASYNC loading assetbundle for game " + gameName);
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
                    for (int i = 0; i < entitiesAtSameBeat.Count; i++)
                    {
                        string gameName = entitiesAtSameBeat[i].datamodel.Split('/')[0];
                        var inf = GetGameInfo(gameName);
                        if (inf.usesAssetBundle && !inf.AssetsLoaded) 
                        {
                            Debug.Log("ASYNC loading assetbundle for game " + gameName);
                            StartCoroutine(inf.LoadCommonAssetBundleAsync());
                            StartCoroutine(inf.LoadLocalizedAssetBundleAsync());
                        }
                    }
                    currentPreEvent++;
                }
            }
        }

        public void SeekAheadAndDoPreEvent(float start, float seekTime = 1f)
        {
            List<float> entities = Beatmap.entities.Select(c => c.beat).ToList();
            if (currentPreSequence < Beatmap.entities.Count && currentPreSequence >= 0)
            {
                if (start + seekTime >= entities[currentPreSequence])
                {
                    float beat = Beatmap.entities[currentPreSequence].beat;
                    var entitiesAtSameBeat = Beatmap.entities.FindAll(c => c.beat == Beatmap.entities[currentPreSequence].beat);
                    foreach (DynamicBeatmap.DynamicEntity entity in entitiesAtSameBeat)
                    {
                        eventCaller.CallPreEvent(entity);
                    }
                    currentPreSequence++;
                }
            }
        }

        // LateUpdate works a bit better(?) but causes some bugs (like issues with bop animations).
        private void Update()
        {
            PlayerInput.UpdateInputControllers();

            if (BeatmapEntities() < 1) //bruh really you forgot to ckeck tempo changes
                return;
            if (!Conductor.instance.isPlaying)
                return;
            

            List<float> entities = Beatmap.entities.Select(c => c.beat).ToList();

            List<float> tempoChanges = Beatmap.tempoChanges.Select(c => c.beat).ToList();
            if (currentTempoEvent < Beatmap.tempoChanges.Count && currentTempoEvent >= 0)
            {
                if (Conductor.instance.songPositionInBeats >= tempoChanges[currentTempoEvent])
                {
                    Conductor.instance.SetBpm(Beatmap.tempoChanges[currentTempoEvent].tempo);
                    currentTempoEvent++;
                }
            }

            List<float> volumeChanges = Beatmap.volumeChanges.Select(c => c.beat).ToList();
            if (currentVolumeEvent < Beatmap.volumeChanges.Count && currentVolumeEvent >= 0)
            {
                if (Conductor.instance.songPositionInBeats >= volumeChanges[currentVolumeEvent])
                {
                    Conductor.instance.SetVolume(Beatmap.volumeChanges[currentVolumeEvent].volume);
                    currentVolumeEvent++;
                }
            }

            List<float> chartSections = Beatmap.beatmapSections.Select(c => c.beat).ToList();
            if (currentSectionEvent < Beatmap.beatmapSections.Count && currentSectionEvent >= 0)
            {
                if (Conductor.instance.songPositionInBeats >= chartSections[currentSectionEvent])
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
            SeekAheadAndPreload(Conductor.instance.songPositionInBeats, seekTime);

            SeekAheadAndDoPreEvent(Conductor.instance.songPositionInBeats, 1f);

            if (currentEvent < Beatmap.entities.Count && currentEvent >= 0)
            {
                if (Conductor.instance.songPositionInBeats >= entities[currentEvent] /*&& SongPosLessThanClipLength(Conductor.instance.songPositionInBeats)*/)
                {
                    // allows for multiple events on the same beat to be executed on the same frame, so no more 1-frame delay
                    var entitiesAtSameBeat   = Beatmap.entities.FindAll(c => c.beat == Beatmap.entities[currentEvent].beat && !EventCaller.FXOnlyGames().Contains(EventCaller.instance.GetMinigame(c.datamodel.Split('/')[0])));
                    var fxEntities = Beatmap.entities.FindAll(c => c.beat == Beatmap.entities[currentEvent].beat && EventCaller.FXOnlyGames().Contains(EventCaller.instance.GetMinigame(c.datamodel.Split('/')[0])));

                    // FX entities should ALWAYS execute before gameplay entities
                    for (int i = 0; i < fxEntities.Count; i++)
                    {
                        eventCaller.CallEvent(fxEntities[i], true);
                        currentEvent++;
                    }

                    for (int i = 0; i < entitiesAtSameBeat.Count; i++)
                    {
                        var entity = entitiesAtSameBeat[i];
                        // if game isn't loaded, preload game so whatever event that would be called will still run outside if needed
                        if (entitiesAtSameBeat[i].datamodel.Split('/')[0] != currentGame && !preloadedGames.Contains(preloadedGames.Find(c => c.name == entitiesAtSameBeat[i].datamodel.Split('/')[0])))
                        {
                            eventCaller.CallEvent(entitiesAtSameBeat[i], false);
                        }
                        else
                        {
                            eventCaller.CallEvent(entitiesAtSameBeat[i], true);
                        }

                        // Thank you to @shshwdr for bring this to my attention
                        currentEvent++;
                    }

                    // currentEvent += gameManagerEntities.Count;
                }
            }


        }

        public void ToggleInputs(bool inputs)
        {
            canInput = inputs;
        }

        #region Play Events

        public void Play(float beat)
        {
            canInput = true;
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

            var instantiate = true;

            if (preloadedGames.Count > 0)
            {
                for (int i = 0; i < preloadedGames.Count; i++)
                {
                    if (preloadedGames[i].gameObject != null)
                    {
                        if (preloadedGames[i].gameObject.name == game)
                        {
                            preloadedGames[i].SetActive(true);
                            currentGameO = preloadedGames[i];
                            preloadedGames.Remove(preloadedGames[i]);
                            instantiate = false;
                        }
                    }
                }
            }

            if (instantiate)
            {
                currentGameO = Instantiate(GetGame(game));
                currentGameO.transform.parent = eventCaller.GamesHolder.transform;
                currentGameO.name = game;
            }

            SetCurrentGame(game);

            ResetCamera();
        }

        private void PreloadGame(string game)
        {
            if (preloadedGames.Contains(preloadedGames.Find(c => c.name == game)))
                return;

            var g = Instantiate(GetGame(game));
            g.transform.parent = eventCaller.GamesHolder.transform;
            g.SetActive(false);
            g.name = game;
            preloadedGames.Add(g);
        }

        public GameObject GetGame(string name)
        {
            var gameInfo = GetGameInfo(name);
            if (gameInfo != null)
            {
                if (gameInfo.fxOnly)
                {
                    name = Beatmap.entities.FindAll(c => {
                            var gameName = c.datamodel.Split(0);
                            var newGameInfo = GetGameInfo(gameName);
                            if (newGameInfo == null)
                                return false;
                            else
                                return !newGameInfo.fxOnly;
                        }).ToList()[0].datamodel.Split(0);
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