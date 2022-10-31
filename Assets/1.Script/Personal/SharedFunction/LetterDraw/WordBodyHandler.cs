using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 글자 따라쓰기 인식부
public class WordBodyHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler, IPointerExitHandler
{
    #region 변수
    [SerializeField]
    List<DirectionFlag.Direction> directions;
    CheckDirectionScript dirCheck;

    [SerializeField]
    Transform Tuto;

    [SerializeField]
    Animator anim;

    int curLineIndex = 0;
    float time;
    bool onSprite = false;

    // 연결 시킬 스크립트
    ScoreHandler score;
    SpeakerHandler speaker;
    DecorationHandler decoration;
    LineDragController line;
    LetterFillHandler letterFill;

    GraphicRaycaster raycaster;

    [Header("클릭 효과음")]
    [SerializeField]
    AudioClip clips;
    #endregion
    #region 함수
    private void Start()
    {
        GetComponent<Image>().alphaHitTestMinimumThreshold = 0.004f;
        
        raycaster = transform.root.GetComponent<GraphicRaycaster>();

        dirCheck = GetComponent<CheckDirectionScript>();        

        score = FindObjectOfType<ScoreHandler>();
        speaker = FindObjectOfType<SpeakerHandler>();
        letterFill = FindObjectOfType<LetterFillHandler>();
        decoration = FindObjectOfType<DecorationHandler>();

        decoration.SetLine(curLineIndex);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        List<RaycastResult> result = new List<RaycastResult>();
        raycaster.Raycast(eventData, result);
        foreach(var hit in result)
        {
            if (hit.gameObject.GetComponent<LineDragController>())
            {
                time = 0;
                onSprite = true;

                if (clips)
                {   
                    speaker.SoundByClip(clips);
                    Tuto.GetComponent<Image>().enabled = false;
                    SetParam(1);
                }
                else
                {
                    print("PIP");
                }
            }
        }        
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (onSprite)
        {
            List<RaycastResult> result = new List<RaycastResult>();
            raycaster.Raycast(eventData, result);
            foreach (var hit in result)
            {
                if (dirCheck.CheckDir(eventData.delta, DirectionFlag.GetDirection(directions[curLineIndex])))
                {
                    if (hit.gameObject.GetComponent<LineDragController>())
                    {
                        line = hit.gameObject.GetComponent<LineDragController>();                        
                        line.ShowMouse(eventData);
                        time = 0;                        
                    }
                    if (hit.gameObject.GetComponent<DecorationHit>())
                    {   
                        letterFill.Fill(curLineIndex, hit.gameObject.GetComponent<DecorationHit>().GetParam().Item1, hit.gameObject.GetComponent<DecorationHit>().GetParam().Item2);
                        hit.gameObject.GetComponentInParent<DecorationController>().SetIndex(hit.gameObject.GetComponent<DecorationHit>().GetParam().Item1);
                        hit.gameObject.GetComponent<Image>().raycastTarget = false;
                        hit.gameObject.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                        line.ParticleMake();                        
                    }
                }
                else
                {
                    time += Time.deltaTime;
                    if (time > 1)
                    {
                        onSprite = false;
                    }
                }

                if (hit.gameObject.GetComponent<IPointer>() != null)
                {
                    if (hit.gameObject.GetComponent<IPointer>().NextLine().Item1 < directions.Count &&
                    hit.gameObject.GetComponent<IPointer>().NextLine().Item2 ==
                    DirectionFlag.GetDirection(directions[hit.gameObject.GetComponent<IPointer>().NextLine().Item1]))
                    {
                        letterFill.Fill(curLineIndex, 1, 1);

                        hit.gameObject.GetComponent<Image>().raycastTarget = false;
                        curLineIndex = hit.gameObject.GetComponent<IPointer>().NextLine().Item1;
                        decoration.SetLine(curLineIndex);

                        if (hit.gameObject.GetComponent<IPointer>().NextLine().Item3)
                        {
                            line.SetStartPoint(curLineIndex);
                            score.SetScore();
                        }
                    }
                    else if(hit.gameObject.GetComponent<IPointer>().NextLine().Item2 == Vector2.zero
                            && hit.gameObject.GetComponent<IPointer>().NextLine().Item1 == curLineIndex + 1)
                    {
                        hit.gameObject.GetComponent<Image>().raycastTarget = false;
                        letterFill.Fill(curLineIndex, 1, 1);
                        score.SetScore();
                    }
                }
            }
        }
        
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        SetParam(0);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        List<RaycastResult> result = new List<RaycastResult>();
        raycaster.Raycast(eventData, result);
        foreach (var hit in result)
        {
            if (hit.gameObject.GetComponent<LineDragController>())
            {
                onSprite = true;

                if (clips)
                {
                    speaker.SoundByClip(clips);
                }
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onSprite = false;
        SetParam(0);
    }
    #endregion

    #region 에니메이터 함수
    public void SetParam(int num)
    {
        anim.SetInteger("Param", num);
    }
    #endregion
}
