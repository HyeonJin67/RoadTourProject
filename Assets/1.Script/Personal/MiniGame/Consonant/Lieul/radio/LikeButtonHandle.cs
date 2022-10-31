using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//오브젝트를 버튼처럼 마우스의 움직임에 따라 반응하게 만드는 스크립트
public class LikeButtonHandle : MonoBehaviour
{
    [Tooltip("단어별 오디오의 인덱스를 입력해주세요")]
    [SerializeField]
    int connectAudio;
    [SerializeField]
    ScoreHandler scoreCase;
    private void Start()
    {
        FindObjectOfType<LieulMissionManager>().CallObject += ColliderActive;
    }
    private void OnMouseEnter()
    {
        print("OnMouseEnter");
        GetComponent<MeshRenderer>().material.color = new Color(1, 0.8f, 0, 1);
    }
    private void OnMouseDown()
    {
        print("OnMouseDown");
        GetComponent<MeshRenderer>().material.color = new Color(0.1f, 0.8f, 0, 1);
    }
    private void OnMouseUp() //cf)OnMouseUpAsButton
    {
        print("OnMouseUp");
        GetComponent<MeshRenderer>().material.color = new Color(1, 0.8f, 0, 1);
        scoreCase.SetScore(); //ScoreCase프리팹 연결
    }
    private void OnMouseExit()
    {
        print("OnMouseExit");
        GetComponent<MeshRenderer>().material.color = new Color(0.7f, 0.8f, 0, 0.5f);
    }
    //콜라이더 활성화 함수(이벤트에 넣어주는용)
    public void ColliderActive(int num)
    {
        if(num == connectAudio) //재생된 소리에 맞는 오브젝트인지 체크
        {
            if(GetComponent<Collider>())
            {
                GetComponent<Collider>().enabled = true;
                GetComponent<MeshRenderer>().material.color = new Color(1, 0.8f, 0, 1);
            }
        }
    }
}
