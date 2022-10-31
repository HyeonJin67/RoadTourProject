using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public enum FishState
{
    None,  // 통상 상태
    Catch, // 손에 잡힘
    Hook,   // 손에서 놓음
    OnIce,  // 얼음 위의 생선
    End
}

public class FishHandler : MonoBehaviour
{
    #region 변수
    FlotsamController controller;

    List<Transform> Point = new List<Transform>();

    Transform Font;
    public Vector3 ta;
    Vector3 oriScale;

    Collider collider;
    Rigidbody rigidbody;

    ScoreHandler score;

    // 순회 순서
    int nextIndex;
    int index;
    public int Index
    {
        get => index;
        set
        {
            index = value % Point.Count;
            nextIndex = (Index + 1) % Point.Count;
            MoveNext();
        }
    }

    // 현제 상태
    [SerializeField]
    FishState state;    
    public FishState State
    {
        get => state;
        set
        {   
            Operate = value switch
            {
                FishState.None => MoveNext,
                FishState.Catch => Catch,
                FishState.Hook => Hook,
                FishState.End when state == FishState.Catch => MoveNext,
                _ => null,
            };
            state = value;
            Operate?.Invoke();
        }
    }
    public System.Action Operate;
    //
    #endregion

    #region 함수
    #region None 상태
    // 움직임
    public void MoveNext()
    {
        collider.isTrigger = true;

        transform.DOMove(Point[nextIndex].position, 2f).From(transform.position).SetEase(Ease.Linear).OnComplete(() => Index++);
        transform.DOLookAt(Point[nextIndex].position, 2f);
        
        Font.gameObject.SetActive(true);
        transform.localScale = oriScale;
    }
    // 글자 카메라 시점 고정
    private void Update()
    {
        Vector3 pa = transform.localRotation.eulerAngles;
        Vector3 ca = Camera.main.transform.localRotation.eulerAngles;

        Font.localRotation = Quaternion.Euler(ca - pa - ta);
    }
    #endregion

    #region Catch 상태
    // 잡혔을 때 움직임 멈추기
    void Catch()
    {
        transform.DOKill();
        Font.gameObject.SetActive(false);
    }
    #endregion

    #region Hook 상태
    // 중력 적용
    void Hook()
    {
        rigidbody.isKinematic = false;
        collider.isTrigger = false;
    }
    #endregion

    #endregion
    private void Awake()
    {
        controller = FindObjectOfType<FlotsamController>();
        Point = controller.wayPoint;
        collider = GetComponent<Collider>();
        rigidbody = GetComponent<Rigidbody>();

        Font = transform.GetChild(1);
        oriScale = transform.localScale;
        score = FindObjectOfType<ScoreHandler>();

        score.SceneComplete += () => { State = FishState.End; };
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.transform.name == "Under" && State == FishState.OnIce)
        {
            score.RemoveScore();
            rigidbody.isKinematic = true;
            State = FishState.None;
        }
        else if (other.transform.name == "Under" && (State == FishState.End || State == FishState.Hook))
        {
            rigidbody.isKinematic = true;
            State = FishState.None;
        }
        
    }
}
