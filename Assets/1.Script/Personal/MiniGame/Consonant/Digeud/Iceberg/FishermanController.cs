using UnityEngine;
// 낚시꾼 컨트롤러
public class FishermanController : MonoBehaviour
{
    #region 변수
    bool EndGame = false;
    // 무언가를 잡았다
    public bool grabbed = false;
    // 기본 레이
    Ray moRay;
    //무엇을 잡았고, 땅은 어느 것인가?
    public Transform catchTransform;
    public Transform ground;
    // 잡을 것의 레이어와 땅의 레이어
    public LayerMask catchLayer;
    public LayerMask groundLayer;
    // 잡기위한 레이와 땅의 레이
    RaycastHit hit;
    RaycastHit groundHit;

    // 마우스 마커 관련 설정들
    public Transform mousePosMarker;
    RaycastHit mousePosHit;
    // 잡힌 물체의 높이
    public float offsetY = 0;
    // 마커의 높이
    public float mouseposOffsetFromGround = 0;
    public Vector3 mousePos;
    // 물 확인
    public Collider water;


    ScoreHandler score;
    #endregion
    #region
    private void Start()
    {
        hit = new RaycastHit();
        groundHit = new RaycastHit();
        mousePosHit = new RaycastHit();

        score = FindObjectOfType<ScoreHandler>();
        score.SceneComplete += EndSetting;
    }

    private void Update()
    {
        moRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            CatchObject();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (!EndGame)
            {
                DropObject();
            }
        }

        grabbed = catchTransform != null;
        mousePosMarker.gameObject.SetActive(grabbed);

        if (grabbed)
        {
            TraceMousePostion();
        }
    }
    // 오브젝트 잡기
    void CatchObject()
    {
        if (EndGame) { return; }
        if (Physics.Raycast(moRay, out hit, Mathf.Infinity, catchLayer))
        {
            
            catchTransform = hit.transform;
            if (catchTransform.GetComponent<FishHandler>())
            {
                if(catchTransform.GetComponent<FishHandler>().State == FishState.OnIce)
                {
                    score.RemoveScore();
                }
                catchTransform.GetComponent<FishHandler>().State = FishState.Catch;
            }
            FindGround();
            water.enabled = true;
        }
    }
    // 땅을 찾기
    void FindGround()
    {
        if (Physics.Raycast(catchTransform.position, Vector3.down, out groundHit, Mathf.Infinity, groundLayer))
        {
            ground = groundHit.transform;
        }
    }

    void TraceMousePostion()
    {
        if (Physics.Raycast(moRay, out mousePosHit, Mathf.Infinity, groundLayer))
        {
            mousePos = mousePosHit.point;
            catchTransform.position = new Vector3(mousePos.x, mousePos.y + offsetY, mousePos.z);
            mousePosMarker.position = new Vector3(mousePos.x, mousePos.y + mouseposOffsetFromGround, mousePos.z);
        }
    }
    // 오브젝트 떨어트리기
    void DropObject()
    {   
        if (catchTransform != null)
        {
            catchTransform.GetComponent<FishHandler>().State = FishState.Hook;
        }

        catchTransform = null;
        ground = null;

        water.enabled = false;
    }

    void EndSetting()
    {
        DropObject();
        EndGame = true;
    }
    #endregion

    #region
    private RaycastHit CastRay()
    {
        Vector3 screenMousePosFar = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.farClipPlane);
        Vector3 screenMousePosNear = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
        Vector3 wordMousePosFar = Camera.main.ScreenToWorldPoint(screenMousePosFar);
        Vector3 wordMousePosNear = Camera.main.ScreenToWorldPoint(screenMousePosNear);

        RaycastHit hit;
        Physics.Raycast(wordMousePosNear, wordMousePosFar - screenMousePosNear, out hit);

        return hit;
    }
    #endregion

}
