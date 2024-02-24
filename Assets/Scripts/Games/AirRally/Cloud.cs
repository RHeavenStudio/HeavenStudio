using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AirRally
{
    public class Cloud : MonoBehaviour
    {
        private enum Type
        {
            Cloud,
            Snowflake,
            Tree
        }
        [SerializeField] private CloudsManager manager;
        [SerializeField] private Type type = Type.Cloud;
        [SerializeField] Sprite[] sprites;
        [SerializeField] Vector3 spawnRange;
        [SerializeField] float baseSpeed = 1f;
        [SerializeField] float fadeDist = 10f;
        [SerializeField] float lifeTime = 6f;
        [SerializeField] float fadeInTime = 0.25f;
        [SerializeField] private Animator treeAnim;
        [System.Serializable]
        public struct Weight
        {
            public int weight;
            public int type;
        }
        [SerializeField] private Weight[] weights;
        private int weightSum;

        Camera cam;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private SpriteRenderer crownRenderer;
        [SerializeField] private SpriteRenderer reflectionRenderer;
        private float reflectionOpacity = 1f;
        float time = 0f;

        public bool isWorking = false;

        public void Init()
        {
            cam = GameCamera.GetCamera();
            if (type == Type.Tree)
            {
                reflectionOpacity = reflectionRenderer.color.a;
                reflectionRenderer.color = new Color(1, 1, 1, 0);
                crownRenderer.color = new Color(1, 1, 1, 0);
            }
            spriteRenderer.color = new Color(1, 1, 1, 0);
            if (weights.Length > 0) weightSum = weights[0].weight;
        }

        // Update is called once per frame
        void Update()
        {
            time += Time.deltaTime * manager.speedMult;
            transform.position += Vector3.forward * -baseSpeed * manager.speedMult * Time.deltaTime;

            // get distance to camera
            float dist = Vector3.Distance(cam.transform.position, transform.position);
            if (dist <= fadeDist)
            {
                spriteRenderer.color = new Color(1, 1, 1, Mathf.Clamp01(dist / fadeDist));
                if (type == Type.Tree)
                {
                    crownRenderer.color = new Color(1, 1, 1, Mathf.Clamp01(dist / fadeDist));
                    reflectionRenderer.color = new Color(1, 1, 1, Mathf.Clamp(dist / fadeDist, 0, reflectionOpacity));
                }
            }
            else if (time < fadeInTime)
            {
                spriteRenderer.color = new Color(1, 1, 1, Mathf.Clamp01(time/fadeInTime));
                if (type == Type.Tree)
                {
                    crownRenderer.color = new Color(1, 1, 1, Mathf.Clamp01(time / fadeInTime));
                    reflectionRenderer.color = new Color(1, 1, 1, Mathf.Clamp(time / fadeInTime, 0, reflectionOpacity));
                }
            }

            if (time > lifeTime)
            {
                isWorking = false;
                gameObject.SetActive(false);
                spriteRenderer.color = new Color(1, 1, 1, 0);
                if (type == Type.Tree)
                {
                    reflectionRenderer.color = new Color(1, 1, 1, 0);
                    crownRenderer.color = new Color(1, 1, 1, 0);
                }
            }
        }

        public void StartCloud(Vector3 origin, bool prebake)
        {
            isWorking = true;
            time = 0f;
            gameObject.SetActive(true);
            switch (type)
            {
                case Type.Cloud:
                    spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
                    break;
                case Type.Snowflake:
                    transform.localEulerAngles = new Vector3(0, 0, Random.Range(0f, 360f));
                    break;
                case Type.Tree:
                    int randomNumber = Random.Range(0, weightSum);
                    for (int i = weights.Length - 1; i >= 0; i--)
                    {
                        if (randomNumber < weights[i].weight)
                        {
                            if (weights[i].type != 5)
                            {
                                treeAnim.Play("Tree" + weights[i].type, 0, 0);
                            }
                            else
                            {
                                treeAnim.Play("Threefish", 0, 0);
                            }
                            break;
                        }
                    }
                    break;
                default: break;
            }
            transform.position = origin;
            transform.position += new Vector3(Random.Range(-spawnRange.x, spawnRange.x), Random.Range(-spawnRange.y, spawnRange.y), Random.Range(-spawnRange.z, spawnRange.z));
            if (prebake)
            {
                time = Random.Range(0, lifeTime);
                transform.position += Vector3.forward * -baseSpeed * time;
                float dist = Vector3.Distance(cam.transform.position, transform.position);
                spriteRenderer.color = new Color(1, 1, 1, Mathf.Clamp01(dist / fadeDist));
                if (type == Type.Tree)
                {
                    crownRenderer.color = new Color(1, 1, 1, Mathf.Clamp01(dist / fadeDist));
                    reflectionRenderer.color = new Color(1, 1, 1, Mathf.Clamp(dist / fadeDist, 0, reflectionOpacity));
                }
            }
        }
    }
}