using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//멈췄을 때 퀴즈를 풀기 위한 오브젝트 활성화용
public class ActiveQuizObjectHandle : MonoBehaviour
{
    [Tooltip("퀘스트별 퀴즈시작때 활성화가 필요한 오브젝트들만 다 넣어주세요")]
    [SerializeField]
    GameObject[] quizObjects; //퀘스트별 퀴즈시작시 활성화해줘야 할 오브젝트
    [Tooltip("퀴즈시작때 떨어뜨려줘야 할 오브젝트들만 다 넣어주세요")]
    [SerializeField]
    Rigidbody[] drops; //퀘스트별 퀴즈시작시 떨어뜨려줘야 할 오브젝트
    int count;
    private void OnTriggerEnter(Collider other)
    {
        FindObjectOfType<QuizManager>().QuizOrder = count; //몇번째 퀴즈인지 카운트용 프로퍼티 호출
        if (quizObjects != null)
        {
            for (int i = 0; i < quizObjects.Length; i++)
            {
                quizObjects[i].SetActive(true);
            }
        }
        if (drops != null)
        {
            for (int j = 0; j < drops.Length; j++)
            {
                drops[j].useGravity = true;
            }
        }
    }
}
