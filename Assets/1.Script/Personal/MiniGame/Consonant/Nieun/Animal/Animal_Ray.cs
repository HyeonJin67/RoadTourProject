using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal_Ray : MonoBehaviour
{
    //particle 위치
    [SerializeField]
    Transform dropPosition;
    //Ray를 생성한다
    Ray ray = new();
    //한번 활성화가 되었을때
    private void OnEnable()
    {
        dropPosition.gameObject.SetActive(true);
    }
    //비활성화가 되었을때
    private void OnDisable()
    {
/*        dropPosition.gameObject.SetActive(false);*/
    }
    private void Update()
    {
        //자신의 위치를 저장한다(origin : 자기자신 시작점)
        ray.origin = transform.position;
        //도착지점 위치를 저장한다(direction : 끝지점 도착점)
        ray.direction = -transform.up;
        //레이를 쏴서 맞은 지점 위치를 dropPosition 위치로 저장한다.
        if (Physics.Raycast(ray, out RaycastHit hitinfo))
        {
            dropPosition.position = hitinfo.point;
        }
    }
}
