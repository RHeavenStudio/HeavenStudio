using UnityEngine;
using static UnityEngine.UI.Extensions.UIVerticalScroller;

namespace HeavenStudio.Editor.VideoExport
{
    public class VideoExportDialog : Dialog
    {
        [SerializeField] private VideoExport videoExport;
        [SerializeField] private GameObject MainContent, NoEndEventContent;

        public void SwitchVideoExportDialog()
        {
            var endEvent = GameManager.instance.Beatmap.entities.Find(c => c.datamodel == "gameManager/end"); // could be a better way to do this, i don't care

            if (dialog.activeSelf)
            {
                videoExport.Close();
                Editor.instance.canSelect = true;
                Editor.instance.inAuthorativeMenu = false;
                dialog.SetActive(false);
            }
            else
            {
                MainContent.gameObject.SetActive(endEvent != null);
                NoEndEventContent.gameObject.SetActive(endEvent == null);
                if (endEvent != null)
                {
                    videoExport.Open(endEvent);
                }
                else
                {

                }
                ResetAllDialogs();
                Editor.instance.canSelect = false;
                Editor.instance.inAuthorativeMenu = true;
                dialog.SetActive(true);

                // BuildDateDisplay.text = GlobalGameManager.buildTime;
            }
        }
    }
}