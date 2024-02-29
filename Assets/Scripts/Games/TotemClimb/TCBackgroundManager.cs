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
            private List<float> _objectOffsets = new();
            private float _xDistance;

            private float GetDistance()
            {
                return second.localPosition.x - first.localPosition.x;
            }

            public void InitClones(Transform parent)
            {
                _xDistance = GetDistance();
                _objects.Add(first);
                _objectOffsets.Add(first.localPosition.x);
                _objects.Add(second);
                _objectOffsets.Add(second.localPosition.x);

                for (int i = 0; i < BACKGROUND_OBJECT_AMOUNT; i++)
                {
                    Transform spawnedObject = Instantiate(first, parent);
                    spawnedObject.localPosition = new Vector3(second.localPosition.x + (_xDistance * (i + 1)), first.localPosition.y, first.localPosition.z);
                    _objects.Add(spawnedObject);
                    _objectOffsets.Add(spawnedObject.localPosition.x);
                }
            }

            public void ScrollClones()
            {
                for (int i = 0; i < _objects.Count; i++)
                {
                    var b = _objects[i];
                    var bOffset = _objectOffsets[i];
                    float songPos = Conductor.instance.songPosition;

                    b.localPosition = new Vector3(bOffset - (songPos * moveSpeed), b.localPosition.y);

                    float fullDistance = (BACKGROUND_OBJECT_AMOUNT + 2) * _xDistance;

                    if (b.localPosition.x <= -fullDistance / 2)
                    {
                        _objectOffsets[i] = bOffset + fullDistance;
                    }
                }
            }
        }
    }
}

