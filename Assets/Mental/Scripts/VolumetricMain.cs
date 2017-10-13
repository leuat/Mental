using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace LemonSpawn
{

    [System.Serializable]
    public class VolumetricParams
    {
        public enum RenderType { Hard, Opacity };

        [SerializeField]
        [Range(0, 2)]
        public float opacity = 1;
        [SerializeField]
        [Range(0, 1)]
        public float cutoff = 0.0f;
        [SerializeField]
        [Range(0, 100)]
        private float shininess = 50f;
        [SerializeField]
        [Range(-1, 1)]
        public float splitPosX = -1f;
        [SerializeField]
        [Range(-1, 1)]
        public float splitPosY = -1f;
        [SerializeField]
        [Range(-1, 1)]
        public float splitPosZ = -1f;
        [SerializeField]
        public RenderType renderType = RenderType.Hard;
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
        [SerializeField]
        [Range(0, 1)]
        public float ColorCutoff = 0;
        [SerializeField]
        [Range(0, 1)]
        public float ColorCutoffStrength = 0;
        [SerializeField]
        [Range(0, 1)]
        public float DotStrength = 1;

        public Material rayMarchMat;
        public Material CrossectionMat;

        private Matrix4x4 ViewMat;
        public Vector3 splitPlane = new Vector3(1, 0, 0);
        public Vector3 splitPos = Vector3.zero;

        public Vector3 rayCamera, rayTarget;
        public Vector3 interactColor = new Vector3(1, 1, 1);

        public Vector3 lightDir = new Vector3(1, 1, 0).normalized;

        public Vector3 internalScale = Vector3.one;

        public GameObject prefabButton;


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

        /*
		 * 1)	scale properly
		 * 2) 	Atlas structures on/ off + semi-transparent
		 * 3)	Parameters for cutting etc + arbitrary 
		 * 
		 * 
		 * 
		 * 
		 * 
		 * */


        public void UpdateMaterials()
        {
            //            ViewMat = Matrix4x4.LookAt(rayCamera, rayTarget, Vector3.up);
            ViewMat = Matrix4x4.LookAt(rayTarget, rayCamera, Vector3.up);
            rayMarchMat.SetMatrix("_ViewMatrix", ViewMat);
            rayMarchMat.SetFloat("_Perspective", FOV);
            rayMarchMat.SetVector("_Camera", rayCamera);
            rayMarchMat.SetVector("_CameraDirection", (rayCamera - rayTarget).normalized);
            rayMarchMat.SetVector("_LightDir", lightDir);

            rayMarchMat.SetVector("_SplitPlane", splitPlane);
            // Make sure point is always closest to origin

            splitPos = new Vector3(splitPosX, splitPosY, splitPosZ);
            float d = Vector3.Dot(splitPos, splitPlane);
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
            Vector3 cam = 2 * splitPlane + splitPos;
            ViewMat = Matrix4x4.LookAt(splitPos, cam, Vector3.up);

            CrossectionMat.SetMatrix("_ViewMatrix", ViewMat);
            CrossectionMat.SetFloat("_Perspective", 50);
            CrossectionMat.SetVector("_Camera", cam);
            CrossectionMat.SetFloat("_IntensityScale", IntensityScale);


            rayMarchMat.SetFloat("_ColorCutoff", ColorCutoff);
            rayMarchMat.SetFloat("_ColorCutoffStrength", ColorCutoffStrength);
            rayMarchMat.SetFloat("_DotStrength", DotStrength);

            rayMarchMat.SetVector("_InternalScale", internalScale);
            CrossectionMat.SetVector("_InternalScale", internalScale);


            UpdateKeywords();
        }

        public void ApplyTexture(Texture3D texture, Texture3D atlas)
        {
            rayMarchMat.SetTexture("_VolumeTex", texture);
            CrossectionMat.SetTexture("_VolumeTex", texture);
            rayMarchMat.SetTexture("_VolumeTexDots", atlas);

        }


    }


    public class AnchorImage
    {

        public Texture2D image;
        public Vector3 o, u, v;
        public string url;
        public Material material;



        public IEnumerator LoadFromUrl(string s, Material m)
        {
            WWW www = new WWW(s);
            material = m;
            yield return www;
            image = www.texture;
            float alpha = 0.95f;
            Util.SetTransparent(image, new Color(alpha, alpha, alpha, alpha));

            Apply();
        }

        public void Apply()
        {
            //material.SetTexture ("_MainTex", image); 
        }

    }


    public class VolumetricMain
    {
        private VolumetricParams vParams;
        private GameObject plane;

        public VolumetricMain(GameObject pl, VolumetricParams vp)
        {
            vParams = vp;
            plane = pl;

        }

        public VolumetricTexture volTex = new VolumetricTexture();
        public VolumetricTexture atlas = new VolumetricTexture();

        public Material anchorMaterial;
        public Nifti atlasNifti;

        public Vector3 atlasScaleValues;

        void Start()
        {

        }

        public void CreateAtlasFromNifti(int forceValue)
        {
            Vector3 scaleValues = atlasNifti.findNewResolutionScale(forceValue);

            atlas = atlasNifti.toTexture(scaleValues, true);

        }

        public void TestDotTexture(VolumetricTexture org)
        {
            VolumetricTexture vtDots = new VolumetricTexture(org.size);
            vtDots.RandomDots(250000, 0.01f, org);



        }



        public void LoadAtlas(string filename)
        {
            atlasNifti = new Nifti(filename, Application.dataPath + "/../data/");

            CreateAtlasFromNifti(int.Parse(Util.getComboValue("cmbForceResolution")));

        }

    }

}