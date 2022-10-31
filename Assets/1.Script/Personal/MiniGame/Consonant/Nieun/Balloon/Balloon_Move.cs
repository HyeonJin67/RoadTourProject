using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//안쓰는 스크립트
public class Balloon_Move : MonoBehaviour
{
    public float startTime;
    public float minX, maxX;
    public float minY, maxY;
    [Range(1,100)]
    public float speedX;
    [Range(0, 100)]
    public int speedY;


    private int sign2 = -1;
    //풍선 회전
    public float turnSpeed = 10;
    [SerializeField]
    GameObject Balloon;//풍차
    [SerializeField]
    GameObject Animal;//동물
    [SerializeField]
    int sign1;

    private void FixedUpdate()
    {
        if (Time.time >= startTime)
        {
            //이동 로직 처리.
            transform.position += new Vector3(speedX * Time.deltaTime * sign1, speedY * Time.deltaTime * sign2, 0);
            if (transform.position.x <= minX || transform.position.x >= maxX)
            {
                sign1 *= -1;//부호변경
            }
            if (transform.position.y <= minY || transform.position.y >= maxY)
            {
                sign2 *= -1;
            }
        }
        //풍선 회전 
        Balloon.transform.Rotate(new Vector3(0,turnSpeed * Time.deltaTime, 0));
        Animal.transform.Rotate(new Vector3(0, turnSpeed * Time.deltaTime, 0));
    }
}
