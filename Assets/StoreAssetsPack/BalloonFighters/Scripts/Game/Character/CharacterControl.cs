using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Generic;

public enum ECharacterEvent
{
    AttackSuccess,
    Damaged,
}

public class CharacterControl : MonoBehaviour, ICharacter, ICharacterEventor
{
    ////////////////////////////////////////////////////////////////////////////////
    //  Member Variable
    //////////////////////////////////////////////////////////////////////////////// 
    bool _isVReaction = false;

    float _moveSpeed = 2f;
    float _jumpPower = 3f;
    float _hReactionPower = 5f;
    float _vReactionPower = 2f;
    float _speedMax = 7f;
    float _vReaction = 1f;

    Rigidbody _rigidbody = null;
    Vector3 _movePosition = Vector3.zero;
    Vector3 _jumpVelocity = Vector3.zero;
    Vector3 _limitPosition = Vector3.zero;

    ////////////////////////////////////////////////////////////////////////////////
    //  Protected Variable
    //////////////////////////////////////////////////////////////////////////////// 
    protected Character[] _characters;
    protected CharacterInfo _characterInfo;
    protected LayerMask _noneMoveCollisionlayer;
    protected LayerMask _ignoreGroundlayer;

    protected bool           _isJumping        = false;
    protected bool           _isMove           = false;
    protected bool           _isDie            = false;
    protected bool           _prevHit          = false;
    protected bool           _isGround         = false;
    protected Vector3        _moveDir          = Vector3.zero;
    protected float          _rotationAngle    = 50f;
    protected float          _rotationSpeed    = 2f;
    protected float          _deltaSpeed       = 0f;
    protected int            _currentBalloonID = 0;
    protected StatusUI       _statusUI         = null;

    protected CharacterState _currentState     = CharacterState.None;
   
    ////////////////////////////////////////////////////////////////////////////////
    //  Property
    ////////////////////////////////////////////////////////////////////////////////
    public bool IsGround
    {
        get
        {
            _limitPosition = Vector3.zero;
            if (_characters == null)
                return false;

            for (int i = 0; i < _characters.Length; i++)
            {
                if (_characters[i] != null)
                {
                    CollisionChecker collChecker = _characters[i].GetComponent<CollisionChecker>();
                    if (collChecker != null)
                    {
                        _limitPosition = collChecker.LimitPosition;
                        if (collChecker.IsGround)
                            return true;
                    }
                }
            }
            return false;
        }
    }

    public bool IsDie { get { return _isDie; } }

