using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hammer : MonoBehaviour
{
    [SerializeField]
    public float maxY;       //망치의 최대 y위치
    [SerializeField]
    public float minY;       //망치의 최소 y위치
    [SerializeField]
    private GameObject moleHitEffectPrefab;     //두더지 타격 효과 프리팹
    [SerializeField]
    private AudioClip[] audioClips;             //두더지를 타격했을 떄 재생되는 사운드
    public ObjectDetector objectDetector;       //마우스 클릭으로 오브젝트 선택을 위한 ObjectDetector 
    private Movement3D movement3D;              //망치 이동을위한 movement
    private AudioSource audioSource;            //두더지를 타격했을 때 소리를 재생하는 AudioSource


    private void Awake()
    {
        movement3D = GetComponent<Movement3D>();
        audioSource = GetComponent<AudioSource>();  
        //OnHit 메소드를 objectDetector Class의 raycast에 이벤트로 등록
        //objectDetector의 raycastEvent.Invoke(hit.transform)메소드가
        //호출될때 마다Onhit 메소드가 호출된다.
        objectDetector.raycastEvent.AddListener(OnHit);
    }

    private void OnHit(Transform target)
    {
        if (target.CompareTag("Mole"))

        { //타겟 두더지의 컴포넌트 가져오기
            MoleFSM mole = target.GetComponent<MoleFSM>();

            //두더지의 상태가 홀 안에있을때는 공격 불가
            if (mole.MoleState == MoleState.UnderGround)
                return;

            //망치의 위치 설정
            transform.position = new Vector3(target.position.x, minY, target.position.z);

            //두더지는 맞았으니 언더그라운드 상태로 변경
            mole.ChangeState(MoleState.UnderGround);

            //카메라 흔들기
            ShakeCamera.Instance.OnShakeCamera(0.1f, 0.1f);

            //두더지 타격 효과 생성 (particle의 색상을 두더지 색상과 동일하게 설정)
            GameObject clone = Instantiate(moleHitEffectPrefab, transform.position, Quaternion.identity);
            ParticleSystem.MainModule main = clone.GetComponent<ParticleSystem>().main;
            main.startColor = mole.GetComponent<MeshRenderer>().material.color;

            //점수 증가 (+50)
            //gameController.Score += 50;
            //두더지 색상에 따라 처리 (점수 , 시간 , 사운드재생)
            MoleHitProcess(mole);

            //망치를 다시 위로 이동시키는 코루틴함수 재생
            StartCoroutine("MoveUp");
        }
    }

    private IEnumerator MoveUp()
    {
        //이동방향은 위
        movement3D.MoveTo(Vector3.up);

        while (true)
        {
            if (transform.position.y >= maxY)
            {
                movement3D.MoveTo(Vector3.zero);
                break;

            }

            yield return null;
        }
    }
    private void MoleHitProcess(MoleFSM mole)
    {
       //사운드 재생 (Normal = 0 , Red = 1 , Blue = 2)
      PlaySound((int)mole.MoleType);
    }
    private void PlaySound(int index)
    {
        audioSource.Stop();
        audioSource.clip = audioClips[index];   
        audioSource.Play();
    }

}