using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LemonSpawn
{

    public class VolTexMonoData : MonoBehaviour
    {

        public string filename;
        public VolumetricParams vParams;
        public VolumetricMain vMain;

        public GameObject parent;

        public Vector3 rotation;
        public Vector3 position;

        private void Load(string fname)
        {
            Nifti n = new Nifti(fname, Application.dataPath + "/../data/");
            //  Nifti n = new Nifti("WHS_SD_rat_T2star_v1.01.nii", Application.dataPath + "/../data/");

            Vector3 scaleValues = n.findNewResolutionScale(256);

            vParams.internalScaleData = n.scaleValues;

            vMain.volTex = n.toTexture(scaleValues, false);


            vParams.ApplyTexture(vMain.volTex.texture, vMain.volTex.texture, vMain.atlas.texture);


        }

        // Use this for initialization
        void Start()
        {
            vMain = new VolumetricMain(vParams);
            //vParams.rayMarchMat = material;
            Load(filename);
            //       "t1_mprage_sag.nii"
            //t1_mprage_sag.nii
            //WHS_SD_rat_T2star_v1.01.nii

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}