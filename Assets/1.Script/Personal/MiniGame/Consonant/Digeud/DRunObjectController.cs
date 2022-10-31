using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DRunObjectController : MonoBehaviour
{
    #region 변수
    [SerializeField]
    HangeulSpriteContainer hangeul;
    DRunDotoriHandler dotori;

    int pointIndex;
    int paramIndex;

    bool active = true;

    List<Sprite> consonant = new List<Sprite>();

    ScoreHandler scoreHandler;
    #endregion

    private void OnEnable()
    {
        hangeul.Init();
        consonant = hangeul.GetConsonant();
        scoreHandler = FindObjectOfType<ScoreHandler>();
    }

    public void SettingParam(int num)
    {
        paramIndex = num;
        transform.GetComponentInChildren<SpriteRenderer>().sprite = consonant[num];
    }


    public void Detection()
    {
        transform.GetComponent<Collider>().enabled = false;
        scoreHandler.SetScore();

        dotori = GetComponentInChildren<DRunDotoriHandler>();
        dotori?.Shotting();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<DRunNpcMoveController>() != null)
        {
            transform.GetComponentInChildren<SpriteRenderer>().enabled = true;
        }
    }


    #region 파라미터 함수
    //
    public int GetParam()
    {
        return paramIndex;
    }

    public void SetPointINdex(int num)
    {
        pointIndex = num;
    }

    public int GetPointIndex()
    {
        return pointIndex;
    }
    public bool GetActive()
    {
        return active;
    }

    public void SetActive(bool boo)
    {
        active = boo;
    }
    #endregion
}


