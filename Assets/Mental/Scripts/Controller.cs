using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LemonSpawn
{
    public class Controller : MonoBehaviour
    {

        private SteamVR_TrackedController _controller;
        public VolTexMonoData vData;
        public bool triggerClicked = false;
        SteamVR_Controller.Device device;
        SteamVR_TrackedObject trackedobj;

        private void OnEnable()
        {
            _controller = GetComponent<SteamVR_TrackedController>();
            _controller.TriggerClicked += HandleTriggerClicked;
            _controller.TriggerUnclicked += HandleTriggerUnClicked;
//            _controller.OnPadTouched += HandlePadTouched;
            //            _controller.PadClicked += HandlePadClicked;
        }

        private void HandleTriggerClicked(object sender, ClickedEventArgs e)
        {
            triggerClicked = true;
        }
        private void HandleTriggerUnClicked(object sender, ClickedEventArgs e)
        {
            triggerClicked = false;
        }

  /*      private void HandlePadTouched(object sender, ClickedEventArgs e)
        {
            float tiltAroundX = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).x;
            float tiltAroundY = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y;
        }
        */
        // Use this for initialization
        void Start()
        {
            trackedobj = GetComponent<SteamVR_TrackedObject>();
            device = SteamVR_Controller.Input((int)trackedobj.index);
        }

        // Update is called once per frame
        void Update()
        {
//            if (MenuController.controllerType == MenuController.PadControllerType.Rotation)
            if (_controller.triggerPressed)
            {
                vData.vParams.splitPosX = transform.position.x + vData.position.x;
                vData.vParams.splitPosY = transform.position.y + vData.position.y;
                vData.vParams.splitPosZ = transform.position.z + vData.position.z;
                vData.vParams.splitPlane = transform.rotation * (Vector3.up * -1);
            }
            if (_controller.padTouched)
            {
     
                float tiltAroundX = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).x;
                float tiltAroundY = device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y;
                float theta = (Mathf.Atan2(tiltAroundX, tiltAroundY));

                if (MenuController.controllerType == MenuController.PadControllerType.Intensity)
                {
                    vData.vParams.IntensityScale = theta*0.5f;
                }
                if (MenuController.controllerType == MenuController.PadControllerType.Opacity)
                {
                    vData.vParams.opacity = theta*0.5f;
                }
                if (MenuController.controllerType == MenuController.PadControllerType.Rotation)
                {
                    vData.rotation.x = tiltAroundX * 100;
                    vData.rotation.y = tiltAroundY * 100;
                }
            }
        }
    }

}