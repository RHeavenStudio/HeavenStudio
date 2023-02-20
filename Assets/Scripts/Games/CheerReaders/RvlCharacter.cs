using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RvlCharacter : MonoBehaviour
{
    [Header("Objects")]
    public GameObject BaseModel;
    public Animator BaseAnim;

    public int row;
    public int col;

    private bool firstCue = true;
    private bool bookFront = false;

    // Start is called before the first frame update
    void Awake()
    {
        BaseAnim = BaseModel.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
