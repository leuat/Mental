using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.UI;

namespace LemonSpawn
{

    [System.Serializable]
    public class Settings
    {
        public float version = 0.01f;
    }


 

    public class MentalMain : MonoBehaviour
    {

        public VolumetricParams vParams;
        private VolumetricMain vMain;
        private CameraRotator camrot;

        // Use this for initialization
        public GameObject plane;
        private Settings settings = new Settings();
        private Chunks chunks, chunksDetail;
        private int currentResolution = 320*2;
        private Vector3 currentPos = Vector3.zero;

        public IEnumerator ImageLoadFromUrl(string s, Material m)
        {
            WWW www = new WWW(s);
            //material = m;
            yield return www;
            Texture2D image = www.texture;
            float alpha = 0.95f;
            Util.SetTransparent(image, new Color(alpha, alpha, alpha, alpha));

           // Apply();
        }



        public void LoadFile()
        {
            string filename = Util.getComboValue("drpSelectFileData");
            int forceValue = int.Parse(Util.getComboValue("cmbForceResolution"));
            Nifti n = new Nifti(filename, Application.dataPath + "/../data/");

            Vector3 scaleValues = n.findNewResolutionScale(forceValue);

            vParams.internalScaleData = n.scaleValues;

            vMain.volTex = n.toTexture(scaleValues, false);
            UpdateTextures();
            //			TestDotTexture (atlas);

        }

        public void LoadAtlas()
        {
            string filename = Util.getComboValue("drpSelectFileAtlas");
            Debug.Log(filename);
            int forceValue = int.Parse(Util.getComboValue("cmbForceResolution"));
            vMain.LoadAtlas(filename);
            UpdateTextures();
            PopulateScrollView();


        }



        public void Clip()
        {
            vMain.volTex.ClipData(vMain.atlas, 0);
            UpdateTextures();
        }

        public void ClipInverse()
        {
            vMain.volTex.ClipData(vMain.atlas, 1);
            UpdateTextures();
        }

        public void PopulateScrollView()
        {
            bool isa = GetComponent<UIManager>().pnlAtlas.activeSelf;
            GetComponent<UIManager>().pnlAtlas.SetActive(true);
            GameObject go = GameObject.Find("Content");
            for (int i = 0; i < go.transform.childCount; i++)
            {
                GameObject.Destroy(go.transform.GetChild(i).gameObject);
            }

            if (vMain.atlasNifti!=null)
            foreach (KeyValuePair<int, Nifti.Label> e in vMain.atlasNifti.indexedColors)
            {

                GameObject goButton = (GameObject)Instantiate(vParams.prefabButton);
                goButton.transform.SetParent(go.transform);
                goButton.transform.GetChild(1).GetComponent<Text>().text = e.Value.name;
                goButton.transform.localScale = new Vector3(1, 1, 1);


                Toggle toggle = goButton.GetComponent<Toggle>();
                toggle.isOn = e.Value.toggle;
                toggle.onValueChanged.AddListener((value) =>
                {
                    vMain.atlasNifti.indexedColors[e.Key].toggle = toggle.isOn;
                });
                //				e.Value.toggle = true;

            }
            GetComponent<UIManager>().pnlAtlas.SetActive(isa);

        }

        public void InvertToggle()
        {
            foreach (KeyValuePair<int, Nifti.Label> e in vMain.atlasNifti.indexedColors)
                e.Value.toggle = !e.Value.toggle;

            PopulateScrollView();


        }

        // Use this for initialization
        void Start()
        {
            vMain = new VolumetricMain(plane, vParams);
            camrot = GetComponent<CameraRotator>();

            plane.GetComponent<Renderer>().material = vParams.rayMarchMat;
            camrot.cameraPos.z = 1.5f;

            Util.PopulateFileList("drpSelectFileData", Application.dataPath + "/../data", "nii");
            Util.PopulateFileList("drpSelectFileAtlas", Application.dataPath + "/../data", "label");

            vMain.volTex.CreateNoise(Vector3.one * 64, 3, 0);
            UpdateTextures();

            AnchorImage ai = new AnchorImage();
            //StartCoroutine(ai.LoadFromUrl("http://cmbn-navigator.uio.no/navigator/feeder/preview/?id=33133", vMain.anchorMaterial));



            PopulateScrollView();
 
            GameObject.Find("txtVersion").GetComponent<Text>().text = "Version " + settings.version;
            UpdateShader();

//            cr = this.gameObject.AddComponent<ChunkReader>();
            //cr.getChunk("test");

        }


        private void UpdateTextures()
        {
            if (chunks == null)
                vParams.ApplyTexture(vMain.volTex.texture, vMain.volTex.texture, vMain.atlas.texture);
            else
                vParams.ApplyTexture(chunks.volTex.texture, chunksDetail.volTex.texture, vMain.atlas.texture);
                
        }


        public void ConstrainAtlas()
        {
            int forceValue = int.Parse(GetComponent<UIManager>().getComboValue("cmbForceResolution"));
            vMain.CreateAtlasFromNifti(forceValue);
            UpdateTextures();


        }


        private void OnApplicationQuit()
        {
            //Debug.Log("EEH");
            if (chunks != null)
                chunks.DestroyThreads();

            if (chunksDetail != null)
                chunksDetail.DestroyThreads();
        }

        public string dataSource = "";

