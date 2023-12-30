using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemRenderer : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private float rotationDegreesPerSecond = 15f;
    GameObject _instance;


    // Start is called before the first frame update
    void Start()
    {
        _instance = Instantiate(prefabs[UnityEngine.Random.Range(0, prefabs.Length)], transform);
    }

    public void ChangeMem()
    {
        Debug.Log("ChangeMem");
        Destroy(_instance);
        _instance = Instantiate(prefabs[UnityEngine.Random.Range(0, prefabs.Length)], transform);
    }

    void Update()
    {
        _instance.transform.eulerAngles = new Vector3(0, _instance.transform.eulerAngles.y + (rotationDegreesPerSecond * Time.deltaTime), 0);
    }
}
