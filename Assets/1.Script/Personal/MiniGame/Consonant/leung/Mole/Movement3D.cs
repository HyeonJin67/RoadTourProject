using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement3D : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 0;                  //이동속도
    private Vector3 moveDirection = Vector3.zero; //이동방향

    private void Update()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;//오브젝트를 이동시킨다.
    }
    public void MoveTo(Vector3 direction)       //이동방향을 설정
    {
        moveDirection = direction;
    }
}
