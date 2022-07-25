using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using HeavenStudio;
using HeavenStudio.Util;
using HeavenStudio.InputSystem;
using static JSL;

namespace HeavenStudio.Editor 
{
    public class ControllerSettings : MonoBehaviour
    {
        [SerializeField] private TMP_Text numConnectedLabel;
        [SerializeField] private TMP_Text currentControllerLabel;
        [SerializeField] private TMP_Dropdown controllersDropdown;
        [SerializeField] private GameObject pairSearchItem;
        [SerializeField] private GameObject autoSearchLabel;
        [SerializeField] private GameObject pairSearchLabel;
        [SerializeField] private TMP_Text pairingLabel;
        [SerializeField] private List<GameObject> controllerIcons;

        [SerializeField] private Material controllerMat;

        private bool isAutoSearching = false;
        private bool isPairSearching = false;
        private bool pairSelectLR = false;  //true = left, false = right

        private void Start() {
            numConnectedLabel.text = "Connected: " + PlayerInput.GetNumControllersConnected();
            currentControllerLabel.text = "Current Controller: " + PlayerInput.GetInputController(1).GetDeviceName();
            PopulateControllersDropdown();

            ShowControllerIcon(PlayerInput.GetInputController(1));

            controllersDropdown.onValueChanged.AddListener(delegate 
            {
                InputController lastController = PlayerInput.GetInputController(1);
                InputController newController = PlayerInput.GetInputControllers()[controllersDropdown.value];
                lastController.SetPlayer(-1);
                newController.SetPlayer(1);

                if (typeof(InputJoyshock) == lastController.GetType()) {
                    InputJoyshock con = (InputJoyshock) lastController;
                    con.UnAssignOtherHalf();
                }

                if (typeof(InputJoyshock) == newController.GetType()) {
                    InputJoyshock con = (InputJoyshock) newController;
                    StartCoroutine(SelectionVibrate(con));
                    con.UnAssignOtherHalf();
                }

                currentControllerLabel.text = "Current Controller: " + newController.GetDeviceName();
                ShowControllerIcon(newController);

                InputController.InputFeatures features = newController.GetFeatures();
                if (features.HasFlag(InputController.InputFeatures.Extra_SplitControllerLeft) || features.HasFlag(InputController.InputFeatures.Extra_SplitControllerRight))
                {
                    pairSelectLR = !features.HasFlag(InputController.InputFeatures.Extra_SplitControllerLeft);
                    pairSearchItem.SetActive(true);
                    StartPairSearch();
                }
                else
                {
                    pairSearchItem.SetActive(false);
                    CancelPairSearch();
                }

            });
        }

        private void Update() {
            if (isAutoSearching) {
                var controllers = PlayerInput.GetInputControllers();
                foreach (var controller in controllers) {
                    if (controller.GetLastButtonDown() > 0 || controller.GetLastKeyDown() > 0) {
                        InputController lastController = PlayerInput.GetInputController(1);
                        lastController.SetPlayer(-1);
                        controller.SetPlayer(1);
                        isAutoSearching = false;
                        autoSearchLabel.SetActive(false);
                        controllersDropdown.value = PlayerInput.GetInputControllerId(1);

                        if (typeof(InputJoyshock) == lastController.GetType()) {
                            ((InputJoyshock)lastController).UnAssignOtherHalf();
                        }

                        if (typeof(InputJoyshock) == controller.GetType()) {
                            InputJoyshock con = (InputJoyshock) controller;
                            StartCoroutine(SelectionVibrate(con));
                            con.UnAssignOtherHalf();
                        }

                        currentControllerLabel.text = "Current Controller: " + controller.GetDeviceName();
                        ShowControllerIcon(controller);

                        InputController.InputFeatures features = controller.GetFeatures();
                        if (features.HasFlag(InputController.InputFeatures.Extra_SplitControllerLeft) || features.HasFlag(InputController.InputFeatures.Extra_SplitControllerRight))
                        {
                            pairSelectLR = !features.HasFlag(InputController.InputFeatures.Extra_SplitControllerLeft);
                            pairSearchItem.SetActive(true);
                            StartPairSearch();
                        }
                        else
                        {
                            pairSearchItem.SetActive(false);
                            CancelPairSearch();
                        }

                    }
                }
            }
            else if (isPairSearching) {
                var controllers = PlayerInput.GetInputControllers();
                InputController.InputFeatures lrFlag = pairSelectLR ? InputController.InputFeatures.Extra_SplitControllerLeft : InputController.InputFeatures.Extra_SplitControllerRight;
                foreach (var controller in controllers) {
                    if (controller == PlayerInput.GetInputController(1)) continue;
                    InputController.InputFeatures features = controller.GetFeatures();
                    if (!features.HasFlag(lrFlag)) continue;
                    if (controller.GetLastButtonDown() > 0 || controller.GetLastKeyDown() > 0) {
                        InputJoyshock con = (InputJoyshock) PlayerInput.GetInputController(1);
                        con.AssignOtherHalf((InputJoyshock) controller);
                        isPairSearching = false;
                        pairSearchLabel.SetActive(false);
                        currentControllerLabel.text = "Current Controller: " + controller.GetDeviceName();
                        pairingLabel.text = "Joy-Con (L / R) Selected\nPairing Successful!";
                        ShowControllerIcon(controller);

                        StartCoroutine(SelectionVibrate(con));
                        StartCoroutine(SelectionVibrate((InputJoyshock) controller));
                    }
                }
            }
        }

        public void StartAutoSearch() {
            if (!isPairSearching)
            {
                autoSearchLabel.SetActive(true);
                isAutoSearching = true;
            }
        }

