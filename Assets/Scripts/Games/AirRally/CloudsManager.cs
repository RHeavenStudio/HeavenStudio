using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AirRally
{
    public class CloudsManager : MonoBehaviour
    {
        [SerializeField] Transform cloudRoot;
        [SerializeField] GameObject cloudPrefab;
        [SerializeField] int maxCloudAmt = 32;
        [SerializeField] int cloudsToPreBake = 10;
        [SerializeField] float cloudRepeatRate = 0.1f;


        Cloud[] pool;
        float time = 0f;
        float lastTime = 0f;

        // Start is called before the first frame update
        void Start()
        {
            pool = new Cloud[maxCloudAmt];
            for (int i = 0; i < maxCloudAmt; i++)
            {
                GameObject cloudObj = Instantiate(cloudPrefab, cloudRoot);
                cloudObj.SetActive(false);
                pool[i] = cloudObj.GetComponent<Cloud>();
                pool[i].Init();
            }

            for (int i = 0; i < cloudsToPreBake; i++)
            {
                Cloud cloud = GetAvailableCloud();
                if (cloud != null)
                {
                    cloud.StartCloud(cloudRoot.position, true);
                }
            }
        }

        Cloud GetAvailableCloud()
        {
            foreach (Cloud cloud in pool)
            {
                if (!cloud.isWorking)
                {
                    return cloud;
                }
            }
            return null;
        }

        // Update is called once per frame
        void Update()
        {
            time += Time.deltaTime;
            if (time - lastTime > cloudRepeatRate)
            {
                lastTime = time;
                GetAvailableCloud()?.StartCloud(cloudRoot.position, false);
            }
        }
    }
}