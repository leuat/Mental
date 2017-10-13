using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace LemonSpawn
{

    public class UIManager : MonoBehaviour
    {

        public GameObject pnlAtlas, pnlData, pnlParameters;
        public GameObject dropForceResolution;

        public GameObject pnlActive = null;

        private void HideAllPanels()
        {
            pnlAtlas.SetActive(false);
            pnlData.SetActive(false);
            pnlParameters.SetActive(false);
        }

        private void ShowAllPanels()
        {
            pnlAtlas.SetActive(true);
            pnlData.SetActive(true);
            pnlParameters.SetActive(true);
        }



        public void SetActive(GameObject p)
        {
            if (p!=null)
                p.SetActive(true);
            pnlActive = p;
        }

        public void ClickPanelData()
        {
            HideAllPanels();
            SetActive(pnlData);
            
        }

        public void ClickPanelAtlas()
        {
            HideAllPanels();
            SetActive(pnlAtlas);
        }

        public void ClickPanelParameters()
        {
            HideAllPanels();
            SetActive(pnlParameters);
        }

        public string getComboValue(string s)
        {
            ShowAllPanels();
            Dropdown d = GameObject.Find(s).GetComponent<Dropdown>();
            HideAllPanels();
            SetActive(pnlActive);
            return d.options[d.value].text;

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