        public void StartPairSearch() {
            if (!isAutoSearching) {
                pairSearchLabel.SetActive(true);
                isPairSearching = true;
                pairingLabel.text = "Joy-Con (L / R) Selected\nPairing Second Joy-Con...";
            }
        }

        public void CancelPairSearch() {
            if (isPairSearching) {
                pairSearchLabel.SetActive(false);
                isPairSearching = false;
                pairingLabel.text = "Joy-Con (L / R) Selected\nPairing was cancelled.";
            }
        }

        public void SearchAndConnectControllers()
        {
            int connected = PlayerInput.InitInputControllers();
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
            controllersDropdown.AddOptions(dropDownData);
            controllersDropdown.value = 0;
        }

        public void ShowControllerIcon(InputController controller)
        {
            string name = controller.GetDeviceName();
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
                    controllerMat.SetColor("_BodyColor", BitwiseUtils.IntToRgb(JslGetControllerColour(joy.GetHandle())));
                    controllerMat.SetColor("_BtnColor", BitwiseUtils.IntToRgb(JslGetControllerButtonColour(joy.GetHandle())));
                    controllerMat.SetColor("_LGripColor", ColorUtility.TryParseHtmlString("#2F353A", out colour) ? colour : Color.white);
                    controllerMat.SetColor("_RGripColor", ColorUtility.TryParseHtmlString("#2F353A", out colour) ? colour : Color.white);
                    break;
                case "Joy-Con Pair":
                    joy = (InputJoyshock) controller;
                    int joySide = JslGetControllerSplitType(joy.GetHandle());
                    controllerMat.SetColor("_BodyColor",  BitwiseUtils.IntToRgb(joySide == SplitRight ? JslGetControllerButtonColour(joy.GetHandle()) : JslGetControllerButtonColour(joy.GetOtherHalf().GetHandle())));
                    controllerMat.SetColor("_BtnColor",   BitwiseUtils.IntToRgb(joySide == SplitLeft ? JslGetControllerButtonColour(joy.GetHandle()) : JslGetControllerButtonColour(joy.GetOtherHalf().GetHandle())));
                    controllerMat.SetColor("_LGripColor", BitwiseUtils.IntToRgb(joySide == SplitLeft ? JslGetControllerColour(joy.GetHandle()) : JslGetControllerColour(joy.GetOtherHalf().GetHandle())));
                    controllerMat.SetColor("_RGripColor", BitwiseUtils.IntToRgb(joySide == SplitRight ? JslGetControllerColour(joy.GetHandle()) : JslGetControllerColour(joy.GetOtherHalf().GetHandle())));
                    break;
                case "Pro Controller":
                    joy = (InputJoyshock) controller;
                    controllerMat.SetColor("_BodyColor", BitwiseUtils.IntToRgb(JslGetControllerColour(joy.GetHandle())));
                    controllerMat.SetColor("_BtnColor", BitwiseUtils.IntToRgb(JslGetControllerButtonColour(joy.GetHandle())));
                    controllerMat.SetColor("_LGripColor", BitwiseUtils.IntToRgb(JslGetControllerLeftGripColour(joy.GetHandle())));
                    controllerMat.SetColor("_RGripColor", BitwiseUtils.IntToRgb(JslGetControllerRightGripColour(joy.GetHandle())));
                    break;
                //TODO: dualshock 4 and dualsense lightbar colour support
                case "DualShock 4":
                    controllerMat.SetColor("_BodyColor", ColorUtility.TryParseHtmlString("#E1E2E4", out colour) ? colour : Color.white);
                    controllerMat.SetColor("_BtnColor", ColorUtility.TryParseHtmlString("#414246", out colour) ? colour : Color.white);
                    controllerMat.SetColor("_LGripColor", ColorUtility.TryParseHtmlString("#1E6EFA", out colour) ? colour : Color.white);
                    controllerMat.SetColor("_RGripColor", ColorUtility.TryParseHtmlString("#1E6EFA", out colour) ? colour : Color.white);
                    break;
                case "DualSense":
                    controllerMat.SetColor("_BodyColor", ColorUtility.TryParseHtmlString("#DEE0EB", out colour) ? colour : Color.white);
                    controllerMat.SetColor("_BtnColor", ColorUtility.TryParseHtmlString("#272D39", out colour) ? colour : Color.white);
                    controllerMat.SetColor("_LGripColor", ColorUtility.TryParseHtmlString("#1E6EFA", out colour) ? colour : Color.white);
                    controllerMat.SetColor("_RGripColor", ColorUtility.TryParseHtmlString("#1E6EFA", out colour) ? colour : Color.white);
                    break;
                default:
                    controllerMat.SetColor("_BodyColor", new Color(1, 1, 1, 1));
                    controllerMat.SetColor("_BtnColor", new Color(1, 1, 1, 1));
                    controllerMat.SetColor("_LGripColor", new Color(1, 1, 1, 1));
                    controllerMat.SetColor("_RGripColor", new Color(1, 1, 1, 1));
                    break;
            }
        }

        IEnumerator SelectionVibrate(InputJoyshock controller)
        {
            JslSetRumbleFrequency(controller.GetHandle(), 0.4f, 0.3f, 80f, 160f);
            yield return new WaitForSeconds(0.15f);
            JslSetRumbleFrequency(controller.GetHandle(), 0f, 0f, 0f, 0f);
            yield return new WaitForSeconds(0.05f);
            JslSetRumbleFrequency(controller.GetHandle(), 0.45f, 0.45f, 160f, 320f);
            yield return new WaitForSeconds(0.25f);
            JslSetRumbleFrequency(controller.GetHandle(), 0f, 0f, 0f, 0f);
        }
    }
}