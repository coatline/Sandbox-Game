using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowWithBarriers : MonoBehaviour
{
    [Header("Automatically offsets barrier positions")]
    public Transform bottomLeftBarrier;
    public Transform topRightBarrier;
    public Transform followObject;
    public Vector2 cameraSizeInUnits;


    [Range(.01f, 1f)]
    [SerializeField] float speed;

    private void Awake()
    {
        var cam = GetComponent<Camera>();

        cameraSizeInUnits.x = cam.orthographicSize * cam.aspect;
        cameraSizeInUnits.y = cam.orthographicSize;
    }

    private void Start()
    {
        bottomLeftBarrier.transform.position += new Vector3((cameraSizeInUnits.x), cameraSizeInUnits.y, 0);
        topRightBarrier.transform.position -= new Vector3((cameraSizeInUnits.x), cameraSizeInUnits.y, 0);
    }

    void LateUpdate()
    {
        if (!bottomLeftBarrier) { return; }

        Vector3 movement = new Vector3(followObject.position.x - transform.position.x, followObject.position.y - transform.position.y);

        if (transform.position.x <= bottomLeftBarrier.position.x)
        {
            if (movement.x < 0)
            {
                transform.position = new Vector3(bottomLeftBarrier.position.x, transform.position.y, -10);
                movement.x = 0;
            }
        }
        if (transform.position.y <= bottomLeftBarrier.position.y)
        {
            if (movement.y < 0)
            {
                transform.position = new Vector3(transform.position.x, bottomLeftBarrier.position.y, -10);
                movement.y = 0;
            }
        }
        if (transform.position.x >= topRightBarrier.position.x)
        {
            if (movement.x > 0)
            {
                transform.position = new Vector3(topRightBarrier.position.x, transform.position.y, -10);
                movement.x = 0;
            }
        }
        if (transform.position.y >= topRightBarrier.position.y)
        {
            if (movement.y > 0)
            {
                transform.position = new Vector3(transform.position.x, topRightBarrier.position.y, -10);
                movement.y = 0;
            }
        }

        transform.Translate(movement * speed);
    }
}
