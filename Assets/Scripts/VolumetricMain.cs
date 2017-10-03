using UnityEngine;
using System.Collections;



namespace LemonSpawn
{



    public class VolumetricMain : MonoBehaviour
    {
        [SerializeField]
        [Range(0, 2)]
        private float opacity = 1;
        [SerializeField]
        [Range(0, 1)]
        private float cutoff = 0.15f;
        [SerializeField]
        [Range(0, 100)]
        private float shininess = 50f;
        [SerializeField]
        [Range(0, 4)]
        private float renderType = 0f;
        [SerializeField]
        [Range(-1, 1)]
        private float splitPosX = -1f;
        [SerializeField]
        [Range(-1, 1)]
        private float splitPosY = -1f;
        [SerializeField]
        [Range(-1, 1)]
        private float splitPosZ = -1f;

        [SerializeField]
        private Vector4 clipDimensions = new Vector4(100, 100, 100, 0);

        public float FOV = 70;


        // Use this for initialization
        public GameObject plane;
        public Material rayMarchMat;
        public Vector3 lightDir = new Vector3(1, 1, 0).normalized;

        public Vector3 rayCamera, rayTarget;
        private Matrix4x4 ViewMat;

        public Vector3 splitPlane = new Vector3(1, 0, 0);
        public Vector3 splitPos = Vector3.zero;

        public Vector3 interactColor = new Vector3(1, 1, 1);
        

        public void CreateViewport()
        {
            //            ViewMat = Matrix4x4.LookAt(rayCamera, rayTarget, Vector3.up);
            ViewMat = Matrix4x4.LookAt(rayTarget, rayCamera, Vector3.up);
            rayMarchMat.SetMatrix("_ViewMatrix", ViewMat);
            rayMarchMat.SetFloat("_Perspective", FOV);
            rayMarchMat.SetVector("_Camera", rayCamera);
            rayMarchMat.SetVector("_LightDir", lightDir);

            rayMarchMat.SetVector("_SplitPlane", splitPlane);
            splitPos = new Vector3(splitPosX, splitPosY, splitPosZ);
            rayMarchMat.SetVector("_SplitPos", splitPos);
            rayMarchMat.SetFloat("_Cutoff", cutoff);
            rayMarchMat.SetFloat("_Shininess", shininess);
            rayMarchMat.SetVector("_InteractColor", interactColor);
            rayMarchMat.SetFloat("_RenderType", renderType);
        }


        void Start()
        {
            plane.GetComponent<Renderer>().material = rayMarchMat;
            cameraPos.z = 4;

            VolumetricTexture vt = new VolumetricTexture();
            vt.CreateNoise(Vector3.one * 128, 3);

 //           Nifti n = new Nifti(Application.dataPath + "/../data/atlas3.nii",3);
          Nifti n = new Nifti(Application.dataPath + "/../data/atlas1.nii", 3);
            //Nifti n = new Nifti(Application.dataPath + "/../data/atlas2.nii",2);
            vt = n.toTexture(new Vector3(1, 2, 1));

            rayMarchMat.SetTexture("_VolumeTex", vt.texture);

        }

        // Update is called once per frame

        Vector3 cameraPos, cameraAdd, cameraRotate = new Vector3(0, 0, 1);

        void UpdateCameraRotate(float s)
        {

            if (Input.GetMouseButton(1))
            {
                cameraAdd.x = 2 * s * Input.GetAxis("Mouse X") * -1f;
                cameraAdd.y = 2 * s * Input.GetAxis("Mouse Y") * -1.0f;
            }
            cameraAdd.z += 1 * s * Input.GetAxis("Mouse ScrollWheel") * -1.0f;
            cameraPos += cameraAdd;
            cameraAdd *= 0.9f;

        }

        void Update()
        {
            rayMarchMat.SetFloat("_Opacity", opacity); // Blending strength 
            rayMarchMat.SetVector("_ClipDims", clipDimensions / 100f); // Clip box
            UpdateCameraRotate(1);
            cameraRotate = Vector3.left;
            cameraRotate = Quaternion.AngleAxis(cameraPos.x, Vector3.up) * cameraRotate;
            cameraRotate = Quaternion.AngleAxis(cameraPos.y, Vector3.Cross(cameraRotate, Vector3.up).normalized) * cameraRotate;
            //            cameraRotate = Quaternion.Euler(0, cameraPos.y, 0) * Vector3.left;
            rayCamera = cameraPos.z * cameraRotate;
            // Debug.Log(cameraPos.z);
            CreateViewport();


            float t = Time.time * 1;
            lightDir = new Vector3(Mathf.Cos(t), -0.1f, Mathf.Sin(t)).normalized;

        }
    }

}