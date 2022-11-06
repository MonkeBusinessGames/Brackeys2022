using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeController : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private LineRenderer line;

    private void Start()
    {
        rb.velocity = new Vector2(speed/2, 0);
    }

    private void Update()
    {
        line.SetPosition(1, rb.transform.localPosition);
        rb.transform.up = new Vector2(transform.position.x - rb.transform.position.x, transform.position.y - rb.transform.position.y);
    }

    void FixedUpdate()
    {
        if (rb.velocity != Vector2.zero)
        {
            return;
        }
        else
        {
            speed *= -1;
            rb.velocity = new Vector2(speed, 0);

        }

    }
}
