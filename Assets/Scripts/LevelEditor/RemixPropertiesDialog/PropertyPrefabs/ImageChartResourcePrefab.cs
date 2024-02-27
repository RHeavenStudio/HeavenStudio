using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
using System.IO;

using SFB;
using Jukebox;
using TMPro;
using System.Collections;

namespace HeavenStudio.Editor
{
    public class ImageChartResourcePrefab : RemixPropertyPrefab
    {
        [Header("Image Resource")]
        [Space(10)]
        [SerializeField] Button uploadButton;
        [SerializeField] Image imagePreview;

        string resourcePath;
        string resourceName;

        new public void SetProperties(RemixPropertiesDialog diag, string propertyName, object type, string caption)
        {
            InitProperties(diag, propertyName, caption);

            EntityTypes.Resource resource = (EntityTypes.Resource) parameterManager.chart[propertyName];
            resourcePath = resource.path;
            resourceName = resource.name;
            if (resourcePath != null)
            {
                try
                {
                    string fsPath = RiqFileHandler.GetResourcePath(resourceName, resourcePath);
                    // fetch the image using UnityWebRequest
                    parameterManager.StartCoroutine(LoadImage(fsPath));
                }
                catch (System.IO.DirectoryNotFoundException)
                {
                    Debug.Log("image resource doesn't exist, using blank placeholder");
                    imagePreview.sprite = null;
                }
            }

            uploadButton.onClick.AddListener(
                () =>
                {
                    UploadImage();
                }
            );
        }

        private void UploadImage()
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
                    StartCoroutine(UploadImage(path));
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error uploading image: {e.Message}"); 
                    GlobalGameManager.ShowErrorMessage("Error Uploading Image", e.Message + "\n\n" + e.StackTrace);
                    return;
                }
            });
        }

        IEnumerator LoadImage(string path)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + path);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
                imagePreview.sprite = null;
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                imagePreview.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                imagePreview.preserveAspect = true;
            }
        }

        IEnumerator UploadImage(string path)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture("file://" + path);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
                imagePreview.sprite = null;
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                imagePreview.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                imagePreview.preserveAspect = true;
                RiqFileHandler.AddResource(path, resourceName, resourcePath);
                Debug.Log("Uploaded image successfully!");
            }
        }

        private void Update()
        {
        }
    }
}