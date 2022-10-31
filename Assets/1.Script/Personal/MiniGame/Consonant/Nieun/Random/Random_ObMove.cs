using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//안쓰는 스크립트
public class Random_ObMove : MonoBehaviour
{
    public Rigidbody rb;

    private void Start()
    {
        StartCoroutine(MoveObject());
    }
    IEnumerator MoveObject()
    {
        rb = GetComponent<Rigidbody>(); 

        while (true)
        {
            float dir1 = Random.Range(-0.5f, 0.5f);
            float dir2 = Random.Range(-0.5f, 0.5f);

            yield return new WaitForSeconds(2);
            rb.velocity = new Vector3 (dir1, 0,dir2);
        }
    }
}
