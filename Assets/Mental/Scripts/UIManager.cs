using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LemonSpawn
{

    public class UIManager : MonoBehaviour
    {

        public GameObject pnlAtlas, pnlData;


        private void HideAllPanels()
        {
            pnlAtlas.SetActive(false);
            pnlData.SetActive(false);
        }


        public void ClickPanelData()
        {
            HideAllPanels();
            pnlData.SetActive(true);
        }

        public void ClickPanelAtlas()
        {
            HideAllPanels();
            pnlAtlas.SetActive(true);
        }


        // Use this for initialization
        void Start()
        {
            HideAllPanels();
            ClickPanelData();

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
