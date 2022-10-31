using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ScoreHandler : MonoBehaviour
{
    #region 변수
    [SerializeField]
    int MaxScore;
    int curScore = 0;

    [Header("화면 잠금")]
    [SerializeField]
    GameObject ScreenSaver;
    
    [Header("스코어 변화 UI")]
    [SerializeField]
    Sprite scoreCase;
    [SerializeField]
    Sprite scoreFill;
    [SerializeField]
    GameObject fillParticle;

    [Header("오디오 클립")]
    [SerializeField]
    AudioClip[] clips;    
    AudioSource sources;

    [Header("클리어 파티클")]
    [SerializeField]
    GameObject[] particles;
    [Header("파티클이 터질 위치와 크기")]
    public int ParticleDistance = 10;
    public int ParticleScale = 5;

    bool CompleteCheck = false;
    private Transform EndPoint;
    #endregion

    #region 이벤트
    public event System.Action SceneComplete;
    public event System.Action SceneStart;
    #endregion

    #region 함수
    private void Awake()
    {
        sources = GetComponent<AudioSource>();

        for (int i = 0; i < MaxScore; i++)
        {
            GameObject gg = new GameObject();
            gg.AddComponent<UnityEngine.UI.Image>();
            gg.transform.SetParent(transform);
            gg.transform.localScale = Vector3.one * 1.5f;
            gg.transform.position = transform.position;
            gg.transform.localRotation = Quaternion.Euler(Vector3.zero);
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).GetComponent<UnityEngine.UI.Image>() != null)
            {
                transform.GetChild(i).GetComponent<UnityEngine.UI.Image>().sprite = scoreCase;
            }
        }
        EndPoint = MakeFireFlowerPoint();

        SceneComplete += Comp;
        SceneStart += OnScreenSaver;
    }

    public void SetScore()
    {   
        var filled = Instantiate(fillParticle);
        filled.transform.position = transform.GetChild(curScore).position;

        transform.GetChild(curScore).GetComponent<UnityEngine.UI.Image>().sprite = scoreFill;
        transform.GetChild(curScore).GetComponent<UnityEngine.UI.Image>().DOFade(1, 4f).From(0);

        if(curScore < MaxScore - 1)
        {
            SoundPlay(0);
            curScore++;
        }
        else
        {
            SoundPlay(1);
            CompleteCheck = true;
            OnComplete();
        }
    }
    // 감점
    public void RemoveScore()
    {   
        curScore--;
        transform.GetChild(curScore).DOComplete();
        transform.GetChild(curScore).GetComponent<UnityEngine.UI.Image>().DOFade(0.5f, 1f).From(1).OnComplete(() =>
            { transform.GetChild(curScore).GetComponent<UnityEngine.UI.Image>().sprite = scoreCase;
              transform.GetChild(curScore).GetComponent<UnityEngine.UI.Image>().DOFade(1, 2f).From(0.5f);
            }
        );
    }

    // 컴플리트 이벤트 실행하기
    public void OnComplete()
    {
        SceneComplete?.Invoke();
    }
    //클리어 파티클 터트리기
    IEnumerator ClearParticle()
    {
        while (true)
        {
            int num = Random.Range(4, 9);
            for (int i = 0; i < num; i++)
            {
                var ex = Instantiate(particles[Random.Range(0, particles.Length)]);
                Vector3 subPos = EndPoint.transform.position +
                    Camera.main.transform.right * Random.Range(-8, 8) +
                    Camera.main.transform.up * Random.Range(-4, 4);

                ex.transform.position = subPos;
                ex.transform.localScale = Vector3.one * ParticleScale;
                ex.transform.SetParent(EndPoint);
            }
            yield return new WaitForSeconds(1f);
        }        
    }

    // 카메라 앞쪽 지점에서 위치시키기
    public Transform MakeFireFlowerPoint()
    {
        GameObject TestCube = new GameObject();        
        TestCube.transform.position = Camera.main.transform.position + Camera.main.transform.forward * ParticleDistance;
        TestCube.transform.rotation = new Quaternion(0.0f, Camera.main.transform.rotation.y, 0.0f, Camera.main.transform.rotation.w);        
        
        return TestCube.transform;
    }
    #endregion

    #region
    public void SoundPlay(int num)
    {
        if (num < clips.Length)
        {
            sources.PlayOneShot(clips[num]);
        }
    }

    public void Comp()
    {
        OnScreenSaver();
        StartCoroutine(ClearParticle());
    }

    public void OnScreenSaver()
    {
        if(ScreenSaver != null)
        {
            ScreenSaver?.SetActive(true);
        }        
    }

    public void OffScreenSaver()
    {
        if (ScreenSaver != null)
            ScreenSaver?.SetActive(false);
    }

    public bool CompCheck()
    {
        return CompleteCheck;
    }
        
    #endregion
}
