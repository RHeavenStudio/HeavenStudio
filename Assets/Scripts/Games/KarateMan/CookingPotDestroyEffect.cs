using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

namespace HeavenStudio.Games.Scripts_KarateMan
{
    // Physics in Rhythm Heaven Mania? nah im just fuckin lazy
    public class CookingPotDestroyEffect : MonoBehaviour
    {
        public SpriteRenderer SpriteRenderer;
        public int spriteIndex;
        public int index;

        private float rotationSpeed;

        public GameObject pot;

        private void Start()
        {
            SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = KarateMan.instance.CookingPotSprites[1];

            Rigidbody2D rb2d = gameObject.AddComponent<Rigidbody2D>();
            rb2d.gravityScale = 5;
            rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            rb2d.AddForce(Vector3.up * Random.Range(875, 925));

            rotationSpeed = Random.Range(100, 200);

            PhysicsMaterial2D mat = new PhysicsMaterial2D();
            mat.bounciness = 0;

            StartCoroutine(FadeOut());

            gameObject.name = "cookingpot_lid";
        }

        private void Update()
        {
            transform.eulerAngles -= new Vector3(0, 0, rotationSpeed * Time.deltaTime);
            transform.position = new Vector3(pot.transform.position.x, transform.position.y, transform.position.z);
        }

        private IEnumerator FadeOut()
        {
            yield return new WaitForSeconds(Conductor.instance.secPerBeat * 3);
            Destroy(gameObject);
        }
    }
}