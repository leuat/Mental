using UnityEngine;

// Very simple smooth mouselook modifier for the MainCamera in Unity
// by Francis R. Griffiths-Keam - www.runningdimensions.com
// http://forum.unity3d.com/threads/a-free-simple-smooth-mouselook.73117/


public class MouseLook : MonoBehaviour
{

    public Vector3 mousePos, mouseAcc;
    public GameObject player;

    private float movesSpeedStrife = 0;
    private float movesSpeedForward = 0;

    public void Start()
    {

        mousePos = Input.mousePosition;
    }



    public void Update()
    {
        Vector3 mouseDeltaAcc = mousePos - Input.mousePosition;
        mouseAcc += mouseDeltaAcc;
        mouseAcc *= 0.9f;
        mousePos = Input.mousePosition;

        float moveSpeed = 0.0025f;
        float mouseMoveSpeed = 0.025f;


        //        transform.Rotate(transform.right, -mouseAcc.y*s, Space.Self);
        //      transform.Rotate(transform.up, -mouseAcc.x*s, Space.Self);

        transform.Rotate(0, -mouseAcc.x*mouseMoveSpeed, 0,Space.Self);
        transform.Rotate(-mouseAcc.y * mouseMoveSpeed, 0, 0, Space.Self);

        float z = transform.eulerAngles.z;
        if (z < -180) z += 360;
        if (z > 180) z -= 360;
        transform.Rotate(0, 0, -z*0.2f, Space.Self);


        if (Input.GetKey(KeyCode.D))
            movesSpeedStrife += 1f;
        if (Input.GetKey(KeyCode.A))
            movesSpeedStrife -= 1f;
        if (Input.GetKey(KeyCode.W))
            movesSpeedForward += 1f;
        if (Input.GetKey(KeyCode.S))
            movesSpeedForward -= 1f;
        movesSpeedStrife = Mathf.Clamp(movesSpeedStrife, -10, 10);
        movesSpeedForward = Mathf.Clamp(movesSpeedForward, -10, 10);

        movesSpeedForward *= 0.9f;
        movesSpeedStrife *= 0.9f;

        player.gameObject.transform.position += transform.right * movesSpeedStrife*moveSpeed;
        player.gameObject.transform.position += transform.forward * movesSpeedForward*moveSpeed;


    }
}