using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSetup : MonoBehaviour
{
    public void Setup(CharacterType type)
    {
        switch (type)
        {
            case CharacterType.Player:gameObject.AddComponent<PlayerControl>(); break;
            case CharacterType.AI: gameObject.AddComponent<AIControl>(); break;
            case CharacterType.None:Debug.Log("Please, Select Character Type"); break;
        }
    }
}
