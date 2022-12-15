using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuckDibleScript : MonoBehaviour
{
    [SerializeField] int points = 10;

    public static Action<int> puckDibleAddToScore;
    public static Action powerPelletActivated;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            puckDibleAddToScore?.Invoke(points);

            if(this.CompareTag("PowerPellet"))
            {
                //Debug.Log("PowerPellet");
                powerPelletActivated?.Invoke();
            }

            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(collision.ToString());
    }
}
