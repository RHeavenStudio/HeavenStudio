using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteLines : MonoBehaviour
{
    float speed;
    float startAt = 4.80f;
    float endAt = -3.1f;
    [SerializeField] SpriteRenderer line;
    public int rngEarlyGone, rngMiddleLine;
    [SerializeField] bool isRandomLineMiddle;
    bool checkAnother, checkOnce;

    void Start()
    {
        //speed = Random.Range(0.005f, 0.007f);
        speed = Random.Range(0.005f, 0.009f);
        rngEarlyGone = Random.Range(0, 5);
        if (isRandomLineMiddle)
        {
            rngMiddleLine = Random.Range(0, 101);
        }
    }

    void Update()
    {
        if(transform.position.y > endAt && !isRandomLineMiddle)
        {
            transform.position += new Vector3(0, -speed * 1f, 0);
        }
        else if(transform.position.y <= endAt && !isRandomLineMiddle)
        {
            speed = Random.Range(0.005f, 0.009f);
            transform.position = new Vector3(0, startAt, 0);
            rngEarlyGone = Random.Range(0, 5);
        }

        if(rngEarlyGone > 0 && !isRandomLineMiddle)
        {
            line.color += new Color(1f, 1f, 1f, -0.01f);
            if(line.color.a <= 0)
            {
                rngEarlyGone = Random.Range(0, 5);
                line.color = new Color(1f, 1f, 1f, .10f);
                transform.position = new Vector3(0, startAt, 0);    
            }
        }




        if (isRandomLineMiddle)
        { 
            if(rngMiddleLine > 1 && !checkAnother)
            {
                rngMiddleLine = Random.Range(0, 101);
            }
            if(rngMiddleLine <= 1)
            {
                line.color += new Color(1f, 1f, 1f, 0.01f);
                checkAnother = true;

                if(!checkOnce && line.color.a > .5f)
                {
                    checkOnce = true;
                }
            }
            if(checkOnce)
            {
                line.color -= new Color(1f, 1f, 1f, 0.02f);
                if (line.color.a <= 0)
                {
                    line.color = new Color(1f, 1f, 1f, 0f);
                    rngMiddleLine = Random.Range(0, 101);
                    transform.position = new Vector3(0, Random.Range(-1, 5));
                    checkAnother = false;
                    checkOnce = false;
                }

            }

            
        }
    }
}
