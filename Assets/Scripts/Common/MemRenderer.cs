using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemRenderer : MonoBehaviour
{
    private GameObject[] prefabs;
    [SerializeField] private float rotationDegreesPerSecond = 15f;
    GameObject _instance;


    // Start is called before the first frame update
    void Start()
    {
        prefabs = Resources.LoadAll<GameObject>("Sprites/UI/Mems/Prefabs");
        _instance = Instantiate(prefabs[UnityEngine.Random.Range(0, prefabs.Length)], transform);
        _instance.layer = LayerMask.NameToLayer("Mem");
    }

    public void ChangeMem()
    {
        Destroy(_instance);
        _instance = Instantiate(prefabs[UnityEngine.Random.Range(0, prefabs.Length)], transform);
        _instance.layer = LayerMask.NameToLayer("Mem");
    }

    void Update()
    {
        _instance.transform.eulerAngles = new Vector3(0, _instance.transform.eulerAngles.y + (rotationDegreesPerSecond * Time.deltaTime), 0);
    }
}
