using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [SerializeField]
    private GameController gameController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Obstacle"))
        {
            gameController.GameOver();
        }
        else if (other.tag.Equals("Coin"))
        {
            gameController.IncreaseCoinCount();
        }
    }
}
