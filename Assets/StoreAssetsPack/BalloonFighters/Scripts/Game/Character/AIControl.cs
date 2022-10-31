using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Generic;

public class AIControl : CharacterControl
{
    bool _jumpStop    = false;
    bool _isStartWait = false;

    Coroutine _moveRoutine;
    Coroutine _jumpRoutine;
    Coroutine _JumpStopRoutine;
    Coroutine _downTimeRoutine;
    Coroutine _waitingRoutine;

    public override void Initialize(Vector3 spawnPoint, StatusUI statusUI)
    {
        base.Initialize(spawnPoint,statusUI);

        _currentBalloonID = 2;
        InitBallooon(_currentBalloonID, GameConstants.BALLOON_COLOR_AI);
        SetType(CharacterType.AI);
    }
    public override void Update()
    {
        base.Update();
        if (!_isStartWait && IsGround && _currentState == CharacterState.LastOne)
        {
            _isStartWait = true;

            StopAI();
            _waitingRoutine = StartCoroutine(GroundWaitRoutine());
        }
    }

    void StartAI()
    {
        StopAI();

        int rand = Random.Range(2,3);
        
        PumpingBalloon(2,(float)rand, ()=> 
        {
            _isMove = true;
            _moveRoutine = StartCoroutine(MoveRoutine());
            _jumpRoutine = StartCoroutine(JumpRoutine());
            _JumpStopRoutine = StartCoroutine(JumpStopRoutine());
            _characterInfo.ChangeState(CharacterState.Moveable);
            _characterInfo.ChangeLife(2);
        });
    }
    void StopAI()
    {
        _isMove = false;
        this.SafeStopCoroutine(_moveRoutine);
        this.SafeStopCoroutine(_jumpRoutine);
        this.SafeStopCoroutine(_JumpStopRoutine);
        this.SafeStopCoroutine(_downTimeRoutine);
        this.SafeStopCoroutine(_waitingRoutine);
    }

    public override void OnChangeLife(int life)
    {
        base.OnChangeLife(life);
    }

    public override void OnAddScore(int score)
    {
        base.OnAddScore(score);
    }

    public override void OnChangeState(CharacterState state)
    {
        base.OnChangeState(state);
        
        switch (state)
        {
            case CharacterState.Start: _characterInfo.ChangeState(CharacterState.Pumping); break;
            case CharacterState.Pumping: StartAI(); break;
            case CharacterState.Moveable: break;
            case CharacterState.LastOne:
                {
                    PopBalloon();
                    StopAI();
                    _moveDir        = Vector3.zero;
                }
                break;
        }
    }

    IEnumerator PumpingRoutine()
    {
        yield return new WaitForSeconds(3f);
        
    }

    IEnumerator JumpRoutine()
    {
        yield return new WaitForSeconds(0.25f);
        _isJumping = !_jumpStop;

        _jumpRoutine = StartCoroutine(JumpRoutine());
    }

    IEnumerator MoveRoutine()
    {
        yield return new WaitForSeconds(3f);

        int x    = Random.Range(0, 2);
        if (x == 0) x = -1;
        _moveDir = new Vector3(x,0,0);

        _moveRoutine = StartCoroutine(MoveRoutine());
    }

    IEnumerator JumpStopRoutine()
    {
        yield return new WaitForSeconds(5f);
        _jumpStop = true;

        _downTimeRoutine = StartCoroutine(DownTimeRoutine());
    }

    IEnumerator DownTimeRoutine()
    {
        yield return new WaitForSeconds(2f);
        _jumpStop = false;

        _JumpStopRoutine = StartCoroutine(JumpStopRoutine());
    }

    IEnumerator GroundWaitRoutine()
    {
        yield return new WaitForSeconds(8f);

        _characterInfo.ChangeState(CharacterState.Pumping);
        Clear();
        _isStartWait = false;

    }
    
    public override void OnDie()
    {
        base.OnDie();

        StopAI();

        Die(0.25f, () => Destroy(gameObject));

        if (StageManager.instance.IsStageClear)
            StageManager.instance.NextStage();  
    }
    
    public override void OnEventCharacter(ECharacterEvent charEvent, CharacterInfo other)
    {
        base.OnEventCharacter(charEvent, other);

        if (_characterInfo.Type == other.Type)
            return;


        if (charEvent == ECharacterEvent.AttackSuccess)
        {
            
        }
        else if (charEvent == ECharacterEvent.Damaged)
        {
            int life = _characterInfo.Life - 1;
            _characterInfo.ChangeLife(life);
            if(life == 1)
                _characterInfo.ChangeState(CharacterState.LastOne);
        }
    }
}
