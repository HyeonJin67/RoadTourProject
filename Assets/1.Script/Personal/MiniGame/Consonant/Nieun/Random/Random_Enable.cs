using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Random_Enable : MonoBehaviour
{
    Index myIndex = new Index();  
    //랜덤하게 오브젝트 활성화 
    [SerializeField]
    List<GameObject> gameObjects = new List<GameObject>();
    public int[] shuffledArray; //랜덤 배열
    private void Awake()
    {
        var indexArray = Enumerable.Range(0, gameObjects.Count).ToArray();//gameObjects 갯수만큼 랜덤
        System.Random random = new System.Random();
        shuffledArray = indexArray.OrderBy(x => random .Next()).ToArray();
        Choice();
    }
    public void Choice()//동물이 하나씩 활성화 된다.
    {
        if (myIndex.index < 5)
        {
            gameObjects[shuffledArray[myIndex.index]].SetActive(true);
            myIndex.index++;
        }
    }
}
public class Index
{
    public int index;
    public int Number
    {
        get => index;

        set => index = value;   
    }
}

