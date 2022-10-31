using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System;
public class CountDown : MonoBehaviour
{
    [Serializable]
    private class CountDownEvent : UnityEvent { }
    private CountDownEvent endOfCountDown;     //카운트 다운 종료 후 외부 메소드 실행을 위해 이벤트 클래스 사용

    private TextMeshProUGUI textCountDown;      //카운트 다운 텍스트를 출력하는 Text UI
    private AudioSource audioSource;           //카운트 다운 사운드 재생

    [SerializeField]
    private int maxFontSize;                    //폰트의 최대 크기
    [SerializeField]
    private int minFontSize;                    //폰트의 최소 크기

    private void Awake()
    {
        endOfCountDown = new CountDownEvent();
        textCountDown = GetComponent<TextMeshProUGUI>();
        audioSource = GetComponent<AudioSource>();
    }
    public void StartCountDown(UnityAction action, int start = 3, int end = 1)
    {
        StartCoroutine(OnCountDown(action, start, end));
    }
    private IEnumerator OnCountDown(UnityAction action, int start, int end)
    {
        //action 메소드를 이벤트에 등록
        endOfCountDown.AddListener(action);    

        while (start > end -1)
        {
            //카운트 다운 사운드 재생 
            audioSource.Play();
            //카운트 다운 텍스트 설정
            textCountDown.text = start.ToString();
            //폰트 크기를 변경한는 애니메이션 (재생 완료 시 아래 코드 실행)
            yield return StartCoroutine("OnFontAnimation");
            //카운트 다운 숫자 1감소
            start--;
        }
        //action 메소드를 실행 
        endOfCountDown.Invoke();
        //action 메소드를 이벤트에서 제거
        endOfCountDown.RemoveListener(action);
        //카운트 다운 오브젝트 비활성화
        gameObject.SetActive(false);
    }

    private IEnumerator OnFontAnimation()
    {
        float percent = 0;

        while(percent < 1)
        {
            percent += Time.deltaTime;
            //폰트 크기를 200에서 100까지 percent 시간 동안 감소
            textCountDown.fontSize = Mathf.Lerp(maxFontSize, minFontSize , percent);
            yield return null;
        }
    }
}
