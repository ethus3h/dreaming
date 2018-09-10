using UnityEngine;

public class MovingObstacleController : MonoBehaviour {

    int step = 0;
    int getBigger = 0;
    private Rigidbody rb;
    private Vector3 pos;
    public float ldelta = 0.1f;

    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        pos = transform.position;
    }

    // Update is called once per frame
    void Update () {
        float delta = ldelta;
        if(getBigger == 0)
        {
            delta = -1 * delta;
        }
        if (step < -80)
        {
            getBigger = 1;
        }
        if (step > 160)
        {
            getBigger = 0;
        }
        if (delta < 0)
        {
            step = step - 1;
        }
        else
        {
            step = step + 1;
        }
        rb.MovePosition(new Vector3(rb.position.x, rb.position.y, rb.position.z + delta));
        if (step == 0 && getBigger == 1)
        {
            transform.position = pos;
        }
        /* // Tries to prevent it jumping when it hits zero by adding extra checks - seems to be working fine without it now though *shrug*
         if ( ( (rb.position.z > (pos.z + (160 * delta) + 1)) || (rb.position.z < (pos.z + (-80 * delta) - 1)) ) && step == 0 )
        {
            transform.position = pos;
        } */
        /* Debug.Log(step);
        Debug.Log(delta);
        Debug.Log(getBigger); */
    }
}
