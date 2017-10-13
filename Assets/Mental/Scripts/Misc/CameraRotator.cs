using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LemonSpawn
{

    public class CameraRotator : MonoBehaviour
    {
        public Vector3 cameraPos, cameraAdd, cameraRotate = new Vector3(0, 0, 1);
        public Vector3 rotatePlaneAdd, rotatePlane;
        public GameObject hitObject;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            UpdateCameraRotate(1);
        }

        void UpdateCameraRotate(float s)
        {

            bool ok = false;
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, r.direction, out hit))
            {
                if (hit.transform.gameObject == hitObject)
                    ok = true;
            }
            if (!ok)
                return;
            if (Input.GetMouseButton(1))
            {
                cameraAdd.x = 2 * s * Input.GetAxis("Mouse X") * -1f;
                cameraAdd.y = 2 * s * Input.GetAxis("Mouse Y") * -1.0f;
            }
            cameraAdd.z += 0.2f * s * Input.GetAxis("Mouse ScrollWheel") * -1.0f;
            cameraPos += cameraAdd;
            cameraAdd *= 0.9f;


            if (Input.GetMouseButton(0))
            {
                rotatePlaneAdd.x = 2 * s * Input.GetAxis("Mouse X") * -1f;
                rotatePlaneAdd.y = 2 * s * Input.GetAxis("Mouse Y") * -1.0f;
            }
            rotatePlane += rotatePlaneAdd;
            rotatePlaneAdd *= 0.8f;
            rotatePlane *= 0.8f;

            cameraRotate = Vector3.left;
            cameraRotate = Quaternion.AngleAxis(cameraPos.x, Vector3.up) * cameraRotate;
            cameraRotate = Quaternion.AngleAxis(cameraPos.y, Vector3.Cross(cameraRotate, Vector3.up).normalized) * cameraRotate;

            //            vParams.splitPlane = 

        }

        public Vector3 getSplitPlane(Vector3 plane)
        {
            return Quaternion.Euler(rotatePlane.x, rotatePlane.y, 0) * plane; 
        }


    }
}
