using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularMotion : MonoBehaviour
{
    [SerializeField] float timeCounter = 0;
    [SerializeField] Transform rootPos;
    [SerializeField] float speed;
    [SerializeField] float width;
    [SerializeField] float height;

    private void Start()
    {
        timeCounter = 0;
    }

    private void Update()
    {
        timeCounter += Time.deltaTime * speed;
        float x = Mathf.Cos(timeCounter) * width + rootPos.position.x;
        float y = Mathf.Sin(timeCounter) * height + rootPos.position.y;
        float z = rootPos.position.z;

        transform.position = new Vector3(x, y, z);
    }
}
