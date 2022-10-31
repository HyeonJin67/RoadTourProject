using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterEffect : MonoBehaviour
{
    [SerializeField]
    Effect[] effectPrefabs;

    public void Play(string name,Vector3 positon)
    {
        if (effectPrefabs == null)
            return;
        for (int i = 0; i < effectPrefabs.Length; i++)
        {
            if (effectPrefabs[i].Name == name)
            {
                Instantiate(effectPrefabs[i], positon, effectPrefabs[i].transform.rotation);
                break ;
            }
        }
    }
}
