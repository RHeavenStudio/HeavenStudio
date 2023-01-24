using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HeavenStudio.Editor 
{
    public class ChartInfoProperties : TabsContent
    {
        [Header("General References")]
        [SerializeField] private GameObject propertyHolder;
        RemixPropertiesDialog dialog;

        [Header("Property Prefabs")]
        [SerializeField] private GameObject IntegerP;
        [SerializeField] private GameObject FloatP;
        [SerializeField] private GameObject BooleanP;
        [SerializeField] private GameObject DropdownP;
        [SerializeField] private GameObject ColorP;
        [SerializeField] private GameObject StringP;

        [Header("Layout Prefabs")]
        [SerializeField] private GameObject DividerP;
        [SerializeField] private GameObject HeaderP;
        [SerializeField] private GameObject SubHeaderP;

        [Header("Editable Properties")]
        [SerializeField] RemixPropertiesDialog.PropertyTag[] tags;

        bool initialized = false;

        public void Init(RemixPropertiesDialog diag)
        {
            dialog = diag;
            dialog.SetupDialog(tags, this);
            initialized = true;
        }

        public void AddParam(RemixPropertiesDialog diag, string propertyName, object type, string caption, bool isReadOnly = false, string tooltip = "")
        {
            GameObject prefab = diag.IntegerP;
            GameObject input;

            var objType = type.GetType();

            if (objType == typeof(EntityTypes.Integer))
            {
                prefab = diag.IntegerP;
                input = InitPrefab(prefab, tooltip);
                var property = input.GetComponent<NumberChartPropertyPrefab>();
                property.SetProperties(diag, propertyName, type, caption);
            }
            else if (objType == typeof(EntityTypes.Float))
            {
                prefab = diag.FloatP;
                input = InitPrefab(prefab, tooltip);
                var property = input.GetComponent<NumberChartPropertyPrefab>();
                property.SetProperties(diag, propertyName, type, caption);
            }
            else if (type is bool)
            {
                prefab = diag.BooleanP;
                input = InitPrefab(prefab, tooltip);
                var property = input.GetComponent<BoolChartPropertyPrefab>();
                property.SetProperties(diag, propertyName, type, caption);
            }
            else if (objType.IsEnum)
            {
                prefab = diag.DropdownP;
                input = InitPrefab(prefab, tooltip);
                var property = input.GetComponent<EnumChartPropertyPrefab>();
                property.SetProperties(diag, propertyName, type, caption);
            }
            else if (objType == typeof(Color))
            {
                prefab = diag.ColorP;
                input = InitPrefab(prefab, tooltip);
                var property = input.GetComponent<ColorChartPropertyPrefab>();
                property.SetProperties(diag, propertyName, type, caption);
            }
            else if (objType == typeof(string))
            {
                prefab = diag.StringP;
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

        public void AddDivider(RemixPropertiesDialog diag)
        {
            InitPrefab(diag.DividerP);
        }

        public void AddHeader(RemixPropertiesDialog diag, string text)
        {
            var input = InitPrefab(diag.HeaderP);
            input.GetComponent<RemixPropertyPrefab>().InitProperties(diag, "", text);
        }

        public void AddSubHeader(RemixPropertiesDialog diag, string text)
        {
            var input = InitPrefab(diag.SubHeaderP);
            input.GetComponent<RemixPropertyPrefab>().InitProperties(diag, "", text);
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
            if (dialog != null && !initialized)
            {
                initialized = true;
                dialog.SetupDialog(tags, this);
            }
        }

        public override void OnCloseTab()
        {
            initialized = false;
            foreach (Transform child in propertyHolder.transform) {
                Destroy(child.gameObject);
            }
        }
    }
}