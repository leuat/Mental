using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LemonSpawn
{

    public class MentalMain : MonoBehaviour
    {

        public VolumetricParams vParams;
        private VolumetricMain vMain;
        private CameraRotator camrot;

        // Use this for initialization
        public GameObject plane;


        public void LoadFile()
        {
            string filename = Util.getComboValue("drpSelectFile");
            int forceValue = int.Parse(Util.getComboValue("cmbForceResolution"));
            Nifti n = new Nifti(filename, Application.dataPath + "/../data/");

            Vector3 scaleValues = n.findNewResolutionScale(forceValue);

            vMain.volTex = n.toTexture(scaleValues, false);
            vParams.ApplyTexture(vMain.volTex.texture);
            //			TestDotTexture (atlas);

        }


        public void Clip()
        {
            vMain.volTex.ClipData(vMain.atlas, 0);
            vParams.ApplyTexture(vMain.volTex.texture);
        }

        public void ClipInverse()
        {
            vMain.volTex.ClipData(vMain.atlas, 1);
            vParams.ApplyTexture(vMain.volTex.texture);
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

            Util.PopulateFileList("drpSelectFile", Application.dataPath + "/../data");

            vMain.volTex.CreateNoise(Vector3.one * 64, 3, 0);
            vParams.ApplyTexture(vMain.volTex.texture);

            AnchorImage ai = new AnchorImage();
            StartCoroutine(ai.LoadFromUrl("http://cmbn-navigator.uio.no/navigator/feeder/preview/?id=33133", vMain.anchorMaterial));

            vMain.LoadAtlas("WHS_SD_rat_atlas_v2.label");

            PopulateScrollView();
            vParams.internalScale = new Vector3(1, 2, 1);


        }

        public void ConstrainAtlas()
        {
            int forceValue = int.Parse(GetComponent<UIManager>().getComboValue("cmbForceResolution"));
            vMain.CreateAtlasFromNifti(forceValue); 

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
            ui.pnlParameters.SetActive(isa);

        }

        // Update is called once per frame
        void Update()
        {

            if (camrot == null)
                return;

            //            cameraRotate = Quaternion.Euler(0, cameraPos.y, 0) * Vector3.left;
            vParams.rayCamera = camrot.cameraPos.z * camrot.cameraRotate;
            // Debug.Log(cameraPos.z);
            vParams.UpdateMaterials();

            float t = Time.time * 1;
            if (vParams.movingLight)
                vParams.lightDir = new Vector3(Mathf.Cos(t), -0.1f, Mathf.Sin(t)).normalized;

            vParams.splitPlane = camrot.getSplitPlane(vParams.splitPlane);

            UpdateParameters();

        }
    }

}