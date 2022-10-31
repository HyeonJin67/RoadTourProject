using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/SoundBox", fileName = "DefaultContainer")]
public class NarrationSetContainer : ScriptableObject
{
    #region 변수
    [SerializeField]
    private List<Narraition> giyeog, nieun, digeud, lieul, mieum, bieub, sios, ieung, jieuj, chieuch, kieuk, tieut, pieup, hieuh;
    private Dictionary<int, List<Narraition>> audioDict = new Dictionary<int, List<Narraition>>();
    #endregion

    #region 함수
    public void Init()
    {
        audioDict.Clear();
        audioDict.Add(0, giyeog);
        audioDict.Add(1, nieun);
        audioDict.Add(2, digeud);
        audioDict.Add(3, lieul);
        audioDict.Add(4, mieum);
        audioDict.Add(5, bieub);
        audioDict.Add(6, sios);
        audioDict.Add(7, ieung);
        audioDict.Add(8, jieuj);
        audioDict.Add(9, chieuch);
        audioDict.Add(10, kieuk);
        audioDict.Add(11, tieut);
        audioDict.Add(12, pieup);
        audioDict.Add(13, hieuh);
    }
    #endregion
    #region 클래스
    [Serializable]
    public class Narraition
    {
        public string Name;
        public AudioClip[] Clips;
    }
    #endregion

    public Dictionary<int, List<Narraition>> GetDictionary()
    {
        return audioDict;
    }
}
