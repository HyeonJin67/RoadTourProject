using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum ClawState
{
    // None => Fall , None => Move , Move => None , Fall => None 만 가능하게 만들기
    None,  // 통상상태      =>  천장으로 자석이 올라오기
    Move,  // 움직이는 경우 =>  하강을 막으며 천장에 붙어있기
    Fall,  // 잡기 시도     =>  상하로 움직이며 움직임을 막기
    End
}


// 게임박스에 삽입하여 전체 게임을 조종하기 위한 컨트롤러
public class ClawController : MonoBehaviour
{
    #region 변수
    // Get을 사용할 경우에만 필요, 이전 상태 비교 용으로도 있으므로 삭제 하면 안 됨
    [SerializeField]
    ClawState state;
    public ClawState State
    {
        //get은 삭제 가능
        get => state;
        set
        {   
            Operate = value switch
            {
                ClawState.None => DefaultSetting,
                ClawState.Move when state == ClawState.None => ClawMoveStart,
                ClawState.Fall when state == ClawState.Move => MagnetFall,
                _ => null,
            };
            state = value;
            Operate?.Invoke();
        }
    }
    //부딪힐 경우..?
    public bool Collide
    {
        set
        {
            MagnetCollision?.Invoke();
            IsTrigger = true;
            //SetBodyTrigger(true);
        }
    }
    public bool IsTrigger
    {
        set
        {
            magnetTransform.GetComponent<Collider>().isTrigger = value;
            foreach (Transform t in magnetParts)
            {
                t.GetComponent<Collider>().isTrigger = value;
            }
        }
    }
    public System.Action Operate;
    List<Transform> magnetParts = new List<Transform>();

    // 클릭 지점 레이 | 자석 기준 위아래 방향 레이 
    Ray moRay;
    RaycastHit RoofHit;
    RaycastHit UnderHit;

    // 자석 오브젝트 | 하단 오브젝트 | 천장 오브젝트
    public Transform magnetTransform;
    public Transform Cable;
    Transform roofTransform;
    Transform underTransform;

    // 시작지점
    public Vector3 StartPoint;

    // 레이어 마스크
    public LayerMask roofLayer;
    public LayerMask UnderLayer;

    // 마우스 마커 관련 설정들
    public Transform mousePosMarker;
    RaycastHit mousePosHit;
    // 자석의 높이
    public float offsetY = 0;
    // 마커의 높이
    public float mouseposOffsetFromGround = 0;
    public Vector3 mousePos;

    Rigidbody rig;
    #endregion

    #region 이벤트
    // 자석 충돌시 발생할 이벤트
    public event System.Action MagnetCollision;
    // 자석에서 물체를 떨어트릴 때 발생할 이벤트
    public event System.Action MagnetPutDown;
    #endregion


    #region
    private void Awake()
    {
        rig = magnetTransform.GetComponent<Rigidbody>();

        State = ClawState.Move;

        MagnetCollision += MagnetLift;
        //MagnetCollision += DefaultSetting;
        MagnetCollision += RelaxProduct;

        MagnetPutDown += RemovePart;
    }


    private void Update()
    {
        moRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonUp(0))
        {
            State = ClawState.Fall;
            //
            //MagnetFall();
        }



        if (Input.GetKeyDown(KeyCode.Space))
        {
            MagnetLift();
        }

        if (Input.GetMouseButton(0))
        {
            ClawMove();
        }

        if (Input.GetMouseButtonDown(1))
        {
            MagnetPutDown?.Invoke();
        }
    }


    #region Catch & Release
    // 잡은 오브젝트 리스트에 추가
    public void AddBodyPart(GameObject obj)
    {
        Transform newpart = obj.transform;
        magnetParts.Add(newpart);
    }
    // 리스트 비우기
    void RemovePart()
    {
        magnetParts.Clear();
    }

    
    #endregion

    #region None
    // 기본 세팅으로 변경
    public void DefaultSetting()
    {
        IsTrigger = false;
        //SetBodyTrigger(false);
        RelaxProduct();
        magnetTransform.GetComponent<Collider>().isTrigger = false;
    }
    #endregion

    #region Move
    void ClawMove()
    {
        if (state == ClawState.None || state == ClawState.Move)
        {
            if (Physics.Raycast(moRay, out mousePosHit, Mathf.Infinity, UnderLayer))
            {
                State = ClawState.Move;
                mousePos = mousePosHit.point;

                Vector3 vec = new Vector3(mousePos.x, magnetTransform.position.y, mousePos.z);

                magnetTransform.position = vec;
                //Cable.position = new Vector3(mousePos.x, Cable.position.y, mousePos.z);
                mousePosMarker.position = new Vector3(mousePos.x, mousePos.y + mouseposOffsetFromGround, mousePos.z);
            }
        }
    }

    void ClawMoveStart()
    {
        mousePosMarker.gameObject.SetActive(true);
    }
    #endregion

    #region Fall
    // 자석 내리기
    void MagnetFall()
    {
        if (Physics.Raycast(magnetTransform.position, Vector3.up, out RoofHit, Mathf.Infinity, roofLayer))
        {
            roofTransform = RoofHit.transform;
        }

        rig.isKinematic = false;
        underTransform = null;        
    }
    //자석 올리기
    public void MagnetLift()
    {   
        rig.isKinematic = true;
        magnetTransform.GetComponent<Collider>().isTrigger = true;
        if (roofTransform != null)
        {
            Vector3 vec3 = new Vector3(Cable.position.x, roofTransform.position.y - offsetY, Cable.position.z);
            magnetTransform.DOMove(vec3, 2f).OnComplete(() => State = ClawState.None);
        }
        mousePosMarker.gameObject.SetActive(false);

        roofTransform = null;
    }
    // 오브젝트 회전력 줄이기
    public void RelaxProduct()
    {
        foreach (Transform t in magnetParts)
        {
            t.GetComponent<Rigidbody>().velocity = Vector3.zero;

        }
    }
    #endregion
    #endregion
}
