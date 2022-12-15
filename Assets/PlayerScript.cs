using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] float speed;

    bool movementInXAxis = false;
    bool movementInYAxis = false;

    Vector2 movement = Vector2.zero;

    private void Start()
    {
        
    }

    private void FixedUpdate()
    {
        CheckForNewDirection();
        UpdatePosition();
    }

    private void CheckForNewDirection()
    {
        Vector2 axisInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (axisInput.x > 0 && !movementInYAxis)
        {
            movementInXAxis = true;
            movement.x = 1;
            movement.y = 0;
            return;
        }
        else if(axisInput.x < 0 && !movementInYAxis)
        {
            movementInXAxis = true;
            movement.x = -1;
            movement.y = 0;
            return;
        }
        else
        {
            movementInXAxis = false;
        }

        if (axisInput.y > 0 && !movementInXAxis)
        {
            movementInYAxis = true;
            movement.y = 1;
            movement.x = 0;
            return;
        }
        else if (axisInput.y < 0 && !movementInXAxis)
        {
            movementInYAxis = true;
            movement.y = -1;
            movement.x = 0;
            return;
        }
        else
        {
            movementInYAxis = false;
        }
    }

    private void UpdatePosition()
    {
        transform.position += new Vector3(movement.x * Time.deltaTime * speed, movement.y * Time.deltaTime * speed, 0);

    }
}
