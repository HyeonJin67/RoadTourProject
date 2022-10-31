using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mole_GameContoroller : MonoBehaviour
{
    [SerializeField]
    private CountDown countDown;
    [SerializeField]
    private MoleSpawner moleSpawner;


    private void Start()
    {
        countDown.StartCountDown(GameStart);
    }
    private void GameStart()
    {
        moleSpawner.Setup();
    }
}
