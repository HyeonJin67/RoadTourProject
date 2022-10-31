using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudMove : MonoBehaviour
{
    private float startTime = 1f;//Scene이 시작되고 몇초후에 움직일지 변수
    public float minX, maxX; //x축으로 이동을 어디서부터 어디까지 할지 변수 
    public float minY, maxY; //y측으로 이동을 어디서부터 어디까지 할지 변수
    public float speedX; //x축 이동 속도
    public float speedY; //y축 이동 속도

    [SerializeField]
    int sign1; //어느 방향에서 시작할지 방향 체크
    private int sign2 = -1;
    //일정한 시간만큼 구름이 움직임
    private void FixedUpdate()
    {
        if (Time.time >= startTime)
        {
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

    }
}
