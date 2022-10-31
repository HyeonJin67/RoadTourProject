using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
//방향키대신 공(Mover)의 자동 움직임을 담당해주는 NavMesh기능  
public class NaviMoveManager : MonoBehaviour
{
    [Tooltip ("순서대로 도착할 곳의 위치(NavMesh의 타겟)를 넣어주세요")]
    [SerializeField]
    Transform[] target; //순서대로 NavMesh의 도착지점 넣어주기

    Vector3 destination;
    NavMeshAgent agent;

    //골목 회전용
    int index;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        destination = target[0].position; //처음에 한번만 1번 타겟(도착지점)으로 NavMesh가 출발하도록 설정
    }
    void Update()
    {
        agent.SetDestination(destination); //NavMesh의 타겟(도착지점) 지정
    }
    //콜라이더를 OnTrigger로 만났을때 방향 NavMesh의 타겟(도착지점)을 다음 타겟으로 바꿔주는 프로퍼티 
    public int DestinationIndex
    {
        get => index; //타겟(Transform)들의 배열 인덱스 
        set
        {
            index = value;
            destination = target[index].position; //NavMesh의 도착지점을 타겟의 위치로 지정
        }
    }
    //퀴즈 정답 카운트 다 끝나서 0되면 다시 네브메쉬 움직이게 해주는 프로퍼티
    public bool Check
    {
        set
        {
            if(value) agent.isStopped = false;
        }
    }
    //공이 골목을 지나가면서 투명콜라이더와 만나면 네브 메쉬 스탑
    public void OnTriggerEnter(Collider other) //Collider에 Is Trigger 체크되어있어야 충돌 //통과가능(물리연산X)
    {
        if (other != null)
        {
            print("온트리거엔터" + other);
            agent.isStopped = true;
        }
    }
    //코너에서 만난 투명collider의 설정되어 있는 회전 방향으로 공도 똑같이 회전
    private void OnCollisionEnter(Collision collision) //Collider에 Is Trigger 체크안되어 있어야 충돌 //통과안됨(물리연산O)
    {
        if (collision != null)
        {
            print("온콜리전엔터" + collision.gameObject.name);
            if (index < 4)
            {
                DestinationIndex = ++index; //다음 타겟으로 인덱스값 +1해서 넘겨주기
            }
        }
    }
}
