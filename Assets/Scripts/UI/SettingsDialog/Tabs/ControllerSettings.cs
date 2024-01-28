using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

using HeavenStudio;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using static JSL;

namespace HeavenStudio.Editor
{
    public class ControllerSettings : TabsContent
    {
        [SerializeField] private TMP_Dropdown stylesDropdown;

        [SerializeField] private TMP_Text numConnectedLabel;
        [SerializeField] private TMP_Text currentControllerLabel;
        [SerializeField] private TMP_Dropdown controllersDropdown;
        [SerializeField] private GameObject pairSearchItem;
        [SerializeField] private GameObject autoSearchLabel;
        [SerializeField] private GameObject pairSearchLabel;
        [SerializeField] private GameObject pairSearchCancelBt;
        [SerializeField] private TMP_Text pairingLabel;
        [SerializeField] private List<GameObject> controllerIcons;

        [SerializeField] private Material controllerMat;

        [SerializeField] private List<GameObject> PadBindingsMenus;
        [SerializeField] private List<GameObject> BatonBindingsMenus;
        [SerializeField] private List<GameObject> TouchBindingsMenus;

        [SerializeField] private List<TMP_Text> PadBindingsTxt;
        [SerializeField] private List<TMP_Text> BatonBindingsTxt;
        [SerializeField] private List<TMP_Text> TouchBindingsTxt;

        [SerializeField] private Slider cursorSensitivitySlider;

        private bool isAutoSearching = false;
        private bool isPairSearching = false;
        private bool pairSelectLR = false;  //true = left, false = right

        private bool bindAllMode;
        private int currentBindingBt;

        private void Start()
        {
            numConnectedLabel.text = "Connected: " + PlayerInput.GetNumControllersConnected();
            currentControllerLabel.text = "Current Controller: " + PlayerInput.GetInputController(1).GetDeviceName();
            PopulateControllersDropdown();
            PopulateStylesDropdown();

            pairSearchItem.SetActive(false);

            InputController initController = PlayerInput.GetInputController(1);
            if (!initController.GetCurrentStyleSupported())
            {
                PlayerInput.CurrentControlStyle = initController.GetDefaultStyle();
                stylesDropdown.SetValueWithoutNotify((int)PlayerInput.CurrentControlStyle);
            }

            UpdateControlStyleMapping();
            ShowControllerBinds(initController);
            ShowControllerIcon(initController);
        }

        private void Update()
        {
            InputController currentController = PlayerInput.GetInputController(1);
            if (currentBindingBt >= 0)
            {
                int bt = currentController.GetLastButtonDown();
                if (bt > 0)
                {
                    InputController.ControlBindings binds = currentController.GetCurrentBindings();
                    switch (PlayerInput.CurrentControlStyle)
                    {
                        case InputController.ControlStyles.Touch:
                            binds.Touch[currentBindingBt] = bt;
                            break;
                        case InputController.ControlStyles.Baton:
                            binds.Baton[currentBindingBt] = bt;
                            break;
                        default:
                            binds.Pad[currentBindingBt] = bt;
                            break;
                    }
                    currentController.SetCurrentBindings(binds);
                    currentControllerLabel.text = "Current Controller: " + currentController.GetDeviceName();
                    ShowControllerBinds(currentController);
                    AdvanceAutoBind(currentController);
                }
                return;
            }
            else
            {
                if (isAutoSearching)
                {
                    var controllers = PlayerInput.GetInputControllers();
                    foreach (var newController in controllers)
                    {
                        if (newController.GetLastButtonDown(true) > 0)
                        {
                            autoSearchLabel.SetActive(false);
                            AssignController(newController, currentController);

                            controllersDropdown.SetValueWithoutNotify(PlayerInput.GetInputControllerId(1));
                            isAutoSearching = false;
                        }
                    }
                }
                else if (isPairSearching)
                {
                    var controllers = PlayerInput.GetInputControllers();
                    InputController.InputFeatures lrFlag = pairSelectLR ? InputController.InputFeatures.Extra_SplitControllerLeft : InputController.InputFeatures.Extra_SplitControllerRight;
                    foreach (var pairController in controllers)
                    {
                        if (pairController == currentController) continue;

                        InputController.InputFeatures features = pairController.GetFeatures();
                        if (!features.HasFlag(lrFlag)) continue;

                        if (pairController.GetLastButtonDown() > 0)
                        {
                            (PlayerInput.GetInputController(1) as InputJoyshock)?.AssignOtherHalf((InputJoyshock)pairController);
                            isPairSearching = false;
                            pairSearchLabel.SetActive(false);
                            currentControllerLabel.text = "Current Controller: " + pairController.GetDeviceName();
                            pairingLabel.text = "Joy-Con Pair Selected\nPairing Successful!";
                            ShowControllerIcon(pairController);

                            currentController.OnSelected();
                            pairController.OnSelected();
                        }
                    }
                }
            }
        }

