using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpTunnel : MonoBehaviour
{
    [SerializeField] GameObject partnerWarpTunnel;
    public float direction;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Ghost"))
        {
            collision.transform.position = partnerWarpTunnel.transform.position + new Vector3(direction, 0.0f, 0.0f);
        }
    }

    public void SetPartner(GameObject partner)
    {
        partnerWarpTunnel = partner;
    }
}
