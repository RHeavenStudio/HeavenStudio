using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

using Jukebox;
using TMPro;
using SFB;

namespace HeavenStudio.Editor
{
    public class RatingScreenPropertyDialog : RemixPropertyPrefab
    {
        enum Ranks
        {
            Ng,
            Ok,
            Hi
        }

        [SerializeField] TMP_InputField headerInput;
        [SerializeField] TMP_InputField messageInput;
        [SerializeField] TMP_InputField epilogueInput;

        [SerializeField] Image imagePreview;
        [SerializeField] Image rankPreview;

        [SerializeField] Sprite catOff;
        [SerializeField] Button[] catButtons;
        [SerializeField] Sprite[] catSprites;

        [SerializeField] Sprite[] rankSprites;

        Sprite[] rankImages;

        bool initHooks;
        RemixPropertiesDialog diag;
        Ranks currentEditingRank;

        new public void InitProperties(RemixPropertiesDialog diag, string propertyName, string caption)
        {
            this.diag = diag;
            currentEditingRank = Ranks.Ok;
            rankImages = new Sprite[3];
            diag.StartCoroutine(LoadRankImages());
            UpdateInfo();

            if (!initHooks)
            {
                initHooks = true;
                headerInput.onSelect.AddListener(
                    _ =>
                        Editor.instance.editingInputField = true
                );
                headerInput.onValueChanged.AddListener(
                    _ =>
                    {
                        diag.chart["resultcaption"] = headerInput.text;
                        Editor.instance.editingInputField = false;
                    }
                );

                epilogueInput.onSelect.AddListener(
                    _ =>
                        Editor.instance.editingInputField = true
                );
                epilogueInput.onValueChanged.AddListener(
                    _ =>
                    {
                        string propSuffix = currentEditingRank switch
                        {
                            Ranks.Ng => "ng",
                            Ranks.Hi => "hi",
                            _ => "ok",
                        };
                        diag.chart["epilogue_" + propSuffix] = epilogueInput.text;
                        Editor.instance.editingInputField = false;
                    }
                );

                messageInput.onSelect.AddListener(
                    _ =>
                        Editor.instance.editingInputField = true
                );
                messageInput.onValueChanged.AddListener(DoMessageInput);
            }
        }

        void UpdateInfo()
        {
            headerInput.text = (string)diag.chart["resultcaption"];
            imagePreview.sprite = rankImages[(int)currentEditingRank];
            imagePreview.preserveAspect = true;

            rankPreview.sprite = rankSprites[(int)currentEditingRank];

            string propSuffix = currentEditingRank switch
            {
                Ranks.Ng => "ng",
                Ranks.Hi => "hi",
                _ => "ok",
            };
            epilogueInput.text = (string)diag.chart["epilogue_" + propSuffix];

            //todo: check categories
            messageInput.text = (string)diag.chart["resultcommon_" + propSuffix];
        }

        void DoMessageInput(string _)
        {
            string propSuffix = currentEditingRank switch
            {
                Ranks.Ng => "ng",
                Ranks.Hi => "hi",
                _ => "ok",
            };
            diag.chart["resultcommon_" + propSuffix] = messageInput.text;
            Editor.instance.editingInputField = false;
        }

        public void GoPrevRank()
        {
            currentEditingRank--;
            if (currentEditingRank < 0)
                currentEditingRank = Ranks.Hi;
            UpdateInfo();
        }

        public void GoNextRank()
        {
            currentEditingRank++;
            if (currentEditingRank > Ranks.Hi)
                currentEditingRank = Ranks.Ng;
            UpdateInfo();
        }

        public void UploadImage()
        {
            var extensions = new[]
            {
                new ExtensionFilter("Image Files", "png", "jpg", "jpeg" ),
            };

            StandaloneFileBrowser.OpenFilePanelAsync("Open Image", "", extensions, false, (string[] paths) =>
            {
                var path = Path.Combine(paths);
                if (path == string.Empty)
                {
                    return;
                }

                try
                {
                    // fetch the image using UnityWebRequest
                    string resource = currentEditingRank switch
                    {
                        Ranks.Ng => "Ng",
                        Ranks.Hi => "Hi",
                        _ => "Ok",
                    };
                    StartCoroutine(UploadImage(path, currentEditingRank));
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error uploading image: {e.Message}");
                    GlobalGameManager.ShowErrorMessage("Error Uploading Image", e.Message + "\n\n" + e.StackTrace);
                    return;
                }
            });
        }

        IEnumerator LoadRankImages()
        {
            for (Ranks i = 0; i <= Ranks.Hi; i++)
            {
                string resource = i switch
                {
                    Ranks.Ng => "Ng",
                    Ranks.Hi => "Hi",
                    _ => "Ok",
                };
                string path;
                try
                {
                    path = RiqFileHandler.GetResourcePath(resource, "Images/Epilogue/");
                }
                catch (System.Exception e)
                {
                    Debug.Log($"Error loading image: {e.Message}, using fallback");
                    rankImages[(int)i] = null;
                    continue;
                }

                UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + path);
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log(www.error);
                    rankImages[(int)i] = null;
                }
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(www);
                    rankImages[(int)i] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    Debug.Log("Uploaded image successfully!");
                }
            }

            imagePreview.sprite = rankImages[(int)currentEditingRank];
            imagePreview.preserveAspect = true;
        }

        IEnumerator UploadImage(string path, Ranks rank)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + path);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
                rankImages[(int)rank] = null;
                imagePreview.sprite = null;
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                rankImages[(int)rank] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                imagePreview.sprite = rankImages[(int)rank];
                imagePreview.preserveAspect = true;

                string resource = rank switch
                {
                    Ranks.Ng => "Ng",
                    Ranks.Hi => "Hi",
                    _ => "Ok",
                };

                RiqFileHandler.AddResource(path, resource, "Images/Epilogue/");
                Debug.Log("Uploaded image successfully!");
            }
        }
    }
}