        void AdvanceAutoBind(InputController currentController)
        {
            int pauseVal;
            Type enumType;
            switch (PlayerInput.CurrentControlStyle)
            {
                case InputController.ControlStyles.Touch:
                    enumType = typeof(InputController.ActionsTouch);
                    pauseVal = (int)InputController.ActionsTouch.Pause;
                    break;
                case InputController.ControlStyles.Baton:
                    enumType = typeof(InputController.ActionsBaton);
                    pauseVal = (int)InputController.ActionsBaton.Pause;
                    break;
                default:
                    enumType = typeof(InputController.ActionsPad);
                    pauseVal = (int)InputController.ActionsPad.Pause;
                    break;
            }

            if (bindAllMode)
            {
                currentBindingBt++;
                Debug.Log("Binding: " + currentBindingBt);
                while (currentController.GetIsActionUnbindable(currentBindingBt, PlayerInput.CurrentControlStyle) && currentBindingBt < pauseVal)
                {
                    currentBindingBt++;
                    Debug.Log("Unbindable, binding: " + currentBindingBt);
                }
                if (currentBindingBt > pauseVal)
                {
                    currentController.SaveBindings();
                    CancelBind();
                    return;
                }

                currentControllerLabel.text = $"Now Binding: {enumType.GetEnumName(currentBindingBt)}";
            }
            else
            {
                currentController.SaveBindings();
                CancelBind();
            }
        }

        void AssignController(InputController newController, InputController lastController)
        {
            Debug.Log("Assigning controller: " + newController.GetDeviceName());

            lastController.SetPlayer(null);
            newController.SetPlayer(1);

            if ((lastController as InputJoyshock) != null)
            {
                (lastController as InputJoyshock)?.UnAssignOtherHalf();
            }

            if ((newController as InputJoyshock) != null)
            {
                newController.OnSelected();
                (newController as InputJoyshock)?.UnAssignOtherHalf();
            }

            currentControllerLabel.text = "Current Controller: " + newController.GetDeviceName();

            if (!newController.GetCurrentStyleSupported())
            {
                PlayerInput.CurrentControlStyle = newController.GetDefaultStyle();
                stylesDropdown.SetValueWithoutNotify((int)PlayerInput.CurrentControlStyle);
            }

            UpdateControlStyleMapping();
            ShowControllerBinds(newController);
            ShowControllerIcon(newController);

            InputController.InputFeatures features = newController.GetFeatures();
            if (features.HasFlag(InputController.InputFeatures.Extra_SplitControllerLeft))
            {
                pairingLabel.text = "Joy-Con (L) Selected\nPress any button on Joy-Con (R) to pair.";

                pairSelectLR = !features.HasFlag(InputController.InputFeatures.Extra_SplitControllerLeft);
                pairSearchItem.SetActive(true);
                StartPairSearch();
            }
            else if (features.HasFlag(InputController.InputFeatures.Extra_SplitControllerRight))
            {
                pairingLabel.text = "Joy-Con (R) Selected\nPress any button on Joy-Con (L) to pair.";

                pairSelectLR = !features.HasFlag(InputController.InputFeatures.Extra_SplitControllerLeft);
                pairSearchItem.SetActive(true);
                StartPairSearch();
            }
            else
            {
                CancelPairSearch();
                pairSearchItem.SetActive(false);
            }
        }

