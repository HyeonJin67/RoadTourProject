using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
public class GameController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textTitle;
    [SerializeField]
    private TextMeshProUGUI textTapToPlay;

    [SerializeField]
    private TextMeshProUGUI textCoinCount; //coin획득 갯수
    private int             cointCount = 0;

    public bool IsGameStart { private set; get; }

    private void Awake()
    {
        IsGameStart = false;

        textTitle.enabled = true;
        textTapToPlay.enabled = true;
        textCoinCount.enabled = false;
    }
    private IEnumerator Start()
    {
        //마우스 왼쪽 버튼을 누르기 전까지 시작하지 않고 대기
        while(true)
        {
            if(Input.GetMouseButtonDown(0))
            {
                IsGameStart = true;

                textTitle.enabled = false;
                textTapToPlay.enabled = false;
                textCoinCount.enabled = true;

                break;
            }
            yield return null;
        }
    }
    public void IncreaseCoinCount()//코인을 획득했을때 호출
    {
        cointCount++;//코인 갯수
        textCoinCount.text = cointCount.ToString();//text에 갯수 갱신
    }
    public void GameOver()
    {
        //현재 씬을 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
