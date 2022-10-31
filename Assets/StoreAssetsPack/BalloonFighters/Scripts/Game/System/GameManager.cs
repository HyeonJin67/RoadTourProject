using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Generic;

public class GameManager : MonoBehaviour
{	
    [SerializeField]
    StageManager _stageManager;
    void Awake()
    {
        if (_stageManager != null)
            _stageManager.Initialized();
    }
}
