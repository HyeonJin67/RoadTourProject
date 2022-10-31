using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//안쓰는 스크립트
public class AI_BallSpawn : MonoBehaviour
{
    public GameObject red;

    public List<GameObject> redarray;

    // Start is called before the first frame update
    void Start()
    {
        redarray = new List<GameObject>();
        for (int i = 0; i < 5; i++)
        {
            RedSpawn();
        }
        InvokeRepeating("RedSpawn", 10, 1);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void RedSpawn()
    {
        float Xpos = Random.Range(-9f, 9f);
        float Zpos = Random.Range(-8f, 8f);
        if (redarray.Count <= 10)
        {
            redarray.Add((GameObject)Instantiate(red, new Vector3(Xpos, 0.5f, Zpos), Quaternion.identity));
        }
    }
}
