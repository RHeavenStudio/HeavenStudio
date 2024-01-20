using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Editor.Track;
using Jukebox;
using Jukebox.Legacy;
using System.Linq;

namespace HeavenStudio.Editor
{
    public class EventParameterManager : MonoBehaviour
    {
        [Header("General References")]
        [SerializeField] private GameObject eventSelector;
        [SerializeField] private GridGameSelector gridGameSelector;

        [Header("Property Prefabs")]
        [SerializeField] private GameObject IntegerP;
        [SerializeField] private GameObject FloatP;
        [SerializeField] private GameObject BooleanP;
        [SerializeField] private GameObject DropdownP;
        [SerializeField] private GameObject ColorP;
        [SerializeField] private GameObject StringP;

        public RiqEntity entity;

        public bool active;

        private int childCountAtStart;

        public bool canDisable = true;

        public static EventParameterManager instance { get; set; }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            childCountAtStart = transform.childCount;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (canDisable && active)
                {
                    Disable();
                }
            }
            canDisable = true;
        }

        public void Disable()
        {
            active = false;
            eventSelector.SetActive(true);

            DestroyParams();
            Editor.instance.SetGameEventTitle($"Select game event for {gridGameSelector.SelectedMinigame.displayName.Replace("\n", "")}");
        }

        public void StartParams(RiqEntity entity)
        {
            active = true;
            AddParams(entity);
        }

        static string TrackToThemeColour(int track) => track switch
        {
            1 => EditorTheme.theme.properties.Layer2Col,
            2 => EditorTheme.theme.properties.Layer3Col,
            3 => EditorTheme.theme.properties.Layer4Col,
            4 => EditorTheme.theme.properties.Layer5Col,
            _ => EditorTheme.theme.properties.Layer1Col
        };

        private void AddParams(RiqEntity entity)
        {
            string[] split = entity.datamodel.Split('/');
            var minigame = EventCaller.instance.GetMinigame(split[0]);
            int actionIndex = minigame.actions.IndexOf(minigame.actions.Find(c => c.actionName == split[1]));
            Minigames.GameAction action = minigame.actions[actionIndex];

            if (action.parameters != null)
            {
                eventSelector.SetActive(false);
                this.entity = entity;

                string col = TrackToThemeColour((int)entity["track"]);
                Editor.instance.SetGameEventTitle($"Properties for <color=#{col}>{action.displayName}</color> on Beat {entity.beat.ToString("F2")} on <color=#{col}>Track {(int)entity["track"] + 1}</color>");

                DestroyParams();

                Dictionary<string, GameObject> ePrefabs = new();

                for (int i = 0; i < action.parameters.Count; i++)
                {
                    object param = action.parameters[i].parameter;
                    string caption = action.parameters[i].propertyCaption;
                    string propertyName = action.parameters[i].propertyName;
                    string tooltip = action.parameters[i].tooltip;
                    ePrefabs.Add(propertyName, AddParam(propertyName, param, caption, tooltip));
                }

                foreach (var p in action.parameters)
                {
                    if (p.collapseParams == null || p.collapseParams.Count == 0) continue;
                    EventPropertyPrefab input = ePrefabs[p.propertyName].GetComponent<EventPropertyPrefab>();
                    foreach (var c in p.collapseParams)
                    {
                        List<GameObject> collapseables = c.collapseables.Select(x => ePrefabs[x]).ToList();
                        input.propertyCollapses.Add(new EventPropertyPrefab.PropertyCollapse(collapseables, c.CollapseOn, entity));
                    }
                    input.SetCollapses(p.parameter);
                }

                active = true;
            }
            else
            {
                active = false;
            }
        }

        private GameObject AddParam(string propertyName, object type, string caption, string tooltip = "")
        {
            GameObject prefab = IntegerP;
            GameObject input;

            var objType = type.GetType();

            if (objType == typeof(EntityTypes.Integer))
            {
                prefab = IntegerP;
                input = InitPrefab(prefab, tooltip);
                var property = input.GetComponent<NumberPropertyPrefab>();
                property.SetProperties(propertyName, type, caption);
            }
            else if (objType == typeof(EntityTypes.Float))
            {
                prefab = FloatP;
                input = InitPrefab(prefab, tooltip);
                var property = input.GetComponent<NumberPropertyPrefab>();
                property.SetProperties(propertyName, type, caption);
            }
            else if(type is bool)
            {
                prefab = BooleanP;
                input = InitPrefab(prefab, tooltip);
                var property = input.GetComponent<BoolPropertyPrefab>();
                property.SetProperties(propertyName, type, caption);
            }
            else if (objType.IsEnum)
            {
                prefab = DropdownP;
                input = InitPrefab(prefab, tooltip);
                var property = input.GetComponent<EnumPropertyPrefab>();
                property.SetProperties(propertyName, type, caption);
            }
            else if (objType == typeof(Color))
            {
                prefab = ColorP;
                input = InitPrefab(prefab, tooltip);
                var property = input.GetComponent<ColorPropertyPrefab>();
                property.SetProperties(propertyName, type, caption);
            }
            else if(objType == typeof(string))
            {
                prefab = StringP;
                input = InitPrefab(prefab, tooltip);
                var property = input.GetComponent<StringPropertyPrefab>();
                property.SetProperties(propertyName, type, caption);
            }
            else
            {
                Debug.LogError("Can't make property interface of type: " + type.GetType());
                return null;
            }
            return input;
        }

        private GameObject InitPrefab(GameObject prefab, string tooltip = "")
        {
            GameObject input = Instantiate(prefab);
            input.transform.SetParent(this.gameObject.transform);
            input.SetActive(true);
            input.transform.localScale = Vector3.one;

            if(tooltip != string.Empty)
                Tooltip.AddTooltip(input, "", tooltip);

            return input;
        }

        private void DestroyParams()
        {
            Editor.instance.editingInputField = false;
            active = false;
            for (int i = childCountAtStart; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}