using System.Collections;
using UnityEngine;
using UnityEngine.UI;

using TMPro;
using DG.Tweening;

using HeavenStudio.Util;

namespace HeavenStudio
{
    public class Rating : MonoBehaviour
    {
        public GameObject Title;
        public GameObject Desc;
        public GameObject Rank;
        public GameObject Epilogue;
        public GameObject Perfect;

        public GameObject RankingHolder;

        public GameObject Fade;

        private string rank;
        private int rankId;

        public Sprite[] epilogueSprites;
        public Image epilogueImage;

        public TMP_Text epilogueText;

        private void Start()
        {
            float score = GameProfiler.instance.score;
            TMP_Text desc = Desc.GetComponent<TMP_Text>();

            if (GameProfiler.instance.perfect)
            {
                Perfect.SetActive(true);
                Jukebox.PlayOneShot("Rankings/ranking_perfect");
                StartCoroutine(PerfectIE());
            }
            else
            {
                if (score < 59)
                {
                    // try again
                    desc.text = "Your fork technique was rather uncouth. \nYour consecutive stabs needed work.";
                    rank = "Rankings/ranking_tryagain";
                    rankId = 2;
                }
                else if (score >= 59 && score < 79)
                {
                    // ok
                    desc.text = "Eh. Good enough.";
                    rank = "Rankings/ranking_ok";
                    rankId = 1;
                }
                else if (score >= 79)
                {
                    // superb
                    desc.text = "Your fork technique was quite elegant. \nYour consecutive stabs were excellent. \nYour triple-stab technique was sublime.";
                    rank = "Rankings/ranking_superb";
                    rankId = 0;
                }

                StartCoroutine(ShowRank());
            }
        }

        private IEnumerator ShowRank()
        {
            // Title
            yield return new WaitForSeconds(0.5f);

            Jukebox.PlayOneShot("Rankings/ranking_title_show");
            Title.SetActive(true);

            // Desc
            yield return new WaitForSeconds(2f);

            Jukebox.PlayOneShot("Rankings/ranking_desc_show");
            Desc.SetActive(true);

            // Rating
            yield return new WaitForSeconds(2f);

            Jukebox.PlayOneShot(rank);
            Rank.transform.GetChild(rankId).gameObject.SetActive(true);

            // Epilogue
            yield return new WaitForSeconds(5f);
            Fade.GetComponent<Image>().DOColor(Color.black, 0.75f).OnComplete(delegate
            {
                StartCoroutine(ShowEpilogue());
            });
        }

        private IEnumerator ShowEpilogue()
        {
            epilogueImage.sprite = epilogueSprites[rankId];
            switch (rankId)
            {
                case 2:
                    epilogueText.text = "Blood sugar...so...low...";
                    break;
                case 1:
                    epilogueText.text = "I could eat two more dinners!";
                    break;
                case 0:
                    epilogueText.text = "So full! So satisfied!";
                    break;

            }

            yield return new WaitForSeconds(1);
            Fade.GetComponent<Image>().color = new Color(0, 0, 0, 0);
            RankingHolder.SetActive(false);
            Epilogue.SetActive(true);

            switch (rankId)
            {
                case 0:
                    Jukebox.PlayOneShot("Rankings/epilogue_superb");
                    break;
                case 1:
                    Jukebox.PlayOneShot("Rankings/epilogue_ok");
                    break;
                case 2:
                    Jukebox.PlayOneShot("Rankings/epilogue_tryagain");
                    break;
            }

            yield return new WaitForSeconds(8);
            GlobalGameManager.LoadScene(0);
        }

        private IEnumerator PerfectIE()
        {
            yield return new WaitForSeconds(8);
            GlobalGameManager.LoadScene(0);
        }
    }

}