        public void LoadChunks()
        {
            //string data = "https://neuroglancer.humanbrainproject.org/precomputed/BigBrainRelease.2015/8bit/";

            if (dataSource == "")
                dataSource = Util.getComboValue("drpSelectDataSource");
            
            string mu = currentResolution + "um/";
            string muD = currentResolution/2 + "um/";
            /*
            string data = "https://neuroglancer.humanbrainproject.org/precomputed/WHS_SD_rat/templates/v1.01/t2star_masked/";

            /*
             *  For WHD
             * 39um
             * 78um
             * 156um
             * 312um
             * */
            /*
           string mu = "320um/";
           */

            Vector3 pos = currentPos;

            //            Debug.Log("cp: " + mu + " - " + muD);

            int scale = 2;

            if (chunks == null)
                chunks = new Chunks(Vector3.one * 64, Vector3.one * 64*scale, pos, dataSource, mu);

            if (chunksDetail == null)
                chunksDetail = new Chunks(Vector3.one * 64, Vector3.one * 64 * scale, (2*pos) + Vector3.one, dataSource, muD);

            vParams.internalScaleData = Vector3.one;

        }

        private void FlipVolTex()
        {
            VolumetricTexture vt = vMain.volTex;
            vMain.volTex = vMain.volTexDetail;
            vMain.volTexDetail = vt;

        }

        public void ZoomLevels(int direction)
        {
            if (direction == 1)
            {
                //FlipVolTex();
                chunks = chunksDetail;
                //chunks = null;
                chunksDetail = null;
                currentPos = (2*currentPos + Vector3.one);
                currentResolution /=2;
                LoadChunks();
 //               Debug.Log("Zooming IN");
            }
            if (direction == -1)
            {
                //FlipVolTex();
                chunksDetail = chunks;
                chunks = null;
                //chunksDetail = null;
                currentPos = (currentPos  - Vector3.one)/2;
                currentResolution *= 2;
                LoadChunks();
            }
            UpdateTextures();
        }


        public void UpdateShader()
        {
            string val = GetComponent<UIManager>().getComboValue("drpRenderer");
            string ShaderName = "LemonSpawn/RayMarching" + val;
            //Debug.Log(ShaderName);
            //vParams.rayMarchMat.shader = Shader.Find(ShaderName);

            vParams.rayMarchMat = new Material(Shader.Find(ShaderName));

            plane.GetComponent<Renderer>().material = vParams.rayMarchMat;
            UpdateTextures();
        }



        private void UpdateParameters()
        {
            UIManager ui = GetComponent<UIManager>();
            bool isa = ui.pnlParameters.activeSelf;
            ui.pnlParameters.SetActive(true);
            vParams.IntensityScale = Util.getScrollValue("scrIntensity") * 4;
            vParams.DotStrength= Util.getScrollValue("scrAtlasIntensity") * 0.5f;

            vParams.splitPosX = Util.getScrollValue("scrPlaneX") * 2 - 1;
            vParams.splitPosY = Util.getScrollValue("scrPlaneY") * 2 - 1;
            vParams.splitPosZ = Util.getScrollValue("scrPlaneZ") * 2 - 1;

            vParams.cutoff = Util.getScrollValue("scrCutoff");

            string s = Util.getComboValue("drpShaderType");
            if (s == "Hard")
                vParams.renderType = VolumetricParams.RenderType.Hard;
            else
                vParams.renderType = VolumetricParams.RenderType.Opacity;

            vParams.opacity = Util.getScrollValue("scrOpacity") * 4;
            vParams.Power = Util.getScrollValue("scrPower") * 4;
            vParams.ColorCutoff = Util.getScrollValue("scrCCutoff") * 1;
            vParams.ColorCutoffStrength = Util.getScrollValue("scrCStrength");
            ui.pnlParameters.SetActive(isa);

        }

        public void TestDots()
        {
            vMain.TestDotTexture(vMain.atlas);

        }

        // Update is called once per frame


        void UpdateChunks()
        {
            if (chunks != null)
            {
                if (chunks.Update())
                {
                    UpdateTextures();
                }
                if (chunksDetail.Update())
                {
                    UpdateTextures();
                }

                float cz = 0.75f;

                if (camrot.cameraPos.z < cz)
                {
                    camrot.cameraPos.z += cz;
                    ZoomLevels(1);
                }
                if (camrot.cameraPos.z > 2 * cz)
                {
                    camrot.cameraPos.z -= cz;
                    ZoomLevels(-1);
                }

                float opacity = Mathf.Pow((camrot.cameraPos.z - cz) / cz, 2f);
                vParams.rayMarchMat.SetFloat("_OuterOpacity", Mathf.Clamp(opacity, 0, 1));
            }

        }

        private void Move(Vector3 v)
        {
            if (chunks == null)
                return;
            chunks.MoveChunks(v);
            chunksDetail.MoveChunks(2*v);
            currentPos += v;

        }

        void Update()
        {
            if (camrot == null)
                return;

            vParams.rayCamera = camrot.cameraPos.z * camrot.cameraRotate;
            //vParams.rayCamera = Vector3.left * 3;
//            Debug.Log("RAY camera: " + vParams.splitPlane);
            vParams.UpdateMaterials(camrot.extra.transform.up, Quaternion.identity);

            float t = Time.time * 1;
            if (vParams.movingLight)
                vParams.lightDir = new Vector3(Mathf.Cos(t), -0.1f, Mathf.Sin(t)).normalized;

            vParams.splitPlane = camrot.getSplitPlane(vParams.splitPlane);

            UpdateParameters();
            UpdateChunks();
            int m = 2;

            if (Input.GetKeyUp(KeyCode.R))
            {
                chunks = null;
                chunksDetail = null;
                LoadChunks();
            }

            if (Input.GetKeyUp(KeyCode.D))
                Move(new Vector3(1, 0, 0));
            if (Input.GetKeyUp(KeyCode.A))
                Move(new Vector3(-1, 0, 0));

            if (Input.GetKeyUp(KeyCode.W))
                Move(new Vector3(0, 1, 0));
            if (Input.GetKeyUp(KeyCode.S))
                Move(new Vector3(0, -1, 0));

        }
    }

}