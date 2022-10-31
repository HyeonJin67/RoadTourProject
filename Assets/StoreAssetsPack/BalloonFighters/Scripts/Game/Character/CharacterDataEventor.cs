using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterEventor
{
    void OnChangeLife(int life);
    void OnAddScore(int score);
    void OnChangeState(CharacterState state);
    void OnChangeMoveSpeed(float speed);
    void OnChangeJumpPower(float power);
    void OnDie();
}

public abstract class CharacterEventorBase : ICharacterEventor
{
    public virtual void OnChangeLife(int life) { }
    public virtual void OnAddScore(int score) { }
    public virtual void OnChangeState(CharacterState state) { }
    public virtual void OnChangeMoveSpeed(float speed) { }
    public virtual void OnChangeJumpPower(float power) { }
    public virtual void OnDie(){ }
}

public class CharacterDataEventor : CharacterEventorBase
{
    List<ICharacterEventor> _eventors;

    public void Initialize(List<ICharacterEventor> eventors)
    {
        _eventors = eventors;
    }

    public void AddEvent(ICharacterEventor characterEvent)
    {
        _eventors.Add(characterEvent);
    }

    public override void OnChangeLife(int life)
    {
        for (int i = 0; i < _eventors.Count; i++)
        {
            _eventors[i].OnChangeLife(life);
        }
    }

    public override void OnAddScore(int score)
    {
        for (int i = 0; i < _eventors.Count; i++)
        {
            _eventors[i].OnAddScore(score);
        }
    }
    
    public override void OnChangeState(CharacterState state)
    {
        for (int i = 0; i < _eventors.Count; i++)
        {
            _eventors[i].OnChangeState(state);
        }
    }

    public override void OnChangeMoveSpeed(float speed)
    {
        for (int i = 0; i < _eventors.Count; i++)
        {
            _eventors[i].OnChangeMoveSpeed(speed);
        }
    }

    public override void OnChangeJumpPower(float power)
    {
        for (int i = 0; i < _eventors.Count; i++)
        {
            _eventors[i].OnChangeJumpPower(power);
        }
    }

    public override void OnDie()
    {
        for (int i = 0; i < _eventors.Count; i++)
        {
            _eventors[i].OnDie();
        }
    }
}
