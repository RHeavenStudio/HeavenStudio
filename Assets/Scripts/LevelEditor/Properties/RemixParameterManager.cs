using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Editor.Track;

namespace HeavenStudio.Properties
{
    public class RemixParameterManager : MonoBehaviour
    {
        [Header("General References")]
        [SerializeField] private GameObject eventSelector;

        [Header("Property Prefabs")]
        [SerializeField] private GameObject IntegerP;
        [SerializeField] private GameObject FloatP;
        [SerializeField] private GameObject BooleanP;
        [SerializeField] private GameObject DropdownP;
        [SerializeField] private GameObject ColorP;
        [SerializeField] private GameObject StringP;

        public bool active;

        public void StartParams(Beatmap.Entity entity)
        {
            active = true;
            AddParams(entity);
        }

        private void AddParams(Beatmap.Entity entity)
        {
            var minigame = EventCaller.instance.GetMinigame(Properties.instance.datamodel.Split(0));
            int actionIndex = minigame.actions.IndexOf(minigame.actions.Find(c => c.actionName == Properties.instance.datamodel.Split(1)));
            Minigames.GameAction action = minigame.actions[actionIndex];

            if (action.parameters != null)
            {

                for (int i = 0; i < action.parameters.Count; i++)
                {
                    object param = action.parameters[i].parameter;
                    string caption = action.parameters[i].propertyCaption;
                    string propertyName = action.parameters[i].propertyName;
                    string tooltip = action.parameters[i].tooltip;

                    AddParam(propertyName, param, caption);
                }

                active = true;
            }
        }

        private void AddParam(string propertyName, object type, string caption)
        {
            GameObject prefab = IntegerP;

            var objType = type.GetType();

            if (objType == typeof(EntityTypes.Integer))
            {
                prefab = IntegerP;
            }
            else if (objType == typeof(EntityTypes.Float))
            {
                prefab = FloatP;
            }
            else if(type is bool)
            {
                prefab = BooleanP;
            }
            else if (objType.IsEnum)
            {
                prefab = DropdownP;
            }
            else if (objType == typeof(Color))
            {
                prefab = ColorP;
            }
            else if(objType == typeof(string))
            {
                prefab = StringP;
            }

            GameObject input = Instantiate(prefab);
            input.transform.SetParent(this.gameObject.transform);
            input.SetActive(true);
            input.transform.localScale = Vector2.one;

            var property = input.GetComponent<PropertyDefn>();
            property.SetProperties(propertyName, type, caption);
        }
    }
}