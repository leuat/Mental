using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

namespace LemonSpawn
{



    public class VolumetricMain : MonoBehaviour
    {
        public enum RenderType { Hard, Opacity };


        [SerializeField]
        [Range(0, 2)]
        private float opacity = 1;
        [SerializeField]
        [Range(0, 1)]
        private float cutoff = 0.0f;
        [SerializeField]
        [Range(0, 100)]
        private float shininess = 50f;
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
        private RenderType renderType = RenderType.Hard;
        [SerializeField]
        private bool hasShadows = true;
        [SerializeField]
        private bool hasLighting = true;
        [SerializeField]
        private bool movingLight = true;
        [SerializeField]
        [Range(0, 10)]
        public float saturation = 1;
        [SerializeField]
        [Range(0, 180)]
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

        private VolumetricTexture volTex = new VolumetricTexture();


        private void UpdateKeywords()
        {
            if (renderType == RenderType.Hard)
            {
                rayMarchMat.EnableKeyword("SHADING_HARD");
                rayMarchMat.DisableKeyword("SHADING_OPACITY");
            }
            if (renderType == RenderType.Opacity)
            {
                rayMarchMat.DisableKeyword("SHADING_HARD");
                rayMarchMat.EnableKeyword("SHADING_OPACITY");
            }
            if (hasShadows)
                rayMarchMat.EnableKeyword("HAS_SHADOWS");
            else
                rayMarchMat.DisableKeyword("HAS_SHADOWS");

            if (hasLighting)
                rayMarchMat.EnableKeyword("HAS_LIGHTING");
            else
                rayMarchMat.DisableKeyword("HAS_LIGHTING");

        }

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
            rayMarchMat.SetFloat("_Opacity", opacity); // Blending strength 
            rayMarchMat.SetFloat("_Saturation", saturation);

            UpdateKeywords();
        }


        void Start()
        {
            plane.GetComponent<Renderer>().material = rayMarchMat;
            cameraPos.z = 1.5f;

            PopulateFileList();
            volTex.CreateNoise(Vector3.one * 64, 3);
            rayMarchMat.SetTexture("_VolumeTex", volTex.texture);

            //           Nifti n = new Nifti(Application.dataPath + "/../data/atlas3.nii",3);
            //Nifti n = new Nifti(Application.dataPath + "/../data/atlas2.nii",2);


            //            Nifti n = new Nifti(Application.dataPath + "/../data/atlas1.nii", 3);
            //          vt = n.toTexture(new Vector3(2, 4, 2));


        }

        // Update is called once per frame

        Vector3 cameraPos, cameraAdd, cameraRotate = new Vector3(0, 0, 1);
		Vector3 rotatePlaneAdd, rotatePlane;

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


			if (Input.GetMouseButton(0))
			{
				rotatePlaneAdd.x = 2 * s * Input.GetAxis("Mouse X") * -1f;
				rotatePlaneAdd.y = 2 * s * Input.GetAxis("Mouse Y") * -1.0f;
			}
			rotatePlane += rotatePlaneAdd;
			rotatePlaneAdd *= 0.9f;

			splitPlane = Quaternion.Euler (rotatePlane.x, rotatePlane.y, 0)*Vector3.up;


        }




        void PopulateFileList()
        {
            Dropdown cbx = GameObject.Find("drpSelectFile").GetComponent<Dropdown>();
            cbx.ClearOptions();
            DirectoryInfo info = new DirectoryInfo(Application.dataPath + "/../data");
            if (!info.Exists)
            {
                Debug.Log("Could not find data directory!");
                return;
            }
            FileInfo[] fileInfo = info.GetFiles();
            List<Dropdown.OptionData> data = new List<Dropdown.OptionData>();
            foreach (FileInfo f in fileInfo)
            {
                //string name = f.Name.Remove(f.Name.Length-4, 4);

                /*                if (!f.Name.ToLower().Contains(fileType.ToLower()))
                                    continue;
                                    */

                string name = f.Name;

                data.Add(new Dropdown.OptionData(name));

            }
            cbx.AddOptions(data);



        }

        public void LoadFile() {
            string filename = Util.getComboValue("drpSelectFile");
            int forceValue = int.Parse(Util.getComboValue("cmbForceResolution"));
            Nifti n = new Nifti(filename);

            Vector3 scaleValues = n.findNewResolutionScale(forceValue);
            
            volTex = n.toTexture(scaleValues);
            rayMarchMat.SetTexture("_VolumeTex", volTex.texture);


        }

        void Update()
        {
            UpdateCameraRotate(1);
            cameraRotate = Vector3.left;
            cameraRotate = Quaternion.AngleAxis(cameraPos.x, Vector3.up) * cameraRotate;
            cameraRotate = Quaternion.AngleAxis(cameraPos.y, Vector3.Cross(cameraRotate, Vector3.up).normalized) * cameraRotate;
            //            cameraRotate = Quaternion.Euler(0, cameraPos.y, 0) * Vector3.left;
            rayCamera = cameraPos.z * cameraRotate;
            // Debug.Log(cameraPos.z);
            CreateViewport();

            float t = Time.time * 1;
            if (movingLight)
                lightDir = new Vector3(Mathf.Cos(t), -0.1f, Mathf.Sin(t)).normalized;

        }
    }

}