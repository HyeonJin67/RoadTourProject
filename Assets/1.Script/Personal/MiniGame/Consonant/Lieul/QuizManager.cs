using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Tooltip("퀴즈순서대로 장애물들을 넣어주세요")]
[System.Serializable]
public class obstacles
{
    public GameObject[] roadObstacles; //배열안에 배열: 퀴즈별 장애물
}
public class QuizManager : MonoBehaviour //퀴즈의 정답처리 관련 총괄 매니저
{
    //new()로 초기화안해주면 null에러남(public으로 하면 인스펙터창에 보여져야 하기 때문에 초기화가 한번 일어나는데 안해주면 완전 비어있어서 null에러발생)
    List<GameObject> quizObjects = new(); //활성화된 <QuizTouchHandle>스크립트가 들어있는 오브젝트를 넣었다 뺐다 할 리스트
    [SerializeField]
    obstacles[] perQuizObstacles; //퀴즈별 장애물
    int count; //몇번째 퀴즈인지 체크용

    public event System.Action<GameObject> QuizCheck; //퀴즈 맞췄을 때 발생할 이벤트

    private void Start()
    {
        QuizCheck += RemoveObject;
    }
    //퀴즈 오브젝트가 활성화되면 리스트에 오브젝트 추가 & OnMouseUp된 퀴즈의 정답처리가 끝난 오브젝트를 리스트에서 제거하게 하는 프로퍼티
    public GameObject AddNRemove
    {
        set
        {
            if (!quizObjects.Contains(value)) 
            {
                quizObjects.Add(value);
                print("정답 처리 카운트 값" + quizObjects.Count);
            }
            else
                QuizCheck?.Invoke(value);
        }
    }
    //몇번째 퀴즈인지 카운트용 프로퍼티 //퀴즈별 장애물 제거용 인덱스로 사용
    public int QuizOrder
    {
        set
        {
            count++;
            print(count + "번째 퀴즈 ");
        }
    }
    //<QuizTouchHandle>스크립트에서 OnMouseUp일때 리스트에서 오브젝트 제거해주는 함수
    //리스트에 들어갈 오브젝트가 활성화될때 이 함수를 이벤트에 추가해주기
    void RemoveObject(GameObject minus)
    {
        print("정답맞춘 오브젝트 제거");
        quizObjects.Remove(minus);
        if (quizObjects.Count == 0) //지울 오브젝트가 다 소진되면 장애물들 다 치우고 네브메쉬 다시 출발
        {
            for(int i = 0; i < perQuizObstacles[count-1].roadObstacles.Length; i++)
            {
                perQuizObstacles[count - 1].roadObstacles[i].gameObject.SetActive(false);
            }
            FindObjectOfType<NaviMoveManager>().Check = true; //다시 네브메쉬 움직이게 bool값 보내주기
        }
    }
}

