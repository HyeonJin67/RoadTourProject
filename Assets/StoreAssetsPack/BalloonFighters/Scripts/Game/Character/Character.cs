using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Generic;

public interface ICharacter
{
    void Clear();
    void SetType(CharacterType type);
    void SetActive(bool isActive);
    void InitBallooon(int balloonId,string hexColor);
    void PumpingBalloon(int balloonId,float time, Action onComplete);
    void PopBalloon();
    void Rotation(Vector3 direction, float rotationAngle, float speed);
    void Die(float delay, Action onComplete);
}


[Serializable]
public class CharacterAbility
{
    [SerializeField]
    public int Life = 3;
    [SerializeField]
    public int Speed = 3;
    [SerializeField]
    public int JumpPower = 3;
}


public class Character : MonoBehaviour, ICharacter, ICharacterEventor
{


    public int ID = 0;
    int _balloonID = 0;

    string _hexBalloonColor;

    CharacterType _type = CharacterType.None;

    [SerializeField]
    Balloon[] _balloon;

    [SerializeField]
    GameObject _body;

    [Header("Ability")]
    [SerializeField]
    CharacterAbility _ability;

    CharacterState _currentState = CharacterState.None;
    CollisionChecker _collisionChecker;
    CharacterEffect  _effect;

    Coroutine        _pumpingRoutine;
    Coroutine        _dieRoutine;
    
    public CharacterType Type
    {
        get { return _type; }
        set { _type = value; }
    }

    public bool IsActive
    {
        get { return gameObject.activeSelf; }
    }
    public CollisionChecker CollChecker
    {
        get { return _collisionChecker; }
    }

    public CharacterAbility Ability
    {
        get { return _ability; }
    }

    void Awake()
    {
        _collisionChecker = GetComponent<CollisionChecker>();
        if (_collisionChecker == null)
            _collisionChecker = gameObject.AddComponent<CollisionChecker>();

        _effect = GetComponent<CharacterEffect>();
    }
    public void Clear()
    {
        if (_collisionChecker != null)
        {
            if (_collisionChecker.CollisionBounds != null)
            {
                for (int i = 0; i < _collisionChecker.CollisionBounds.Length; i++)
                {
                    _collisionChecker.CollisionBounds[i].Reset();
                }
            }
        }
        for (int i = 0; i < _balloon.Length; i++)
        {
            if (_balloon[i] != null)
            {
                if (_balloon[i].ID == _balloonID)
                {
                    _balloon[i].SetActive(true);
                }
            }
        }
    }

    public void Initialize(Vector3 spawnPoint,bool initActive, Action<Collider, Collider> onTriggerBounds, LayerMask noneMoveCollisionlayer)
    {
        transform.position = spawnPoint;
        gameObject.SetActive(initActive);
        if (_collisionChecker != null)
        {
            _collisionChecker.Initialize(noneMoveCollisionlayer, onTriggerBounds);
        }
    }
    public void InitBallooon(int balloonID,string hexColor)
    {
        for (int i = 0; i < _balloon.Length; i++)
        {
            if (_balloon[i] != null)
            {
                _balloon[i].SetActive(false);
                if (_balloon[i].ID == balloonID)
                {
                    _balloon[i].SetActive(true);
                    _balloon[i].SetColor(hexColor);
                }
            }
        }
        _balloonID       = balloonID;
        _hexBalloonColor = hexColor;
    }

    public void PumpingBalloon(int balloonID,float time, Action onComplete)
    {
        if (IsActive)
        {
            if (_pumpingRoutine != null)
                StopCoroutine(_pumpingRoutine);

            _pumpingRoutine = StartCoroutine(PumpingRoutine(balloonID,time, onComplete));
        }
    }

