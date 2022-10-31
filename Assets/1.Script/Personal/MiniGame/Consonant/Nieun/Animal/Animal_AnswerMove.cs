using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//안쓰는 스크립트
public class Animal_AnswerMove : MonoBehaviour
{
    [SerializeField]
    GameObject Animal;//동물
    private float startTime = 1f;
    [Range(1, 100)]
    public float speedX;
    [Range(0, 100)]
    public int speedZ;
    public float minX, maxX;
    public float minZ, maxZ;
    [SerializeField]
    int sign1;
    private int sign2 = -1;
  
    private void FixedUpdate()
    {
        
        if (Time.time >= startTime)
        {
            //이동 로직 처리
            transform.position += new Vector3(speedX * Time.deltaTime * sign1, 0, speedZ * Time.deltaTime * sign1);
            if (transform.position.x <= minX || transform.position.x >= maxX)
            {
                sign1 *= -1;//부호변경
            }
            if (transform.position.y <= minZ || transform.position.y >= maxZ)
            {
                sign2 *= -1;
            }
        }
    }
    
}
