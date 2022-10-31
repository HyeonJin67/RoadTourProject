using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 단어 출제자
public class LetterImageController : MonoBehaviour
{
    #region 변수
    [SerializeField]
    NarrationSetContainer narrationContainer;
    [SerializeField]
    HangeulSpriteContainer hangeulSpriteContainer;

    PUzzleMoveController moveCon;
    SpeakerHandler speaker;

    [SerializeField]
    Transform Animals;
    [SerializeField]
    Transform[] puzzle;
    [SerializeField]
    int UnitNum;
    int curIndex = 0;
    [SerializeField]
    int[] indexs;

    List<string> word;
    
    char[] letterSplit;
    
    Vector3 StartPos;

    [SerializeField]
    List<AudioClip> clips;
    List<AudioClip[]> narrations;    
    AudioClip[] na;

    List<Sprite> vowel = new List<Sprite>();
    List<Sprite> consonant = new List<Sprite>();
    List<Sprite> LastConsonant = new List<Sprite>();

    private (int, bool) letterCheck;
    #endregion

    private void Start()
    {
        StartPos = transform.position;
        speaker = FindObjectOfType<SpeakerHandler>();
        moveCon = FindObjectOfType<PUzzleMoveController>();

        narrationContainer.Init();
        hangeulSpriteContainer.Init();

        Init();
        StartCoroutine(StartDelay());

        moveCon.Next += NextSetting;
    }
    // 첫 시작까지 약간의 딜레이 => 오류 방지용
    IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(0.001f);
        speaker.SoundByClip(clips[0]);
        vowel = hangeulSpriteContainer.GetVowel();
        consonant = hangeulSpriteContainer.GetConsonant();
        LastConsonant = hangeulSpriteContainer.GetFinalConsonant;
        Recruit();
        yield break;
    }
    // 초기 세팅
    public void Init()
    {
        var array = System.Linq.Enumerable.Range(0, Animals.childCount);
        indexs = array.OrderBy(x => Random.value).ToArray();
        if(narrationContainer.GetDictionary().TryGetValue(UnitNum, out List<NarrationSetContainer.Narraition> sp))
        {
            word = new List<string>();
            word.Clear();
            narrations = new List<AudioClip[]>();
            narrations.Clear();
            foreach(NarrationSetContainer.Narraition nar in sp)
            {
                word.Add(nar.Name);
                narrations.Add(nar.Clips);
            }
        }
    }
    // 단어 뽑기
    public void Recruit()
    {
        if (curIndex == word.Count)
        {
            curIndex = 0;
            Init();
        }

        for (int i = 0; i < Animals.childCount; i++)
        {
            Animals.GetChild(i).gameObject.SetActive(false);
        }

        Animals.GetChild(indexs[curIndex]).gameObject.SetActive(true);
        char wordFirstLetter = word[indexs[curIndex]].FirstOrDefault();
        na = narrations[indexs[curIndex]];

        letterSplit = HangeulUtil.DivideHangul(wordFirstLetter);
        SetPuzzle();
        curIndex++;
    }

    //자모 구분 후 단어 출제
    public void SetPuzzle()
    {
        letterCheck = HangeulUtil.DirAndPiece(letterSplit);
        if(letterCheck.Item1 == 0)
        {
            puzzle[0].localRotation = Quaternion.Euler(new Vector3(0, 0, 00));
            puzzle[1].gameObject.SetActive(true);
            puzzle[1].GetComponentInChildren<SpriteRenderer>().sprite = vowel[letterSplit[1] - 4449];
            puzzle[2].gameObject.SetActive(false);
            puzzle[2].GetComponentInChildren<SpriteRenderer>().sprite = vowel[letterSplit[1] - 4449];

        }
        else if(letterCheck.Item1 == 1)
        {
            puzzle[0].localRotation = Quaternion.Euler( new Vector3(0,0,90));
            puzzle[1].gameObject.SetActive(false);
            puzzle[1].GetComponentInChildren<SpriteRenderer>().sprite = vowel[letterSplit[1] - 4449];
            puzzle[2].gameObject.SetActive(true);
            puzzle[2].GetComponentInChildren<SpriteRenderer>().sprite = vowel[letterSplit[1] - 4449];            
        }
        if (letterCheck.Item2)
        {
            puzzle[0].localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
            puzzle[1].gameObject.SetActive(true);
            puzzle[1].GetComponentInChildren<SpriteRenderer>().sprite = vowel[letterSplit[1] - 4449];
            puzzle[2].gameObject.SetActive(true);
            puzzle[2].GetComponentInChildren<SpriteRenderer>().sprite = LastConsonant[letterSplit[2] - 4519];
        }
    }


    #region

    public void NextSetting(UnityEngine.EventSystems.PointerEventData ee)
    {
        Recruit();
    }
    public void ReturnNa(int num)
    {
        speaker.SoundByClip(na[num]);
    }

    public (int, bool) GetLetterCheck()
    {
        return letterCheck;
    }
    public Vector3 GetPuzPos()
    {
        return StartPos;
    }
    #endregion
}