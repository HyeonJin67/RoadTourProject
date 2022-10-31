using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;


// 퍼즐 움직임 제어
public class PUzzleMoveController : MonoBehaviour
{
    #region 변수
    LetterImageController LIController;
    LetterDrawHandler LIDrawHandler;
    [SerializeField]
    Transform[] PuzzlePos, LetterPos, PuzzlePices;
    [SerializeField]
    Transform WordImage,WordTarget ,WordPuzzle;

    [SerializeField]
    GameObject WordPaint, Arrow;
    Vector3 TargetScale;

    SpeakerHandler speaker;
    ScoreHandler score;    

    #endregion

    public event System.Action<PointerEventData> Next;

    #region 시퀀스
    Sequence SucessSeq;
    Sequence StartSeq;
    Sequence NextSeq;
    #endregion

    private void Start()
    {
        LIController = FindObjectOfType<LetterImageController>();
        LIDrawHandler = FindObjectOfType<LetterDrawHandler>();
        speaker = FindObjectOfType < SpeakerHandler>();
        score = FindObjectOfType<ScoreHandler>();


        TargetScale = WordPuzzle.transform.localScale;

        ReadyWord(false);
        StartSeqMaker();
    }

    // 단어 출제 준비
    public void StartSeqMaker()
    {   
        StartSeq = DOTween.Sequence()
            .AppendInterval(1.5f)
            .OnStart(() => WordTarget.position = LetterPos[1].position)
            .Append(WordImage.DOMove(LetterPos[1].position, 2f).From(LetterPos[3].position))
            .Append(WordImage.DOScale(Vector3.one * 1.5f, 1f).From(Vector3.one).SetLoops(2, LoopType.Yoyo).OnStart(() => CallNa(0)))
            .Append(WordPuzzle.DOScale(TargetScale, 1f).SetEase(Ease.OutQuad).From(Vector3.zero).OnComplete(() => ReadyWord(true)))
            .Join(WordImage.DOScale(Vector3.zero, 1f).From(Vector3.one).OnComplete(() => score.OffScreenSaver()));
    }


    // 단어 완성시 움직임
    public void SucessSeqMaker()
    {
        Vector3 mOne = new Vector3(-1, 1, 1);
        if (LIController.GetLetterCheck().Item2)
        {
            if (LIController.GetLetterCheck().Item1 == 0)
            {
                SucessSeq = DOTween.Sequence()
                .AppendInterval(1f)
                .Append(WordTarget.DOScale(Vector3.one * 1.2f, 0.5f).From(Vector3.one).SetLoops(2, LoopType.Yoyo).OnComplete(() => CallNa(1)))
                .Append(WordTarget.DOMove(PuzzlePos[0].position, 2).From(WordTarget.position))
                .Join(WordTarget.DOScale(Vector3.one * 0.5f, 2).From(Vector3.one))
                .Join(WordImage.DOScale(mOne, 1f))
                .Join(PuzzlePices[0].DOMove(PuzzlePos[1].position, 2f).From(LetterPos[2].position))
                .Join(PuzzlePices[1].DOMove(PuzzlePos[2].position, 2f).From(LetterPos[4].position))
                .Append(WordImage.DOMove(LetterPos[0].position, 3f).From(LetterPos[2].position).SetEase(Ease.OutQuad).OnComplete(() => CallNa(2)))
                .Append(WordImage.DOScale(mOne * 1.2f, 1f).SetLoops(4, LoopType.Yoyo).From(mOne).OnComplete(() => NextSeqMaker()))
                ;
            }
            else if (LIController.GetLetterCheck().Item1 == 1)
            {
                SucessSeq = DOTween.Sequence()
                .AppendInterval(1f)
                .Append(WordTarget.DOScale(Vector3.one * 1.2f, 0.5f).From(Vector3.one).SetLoops(2, LoopType.Yoyo).OnComplete(() => CallNa(1)))
                .Append(WordTarget.DOMove(PuzzlePos[0].position, 2).From(WordTarget.position))
                .Join(WordTarget.DOScale(Vector3.one * 0.5f, 2).From(Vector3.one))
                .Join(WordImage.DOScale(mOne, 1f))
                .Join(PuzzlePices[0].DOMove(PuzzlePos[2].position, 2f).From(LetterPos[2].position))
                .Join(PuzzlePices[1].DOMove(PuzzlePos[3].position, 2f).From(LetterPos[4].position))
                .Append(WordImage.DOMove(LetterPos[0].position, 3f).From(LetterPos[2].position).SetEase(Ease.OutQuad).OnComplete(() => CallNa(2)))
                .Append(WordImage.DOScale(mOne * 1.2f, 1f).SetLoops(4, LoopType.Yoyo).From(mOne).OnComplete(() => NextSeqMaker()))
                ;
            }
        }
        else
        {
            SucessSeq = DOTween.Sequence()
                .AppendInterval(1f)
                .Append(WordTarget.DOScale(Vector3.one * 1.2f, 0.5f).From(Vector3.one).SetLoops(2, LoopType.Yoyo).OnComplete(() => CallNa(1)))
                .Append(WordTarget.DOMove(PuzzlePos[0].position, 2).From(WordTarget.position))
                .Join(WordTarget.DOScale(Vector3.one * 0.5f, 2).From(Vector3.one))
                .Join(WordImage.DOScale(mOne, 1f))
                .Join(PuzzlePices[0].DOMove(PuzzlePos[1].position, 2f).From(LetterPos[2].position))
                .Join(PuzzlePices[1].DOMove(PuzzlePos[2].position, 2f).From(LetterPos[4].position))
                .Append(WordImage.DOMove(LetterPos[0].position, 3f).From(LetterPos[2].position).SetEase(Ease.OutQuad).OnComplete(() => CallNa(2)))
                .Append(WordImage.DOScale(mOne * 1.2f, 1f).SetLoops(4, LoopType.Yoyo).From(mOne).OnComplete(() => NextSeqMaker()));
        }
    }


