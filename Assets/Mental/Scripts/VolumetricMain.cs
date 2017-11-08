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
        [Range(0, 0.15f)]
        public float ColorCutoffStrength = 0;
        [SerializeField]
        [Range(0, 1)]
        public float DotStrength = 1;

        public Material rayMarchMat;
        /*        public Material CrossectionMatX;
                public Material CrossectionMatY;
                public Material CrossectionMatZ;
                */
        public GameObject CrossectionMatX;
        public GameObject CrossectionMatY;
        public GameObject CrossectionMatZ;

        private Matrix4x4 ViewMat;
        public Vector3 splitPlane = new Vector3(1, 0, 0);
        public Vector3 splitPos = Vector3.zero;

        public Vector3 rayCamera, rayTarget;
        public Vector3 interactColor = new Vector3(1, 1, 1);

        public Vector3 lightDir = new Vector3(1, 1, 0).normalized;

        public Vector3 internalScaleData = Vector3.one;
        public Vector3 internalScaleAtlas = Vector3.one;

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


        private void SetCrossSectionMat(GameObject go, Vector3 camera, Vector3 up)
        {
            if (go == null)
                return;
            if (go.GetComponent<Renderer>() == null)
                return;
            Material m = go.GetComponent<Renderer>().material;
            if (m == null)
                return;
            Matrix4x4 mat = Matrix4x4.LookAt(splitPos, camera, up);
        
            m.SetVector("_SplitPlane", splitPlane);
            m.SetVector("_SplitPos", splitPos);
            m.SetMatrix("_ViewMatrix", mat);
            m.SetFloat("_Perspective", 50);
            m.SetVector("_Camera", camera);
            m.SetFloat("_IntensityScale", IntensityScale);
            m.SetVector("_InternalScaleData", internalScaleData);
            m.SetVector("_SplitPlane", (-camera).normalized);
            Vector2 scale = new Vector3(go.transform.localScale.x, go.transform.localScale.z);
            m.SetVector("_Scale2D", scale);
        }

        public void UpdateMaterials(Vector3 up, Material mat, Quaternion q)
        {
            Material m = rayMarchMat;
            rayMarchMat = m;
            UpdateMaterials(up, q);
            rayMarchMat = m;
        }


        public void UpdateMaterials(Vector3 up, Quaternion q)
        {
            //            ViewMat = Matrix4x4.LookAt(rayCamera, rayTarget, Vector3.up);
            ViewMat = Matrix4x4.LookAt(rayTarget, rayCamera, up);
            rayMarchMat.SetMatrix("_ViewMatrix", ViewMat);
            rayMarchMat.SetFloat("_Perspective", FOV);
            rayMarchMat.SetVector("_Camera", rayCamera);
            rayMarchMat.SetVector("_CameraDirection", (rayCamera - rayTarget).normalized);
            rayMarchMat.SetVector("_LightDir", q*lightDir);

            rayMarchMat.SetVector("_SplitPlane", q*splitPlane);
            // Make sure point is always closest to origin

            splitPos = q*new Vector3(splitPosX, splitPosY, splitPosZ);
            float d = Vector3.Dot(splitPos, q*splitPlane);
            splitPos = q*splitPlane.normalized * d;


            Vector3 crossCameraPos = splitPlane.normalized;
            ViewMat = Matrix4x4.LookAt(splitPos, crossCameraPos, up);

            SetCrossSectionMat(CrossectionMatX, crossCameraPos*2 + splitPos, up);
            SetCrossSectionMat(CrossectionMatY, Util.ViewMatrixUp(ViewMat)*2 + splitPos, Util.ViewMatrixLeft(ViewMat));
            SetCrossSectionMat(CrossectionMatZ, Util.ViewMatrixLeft(ViewMat) * 2 + splitPos, Util.ViewMatrixUp(ViewMat));

            rayMarchMat.SetVector("_SplitPos", splitPos);
            rayMarchMat.SetFloat("_Cutoff", cutoff);
            rayMarchMat.SetFloat("_Shininess", shininess);
            rayMarchMat.SetVector("_InteractColor", interactColor);
            rayMarchMat.SetFloat("_Opacity", opacity); // Blending strength 
            rayMarchMat.SetFloat("_Saturation", saturation);

            rayMarchMat.SetFloat("_IntensityScale", IntensityScale);
            rayMarchMat.SetFloat("_Power", Power);
            rayMarchMat.SetFloat("_LTime", Time.time*0.01f);

            // CrossectionMat




            rayMarchMat.SetFloat("_ColorCutoff", ColorCutoff);
            rayMarchMat.SetFloat("_ColorCutoffStrength", ColorCutoffStrength);
            rayMarchMat.SetFloat("_DotStrength", DotStrength);


            //Debug.Log(internalScaleAtlas);

            rayMarchMat.SetVector("_InternalScaleData", internalScaleData);
            rayMarchMat.SetVector("_InternalScaleAtlas", internalScaleAtlas);


            UpdateKeywords();
        }


        public void UpdateMaterialFromMatrix(Matrix4x4 vm, float fov)
        {
            //            ViewMat = Matrix4x4.LookAt(rayCamera, rayTarget, Vector3.up);
            ViewMat = vm;
            rayMarchMat.SetMatrix("_ViewMatrix", ViewMat);
            rayMarchMat.SetFloat("_Perspective", fov);
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
            rayMarchMat.SetFloat("_LTime", Time.time * 0.01f);

            // CrossectionMat
            rayMarchMat.SetFloat("_ColorCutoff", ColorCutoff);
            rayMarchMat.SetFloat("_ColorCutoffStrength", ColorCutoffStrength);
            rayMarchMat.SetFloat("_DotStrength", DotStrength);


            //Debug.Log(internalScaleAtlas);

            rayMarchMat.SetVector("_InternalScaleData", internalScaleData);
            rayMarchMat.SetVector("_InternalScaleAtlas", internalScaleAtlas);


            UpdateKeywords();
        }

        public void ApplyTexture(Texture3D texture, Texture3D textureDetail, Texture3D atlas)
        {
            if (rayMarchMat == null)
                return;
            
            rayMarchMat.SetTexture("_VolumeTex", texture);
            rayMarchMat.SetTexture("_VolumeTexDetail", textureDetail);
            
            if (CrossectionMatX != null)
            {
                CrossectionMatX.GetComponent<Renderer>().material.SetTexture("_VolumeTex", texture);
                CrossectionMatY.GetComponent<Renderer>().material.SetTexture("_VolumeTex", texture);
                CrossectionMatZ.GetComponent<Renderer>().material.SetTexture("_VolumeTex", texture);
            }
            rayMarchMat.SetTexture("_VolumeTexDots", atlas);

        }


    }


    public class AnchorImage
    {

        public Texture2D image;
        public Vector3 o, u, v;
        public string url;
        public Material material;



        
        public void Apply()
        {
            //material.SetTexture ("_MainTex", image); 
        }

    }


    public class VolumetricMain
    {
        private VolumetricParams vParams;
//        private GameObject plane;

        public VolumetricMain(GameObject pl, VolumetricParams vp)
        {
            vParams = vp;
//            plane = pl;

        }

        public VolumetricMain(VolumetricParams vp)
        {
            vParams = vp;

        }


        public VolumetricTexture volTex = new VolumetricTexture();
        public VolumetricTexture volTexDetail = new VolumetricTexture();
        public VolumetricTexture atlas = new VolumetricTexture();

        public Material anchorMaterial;
        public Nifti atlasNifti;

//        public Vector3 atlasScaleValues;

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
            vParams.internalScaleAtlas = atlasNifti.scaleValues;
//            Debug.Log(vParams.internalScaleAtlas);

        }

    }

}