    public void PopBalloon()
    {
        bool isPlayEffect = false;
        bool isActive     = false;

        if (IsActive)
        {
            if (_pumpingRoutine != null)
                StopCoroutine(_pumpingRoutine);
        }

        for (int i = 0; i < _balloon.Length; i++)
        {
            if (_balloon[i] != null)
            {
                if (_balloon[i].ID == _balloonID)
                {
                    _balloon[i].SetActive(false);

                    if (!isPlayEffect)
                    {
                        _effect.Play("Poof", _balloon[i].MainTransForm.position);
                        isPlayEffect = true;
                    }
                }
                if (_balloon[i].ID == _balloonID + 1)
                {
                    _balloon[i].SetActive(true);
                    if (_hexBalloonColor != "")
                        _balloon[i].SetColor(_hexBalloonColor);
                    
                }
                if(!isActive)
                    isActive = _balloon[i].IsActive;
            }
        }
        if (!isActive)
        {
            this.MonoInvoke(ChangeNoneBalloon, 0.2f);
        }
    }
    void ChangeNoneBalloon()
    {
        CollisionBound main    = _collisionChecker.GetCollisionBound("Main");
        CollisionBound body    = _collisionChecker.GetCollisionBound("Body");
        CollisionBound balloon = _collisionChecker.GetCollisionBound("Balloon");

        main.transform.localScale = Vector3.one;
        body.transform.localScale = Vector3.one * 1.2f;
        main.transform.transform.localPosition = _body.transform.transform.localPosition;
        body.transform.transform.localPosition = _body.transform.transform.localPosition;

        balloon.SetActive(false);
    }
    public void Die(float delay, Action onComplete)
    {
        if (IsActive)
        {
            if (_dieRoutine != null)
                StopCoroutine(_dieRoutine);

            _dieRoutine = StartCoroutine(DieRoutine(delay, onComplete));
        }
    }
    IEnumerator DieRoutine(float time,Action onComplete)
    {
        yield return new WaitForSeconds(time);
        if (_body != null)
        {
            _effect.Play("MagicPoof", _body.transform.position);
            if(onComplete != null)
                onComplete();
        }

        this.SafeStopCoroutine(_pumpingRoutine);
    }

    IEnumerator PumpingRoutine(int balloonID,float time, Action onComplete)
    {
        float elapsedTime = 0;
        while (elapsedTime < time)
        {
           
            elapsedTime += Time.deltaTime;
            float normal = elapsedTime / time;

            if (_balloon != null)
            {
                for (int i = 0; i < _balloon.Length;i++)
                {
                    if (_balloon[i] != null)
                    {
                        if (_balloon[i].ID == balloonID)
                        {
                            _balloon[i].SetActive(true);
                            _balloon[i].SetScale(normal);
                        }
                        else
                            _balloon[i].SetActive(false);
                    }
                }
            }
            yield return null;
        }
        if(onComplete !=null)
            onComplete();
    }
    
    public void SettingBalloon()
    {
        Balloon[] balloons = GetComponentsInChildren<Balloon>();
        _balloon           = new Balloon[balloons.Length];
        for (int i = 0; i < balloons.Length;i++)
        {
            _balloon[i] = balloons[i];
            _balloon[i].Setting();
        }
    }
    public void SetType(CharacterType type)
    {
        _type = type;
    }
    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
    public void Rotation(Vector3 direction,float rotationAngle,float speed)
    {
        float eulerAnglesY  = -direction.x * rotationAngle;
        float angleY        = Mathf.LerpAngle(_body.transform.eulerAngles.y, eulerAnglesY, Time.deltaTime * speed);
        _body.transform.eulerAngles = new Vector3(_body.transform.eulerAngles.x, angleY, _body.transform.eulerAngles.z);
    }
    public void SetBalloonScale(float scale)
    {
        if (_balloon == null)
            return;

        for (int i = 0; i < _balloon.Length; i++)
        {
            _balloon[i].SetScale(scale);
            if (scale <= 0)
                _balloon[i].SetActive(false);
            else
                _balloon[i].SetActive(true);
        }
    }
    
    public void OnChangeLife(int life)
    {
        
    }

    public void OnAddScore(int score)
    {
        
    }

    public void OnDie()
    {

    }
    
    public void OnChangeState(CharacterState state)
    {
        _currentState = state;
    }

    public void OnChangeMoveSpeed(float speed)
    {
      
    }

    public void OnChangeJumpPower(float power)
    {
      
    }
}
