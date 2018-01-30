using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class positionController : MonoBehaviour {
    public Camera lead;
    Transform tf;
    private Vector3 dir;

    public float speed = .5f;
    // Update is called once per frame
    public void go_forward () {
        print("forward");
        dir = lead.GetComponent<Transform>().forward;
    }

    public void go_backward()
    {
        print("backwards");
        dir = -lead.GetComponent<Transform>().forward;
    }

    public void go_left()
    {
        print("left");
        dir = -lead.GetComponent<Transform>().right;
    }

    public void go_right()
    {
        print("right");
        dir = lead.GetComponent<Transform>().right;
    }

    public void stop()
    {
        dir = new Vector3(0, 0, 0);
    }

    private void Update()
    {
        tf.position = tf.position + dir * speed * Time.deltaTime;
        transform.position = new Vector3(transform.position.x, 1, transform.position.z);
    }

    private void Start()
    {
        tf = this.gameObject.GetComponent<Transform>();
        dir = new Vector3(0, 0, 0);
    }
}
