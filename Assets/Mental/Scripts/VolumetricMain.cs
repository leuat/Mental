using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;

namespace LemonSpawn
{

	[System.Serializable]
	public class VolumetricParams {
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
		public bool movingLight = true;
		[SerializeField]
		[Range(0, 10)]
		public float saturation = 1;
		[SerializeField]
		[Range(0, 180)]
		public float FOV = 70;
		[SerializeField]
		[Range(0, 3)]
		public float IntensityScale = 1;
		[SerializeField]
		[Range(0, 6)]
		public float Power = 1;

		public Material rayMarchMat;
		public Material CrossectionMat;

		private Matrix4x4 ViewMat;
		public Vector3 splitPlane = new Vector3(1, 0, 0);
		public Vector3 splitPos = Vector3.zero;

		public Vector3 rayCamera, rayTarget;
		public Vector3 interactColor = new Vector3(1, 1, 1);

		public Vector3 lightDir = new Vector3(1, 1, 0).normalized;


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

		public void UpdateMaterials()
		{

			// Make sure point is always closest to origin



			//            ViewMat = Matrix4x4.LookAt(rayCamera, rayTarget, Vector3.up);
			ViewMat = Matrix4x4.LookAt(rayTarget, rayCamera, Vector3.up);
			rayMarchMat.SetMatrix("_ViewMatrix", ViewMat);
			rayMarchMat.SetFloat("_Perspective", FOV);
			rayMarchMat.SetVector("_Camera", rayCamera);
			rayMarchMat.SetVector("_LightDir", lightDir);

			rayMarchMat.SetVector("_SplitPlane", splitPlane);
			splitPos = new Vector3(splitPosX, splitPosY, splitPosZ);
			float d = Vector3.Dot (splitPos, splitPlane);
			splitPos = splitPlane.normalized * d;

			rayMarchMat.SetVector("_SplitPos", splitPos);
			rayMarchMat.SetFloat("_Cutoff", cutoff);
			rayMarchMat.SetFloat("_Shininess", shininess);
			rayMarchMat.SetVector("_InteractColor", interactColor);
			rayMarchMat.SetFloat("_Opacity", opacity); // Blending strength 
			rayMarchMat.SetFloat("_Saturation", saturation);

			rayMarchMat.SetFloat("_IntensityScale", IntensityScale);
			rayMarchMat.SetFloat("_Power", Power);

			// CrossectionMat

			CrossectionMat.SetVector("_SplitPlane", splitPlane);
			CrossectionMat.SetVector("_SplitPos", splitPos);
			/*			Quaternion q = Quaternion.FromToRotation (Vector3.up, splitPlane);
			Matrix4x4 mat = Matrix4x4.TRS (Vector3.zero, q, Vector3.one);
			CrossectionMat.SetMatrix ("_planeMatrix", mat);
*/
			Vector3 cam = 2*splitPlane + splitPos;
			ViewMat = Matrix4x4.LookAt(splitPos, cam, Vector3.up);

			CrossectionMat.SetMatrix("_ViewMatrix", ViewMat);
			CrossectionMat.SetFloat("_Perspective", 30);
			CrossectionMat.SetVector("_Camera", cam);
			CrossectionMat.SetFloat ("_IntensityScale", IntensityScale);


			UpdateKeywords();
		}

		public void ApplyTexture(Texture3D texture) {
			rayMarchMat.SetTexture("_VolumeTex", texture);
			CrossectionMat.SetTexture ("_VolumeTex", texture);
		}


	}


	public class AnchorImage  {

		public Texture2D image;
		public Vector3 o,u,v;
		public string url;
		public Material material;



		public IEnumerator LoadFromUrl(string s, Material m) {
			WWW www = new WWW(s);
			material = m;
			yield return www;
			image = www.texture;
			float alpha = 0.95f;
			Util.SetTransparent (image, new Color (alpha, alpha, alpha, alpha));

			Apply ();
		}

		public void Apply() {
			material.SetTexture ("_MainTex", image); 
		}

	}


    public class VolumetricMain : MonoBehaviour
    {

		public VolumetricParams vParams = new VolumetricParams ();


        // Use this for initialization
        public GameObject plane;


        private VolumetricTexture volTex = new VolumetricTexture();

		public Material anchorMaterial;



        void Start()
        {
            plane.GetComponent<Renderer>().material =  vParams.rayMarchMat;
            cameraPos.z = 1.5f;

			Util.PopulateFileList("drpSelectFile", Application.dataPath + "/../data");

            volTex.CreateNoise(Vector3.one * 64, 3);
			vParams.ApplyTexture (volTex.texture);

			AnchorImage ai = new AnchorImage ();
			StartCoroutine(ai.LoadFromUrl ("http://cmbn-navigator.uio.no/navigator/feeder/preview/?id=33133", anchorMaterial));

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

			vParams.splitPlane = Quaternion.Euler (rotatePlane.x, rotatePlane.y, 0) * vParams.splitPlane;


        }





        public void LoadFile() {
            string filename = Util.getComboValue("drpSelectFile");
            int forceValue = int.Parse(Util.getComboValue("cmbForceResolution"));
			Nifti n = new Nifti(filename,Application.dataPath + "/../data/");

            Vector3 scaleValues = n.findNewResolutionScale(forceValue);
            
            volTex = n.toTexture(scaleValues);
			vParams.ApplyTexture (volTex.texture);


        }

        void Update()
        {
            UpdateCameraRotate(1);
            cameraRotate = Vector3.left;
            cameraRotate = Quaternion.AngleAxis(cameraPos.x, Vector3.up) * cameraRotate;
            cameraRotate = Quaternion.AngleAxis(cameraPos.y, Vector3.Cross(cameraRotate, Vector3.up).normalized) * cameraRotate;
            //            cameraRotate = Quaternion.Euler(0, cameraPos.y, 0) * Vector3.left;
            vParams.rayCamera = cameraPos.z * cameraRotate;
            // Debug.Log(cameraPos.z);
			vParams.UpdateMaterials();

            float t = Time.time * 1;
            if (vParams.movingLight)
                vParams.lightDir = new Vector3(Mathf.Cos(t), -0.1f, Mathf.Sin(t)).normalized;

        }
    }

}