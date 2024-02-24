using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

namespace HeavenStudio
{
    public class DebugUI : MonoBehaviour
    {
        public GameObject Template;

        private int indexL;
        private int indexR;

        private TMP_Text Title;
        private TMP_Text SongPosBeats;
        private TMP_Text SecPerBeat;
        private TMP_Text SongPos;
        private TMP_Text BPM;

        private TMP_Text currEvent;
        private TMP_Text eventLength;
        private TMP_Text eventType;
        private TMP_Text currentGame;


        private TMP_Text graphicsDeviceName;
        private TMP_Text operatingSystem;
        private TMP_Text fps;

        private void Start()
        {
            CreateDebugUI(out Title); SetText(Title, $"Heaven Studio {GlobalGameManager.buildTime}");
            CreateDebugUI(out SongPosBeats);
            CreateDebugUI(out SongPos);
            CreateDebugUI(out SecPerBeat);
            CreateDebugUI(out BPM);

            Separate();

            CreateDebugUI(out currEvent);
            CreateDebugUI(out eventLength);
            CreateDebugUI(out eventType);
            CreateDebugUI(out currentGame);


            CreateDebugUI(out operatingSystem, true); SetText(operatingSystem, SystemInfo.operatingSystem);
            CreateDebugUI(out graphicsDeviceName, true); SetText(graphicsDeviceName, SystemInfo.graphicsDeviceName);
            CreateDebugUI(out fps, true);

            transform.GetChild(0).GetComponent<Canvas>().worldCamera = GameManager.instance.CursorCam;
        }

        private void Update()
        {
            SetText(SongPosBeats, $"Song Position In Beats: {Conductor.instance.songPositionInBeats}");
            SetText(SongPos, $"Song Position: {Conductor.instance.songPosition}");
            SetText(BPM, $"BPM: {Conductor.instance.songBpm}");
            SetText(fps, $"FPS: {1.0f / Time.smoothDeltaTime}");
            SetText(SecPerBeat, $"Seconds Per Beat: {Conductor.instance.secPerBeat}");


            SetText(currentGame, $"Current Game: {GameManager.instance.currentGame}");

            int minus = 0;

            if (GameManager.instance.Beatmap.Entities.Count > 0)
            {
                if (GameManager.instance.currentEvent - 1 >= 0) minus = 1;

                SetText(currEvent, $"CurrentEvent: {GameManager.instance.Beatmap.Entities[GameManager.instance.currentEvent - minus].datamodel}");
                SetText(eventLength, $"Event Length: {GameManager.instance.Beatmap.Entities[GameManager.instance.currentEvent - minus].length}");
            }
        }

        private void CreateDebugUI(out TMP_Text t, bool right = false)
        {
            GameObject debug = Instantiate(Template, Template.transform.parent);
            debug.SetActive(true);

            if (right)
            {
                debug.transform.localPosition = new Vector3(322.69f, Template.transform.localPosition.y - 34f * indexR);
                debug.GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Right;
                debug.transform.GetChild(0).GetComponent<TMP_Text>().alignment = TextAlignmentOptions.Right;
                indexR++;
            }
            else
            {
                debug.transform.localPosition = new Vector3(Template.transform.localPosition.x, Template.transform.localPosition.y - 34f * indexL);
                indexL++;
            }

            t = debug.transform.GetChild(0).GetComponent<TMP_Text>();

        }

        private void Separate(bool right = false)
        {
            if (right)
                indexR++;
            else
                indexL++;
        }

        private void SetText(TMP_Text t, string text)
        {
            t.transform.parent.GetComponent<TMP_Text>().text = $"<mark=#3d3d3d padding=\"44.9301, 44.9301, 44.9301, 44.9301\">{text}</mark>";
            t.text = text;
        }
    }
}