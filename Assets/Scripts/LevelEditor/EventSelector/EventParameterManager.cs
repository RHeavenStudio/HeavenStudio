using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Jukebox;
using System.Linq;
using System;
using static HeavenStudio.EntityTypes;
using HeavenStudio.Common;

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
        [SerializeField] private GameObject ButtonP;
        [SerializeField] private GameObject BooleanP;
        [SerializeField] private GameObject DropdownP;
        [SerializeField] private GameObject ColorP;
        [SerializeField] private GameObject StringP;
        private static Dictionary<Type, GameObject> PropertyPrefabs;

        public RiqEntity entity;

        public bool active;

        private int childCountAtStart;

        public bool canDisable = true;

        public static EventParameterManager instance { get; set; }

        private void Awake()
        {
            instance = this;

            if (PropertyPrefabs == null) {
                PropertyPrefabs = new() {
                    { typeof(Integer), IntegerP },
                    { typeof(Float), FloatP },
                    { typeof(Dropdown), DropdownP },
                    { typeof(Button), ButtonP },
                    { typeof(Color), ColorP },
                    { typeof(bool), BooleanP },
                    { typeof(string), StringP },
                };
            }
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

                string col = (int)entity["track"] switch
                {
                    1 => EditorTheme.theme.properties.Layer2Col,
                    2 => EditorTheme.theme.properties.Layer3Col,
                    3 => EditorTheme.theme.properties.Layer4Col,
                    4 => EditorTheme.theme.properties.Layer5Col,
                    _ => EditorTheme.theme.properties.Layer1Col
                };
                Editor.instance.SetGameEventTitle($"Properties for <color=#{col}>{action.displayName}</color> on Beat {entity.beat.ToString("F2")} on <color=#{col}>Track {(int)entity["track"] + 1}</color>");

                DestroyParams();

                Dictionary<string, GameObject> ePrefabs = new();

                for (int i = 0; i < action.parameters.Count; i++)
                {
                    var p = action.parameters[i];
                    ePrefabs.Add(p.propertyName, AddParam(p.propertyName, p.parameter, p.caption, p.tooltip));
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
            Type typeType = type.GetType();
            GameObject propertyPrefab = DropdownP; // enum check is hardcoded because enums are awesome (lying)
            if (!typeType.IsEnum && !PropertyPrefabs.TryGetValue(typeType, out propertyPrefab)) {
                Debug.LogError("Can't make property interface of type: " + typeType);
                return null;
            }

            GameObject input = Instantiate(propertyPrefab, transform);
            input.SetActive(true);
            input.transform.localScale = Vector3.one;

            if (tooltip != string.Empty) {
                if (PersistentDataManager.gameSettings.showParamTooltips) {
                    Tooltip.AddTooltip(input, tooltip);
                } else {
                    Tooltip.AddTooltip(input, "", tooltip);
                }
            }
            
            EventPropertyPrefab property = input.GetComponent<EventPropertyPrefab>();
            property.SetProperties(propertyName, type, caption);

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