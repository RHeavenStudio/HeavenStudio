using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

namespace HeavenStudio.Games.Scripts_KarateMan
{
    // Physics in Rhythm Heaven Mania? nah im just fuckin lazy
    public class BarrelDestroyEffect : MonoBehaviour
    {
        public SpriteRenderer SpriteRenderer;
        private Rigidbody2D rb2d;
        private BoxCollider2D col;
        public int spriteIndex;
        public int index;

        public bool combo;


        public GameObject shadow;

        private void Start()
        {
            SpriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
            SpriteRenderer.sprite = KarateMan.instance.BarrelSprites[spriteIndex];

            rb2d = this.gameObject.AddComponent<Rigidbody2D>();
            rb2d.gravityScale = 11;
            rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            // rb2d.interpolation = RigidbodyInterpolation2D.Interpolate;

            float yRange = 0;
            float xRange = Random.Range(500, 800);

            switch (index)
            {
                case 0:
                    yRange = Random.Range(400, 1500);
                    break;
                case 1:
                    yRange = Random.Range(200, 700);
                    break;
                case 2:
                    yRange = Random.Range(300, 1200);
                    break;
                case 3:
                    yRange = Random.Range(300, 1200);
                    break;
                case 4:
                    yRange = Random.Range(300, 1200);
                    break;
                case 5:
                    yRange = Random.Range(300, 1200);
                    break;
                case 6:
                    yRange = Random.Range(300, 1200);
                    break;
                case 7:
                    yRange = Random.Range(500, 1600);
                    break;
            }
            if (combo)
            {
                yRange = Random.Range(800, 1600);
                xRange = Random.Range(200, 500);
            }

            rb2d.AddForce(Vector3.up * yRange);
            rb2d.AddForce(Vector3.right * xRange);
            // this.gameObject.AddComponent<Rotate>().rotateSpeed = Random.Range(60, 450);

            col = this.gameObject.AddComponent<BoxCollider2D>();

            PhysicsMaterial2D mat = new PhysicsMaterial2D();
            mat.bounciness = 0;

            col.sharedMaterial = mat;
            col.offset = new Vector2(-0.0574677f, -0.07480353f);
            col.size = new Vector2(0.5694333f, 1.912059f);

            StartCoroutine(FadeOut());

            gameObject.name = "barrel_p";
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.name != "barrel_p")
            {
                Destroy(rb2d);
                Destroy(col);
            }
            else
            {
                Physics2D.IgnoreCollision(collision.collider, col);
            }
        }

        private void Update()
        {
            if (rb2d != null)
            this.transform.eulerAngles = new Vector3(0, 0, rb2d.velocity.magnitude * 4);

            shadow.transform.localPosition = new Vector3(this.transform.localPosition.x, shadow.transform.localPosition.y);
        }

        private IEnumerator FadeOut()
        {
            yield return new WaitForSeconds(Conductor.instance.secPerBeat * 3);
            var shadowSprite = shadow.GetComponent<SpriteRenderer>();
            var fadeColor = shadowSprite.color;
            fadeColor.a = 0;
            SpriteRenderer.DOColor(new Color(1, 1, 1, 0), Conductor.instance.secPerBeat * 3).OnComplete(delegate { Destroy(this.gameObject); });
            shadowSprite.DOColor(fadeColor, Conductor.instance.secPerBeat * 3).OnComplete(delegate { Destroy(shadow); });
        }
    }
}