    // 다음 단어 출제 준비
    public void NextSeqMaker()
    {   
        if (score.CompCheck())
        {
            return;
        }

        if (LIController.GetLetterCheck().Item2)
        {
            if (LIController.GetLetterCheck().Item1 == 0)
            {
                NextSeq = DOTween.Sequence()
                .AppendInterval(2f)
                .Append(WordTarget.DOMove(LetterPos[3].position, 2f))
                .Join(WordImage.DOMove(LetterPos[2].position, 2f))
                .Join(PuzzlePices[0].DOMove(LetterPos[3].position, 2f).From(PuzzlePos[1].position))
                .Join(PuzzlePices[1].DOMove(LetterPos[4].position, 2f).From(PuzzlePos[2].position));
            }
            else if (LIController.GetLetterCheck().Item1 == 1)
            {
                NextSeq = DOTween.Sequence()
                .AppendInterval(2f)
                .Append(WordTarget.DOMove(LetterPos[3].position, 2f))
                .Join(WordImage.DOMove(LetterPos[2].position, 2f))
                .Join(PuzzlePices[0].DOMove(LetterPos[3].position, 2f).From(PuzzlePos[2].position))
                .Join(PuzzlePices[1].DOMove(LetterPos[4].position, 2f).From(PuzzlePos[3].position));
            }
        }
        else
        {
            NextSeq = DOTween.Sequence()
                .AppendInterval(2f)
                .Append(WordTarget.DOMove(LetterPos[3].position, 2f))
                .Join(WordImage.DOMove(LetterPos[2].position, 2f))
                .Join(PuzzlePices[0].DOMove(LetterPos[3].position, 2f).From(PuzzlePos[1].position))
                .Join(PuzzlePices[1].DOMove(LetterPos[4].position, 2f).From(PuzzlePos[2].position));
        }

        StartCoroutine(NextSeqWait());

    }
    
    // 다음 단어 음성 지연
    IEnumerator NextSeqWait()
    {
        yield return new WaitWhile(() => NextSeq.IsPlaying());
        WordTarget.localScale = Vector3.one;
        ReadyWord(false);
        Next?.Invoke(LIDrawHandler.GetED());
        StartSeqMaker();
        yield break;
    }

    public void ReadyWord(bool bol)
    {
        WordPaint.SetActive(bol);
        Arrow.SetActive(bol);
    }

    public void CallNa(int num)
    {
        StartCoroutine(CallDelay(num));
    }

    IEnumerator CallDelay(int num)
    {
        yield return new WaitWhile(() => speaker.GetComponent<AudioSource>().isPlaying);
        LIController.ReturnNa(num);
        yield break;
    }
}
