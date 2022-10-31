using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DRunPlayerController : MonoBehaviour
{
    #region 변수
    Animator animator;
    Vector3 originRoation;
    private Vector3 screenSpace;
    private Vector3 offset;
    bool mouse = false;

    [SerializeField]
    Transform startPoint;

    [SerializeField]
    AudioClip clip;

    
    StartTutorialHandler tuto;
    DRunNpcMoveController npcController;
    DRunFlowerBloomController bloom;
    ScoreHandler score;
    SpeakerHandler speaker;
    Canvas Canvas;

    float firingAngle = 45.0f;
    float gravity = 9.8f;
    Transform player;
    Transform myTransform;
    bool gameStart = false;

    [SerializeField]
    GameObject FindParticle;
    #endregion


    private void Start()
    {
        transform.position = startPoint.position;
        animator = GetComponentInChildren<Animator>();
        originRoation = transform.rotation.eulerAngles;
        bloom = FindObjectOfType<DRunFlowerBloomController>();
        Canvas = FindObjectOfType<Canvas>();
        player = transform;
        myTransform = transform;        
        score = FindObjectOfType<ScoreHandler>();
        speaker = FindObjectOfType<SpeakerHandler>();
        npcController = FindObjectOfType<DRunNpcMoveController>();
        score.SceneComplete += PlayerComplete;
        tuto = FindObjectOfType<StartTutorialHandler>();


        SetState(0);
    }


    private void OnMouseDown()
    {
        if (gameStart)
        {
            mouse = true;
            screenSpace = Camera.main.WorldToScreenPoint(transform.position);
            offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z));            
            speaker.SoundByClip(clip);            
            SetState(3);
        }
    }
    
    private void OnMouseDrag()
    {
        if (mouse)
        {
            var curScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z);
            var curPosition = Camera.main.ScreenToWorldPoint(curScreenSpace) + offset;
            transform.position = curPosition;
            SetState(1);
            tuto.StartGame();

        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (mouse)
        {
            var curScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z);
            var curPosition = Camera.main.ScreenToWorldPoint(curScreenSpace) + offset;
            transform.position = curPosition;

            if (other.transform.GetComponent<DRunObjectController>())
            {
                if (other.transform.GetComponent<DRunObjectController>().GetParam() == bloom.GetTargetNum())
                {
                    other.transform.GetChild(0).GetComponent<SpriteRenderer>().color = Color.white;

                    var par = Instantiate(FindParticle);
                    par.transform.position = other.transform.position;
                    score.SoundPlay(0);
                    other.transform.GetComponent<DRunObjectController>().Detection();
                    other.GetComponent<Collider>().enabled = false;
                    npcController.PlayerEntry();
                }
                else if (other.transform.GetComponent<DRunObjectController>().GetParam() != bloom.GetTargetNum())
                {
                    StartCoroutine(SimulateProjectile());
                    mouse = false;
                }
            }
        }
    }
    //마우스 떨어질 시 state 변경
    private void OnMouseUp()
    {
        if (mouse)
        {
            mouse = false;
            SetState(0);
        }
    }

    IEnumerator SimulateProjectile()
    {
        SetState(2);
        score.SoundPlay(2);
        player.GetComponent<Collider>().enabled = false;
        player.position = myTransform.position + new Vector3(0, 0.0f, 0);

        float target_Distance = Vector3.Distance(player.position, startPoint.position);
        float projectile_Velocity = target_Distance / (Mathf.Sin(2 * firingAngle * Mathf.Deg2Rad) / gravity);

        // 속도 값
        float Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(firingAngle * Mathf.Deg2Rad);
        float Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(firingAngle * Mathf.Deg2Rad);
        // 체공 시간
        float flightDuration = target_Distance / Vx;

        player.rotation = Quaternion.LookRotation(startPoint.position - player.position);

        float elapse_time = 0;

        while (elapse_time < flightDuration)
        {
            player.Translate(0, (Vy - (gravity * elapse_time)) * Time.deltaTime, Vx * Time.deltaTime);
            elapse_time += Time.deltaTime;
            yield return null;
        }
        player.rotation = Quaternion.Euler(originRoation);
        player.GetComponent<Collider>().enabled = true;
        SetState(0);
        yield break;
    }
    public void PlayerComplete()
    {
        mouse = false;
        SetState(3);
        player.GetComponent<Collider>().enabled = false;
    }

    #region 파라미터 함수
    public void SetState(int num)
    {
        animator.SetInteger("State", num);
    }
    public void SetGameStart(bool bol)
    {
        gameStart = bol;
    }
    #endregion
}
