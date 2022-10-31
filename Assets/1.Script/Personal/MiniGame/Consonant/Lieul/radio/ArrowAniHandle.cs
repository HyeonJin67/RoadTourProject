using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//애로우전용 동선에 따라 이동하는 스크립트
public class ArrowAniHandle : MonoBehaviour
{
    Animator arrowAni;
    [SerializeField]
    GameObject[] arrowPosi;
    public event Action ButtonActive;
    
    private void Start()
    {
        arrowAni = GetComponent<Animator>();
        StartCoroutine(Speed_forArrow());
    }
    //애로우가 움직이는 모습을 지연시키면서 애니메이션 바꿔주는 함수
    IEnumerator Speed_forArrow()
    {
        yield return new WaitForSeconds(5.5f);
        arrowAni.SetInteger("ArrowAction", 1);

        for(int i = 0; i< arrowPosi.Length; i++)
        {
            transform.LookAt(arrowPosi[i].transform);
            while (Vector3.Distance(transform.position, arrowPosi[i].transform.position) > 0.1f) //둘사이의 거리가 있는 동안 //첨에 0으로 했다가 너무 느려서 10으로 바꿈
            {
                transform.position = Vector3.Lerp(transform.position, arrowPosi[i].transform.position, Time.deltaTime * 0.8f);
                
                //transform.rotation = Quaternion.Slerp(transform.rotation, arrowPosi[i].transform.rotation, Time.time);
                yield return new WaitForSeconds(Time.deltaTime);
                if (Vector3.Distance(transform.position, arrowPosi[i].transform.position) <= 0.1f)
                {
                    break;
                }
            }
            transform.position = arrowPosi[i].transform.position;
        }
        arrowAni.SetInteger("ArrowAction", 2);
        ButtonActive?.Invoke(); //애로우가 라디오에 도착하면 라디오 랜덤 재생 시작하는 이벤트 실행
        yield break;
    }
}
