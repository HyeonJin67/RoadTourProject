using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInfo : MonoBehaviour, GameInfoBase
{
    int _id;
    int _score;
    int _life;
    float _moveSpeed;
    float _jumpPower;

    CharacterType    _type;
    CharacterState   _state;
    CharacterAbility _ability;
    
    public int ID    { get { return _id; } }
    public int Life  { get { return _life; } }
    public int Score { get { return _score; } }
    public CharacterType   Type { get { return _type; } }
    public CharacterState State { get { return _state; } }
    public CharacterAbility Ability { get { return _ability; } }

    CharacterDataEventor _dataEventor;
    public virtual void Clear()
    {
        _life = GameConstants.MAX_LIFE;
        _score = 0;
    }

    public void Initialize(int id,CharacterType type, CharacterAbility ability)
    {
        _dataEventor = new CharacterDataEventor();
        _dataEventor.Initialize(new List<ICharacterEventor>());
        
        CharacterControl characterControl = GetComponent<CharacterControl>();
        _dataEventor.AddEvent(characterControl);

        Character[] character = characterControl.GetCharacters();
        if (character != null)
        {
            for (int i = 0; i < character.Length; i++)
            {
                if (character[i] != null)
                {
                    _dataEventor.AddEvent(character[i]);
                }
            }
        }
        
        _id   = id;
        _type = type;

        ChangeLife(ability.Life);
        ChangeMoveSpeed(ability.Speed);
        ChangeJumpPower(ability.JumpPower);
        ChangeState(CharacterState.Start);

        _ability = ability;
    }

    public void AddScore(int score)
    {
        _score += score;

        _dataEventor.OnAddScore(score);
    }
    
    public void ChangeLife(int life)
    {
        if (_life != life)
        {
            if (life <= 0)
                _life  = 0;
            else
                _life = life;
        }
        _dataEventor.OnChangeLife(_life);

        if(life <= 0)
            Die();
    }

    public void ChangeState(CharacterState state)
    {
        _state = state;
        _dataEventor.OnChangeState(_state);
    }

    public void ChangeMoveSpeed(float speed)
    {
        if (_moveSpeed != speed)
        {
            _moveSpeed = speed;
        }
        _dataEventor.OnChangeMoveSpeed(_moveSpeed);
    }

    public void ChangeJumpPower(float power)
    {
        if (_jumpPower != power)
        {
            _jumpPower = power;
        }
        _dataEventor.OnChangeJumpPower(_jumpPower);
    }

    public void Die()
    {
        _dataEventor.OnDie();
    }
}
