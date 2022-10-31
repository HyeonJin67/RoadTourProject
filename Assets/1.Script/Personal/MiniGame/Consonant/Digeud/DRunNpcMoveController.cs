using DG.Tweening;
using System.Collections;
using System.Linq;
using UnityEngine;

public class DRunNpcMoveController : MonoBehaviour
{
    #region 변수    
    [SerializeField]
    Transform startPoint, endPoint, subPoint;
    DRunFlowerBloomController flower;
    StartTutorialHandler startTutorialHandler;

    int[] indexArray, subArray;
    bool isMove = false;

    [SerializeField]
    AudioClip[] clips;
    [SerializeField]
    AudioClip[] nav;

    Animator anim;

    float firingAngle = 45.0f;
    float gravity = 9.8f;

    Vector3 oriRot;

    ScoreHandler score;
    SpeakerHandler speaker;

    [SerializeField]
    GameObject particle;
    AudioSource NPCAudio;

    DRunPlayerController player;
    

    bool comp = false;
    #endregion



    private void Start()
    {
        NPCAudio = GetComponent<AudioSource>();
        player = FindObjectOfType<DRunPlayerController>();
        startTutorialHandler = FindObjectOfType<StartTutorialHandler>();


        transform.position = startPoint.position;
        flower = FindObjectOfType<DRunFlowerBloomController>();
        indexArray = flower.GetIndexArray();
        subArray = indexArray.OrderBy(x => Random.value).ToArray();
        InitMovement();
        anim = GetComponentInChildren<Animator>();
        oriRot = transform.eulerAngles;
        
        score = FindObjectOfType<ScoreHandler>();
        speaker = FindObjectOfType<SpeakerHandler>();

        //이벤트 연결
        score.SceneComplete += Complete;
    }

    //시작시 진행해야 할 것들
    public void InitMovement()
    {
        StartCoroutine(SneakDotori());
    }
    IEnumerator SneakDotori()
    {
        NPCAudio.PlayOneShot(nav[0]);
        for (int i = 0; i < subArray.Length; i++)
        {
            yield return new WaitForSeconds(0.2f);

            float target_Distance = Vector3.Distance(transform.position, flower.transform.GetChild(i).position);
            float projectile_Velocity = target_Distance / (Mathf.Sin(2 * firingAngle * Mathf.Deg2Rad) / gravity);
            float Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(firingAngle * Mathf.Deg2Rad);
            float Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(firingAngle * Mathf.Deg2Rad);
            float flightDuration = target_Distance / Vx;

            transform.DOMove(flower.transform.GetChild(i).position, flightDuration).SetEase(Ease.OutQuad).From(transform.position)
                .OnStart(() => {
                    isMove = true; SetState(0);
                    speaker.SoundByClip(clips[0]);
                    transform.DOLookAt(flower.transform.GetChild(i).position, 0.2f);
                }).OnComplete(() => { isMove = false; });
            yield return new WaitForSeconds(0.2f);
            yield return new WaitWhile(() => isMove);
        }
        yield return new WaitForSeconds(0.3f);
        transform.DOMove(endPoint.position, 1f).SetEase(Ease.OutQuad).From(transform.position).
            OnStart(() => { transform.DOLookAt(endPoint.position, 0.2f); speaker.SoundByClip(clips[0]); }).
            OnComplete(() => {
                SetState(1); transform.rotation = Quaternion.Euler(oriRot);
                HelpSign();
            });
        yield break;
    }

    public void Complete()
    {
        comp = true;
        NPCAudio.PlayOneShot(nav[3]);
        SetState(5);
    }

    //조건이 맞을 경우 도토리!
    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<DRunObjectController>())
        {
            if (other.GetComponent<DRunObjectController>().GetParam() == flower.GetTargetNum())
            {
                if (!isMove && other.GetComponent<DRunObjectController>().GetActive())
                {
                    other.GetComponent<DRunObjectController>().SetActive(false);
                    SetState(2);
                    other.GetComponentInChildren<DRunDotoriHandler>().Pop();                    
                    NPCAudio.PlayOneShot(clips[1]);
                    var par = Instantiate(particle);
                    par.transform.position = transform.position;
                }
            }
        }
    }


    public void HelpSign()
    {
        NPCAudio.PlayOneShot(nav[1]);
        transform.DOComplete();
        transform.DOKill();
        transform.GetComponent<Collider>().enabled = false;
        transform.DOJump(subPoint.position, 2, 1, 1).SetLoops(-1, LoopType.Yoyo);
        SetState(7);
        StartCoroutine(stay());
    }

    IEnumerator stay()
    {
        yield return new WaitWhile(() => NPCAudio.isPlaying);
        NPCAudio.PlayOneShot(nav[2]);
        startTutorialHandler.ReadyGame();
        player.SetGameStart(true);

        yield break;
    }

    public void PlayerEntry()
    {
        transform.DOComplete();
        transform.DOKill();
        SetState(0);
        transform.DOMove(endPoint.position, 1f).SetEase(Ease.OutQuad).From(transform.position).OnComplete(() => Compact());
    }
    public void Compact()
    {
        if (comp)
        {
            SetState(5);
        }
        else
        {
            SetState(1);
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
    #region 파라미터 함수
    public void SetState(int num)
    {
        anim.SetInteger("State", num);
    }
    #endregion

}
