
using UnityEngine;
using System.Collections;
using Generic;

public class TimeManager : MonoSingleton<TimeManager>
{
    [SerializeField]
    [Range(0f,1f)]
    private float _scale       = 1f;
    private float _originScale = 1f;
    private float _deltaTime;
    private bool  _isPaused = false;

    public  float UnityDeltaTime { get { return Time.deltaTime; } }
    public  float DeltaTime { get { return _deltaTime; } }
    public  float Scale { get { return _scale; } set { _scale = value; } }
    public  bool isPaused
    {
        get { return _isPaused; }
        set
        {
            _isPaused = value;
            if (_isPaused)
            {
                _originScale = _scale;
                _scale = 0;
            }
            else
                _scale = _originScale;
        }
    }
    
    void Update()
    {
        _deltaTime = Time.deltaTime * _scale;
    }
}
