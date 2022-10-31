using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/HangeulIMG", fileName = "ImageCon")]
public class HangeulSpriteContainer : ScriptableObject
{
    #region 변수
    [SerializeField]
    List<Sprite> giyeog, nieun, digeud, lieul, mieum, bieub, sios, ieung, jieuj, chieuch, kieuk, tieut, pieup, hieuh;
    private Dictionary<int, List<Sprite>> SpriteDictionary = new Dictionary<int, List<Sprite>>();

    [Header("자모음")]
    [SerializeField]
    private List<Sprite> Consonant;
    [SerializeField]
    private List<Sprite> Vowel;
    [SerializeField]
    private List<Sprite> FinalConsonant;
    #endregion

    #region 함수

    public void Init()
    {
        SpriteDictionary.Clear();
        SpriteDictionary.Add(0, giyeog);
        SpriteDictionary.Add(1, nieun);
        SpriteDictionary.Add(2, digeud);
        SpriteDictionary.Add(3, lieul);
        SpriteDictionary.Add(4, mieum);
        SpriteDictionary.Add(5, bieub);
        SpriteDictionary.Add(6, sios);
        SpriteDictionary.Add(7, ieung);
        SpriteDictionary.Add(8, jieuj);
        SpriteDictionary.Add(9, chieuch);
        SpriteDictionary.Add(10, kieuk);
        SpriteDictionary.Add(11, tieut);
        SpriteDictionary.Add(12, pieup);
        SpriteDictionary.Add(13, hieuh);
    }

    public Dictionary<int, List<Sprite>> GetDictionary(){return SpriteDictionary;}

    public List<Sprite> GetConsonant(){return Consonant;}
    public List<Sprite> GetFinalConsonant { get => FinalConsonant; }


    public List<Sprite> GetVowel(){return Vowel;}
    #endregion
}
