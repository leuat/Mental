using UnityEngine;
using UnityEngine.VR;

namespace LemonSpawn
{

    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]

    /*    public class MonoColor
        {
            public Color[] colors;
            public MonoColor(Color[] c)
            {
                colors = c;
            }
        }
        */
    public class VolTexImageRenderer : MonoBehaviour
    {
        public Shader shader;
        private Material _material;
        private int pixelWidth = -1;
        public VolTexMonoData vData;
        //        public Color tint = Color.white;

        protected Material material
        {
            get
            {
                if (_material == null)
                {
                    _material = new Material(shader);
                    _material.hideFlags = HideFlags.HideAndDontSave;
                }
                return _material;
            }
        }




        private void Start()
        {
        }

        public bool isRightEye = true;

        private Vector3 flip(Vector3 f)
        {
            return new Vector3(f.x, f.y, f.z);
            //return f;
        }

        public static float globalScale = 10;
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (shader == null)
            {
                Debug.Log("SHADER NULL");
                return;
            }
            vData.vParams.rayMarchMat = material;
            if (vData != null)
                if (vData.vParams != null)
                    if (vData.vMain.volTex != null)
                        vData.vParams.ApplyTexture(vData.vMain.volTex.texture, vData.vMain.volTex.texture, vData.vMain.atlas.texture);

            float aspect = Screen.width / (float)Screen.height;




            Vector3 left = Quaternion.Inverse(InputTracking.GetLocalRotation(VRNode.LeftEye)) * InputTracking.GetLocalPosition(VRNode.LeftEye);

            Vector3 right = Quaternion.Inverse(InputTracking.GetLocalRotation(VRNode.RightEye)) * InputTracking.GetLocalPosition(VRNode.RightEye);
            Vector3 center = Quaternion.Inverse(InputTracking.GetLocalRotation(VRNode.CenterEye)) * InputTracking.GetLocalPosition(VRNode.CenterEye);

            Vector3 mcenter = transform.position;

            Matrix4x4 m = GetComponent<Camera>().cameraToWorldMatrix;

            Vector3 centerWorld = flip(m.MultiplyPoint(center));
            Vector3 leftWorld = flip(m.MultiplyPoint(left));
            Vector3 rightWorld = flip(m.MultiplyPoint(right));



            if (isRightEye)
                vData.vParams.rayCamera = rightWorld;
            else
                vData.vParams.rayCamera = leftWorld;

            //                        vParams.rayCamera = cam.transform.position;
            //            Debug.Log(vParams.rayCamera.x +  ", " + vParams.rayCamera.y + ", " + vParams.rayCamera.z);

            //            vParams.UpdateMaterialFromMatrix(transform.worldToLocalMatrix, 70);
            //            vParams.FOV = vParams.FOV;

            float scale = globalScale;

            vData.vParams.rayCamera /= scale;
            centerWorld /= scale;
            centerWorld += mcenter;
            vData.vParams.rayCamera += mcenter;


            Vector3 forward = transform.forward - 3 * scale * (centerWorld - vData.vParams.rayCamera);

            vData.vParams.rayTarget = vData.vParams.rayCamera + forward;
            Vector3 lc = Quaternion.Inverse(InputTracking.GetLocalRotation(VRNode.CenterEye)) *InputTracking.GetLocalPosition(VRNode.CenterEye);

//            vData.vParams.rayTarget -= centerWorld +transform.position;
 //           vData.vParams.rayCamera -= centerWorld + transform.position;

            // Now rotate

            /*rotation.x += 2;
            rotation.y += 5;

            position.x = Mathf.Sin(Time.time) * 10;
            position.y = Mathf.Cos(Time.time) * 10;
            position.z = Mathf.Cos(Time.time*1.23f) * 0.65f;

            */


            vData.vParams.rayCamera -= vData.position;
            vData.vParams.rayTarget -= vData.position;

            Quaternion q = Quaternion.Euler(vData.rotation);
            vData.vParams.rayCamera = q * vData.vParams.rayCamera;
            vData.vParams.rayTarget = q * vData.vParams.rayTarget;
            Vector3 up = q * transform.up;




            //vParams.rayCamera.y *= aspect;
            //vParams.rayTarget.y *= aspect;
            aspect = 1;
            Material mat = material;
            mat.SetFloat("_aspectRatio", 1f / aspect);
            //            Debug.Log(aspect);
            //            mat.SetFloat("_monoStrength", settings.monoStrength);
            vData.vParams.UpdateMaterials(up, mat, q);

            Graphics.Blit(source, destination, mat, 0);
        }

        void OnDisable()
        {
            if (_material)
            {
                DestroyImmediate(_material);
            }
        }
    }
}