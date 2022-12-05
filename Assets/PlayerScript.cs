using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] float speed;

    bool movementInXAxis = false;
    bool movementInYAxis = false;

    private void Start()
    {
        
    }

    private void FixedUpdate()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        Vector2 axisInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if ((axisInput.x > 0 || axisInput.x < 0) && !movementInYAxis)
        {
            transform.position = transform.position + new Vector3(axisInput.x * Time.deltaTime * speed, 0, 0);
            movementInXAxis = true;
            return;
        }
        else
        {
            movementInXAxis = false;
        }

        if ((axisInput.y > 0 || axisInput.y < 0) && !movementInXAxis)
        {
            transform.position = transform.position + new Vector3(0, axisInput.y * Time.deltaTime * speed, 0);
            movementInYAxis = true;
            return;
        }
        else
        {
            movementInYAxis = false;
        }
    }
}
