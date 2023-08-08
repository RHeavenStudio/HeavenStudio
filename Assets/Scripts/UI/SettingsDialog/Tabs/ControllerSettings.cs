using System;
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

        private bool isAutoSearching = false;
        private bool isPairSearching = false;
        private bool pairSelectLR = false;  //true = left, false = right

        private bool bindAllMode;
        private int currentBindingBt;

        private void Start() {
            numConnectedLabel.text = "Connected: " + PlayerInput.GetNumControllersConnected();
            currentControllerLabel.text = "Current Controller: " + PlayerInput.GetInputController(1).GetDeviceName();
            PopulateControllersDropdown();
            
            pairSearchItem.SetActive(false);

            ShowControllerBinds(PlayerInput.GetInputController(1));
            ShowControllerIcon(PlayerInput.GetInputController(1));
        }

        private void Update() {
            InputController currentController = PlayerInput.GetInputController(1);
            if (currentBindingBt >= 0)
            {
                int bt = currentController.GetLastButtonDown();
                if (bt > 0)
                {
                    InputController.ControlBindings binds = currentController.GetCurrentBindings();
                    binds.Pad[currentBindingBt] = bt;
                    currentController.SetCurrentBindings(binds);
                    currentControllerLabel.text = "Current Controller: " + currentController.GetDeviceName();
                    ShowControllerBinds(currentController);
                    AdvanceAutoBind(currentController);
                }
                return;
            }
            else
            {
                if (isAutoSearching) {
                    var controllers = PlayerInput.GetInputControllers();
                    foreach (var newController in controllers) {
                        if (newController.GetLastButtonDown() > 0) 
                        {
                            isAutoSearching = false;
                            autoSearchLabel.SetActive(false);
                            AssignController(newController, currentController);

                            controllersDropdown.value = PlayerInput.GetInputControllerId(1);
                        }
                    }
                }
                else if (isPairSearching) {
                    var controllers = PlayerInput.GetInputControllers();
                    InputController.InputFeatures lrFlag = pairSelectLR ? InputController.InputFeatures.Extra_SplitControllerLeft : InputController.InputFeatures.Extra_SplitControllerRight;
                    foreach (var pairController in controllers) {
                        if (pairController == currentController) continue;

                        InputController.InputFeatures features = pairController.GetFeatures();
                        if (!features.HasFlag(lrFlag)) continue;

                        if (pairController.GetLastButtonDown() > 0) 
                        {
                            (PlayerInput.GetInputController(1) as InputJoyshock)?.AssignOtherHalf((InputJoyshock) pairController);
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
            if (bindAllMode)
            {
                currentBindingBt++;
                Debug.Log("Binding: " + currentBindingBt);
                while (currentController.GetIsActionUnbindable(currentBindingBt, InputController.ControlStyles.Pad) && currentBindingBt < (int)InputController.ActionsPad.Pause) 
                {
                    currentBindingBt++;
                    Debug.Log("Unbindable, binding: " + currentBindingBt);
                }
                if (currentBindingBt > (int)InputController.ActionsPad.Pause)
                {
                    currentController.SaveBindings();
                    CancelBind();
                    return;
                }
                
                currentControllerLabel.text = $"Now Binding: {(InputController.ActionsPad) currentBindingBt}";
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

            lastController.SetPlayer(-1);
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
            
            ShowControllerBinds(newController);
            ShowControllerIcon(newController);

            InputController.InputFeatures features = newController.GetFeatures();
            if (features.HasFlag(InputController.InputFeatures.Extra_SplitControllerLeft)) {
                pairingLabel.text = "Joy-Con (L) Selected\nPress any button on Joy-Con (R) to pair.";

                pairSelectLR = !features.HasFlag(InputController.InputFeatures.Extra_SplitControllerLeft);
                pairSearchItem.SetActive(true);
                StartPairSearch();
            }
            else if (features.HasFlag(InputController.InputFeatures.Extra_SplitControllerRight)) {
                pairingLabel.text = "Joy-Con (R) Selected\nPress any button on Joy-Con (L) to pair.";

                pairSelectLR = !features.HasFlag(InputController.InputFeatures.Extra_SplitControllerLeft);
                pairSearchItem.SetActive(true);
                StartPairSearch();
            }
            else {
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
            if (PlayerInput.GetInputController(1).GetIsActionUnbindable(bt, InputController.ControlStyles.Pad)) 
            {
                return;
            }
            currentBindingBt = bt;
            currentControllerLabel.text = $"Now Binding: {(InputController.ActionsPad) bt}";
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

        public void StartAutoSearch() {
            CancelBind();
            if (!isPairSearching)
            {
                autoSearchLabel.SetActive(true);
                isAutoSearching = true;
            }
        }

        public void StartPairSearch() {
            CancelBind();
            if (!isAutoSearching) {
                pairSearchLabel.SetActive(true);
                pairSearchCancelBt.SetActive(true);
                isPairSearching = true;
            }
        }

        public void CancelPairSearch() {
            CancelBind();
            if (isPairSearching) {
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
            controllersDropdown.value = PlayerInput.GetInputControllerId(1);
        }

        public void ShowControllerBinds(InputController controller)
        {
            string[] buttons = controller.GetButtonNames();

            //show binds
            int ac = 0;
            foreach (int i in controller.GetCurrentBindings().Pad)
            {
                if (ac >= PadBindingsTxt.Count) break;
                
                if (i == -1)
                {
                    PadBindingsTxt[ac].text = "NOT BOUND";
                }
                else if (buttons[i] == null)
                {
                    PadBindingsTxt[ac].text = "UNKNOWN";
                }
                else
                {
                    PadBindingsTxt[ac].text = buttons[i];
                }
                ac++;
            }
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
            Color colour;
            switch (name)
            {
                case "Keyboard":
                    controllerMat.SetColor("_BodyColor", ColorUtility.TryParseHtmlString("#F4F4F4", out colour) ? colour : Color.white);
                    break;
                case "Joy-Con (L)":
                case "Joy-Con (R)":
                    InputJoyshock joy = (InputJoyshock) controller;
                    controllerMat.SetColor("_BodyColor", joy.GetBodyColor());
                    controllerMat.SetColor("_BtnColor", joy.GetButtonColor());
                    controllerMat.SetColor("_LGripColor", ColorUtility.TryParseHtmlString("#2F353A", out colour) ? colour : Color.white);
                    controllerMat.SetColor("_RGripColor", ColorUtility.TryParseHtmlString("#2F353A", out colour) ? colour : Color.white);
                    break;
                case "Joy-Con Pair":
                    joy = (InputJoyshock) controller;
                    int joySide = JslGetControllerSplitType(joy.GetHandle());
                    controllerMat.SetColor("_BodyColor",  joySide == SplitRight ? joy.GetButtonColor() : joy.GetOtherHalf().GetButtonColor());
                    controllerMat.SetColor("_BtnColor",   joySide == SplitLeft ? joy.GetButtonColor() : joy.GetOtherHalf().GetButtonColor());
                    controllerMat.SetColor("_LGripColor", joy.GetLeftGripColor());
                    controllerMat.SetColor("_RGripColor", joy.GetRightGripColor());
                    break;
                case "Pro Controller":
                    joy = (InputJoyshock) controller;
                    controllerMat.SetColor("_BodyColor", joy.GetBodyColor());
                    controllerMat.SetColor("_BtnColor", joy.GetButtonColor());
                    controllerMat.SetColor("_LGripColor", joy.GetLeftGripColor());
                    controllerMat.SetColor("_RGripColor", joy.GetRightGripColor());
                    break;
                case "DualShock 4":
                    joy = (InputJoyshock) controller;
                    controllerMat.SetColor("_BodyColor", ColorUtility.TryParseHtmlString("#E1E2E4", out colour) ? colour : Color.white);
                    controllerMat.SetColor("_BtnColor", ColorUtility.TryParseHtmlString("#414246", out colour) ? colour : Color.white);
                    controllerMat.SetColor("_LGripColor", joy.GetLightbarColour());
                    controllerMat.SetColor("_RGripColor", joy.GetLightbarColour());
                    break;
                case "DualSense":
                    joy = (InputJoyshock) controller;
                    controllerMat.SetColor("_BodyColor", ColorUtility.TryParseHtmlString("#DEE0EB", out colour) ? colour : Color.white);
                    controllerMat.SetColor("_BtnColor", ColorUtility.TryParseHtmlString("#272D39", out colour) ? colour : Color.white);
                    controllerMat.SetColor("_LGripColor", joy.GetLightbarColour());
                    controllerMat.SetColor("_RGripColor", joy.GetLightbarColour());
                    break;
                default:
                    controllerMat.SetColor("_BodyColor", Color.white);
                    controllerMat.SetColor("_BtnColor", Color.white);
                    controllerMat.SetColor("_LGripColor", Color.white);
                    controllerMat.SetColor("_RGripColor", Color.white);
                    break;
            }
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