using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//난이도 디버깅용
public class WaveDebugControl : MonoBehaviour
{
    SeaManager waveManager;

    private void Start()
    {
        waveManager = GetComponent<SeaManager>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)){
            if(waveManager.amplitude < 7)
                waveManager.amplitude++;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (waveManager.amplitude > 1)
                waveManager.amplitude--;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (waveManager.speed < 5)
                waveManager.speed++;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (waveManager.speed > 1)
                waveManager.speed--;
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (waveManager.length < 11)
                waveManager.length++;
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (waveManager.length > 1)
                waveManager.length--;
        }
    }
}
