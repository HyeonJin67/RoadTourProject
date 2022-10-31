using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PipeLineHandler : MonoBehaviour
{
    #region º¯¼ö
    [SerializeField]
    List<GameObject> Cube = new List<GameObject>();    
    [SerializeField]
    Transform CubeSeed;
    [SerializeField]
    GameObject CubePre;
    [SerializeField]
    int CubeLine;

    private int[] indexs;
    #endregion

    private void Start()
    {
        float seed = CubeLine / 2.0f;
        CubeSeed.position -= new Vector3(seed, seed, 0);

        InitCube();
    }

    public void InitCube()
    {
        float x = 0;
        float y = 0 ;

        for(int i = 0; i < CubeLine; i++)
        {   
            y = i;
            y *= CubePre.transform.localScale.y;

            for (int j = 0; j < CubeLine; j++)
            {
                x = j * CubePre.transform.localScale.x * 1.2f;

                GameObject cube = Instantiate(CubePre, CubeSeed);
                cube.transform.position = new Vector3(x, y, 0) + CubeSeed.position;
                cube.name = $"({j} , {i})";                
                Cube.Add(cube);
            }
        }
        PipeSetting();
    }

    public void PipeSetting()
    {
        var array = System.Linq.Enumerable.Range(0, Cube.Count);
        indexs = array.OrderBy(x => Random.value).ToArray();

        for(int i = 0; i < CubeLine; i++)
        {
            GameObject NullCube = Cube[indexs[i]];
            NullCube.GetComponentInChildren<Renderer>().material.color = Color.black;
        }
    }


}