    ////////////////////////////////////////////////////////////////////////////////
    //  Base
    ////////////////////////////////////////////////////////////////////////////////
    void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _characterInfo = GetComponent<CharacterInfo>();
    }
    
    ////////////////////////////////////////////////////////////////////////////////
    //  Public Method
    ////////////////////////////////////////////////////////////////////////////////
    public virtual void Initialize(Vector3 spawnPoint, StatusUI statusUI)
    {
        transform.position = spawnPoint;
        _noneMoveCollisionlayer = LayerMap.Item;
        _ignoreGroundlayer = LayerMap.Body;

        _characters         = new Character[2];
        _characters[0]      = GetComponentInChildren<Character>();
        _characters[1]      = Instantiate(_characters[0], transform);
        _characters[1].name = _characters[0].name;
        _statusUI           = statusUI;


        for (int i = 0; i < _characters.Length; i++)
        {
            if (_characters[i] != null)
            {
                bool active = i == 0 ? true : false;
                _characters[i].Initialize(spawnPoint, active, OnTriggerBounds, _noneMoveCollisionlayer);
            }
        }
        
        _isGround = false;
        _isJumping = false;
    }

    public virtual void Update()
    {
        CheckView();
    }

    public virtual void FixedUpdate()
    {
        Move();
        Jump();
    }
    
    public virtual void OnEventCharacter(ECharacterEvent charEvent, CharacterInfo other){ }
    public virtual void OnChangeLife(int life) { }
    public virtual void OnAddScore(int score) { }
    public virtual void OnDie() { }

    public virtual void OnChangeState(CharacterState state)
    {
        _currentState = state;
    }

    public virtual void Clear()
    {
     
    }

    public void OnTriggerBounds(Collider me, Collider other)
    {
        //Custom CollisionCheck
        if (_characterInfo != null)
        {
            CollisionBound collBound = me.gameObject.GetComponent<CollisionBound>();
            CollisionBound otherBound = other.gameObject.GetComponent<CollisionBound>();

            //Character Physix 
            if (_characterInfo.State == CharacterState.Moveable)
            {
                float dirX = 0f;
                float dirY = 0f;


                if (collBound.BoundName == "Main")
                {
                    Vector3 dir = (other.transform.position - me.transform.position).normalized;

                    float leftRight = Vector3.Dot(dir, me.transform.right);
                    dirX = leftRight < 0 ? -1 : 1;
                }

                if (collBound.BoundName == "Balloon")
                {
                    Vector3 dir = (other.transform.position - me.transform.position).normalized;

                    float upDown = Vector3.Dot(dir, me.transform.up);
                    dirY = upDown < 0 ? -1 : 1;
                }

                if (dirX != 0)
                {
                    _deltaSpeed = _hReactionPower * -dirX;
                    if (_speedMax < Mathf.Abs(_deltaSpeed))
                        Debug.LogFormat("CharacterSpeed({0}) Can't higher than SpeedMax({1})", Mathf.Abs(_deltaSpeed), _speedMax);
                }
                if (dirY != 0)
                {
                    bool isvReaction = (((me.transform.tag == "Ground") && dirY < 0)
                        || (me.transform.tag != "Ground"));

                    if (isvReaction)
                    {
                        _isJumping = true;
                        _isVReaction = true;
                        _vReaction = -dirY;
                    }
                }
            }

            //CharacterEvent
            if (collBound != null && otherBound != null)
            {
                var otherInfo = otherBound.transform.parent.parent.GetComponent<CharacterInfo>();
                if (_characterInfo.State == CharacterState.Moveable && collBound.BoundName == GameConstants.BOUND_NAME_BALLOON && otherBound.BoundName == GameConstants.BOUND_NAME_BODY)
                    OnEventCharacter(ECharacterEvent.Damaged, otherInfo);

                if ((_characterInfo.State == CharacterState.LastOne || _characterInfo.State == CharacterState.Pumping)
                    && collBound.BoundName == GameConstants.BOUND_NAME_BODY && otherBound.BoundName == GameConstants.BOUND_NAME_BODY)
                    OnEventCharacter(ECharacterEvent.Damaged, otherInfo);

                if (_characterInfo.State == CharacterState.Moveable && collBound.BoundName == GameConstants.BOUND_NAME_BODY && otherBound.BoundName == GameConstants.BOUND_NAME_BALLOON)
                    OnEventCharacter(ECharacterEvent.AttackSuccess, otherInfo);

                if (_characterInfo.State == CharacterState.Moveable && (otherInfo.State == CharacterState.LastOne || otherInfo.State == CharacterState.Pumping)
                    && collBound.BoundName == GameConstants.BOUND_NAME_BODY && otherBound.BoundName == GameConstants.BOUND_NAME_BODY)
                    OnEventCharacter(ECharacterEvent.AttackSuccess, otherInfo);
            }
        }
    }

    public Character[] GetCharacters()
    {
        if (_characters == null)
        {
            Debug.Log("No Characters");
            return null;
        }

        return _characters;
    }
    public void SetType(CharacterType type)
    {
        if (_characters == null)
            return;

        for (int i = 0; i < _characters.Length; i++)
        {
            if (_characters[i] != null)
                _characters[i].SetType(type);
        }
    }

    public void SetActive(bool isActive)
    {
        if (_characters == null)
            return;

        for (int i = 0; i < _characters.Length; i++)
        {
            if (_characters[i] != null)
                _characters[i].SetActive(isActive);
        }
    }

    public void InitBallooon(int balloonID, string hexColor)
    {
        if (_characters == null)
            return;

        for (int i = 0; i < _characters.Length; i++)
        {
            if (_characters[i] != null)
                _characters[i].InitBallooon(balloonID, hexColor);
        }
    }
    
    public void PumpingBalloon(int balloonId, float time, Action onComplete)
    {
        if (_characters == null)
            return;

        for (int i = 0; i < _characters.Length; i++)
        {
            if (_characters[i] != null)
                _characters[i].PumpingBalloon(balloonId, time, onComplete);
        }
    }

    public void PopBalloon()
    {
        if (_characters == null)
            return;

        for (int i = 0; i < _characters.Length; i++)
        {
            if (_characters[i] != null)
                _characters[i].PopBalloon();
        }
    }

    public void Rotation(Vector3 direction, float rotationAngle, float speed)
    {
        if (_characters == null)
            return;

        for (int i = 0; i < _characters.Length; i++)
        {
            if (_characters[i] != null)
                _characters[i].Rotation(direction, rotationAngle, speed);
        }
    }

    public void Die(float delay,Action onComplete)
    {
        if (_characters == null)
            return;

        for (int i = 0; i < _characters.Length; i++)
        {
            if (_characters[i] != null)
                _characters[i].Die(delay, onComplete);
        }

        _jumpVelocity = Vector3.up * 7;
        _rigidbody.velocity = _jumpVelocity;
        _isJumping = false;
        _isVReaction = false;
        _isDie = true;

        Clear();
    }

    public void OnChangeMoveSpeed(float speed)
    {
        _moveSpeed = speed;
    }

    public void OnChangeJumpPower(float power)
    {
        _jumpPower = power;
    }


    ////////////////////////////////////////////////////////////////////////////////
    //  Private Method
    ////////////////////////////////////////////////////////////////////////////////
    void Jump()
    {
        if (!_isJumping)
            return;

        if (_isVReaction)
            _vReaction = _vReaction * _vReactionPower;
        else
            _vReaction = 1f;

        _jumpVelocity = Vector3.up * (_jumpPower * _vReaction);
        _rigidbody.velocity = _jumpVelocity;
        _isJumping = false;
        _isVReaction = false;
    }
 
    void Move()
    {
        if (!_isMove)
        {
            float horizontal = 0;
            if (_deltaSpeed == 0)
            {
                horizontal = 0f;
                goto CollisionChecker;
            }
            if (_deltaSpeed > 0)
                horizontal = 1f;
            else
                horizontal = -1f;

            _deltaSpeed -= TimeManager.Instance.DeltaTime * (_moveSpeed * horizontal);

            bool isInit = horizontal > 0 ? _deltaSpeed <= 0 : _deltaSpeed >= 0;
            if (isInit)
                _deltaSpeed = 0;
        }
        else
            _deltaSpeed += TimeManager.Instance.DeltaTime * (_moveSpeed * _moveDir.x);

        CollisionChecker:
     
        
        if (Mathf.Abs(_deltaSpeed) > 0)
            _deltaSpeed = Mathf.Min(_speedMax, Mathf.Abs(_deltaSpeed)) * (_deltaSpeed / Mathf.Abs(_deltaSpeed));
        
        Vector3 movePosition = Vector3.right * _deltaSpeed * TimeManager.Instance.DeltaTime;
        _movePosition        = transform.position + movePosition;

        if (IsGround)
        {
            float posY = Mathf.Max(_movePosition.y, _limitPosition.y);
            _movePosition = new Vector3(_movePosition.x, posY, _movePosition.z);
        }

        _rigidbody.MovePosition(_movePosition);
    }
  
    void CheckView()
    {
        for (int i = 0; i < _characters.Length; i++)
        {
            int targetIndex = (i == 0) ? 1 : 0;
            Vector2 viewPortPoint = Camera.main.WorldToViewportPoint(_characters[i].transform.position);
            Vector2 normalPoint = new Vector2((viewPortPoint.x - 0.5f) / 0.5f, (viewPortPoint.y - 0.5f) / 0.5f);

            if (_characters[i].IsActive && !_characters[targetIndex].IsActive)
            {
                if (Mathf.Abs(normalPoint.x) > 0.7f)
                {
                    float height = Camera.main.orthographicSize * 2.0f;
                    float width = height * Camera.main.aspect;

                    float x = normalPoint.x < 0 ? _characters[i].transform.position.x + width : _characters[i].transform.position.x - width;
                    _characters[targetIndex].transform.position = new Vector3(x, _characters[i].transform.position.y, _characters[i].transform.position.z);
                    _characters[targetIndex].SetActive(true);
                }
            }

            if (_characters[i].IsActive)
            {
                if (Mathf.Abs(normalPoint.x) > 1.3f)
                {
                    _characters[i].SetActive(false);
                }
            }
        }
    }

    Vector3 GetDirection(Vector3 position1, Vector3 position2)
    {
        Vector3 dirVec = Vector3.zero;
        Vector3 dir = position2 - position1;

        for (int i = 0; i < 4; i++)
        {
            if (i == 0)
                dirVec = Vector3.left;
            else if (i == 1)
                dirVec = Vector3.right;
            else if (i == 2)
                dirVec = Vector3.up;
            else if (i == 3)
                dirVec = Vector3.down;

            float angle = dir.GetAngleBetween3DVector(dirVec);
            if (angle < (90f / 2f))
                return dirVec;
        }

        return Vector3.zero;
    }
}
