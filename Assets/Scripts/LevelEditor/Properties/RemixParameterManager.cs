using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Editor.Track;

namespace HeavenStudio.Properties
{
    public class EventParameterManager : MonoBehaviour
    {
        [Header("Property Prefabs")]
        [SerializeField] private GameObject IntegerP;
        [SerializeField] private GameObject FloatP;
        [SerializeField] private GameObject BooleanP;
        [SerializeField] private GameObject DropdownP;
        [SerializeField] private GameObject ColorP;
        [SerializeField] private GameObject StringP;


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