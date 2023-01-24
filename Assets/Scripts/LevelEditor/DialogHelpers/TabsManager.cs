using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Editor.Track;

using TMPro;

namespace HeavenStudio.Editor 
{
    public class TabsManager : MonoBehaviour
    {
        [SerializeField] GameObject activeContent;
        [SerializeField] public Transform contentHolder;
        [SerializeField] public Transform buttonsHolder;
        [SerializeField] private GameObject buttonPrefab;

        public void SetActiveContent(GameObject content)
        {
            if (activeContent != null)
            {
                activeContent.GetComponent<TabsContent>().OnCloseTab();
                activeContent.SetActive(false);
            }
            activeContent = content;
            activeContent.SetActive(true);
            activeContent.GetComponent<TabsContent>().OnOpenTab();
        }

        public void OpenContent()
        {
            if (activeContent != null)
            {
                activeContent.GetComponent<TabsContent>().OnOpenTab();
            }
        }

        public void CloseContent()
        {
            if (activeContent != null)
            {
                activeContent.GetComponent<TabsContent>().OnCloseTab();
            }
        }

        public List<GameObject> GenerateTabs(TabsEntry[] tabs)
        {
            List<GameObject> tabContents = new List<GameObject>();
            bool madeFirst = false;
            foreach(var tab in tabs)
            {
                var button = Instantiate(buttonPrefab, buttonsHolder);
                button.GetComponentInChildren<TMP_Text>().text = tab.name;
                var tabContent = Instantiate(tab.tabPrefab, contentHolder);
                if(!madeFirst)
                {
                    madeFirst = true;
                    SetActiveContent(tabContent);
                }
                else
                {
                    tabContent.SetActive(false);
                }
                button.GetComponent<TabButton>().Content = tabContent;
                tabContents.Add(tabContent);
            }
            return tabContents;
        }

        public void CleanTabs()
        {
            foreach(Transform child in buttonsHolder)
            {
                Destroy(child.gameObject);
            }
            foreach(Transform child in contentHolder)
            {
                child.GetComponent<TabsContent>()?.OnCloseTab();
                Destroy(child.gameObject);
            }
        }

        [Serializable]
        public struct TabsEntry
        {
            public string name;
            public GameObject tabPrefab;
        }
    }
}