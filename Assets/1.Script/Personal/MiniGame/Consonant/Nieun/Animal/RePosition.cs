using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
public class RePosition : MonoBehaviour
{
    [SerializeField]
    Balloon_Move isMove;//BalloonMove true변수
    [SerializeField]
    string Answer;//어디에 닿았는지 확인하기 위한 변수
    [SerializeField]
    string notAnswer; //틀린정답
    [SerializeField]
    GameObject bollon;//bollon 오브젝트
    [SerializeField]
    ScoreHandler scoreCase; //미션완료시 공통 별 프리팹 
    Vector3 bollonPosi;
    Vector3 animal;//animal positon
    Vector3 perentsPosition; //ReMove()할때 지속적으로 변경된 tranceform
    SpeakerHandler speakerHandler;
    Balloon_Touch balloon_Touch; //Balloon_Touch SizeReset() 함수 불러오기
    Random_Enable random_Enable;//정답일시 다음 오브젝트 활성화 Choice()함수 불러오기
    Animal_Move animal_Move;//동물 움직임 변수 
    private void Start()
    {
        isMove = GetComponentInParent<Balloon_Move>();
        bollonPosi = bollon.transform.position;//현재풍선 위치를 저장
        animal = this.transform.position;
        perentsPosition = gameObject.transform.parent.gameObject.transform.position;//부모tranceform 저장
        speakerHandler = FindObjectOfType<SpeakerHandler>();
        balloon_Touch = this.transform.parent.GetChild(1).GetComponent<Balloon_Touch>();
        random_Enable = FindObjectOfType<Random_Enable>();
        animal_Move = GetComponent<Animal_Move>();//자기자신의 animalMove를 들고 온다
    }
    //원위치로 돌아가는 함수 
    public void ReMove()
    {
        gameObject.GetComponent<Rigidbody>().isKinematic = true;
        gameObject.GetComponent<Collider>().isTrigger = true;
        PositionReset();
        isMove.enabled = true;
        GetComponent<Collider>().enabled = false;
        bollon.transform.position = bollonPosi;//풍선 원위치 전환
        bollon.SetActive(true);//풍선 다시 활성화

    }
    //농장에 닿였을때 일어나는 상황
    private void OnTriggerEnter(Collider other)
    { 
        if(other.gameObject.layer == LayerMask.NameToLayer(Answer))
        {
            print("정답");
            random_Enable.Choice();
            scoreCase.SetScore();//별 스코어가 올라간다
            animal_Move.AnimalMove();
        }
        if(other.gameObject.name == "DefaultCollision" || other.gameObject.name == notAnswer)
        {
            print("틀림");
            //사운드
            speakerHandler.SoundByNum2(1);
            //원위치
            ReMove();
            //동물크기 리셋 
            balloon_Touch.SizeReset();         
        }
    }
    //동물 원위치로 전환
    private void PositionReset()
    {
        gameObject.transform.parent.gameObject.transform.position = perentsPosition;
        transform.position = animal;//원래 위치로 전환
    }
}

