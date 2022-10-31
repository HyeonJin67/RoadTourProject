using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//테스트용 스크립트
public class Move_Test : MonoBehaviour
{
	Enemy mySelf;

    private void Start()
    {
		mySelf = new Enemy();

		mySelf.hp = 100;
		mySelf.attack = 50;
		mySelf.speed = 0.5f;
		mySelf.name = "Golin";
    }
    private void OnMouseDown()
    {
        print("Name:" + mySelf.name); ;
		print("HP:"+mySelf.hp);
		print("Attack : " + mySelf.attack);
		print("Speed: " + mySelf.speed);
    }
}
public class Enemy
{
	public string name { get; set; }	
	public int hp { get; set; }	
	public int attack { get; set; }	
	public float speed { get; set; }
}

