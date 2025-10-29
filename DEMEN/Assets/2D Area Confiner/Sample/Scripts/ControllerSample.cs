using GBTools.Area2DConfiner;
using UnityEngine;

public class ControllerSample : MonoBehaviour
{
    public GBArea2DConfiner confiner;
    public float moveSpeed = 3f;

    void Update()
    {
        //Get next step movement with axis
        var x = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        var y = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        var desiredPosition = transform.position + new Vector3(x, y);

        //Confine position desired on confinedAREA
        var confinedPosition = confiner.ConstrainsTransform(desiredPosition, transform);

        //Set Position to confinded position
        transform.position = confinedPosition;
    }
}
