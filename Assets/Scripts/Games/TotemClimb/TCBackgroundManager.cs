using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeavenStudio.Games.Scripts_TotemClimb
{
    public class TCBackgroundManager : MonoBehaviour
    {
        private const int BACKGROUND_OBJECT_AMOUNT = 18;

        [SerializeField] private Transform _objectsParent;
        [SerializeField] private List<BackgroundScrollPair> _objects;

        private void Awake()
        {
            foreach (var o in _objects)
            {
                o.InitClones(_objectsParent);
            }
        }

        private void Update()
        {
            foreach (var o in _objects)
            {
                o.ScrollClones();
            }
        }

        [System.Serializable]
        private class BackgroundScrollPair
        {
            public Transform first;
            public Transform second;
            public float moveSpeed = 1.0f;

            private List<Transform> _objects = new();
            private float _xDistance;

            private float GetDistance()
            {
                return second.localPosition.x - first.localPosition.x;
            }

            public void InitClones(Transform parent)
            {
                _xDistance = GetDistance();
                _objects.Add(first);
                _objects.Add(second);

                for (int i = 0; i < BACKGROUND_OBJECT_AMOUNT; i++)
                {
                    Transform spawnedObject = Instantiate(first, parent);
                    spawnedObject.localPosition = new Vector3(second.localPosition.x + (_xDistance * (i + 1)), first.localPosition.y, first.localPosition.z);
                    _objects.Add(spawnedObject);
                }
            }

            public void ScrollClones()
            {
                foreach (var b in _objects)
                {
                    b.localPosition += new Vector3(-Time.deltaTime * moveSpeed, 0);

                    if (b.localPosition.x <= _xDistance * -BACKGROUND_OBJECT_AMOUNT / 2)
                    {
                        b.localPosition = new Vector3(_xDistance * BACKGROUND_OBJECT_AMOUNT / 2, b.localPosition.y, b.localPosition.z);
                    }
                }
            }
        }
    }
}

