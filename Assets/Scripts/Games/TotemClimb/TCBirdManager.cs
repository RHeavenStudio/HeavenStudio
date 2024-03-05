using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_TotemClimb
{
    public class TCBirdManager : MonoBehaviour
    {
        [SerializeField] private float _speedX;
        [SerializeField] private float _speedY;

        [SerializeField] private Transform _birdRef;
        [SerializeField] private Transform _deathThresholdPoint;

        [SerializeField] private Sprite _penguinSprite;

        private List<Bird> _birds = new();

        private void Update()
        {
            foreach (var bird in _birds)
            {
                if (bird.birdTransform == null) continue;
                bird.birdTransform.localPosition -= new Vector3(_speedX * bird.speed * Time.deltaTime, _speedY * bird.speed * Time.deltaTime);
                if (bird.birdTransform.localPosition.x <= _deathThresholdPoint.localPosition.x)
                {
                    Destroy(bird.birdTransform.gameObject);
                }
            }

            _birds.RemoveAll(x => x.birdTransform == null);
        }

        public void AddBird(float speed, bool penguin, int amount)
        {
            amount = Mathf.Clamp(amount, 1, 3);

            Transform spawnedBird = Instantiate(_birdRef, transform);

            if (penguin)
            {
                spawnedBird.GetComponent<SpriteRenderer>().sprite = _penguinSprite;
                spawnedBird.GetChild(0).GetComponent<SpriteRenderer>().sprite = _penguinSprite;
                spawnedBird.GetChild(1).GetComponent<SpriteRenderer>().sprite = _penguinSprite;
            }
            
            spawnedBird.gameObject.SetActive(true);
            if (amount >= 2) spawnedBird.GetChild(0).gameObject.SetActive(true);
            if (amount == 3) spawnedBird.GetChild(1).gameObject.SetActive(true);

            _birds.Add(new Bird(speed, spawnedBird));
        }

        private struct Bird
        {
            public float speed;
            public Transform birdTransform;

            public Bird(float mSpeed, Transform mBirdTransform)
            {
                speed = mSpeed;
                birdTransform = mBirdTransform;
            }
        }
    }
}

