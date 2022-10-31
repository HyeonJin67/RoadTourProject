using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Generic;

public class SystemInitializer : MonoBehaviour
{
    [SerializeField]
    int _taregtFPS = 60;
    static bool _isInitialized;
    void Awake()
    {
        if (_isInitialized)
        {
            Destroy(gameObject);
            return;
        }

        _isInitialized = true;
        DontDestroyOnLoad(gameObject);
        SetGraphics();

        Init();
    }
    void Init()
    {
        
    }
    void SetGraphics()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = _taregtFPS;
    }
    void ServiceBuildSetting()
    {
        //Debug.unityLogger.logEnabled = false;
    }
}
