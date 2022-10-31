using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleAutoDestroyer : MonoBehaviour
{
    private ParticleSystem particle;

    private void Awake()
    {
        particle = GetComponent<ParticleSystem>(); //particleSystem을 가져온다
    }
    private void Update()
    {
        //파티클이 재생중이 아니면 삭제
        if (particle.isPlaying == false)
        {
            Destroy(gameObject);
        }
    }
}

/*현재 게임에서는 오브젝트에 하나의 파티클만 재생중이지만
  여러개의 파티클을 하나의 그룹으로 묶어서 사용할때는 시간이 가장 긴 파티클의 재생
  여부를 검사하거나 파티클 재생이 종료되어도 파티클이 삭제되면 안될 경우에는 시간을 기준으로 
  지우기도 한다
*/