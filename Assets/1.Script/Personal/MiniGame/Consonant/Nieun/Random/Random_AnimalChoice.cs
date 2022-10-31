using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Random_AnimalChoice : MonoBehaviour
{
    //Random_Animal 직렬화 
    [SerializeField]
    Random_Animal Animal;
    //몇번째 동물인지 number숫자로 체크
    public int number;
    //부모의 부모 Random_Animal 스크립트의 랜덤 배열 번호에 따라 자기 자신 밑의 오브젝트 활성화
    private void Start()
    {
        Animal = transform.parent.GetComponentInParent<Random_Animal>();
        transform.GetChild(Animal.shuffledArray[transform.parent.GetSiblingIndex()]).gameObject.SetActive(true);
        number = Animal.shuffledArray[transform.parent.GetSiblingIndex()];
    }
}