        public void ControllerDropdownChange()
        {
            CancelBind();
            InputController lastController = PlayerInput.GetInputController(1);
            lastController.SaveBindings();
            InputController newController = PlayerInput.GetInputControllers()[controllersDropdown.value];

            AssignController(newController, lastController);
        }

        public void StartBindSingle(int bt)
        {
            CancelBind();
            if (PlayerInput.GetInputController(1).GetIsActionUnbindable(bt, PlayerInput.CurrentControlStyle))
            {
                return;
            }
            currentBindingBt = bt;

            switch (PlayerInput.CurrentControlStyle)
            {
                case InputController.ControlStyles.Touch:
                    currentControllerLabel.text = $"Now Binding: {(InputController.ActionsTouch)bt}";
                    break;
                case InputController.ControlStyles.Baton:
                    currentControllerLabel.text = $"Now Binding: {(InputController.ActionsBaton)bt}";
                    break;
                default:
                    currentControllerLabel.text = $"Now Binding: {(InputController.ActionsPad)bt}";
                    break;
            }
        }

        public void StartBindAll()
        {
            CancelBind();
            bindAllMode = true;
            currentBindingBt = -1;
            AdvanceAutoBind(PlayerInput.GetInputController(1));
        }

        public void CancelBind()
        {
            bindAllMode = false;
            currentBindingBt = -1;
            currentControllerLabel.text = "Current Controller: " + PlayerInput.GetInputController(1).GetDeviceName();
        }

        public void ResetBindings()
        {
            CancelBind();
            InputController controller = PlayerInput.GetInputController(1);
            controller.ResetBindings();
            ShowControllerBinds(controller);
            controller.SaveBindings();
        }

        public void StartAutoSearch()
        {
            CancelBind();
            if (!isPairSearching)
            {
                autoSearchLabel.SetActive(true);
                isAutoSearching = true;
            }
        }

        public void StartPairSearch()
        {
            CancelBind();
            if (!isAutoSearching)
            {
                pairSearchLabel.SetActive(true);
                pairSearchCancelBt.SetActive(true);
                isPairSearching = true;
            }
        }

        public void CancelPairSearch()
        {
            CancelBind();
            if (isPairSearching)
            {
                pairSearchLabel.SetActive(false);
                pairSearchCancelBt.SetActive(false);
                isPairSearching = false;
                pairingLabel.text = "Joy-Con Selected\nPairing was cancelled.";
            }
        }

        public void SearchAndConnectControllers()
        {
            int connected = PlayerInput.RefreshInputControllers();
            numConnectedLabel.text = "Connected: " + connected;
            currentControllerLabel.text = "Current Controller: " + PlayerInput.GetInputController(1).GetDeviceName();
            PopulateControllersDropdown();
        }

        public void PopulateStylesDropdown()
        {
            List<TMP_Dropdown.OptionData> dropDownData = new List<TMP_Dropdown.OptionData>();

            var enumNames = Enum.GetNames(typeof(InputController.ControlStyles)).ToList();

            stylesDropdown.ClearOptions();
            stylesDropdown.AddOptions(enumNames);
            stylesDropdown.SetValueWithoutNotify((int)PlayerInput.CurrentControlStyle);
        }

