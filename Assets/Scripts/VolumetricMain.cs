using UnityEngine;
using System.Collections;



namespace LemonSpawn
{

    public class VolumetricTexture
    {

        public Texture3D texture;
        public int size;

        public void CreateNoise(int N, float scale)
        {
            size = N;
            texture = new Texture3D(size, size, size, TextureFormat.ARGB32, true);
            Vector3 cs = new Vector3(0.856f, 1.234f, 2.3f)*scale;
            cols = new Color[size * size * size];
            Color c = new Color(1.0f, 0.7f, 0.2f, 1.0f);
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    for (int k = 0; k < N; k++)
                    {
                        Vector3 v = new Vector3(i / (float)N, j / (float)N, k / (float)N);
                        
                        float val = Util.noise4D.raw_noise_3d(v.x*scale, v.y * scale, v.z * scale) -0.5f;
                        c.r = Util.noise4D.raw_noise_3d(v.x*cs.x, v.y*cs.x, v.z*cs.x);
                        c.g = Util.noise4D.raw_noise_3d(v.x * cs.y, v.y * cs.y, v.z * cs.y);
                        c.b = Util.noise4D.raw_noise_3d(v.x * cs.z, v.y * cs.z, v.z * cs.z);
                        c.a = val;


                        setColor(i,j,k, c);

                    }
            texture.SetPixels(cols);
            texture.Apply();
        }

        private void setColor(int x, int y, int z, Color c)
        {
            if (x < 0 || x >= size)
                return;
            if (y < 0 || y >= size)
                return;
            if (z < 0 || z >= size)
                return;
            //            cols[x * size * size + (size-1-y) * size + z] = c;
            cols[x * size * size + (y) * size + z] = c;
        }

        private void singleGrass(int x, int y, int wx, int wy, float h)
        {
            Color c = new Color(0.3f, 1f, 0.3f, 1);
            float theta = Random.value * 2 * Mathf.PI;


            Vector2 dir = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta));
            Vector2 pert = new Vector2(0, 0);

            for (int i = 0; i < h; i++)
            {
                for (int xx = x - wx; xx < x + wx; xx++)
                    for (int yy = y - wy; yy < y + wy; yy++)
                    {
                        float dist = 1 / (new Vector2(xx - x, yy - y).magnitude + 1);
                        Color col = c * dist;
                        col.a = 1;
                        setColor(xx + (int)pert.x, i, yy + (int)pert.y, col);
                        pert += dir * i * i / size * 0.001f;



                    }

            }


        }
        Color[] cols;

        public void CreateGrass(int N, int Count)
        {
            size = N;
            texture = new Texture3D(size, size, size, TextureFormat.ARGB32, true);

            cols = new Color[size * size * size];
            for (int i = 0; i < cols.Length; i++)
                cols[i] = new Color(0, 0, 0, 0);

            int s = 2;

            for (int i = 0; i < Count; i++)
            {
                int x = (int)Mathf.Clamp((int)(Random.value * size), s, size - s);
                int y = (int)Mathf.Clamp((int)(Random.value * size), s, size - s);
                int h = (int)(size * 0.5f + 0.5f * (int)(Random.value * size));

                singleGrass(x, y, s, s, h);


            }

            texture.SetPixels(cols);
            texture.Apply();
        }




    }

    public class VolumetricMain : MonoBehaviour
    {
        [SerializeField]
        [Range(0, 2)]
        private float opacity = 1;
        [SerializeField]
        private Vector4 clipDimensions = new Vector4(100, 100, 100, 0);

        public float FOV = 70;


        // Use this for initialization
        public GameObject plane;
        public Material rayMarchMat;
        public Vector3 lightDir = new Vector3(1, 1, 0).normalized;

        public Vector3 rayCamera, rayTarget;
        private Matrix4x4 ViewMat;

        public void CreateViewport()
        {
//            ViewMat = Matrix4x4.LookAt(rayCamera, rayTarget, Vector3.up);
            ViewMat = Matrix4x4.LookAt(rayTarget, rayCamera, Vector3.up);
            rayMarchMat.SetMatrix("_ViewMatrix", ViewMat);
            rayMarchMat.SetFloat("_Perspective", FOV);
            rayMarchMat.SetVector("_Camera", rayCamera);
            rayMarchMat.SetVector("_LightDir", lightDir);

        }


        void Start()
        {
            VolumetricTexture vt = new VolumetricTexture();
                       // vt.CreateGrass(256, 200);
            vt.CreateNoise(64, 2);
            //    vt.CreateNoise(64, 6.123f);
            plane.GetComponent<Renderer>().material = rayMarchMat;
            rayMarchMat.SetTexture("_VolumeTex", vt.texture);
            cameraPos.z = 4;
            //  testObject2.GetComponent<Renderer>().material.SetTexture("_MainTex", vt.texture);


        }

        // Update is called once per frame

        Vector3 cameraPos, cameraAdd, cameraRotate = new Vector3(0, 0, 1);

        void UpdateCameraRotate(float s)
        {

            if (Input.GetMouseButton(1))
            {
                cameraAdd.x = 2 * s * Input.GetAxis("Mouse X")*-1f;
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
            cameraRotate = Quaternion.AngleAxis(cameraPos.x, Vector3.up)*cameraRotate;
            cameraRotate = Quaternion.AngleAxis(cameraPos.y, Vector3.Cross(cameraRotate,Vector3.up).normalized) * cameraRotate;
            //            cameraRotate = Quaternion.Euler(0, cameraPos.y, 0) * Vector3.left;
            rayCamera = cameraPos.z * cameraRotate;
           // Debug.Log(cameraPos.z);
            CreateViewport();

            float t = Time.time * 2;
            lightDir = new Vector3(Mathf.Cos(t), 0, Mathf.Sin(t));

        }
    }

}