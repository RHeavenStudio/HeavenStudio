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
        // [SerializeField] private GameObject pairSearchLabel;
        // [SerializeField] private GameObject pairSearchCancelBt;
        // [SerializeField] private TMP_Text pairingLabel;
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
        // private bool isPairSearching = false;
        // private bool pairSelectLR = false;  //true = left, false = right

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
                else if (currentController.GetType() == typeof(InputJoyconPair))
                {
                    InputJoyconPair pair = (InputJoyconPair)currentController;
                    if (!pair.HasControllers())
                    {
                        Debug.Log("Pair searching");
                        List<InputJoyshock> controllers = PlayerInput.GetInputControllers()
                                            .Select(x => x as InputJoyshock)
                                            .Where(x => x != null && x.GetJoyshockType() is TypeJoyConLeft or TypeJoyConRight)
                                            .ToList();
                        foreach (var possibleController in controllers)
                        {
                            if (possibleController.GetLastButtonDown(true) == ButtonMaskZL && possibleController.GetJoyshockType() == TypeJoyConLeft)
                            {
                                pair.SetLeftController(possibleController);
                                pair.SetMaterialProperties(controllerMat);
                            }
                            else if (possibleController.GetLastButtonDown(true) == ButtonMaskZR && possibleController.GetJoyshockType() == TypeJoyConRight)
                            {
                                pair.SetRightController(possibleController);
                                pair.SetMaterialProperties(controllerMat);
                            }
                        }
                        if (pair.HasControllers())
                        {
                            pairSearchItem.SetActive(false);
                            pair.OnSelected();
                            pair.SetPlayer(1);
                            pair.SetMaterialProperties(controllerMat);
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

            currentControllerLabel.text = "Current Controller: " + newController.GetDeviceName();

            if (!newController.GetCurrentStyleSupported())
            {
                PlayerInput.CurrentControlStyle = newController.GetDefaultStyle();
                stylesDropdown.SetValueWithoutNotify((int)PlayerInput.CurrentControlStyle);
            }

            newController.OnSelected();

            if (newController is InputJoyconPair)
            {
                InputJoyconPair pair = (InputJoyconPair)newController;
                pair.SetLeftController(null);
                pair.SetRightController(null);
                pairSearchItem.SetActive(true);
            }
            else
            {
                pairSearchItem.SetActive(false);
            }

            UpdateControlStyleMapping();
            ShowControllerBinds(newController);
            ShowControllerIcon(newController);
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
            InputController currentController = PlayerInput.GetInputController(1);
            if (currentController.GetIsActionUnbindable(bt, PlayerInput.CurrentControlStyle))
            {
                return;
            }
            if (currentController is InputJoyconPair && !(currentController as InputJoyconPair).HasControllers())
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
            InputController currentController = PlayerInput.GetInputController(1);
            if (currentController is InputJoyconPair && !(currentController as InputJoyconPair).HasControllers())
            {
                return;
            }
            bindAllMode = true;
            currentBindingBt = -1;
            AdvanceAutoBind(currentController);
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
            autoSearchLabel.SetActive(true);
            isAutoSearching = true;
        }

        public void StartPairSearch()
        {
            CancelBind();
        }

        public void CancelPairSearch()
        {
            CancelBind();
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