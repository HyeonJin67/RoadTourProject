using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Generic;

public interface GameInfoBase
{
    void Clear();
}
public class GameInfo : Singleton<GameInfo>
{
    public GameInfo()
    {

    }
}
