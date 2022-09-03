using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HeavenStudio.Editor 
{
    public class ChartInfoProperties : TabsContent
    {
        [Header("General References")]
        [SerializeField] private GameObject propertyHolder;

        [Header("Property Prefabs")]
        [SerializeField] private GameObject IntegerP;
        [SerializeField] private GameObject FloatP;
        [SerializeField] private GameObject BooleanP;
        [SerializeField] private GameObject DropdownP;
        [SerializeField] private GameObject ColorP;
        [SerializeField] private GameObject StringP;

        public void AddParam(RemixPropertiesDialog diag, string propertyName, object type, string caption, bool isReadOnly = false, string tooltip = "")
        {
            GameObject prefab = IntegerP;
            GameObject input;

            var objType = type.GetType();

            if (objType == typeof(EntityTypes.Integer))
            {
                prefab = IntegerP;
                input = InitPrefab(prefab, tooltip);
                var property = input.GetComponent<NumberChartPropertyPrefab>();
                property.SetProperties(diag, propertyName, type, caption);
            }
            else if (objType == typeof(EntityTypes.Float))
            {
                prefab = FloatP;
                input = InitPrefab(prefab, tooltip);
                var property = input.GetComponent<NumberChartPropertyPrefab>();
                property.SetProperties(diag, propertyName, type, caption);
            }
            else if (type is bool)
            {
                prefab = BooleanP;
                input = InitPrefab(prefab, tooltip);
                var property = input.GetComponent<BoolChartPropertyPrefab>();
                property.SetProperties(diag, propertyName, type, caption);
            }
            else if (objType.IsEnum)
            {
                prefab = DropdownP;
                input = InitPrefab(prefab, tooltip);
                var property = input.GetComponent<EnumChartPropertyPrefab>();
                property.SetProperties(diag, propertyName, type, caption);
            }
            else if (objType == typeof(Color))
            {
                prefab = ColorP;
                input = InitPrefab(prefab, tooltip);
                var property = input.GetComponent<ColorChartPropertyPrefab>();
                property.SetProperties(diag, propertyName, type, caption);
            }
            else if (objType == typeof(string))
            {
                prefab = StringP;
                input = InitPrefab(prefab, tooltip);
                var property = input.GetComponent<StringChartPropertyPrefab>();
                property.SetProperties(diag, propertyName, type, caption);
            }
            else
            {
                Debug.LogError("Can't make property interface of type: " + type.GetType());
                return;
            }
        }

        private GameObject InitPrefab(GameObject prefab, string tooltip = "")
        {
            GameObject input = Instantiate(prefab);
            input.transform.SetParent(propertyHolder.transform);
            input.SetActive(true);
            input.transform.localScale = Vector2.one;

            if(tooltip != string.Empty)
                Tooltip.AddTooltip(input, "", tooltip);

            return input;
        }

        public override void OnOpenTab()
        {
        }

        public override void OnCloseTab()
        {
            foreach (Transform child in propertyHolder.transform) {
                Destroy(child.gameObject);
            }
        }
    }
}