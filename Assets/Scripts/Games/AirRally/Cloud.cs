using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_AirRally
{
    public class Cloud : MonoBehaviour
    {
        [SerializeField] Sprite[] sprites;
        [SerializeField] Vector3 spawnRange;
        [SerializeField] float baseSpeed = 1f;
        [SerializeField] float fadeDist = 10f;
        [SerializeField] float lifeTime = 6f;
        [SerializeField] float fadeInTime = 0.25f;

        Camera cam;
        SpriteRenderer spriteRenderer;
        float time = 0f;

        public bool isWorking = false;

        public void Init()
        {
            cam = GameCamera.GetCamera();
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(1, 1, 1, 0);
        }

        // Update is called once per frame
        void Update()
        {
            time += Time.deltaTime;
            transform.position += Vector3.forward * -baseSpeed * Time.deltaTime;

            // get distance to camera
            float dist = Vector3.Distance(cam.transform.position, transform.position);
            if (dist <= fadeDist)
            {
                spriteRenderer.color = new Color(1, 1, 1, Mathf.Clamp01(dist / fadeDist));
            }
            else if (time < fadeInTime)
            {
                spriteRenderer.color = new Color(1, 1, 1, Mathf.Clamp01(time/fadeInTime));
            }

            if (time > lifeTime)
            {
                isWorking = false;
                gameObject.SetActive(false);
                spriteRenderer.color = new Color(1, 1, 1, 0);
            }
        }

        public void StartCloud(Vector3 origin, bool prebake)
        {
            isWorking = true;
            time = 0f;
            gameObject.SetActive(true);
            spriteRenderer.sprite = sprites[Random.Range(0, sprites.Length)];
            transform.position = origin;
            transform.position += new Vector3(Random.Range(-spawnRange.x, spawnRange.x), Random.Range(-spawnRange.y, spawnRange.y), Random.Range(-spawnRange.z, spawnRange.z));
            if (prebake)
            {
                time = Random.Range(0, lifeTime);
                transform.position += Vector3.forward * -baseSpeed * time;
                float dist = Vector3.Distance(cam.transform.position, transform.position);
                spriteRenderer.color = new Color(1, 1, 1, Mathf.Clamp01(dist / fadeDist));
            }
        }
    }
}