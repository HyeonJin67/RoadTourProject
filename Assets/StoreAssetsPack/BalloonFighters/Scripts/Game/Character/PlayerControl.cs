using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControl : CharacterControl
{
    ////////////////////////////////////////////////////////////////////////////////
    //  Member Variable
    ////////////////////////////////////////////////////////////////////////////////
    bool      _isInvincible      = false;
    Coroutine _invincibleRoutine = null;


    ////////////////////////////////////////////////////////////////////////////////
    //  Public Method
    ////////////////////////////////////////////////////////////////////////////////
    public override void Initialize(Vector3 spawnPoint, StatusUI statusUI)
    {
        base.Initialize(spawnPoint, statusUI);

        _currentBalloonID = 1;
        InitBallooon(_currentBalloonID, GameConstants.BALLOON_COLOR_PLAYER);
        SetType(CharacterType.Player);
    }

    public override void Update ()
    {
        base.Update();

        if (_isDie)
            return;
        
        if (Input.GetKeyDown(KeyCode.Space))
            _isJumping = true;

        _isMove = false;
        _moveDir = Vector3.zero;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            _moveDir = Vector3.left;
            _isMove = true;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            _moveDir = Vector3.right;
            _isMove = true;
        }
        
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        Rotation(_moveDir, _rotationAngle,_rotationSpeed);
    }

    public override void OnChangeState(CharacterState state)
    {
        base.OnChangeState(state);
        
        switch (state)
        {
            case CharacterState.Start: _characterInfo.ChangeState(CharacterState.Moveable); break;
            case CharacterState.Moveable: break;
        }
    }

    public override void OnChangeLife(int life)
    {
        base.OnChangeLife(life);

        if (_statusUI != null)
            _statusUI.SetLife(_characterInfo.Life);
    }

    public override void OnAddScore(int score)
    {
        base.OnAddScore(score);

        Debug.LogFormat("OnAddScore({0},{1})", _characterInfo.Score, score);

        if(_statusUI != null)
            _statusUI.SetScore(_characterInfo.Score);
    }

    public override void OnDie()
    {
        base.OnDie();
        
        //GameOver
        StageManager.instance.Initialized();
    }

    public override void Clear()
    {
        base.Clear();

        if (_invincibleRoutine != null)
            StopCoroutine(_invincibleRoutine);
    }

    public override void OnEventCharacter(ECharacterEvent charEvent, CharacterInfo other)
    {
        base.OnEventCharacter(charEvent, other);
        
        if (charEvent == ECharacterEvent.AttackSuccess)
        {
            _characterInfo.AddScore(100);
            
        }
        else if (charEvent == ECharacterEvent.Damaged)
        {
            if (!_isInvincible)
            {
                int life = _characterInfo.Life - 1;
                _characterInfo.ChangeLife(life);
                PopBalloon();
                Invincible(0.5f);
            }

        }
    }

    ////////////////////////////////////////////////////////////////////////////////
    //  Private Method
    ////////////////////////////////////////////////////////////////////////////////
    void Invincible(float time)
    {
        if (_invincibleRoutine != null)
            StopCoroutine(_invincibleRoutine);

        _invincibleRoutine = StartCoroutine(InvincibleRoutine(time));
    }

    IEnumerator InvincibleRoutine(float time)
    {
        _isInvincible = true;
        yield return new WaitForSeconds(time);
        _isInvincible = false;
    }

}
