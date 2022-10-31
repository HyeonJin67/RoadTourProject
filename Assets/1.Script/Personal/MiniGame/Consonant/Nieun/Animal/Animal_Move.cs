using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Animal_Move : MonoBehaviour
{
    //풍선 움직임 스크립트 변수
    Balloon_Move b_Move;
    //풍선 행동 제어 
    bool ismove; 
    //풍선이 떨어지고 목표 위치 변수 
    [SerializeField]
    AnimalMovePosition animalMovePosition;
    //랜덤으로 활성화되는 동물의 번호
    Random_AnimalChoice random_AnimalChoice;
    //이벤트
    public static Action animal_move;

    private void Awake()
    {
        animal_move = () =>
        {
            AnimalMove();
        };
    }
    private void Start()
    {
        b_Move = GetComponentInParent<Balloon_Move>();
        random_AnimalChoice = GetComponent<Random_AnimalChoice>(); 
    }
    //Balloon_Move 스크립트 Off
    private void MoveOnOff()//Balloon_Move 스크립트 Off
    {
        b_Move.enabled = !ismove;
    }
    private void OnTriggerEnter(Collider other)
    {
        //기역 농장에 닿였을때
        if (other.gameObject.layer == LayerMask.NameToLayer("GiyeokAnswer"))
        {
            ismove = true;
            MoveOnOff();
            FreezeVelocity();
        }
        //니은 농장에 닿였을때 
        if (other.gameObject.layer == LayerMask.NameToLayer("NieunAnswer"))
        {
            ismove = true;
            MoveOnOff();
            FreezeVelocity();
        }
    }
    //풍선에서 떨어지는 물리적 연산 0으로 초기화
    private void FreezeVelocity()
    {
        this.gameObject.GetComponent<Collider>().isTrigger = false ;
        this.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        this.gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }
    //동물이 지정위치로 이동 
    public void AnimalMove()
    {
        StartCoroutine(MoveFunction());
    }
    //동물이 목적지로 가는 함수
    IEnumerator MoveFunction()
    {
        this.transform.LookAt(animalMovePosition.AnimalPoint[this.random_AnimalChoice.number].transform.position);
        //현재 거리와 목적지가  0.05f 이상이면 실행
        while (Vector3.Distance(transform.position, animalMovePosition.AnimalPoint[this.random_AnimalChoice.number].transform.position) > 0.5f)
        {
            //지연시키기
            yield return new WaitForSeconds(Time.deltaTime);
            
            //현재 위치에서 목적지 까지 Lerp로 이동
            transform.position = Vector3.Lerp(transform.position, animalMovePosition.AnimalPoint[this.random_AnimalChoice.number].transform.position, Time.deltaTime);
        }
        //while문이 끝나면 현재위치는 타겟위치와 같다
        transform.position = animalMovePosition.AnimalPoint[this.random_AnimalChoice.number].transform.position;
        yield break;
    }
}
