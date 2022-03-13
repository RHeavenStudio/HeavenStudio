using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio
{
    public class Prologue : MonoBehaviour
    {
        [SerializeField] private float waitSeconds;

        public GameObject Holder;
        public GameObject pressAny;

        bool inPrologue = false;

        private void Update()
        {
            if (Input.anyKeyDown && !inPrologue)
            {
                pressAny.SetActive(false);
                Holder.SetActive(true);
                StartCoroutine(Wait());
                inPrologue = true;
            }
        }

        IEnumerator Wait()
        {
            transform.GetChild(0).gameObject.SetActive(false);
            yield return new WaitForSeconds(1);
            transform.GetChild(0).gameObject.SetActive(true);
            yield return new WaitForSeconds(waitSeconds);
            transform.GetChild(0).gameObject.SetActive(false);
            yield return new WaitForSeconds(2);
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        }
    }

}