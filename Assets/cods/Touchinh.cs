using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Touchinh : MonoBehaviour


{
    bool moveable;
    public GameObject particlesystem;
    Collider2D c1ollider2D;    // Start is called before the first frame update
    void Start()
    {
        c1ollider2D = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchpos = Camera.main.ScreenToWorldPoint(touch.position);
            if (touch.phase == TouchPhase.Began)
            {
                Collider2D touchedthing = Physics2D.OverlapPoint(touchpos);

                if (c1ollider2D == touchedthing)
                {
                    moveable = true;
                    if (particlesystem != null){Instantiate(particlesystem,transform.position,transform.rotation);
                }
                }
            }
            if (touch.phase == TouchPhase.Moved)
            {
                if (moveable)
                {
                    transform.position = touchpos;
                }


            }
            if (touch.phase == TouchPhase.Ended)
            {
                moveable = false;
            }


        }
    }
}