        public void PopulateControllersDropdown()
        {
            List<TMP_Dropdown.OptionData> dropDownData = new List<TMP_Dropdown.OptionData>();
            var vals = PlayerInput.GetInputControllers();
            for (int i = 0; i < vals.Count; i++)
            {
                TMP_Dropdown.OptionData optionData = new TMP_Dropdown.OptionData();
                optionData.text = vals[i].GetDeviceName();
                dropDownData.Add(optionData);
            }
            controllersDropdown.ClearOptions();
            controllersDropdown.AddOptions(dropDownData);
            controllersDropdown.SetValueWithoutNotify(PlayerInput.GetInputControllerId(1));
        }

        public void ChangeControlStyle()
        {
            CancelBind();
            PlayerInput.CurrentControlStyle = (InputController.ControlStyles)stylesDropdown.value;
        }

        public void UpdateControlStyleMapping()
        {
            switch (PlayerInput.CurrentControlStyle)
            {
                case InputController.ControlStyles.Touch:
                    PadBindingsMenus.ForEach(x => x.SetActive(false));
                    BatonBindingsMenus.ForEach(x => x.SetActive(false));
                    TouchBindingsMenus.ForEach(x => x.SetActive(true));
                    break;
                case InputController.ControlStyles.Baton:
                    PadBindingsMenus.ForEach(x => x.SetActive(false));
                    BatonBindingsMenus.ForEach(x => x.SetActive(true));
                    TouchBindingsMenus.ForEach(x => x.SetActive(false));
                    break;
                default:
                    PadBindingsMenus.ForEach(x => x.SetActive(true));
                    BatonBindingsMenus.ForEach(x => x.SetActive(false));
                    TouchBindingsMenus.ForEach(x => x.SetActive(false));
                    break;
            }

            if (PlayerInput.CurrentControlStyle == InputController.ControlStyles.Touch)
            {
                cursorSensitivitySlider.gameObject.SetActive(true);
            }
            else
            {
                cursorSensitivitySlider.gameObject.SetActive(false);
            }
        }

        public void ShowControllerBinds(InputController controller)
        {
            string[] buttons = controller.GetButtonNames();
            List<TMP_Text> bindsTxt;
            int[] binds;
            InputController.ControlBindings ctrlBinds = controller.GetCurrentBindings();

            switch (PlayerInput.CurrentControlStyle)
            {
                case InputController.ControlStyles.Touch:
                    bindsTxt = TouchBindingsTxt;
                    binds = ctrlBinds.Touch;
                    break;
                case InputController.ControlStyles.Baton:
                    bindsTxt = BatonBindingsTxt;
                    binds = ctrlBinds.Baton;
                    break;
                default:
                    bindsTxt = PadBindingsTxt;
                    binds = ctrlBinds.Pad;
                    break;
            }

            //show binds
            int ac = 0;
            foreach (int i in binds)
            {
                if (ac >= bindsTxt.Count) break;
                if (bindsTxt[ac] == null)
                {
                    ac++;
                    continue;
                }

                if (i == -1)
                {
                    bindsTxt[ac].text = "NOT BOUND";
                }
                else if (buttons[i] == null)
                {
                    bindsTxt[ac].text = "UNKNOWN";
                }
                else
                {
                    bindsTxt[ac].text = buttons[i];
                }
                ac++;
            }

            cursorSensitivitySlider.value = ctrlBinds.PointerSensitivity;
        }

        public void ShowControllerIcon(InputController controller)
        {
            string name = controller.GetDeviceName();

            //show icon
            foreach (var icon in controllerIcons)
            {
                if (icon.name == name)
                {
                    icon.SetActive(true);
                }
                else
                {
                    icon.SetActive(false);
                }
            }

            //setup material
            controller.SetMaterialProperties(controllerMat);
        }

        public void SetCursorSensitivity()
        {
            var currentController = PlayerInput.GetInputController(1);
            InputController.ControlBindings binds = currentController.GetCurrentBindings();
            binds.PointerSensitivity = cursorSensitivitySlider.value;
            currentController.SetCurrentBindings(binds);
        }

        public override void OnOpenTab()
        {
            CancelBind();
        }

        public override void OnCloseTab()
        {
            CancelBind();
        }
    }
}