using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusUI : MonoBehaviour
{
    [SerializeField]
    Text _scoreText;
    [SerializeField]
    Text _stageText;
    [SerializeField]
    Image[] _lifeImage;

    [SerializeField]
    GoToPopup _gotoPopup; 

    public void Initialize(int stage,int score, int life,Action onComplete)
    {
        SetStage(stage);
        SetScore(score);
        SetLife(life);


        if(_gotoPopup != null)
            _gotoPopup.Initialize(onComplete);
    }

    public void SetStage(int stage)
    {
        if (_stageText != null)
            _stageText.text = stage.ToString();
    }

    public void SetScore(int score)
    {
        if (_scoreText != null)
            _scoreText.text = score.ToString();
    }

    public void SetLife(int life)
    {
        if (_lifeImage != null)
        {
            for (int i = 0; i < _lifeImage.Length; i++)
            {
                if (_lifeImage[i] != null)
                {
                    if(i<life)
                        _lifeImage[i].gameObject.SetActive(true);
                    else
                        _lifeImage[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public void ShowGotoPopup()
    {
        if(_gotoPopup!=null)
            _gotoPopup.gameObject.SetActive(true);
    }

    public void HideGotoPopup()
    {
        if (_gotoPopup != null)
            _gotoPopup.gameObject.SetActive(false);
    }

}
