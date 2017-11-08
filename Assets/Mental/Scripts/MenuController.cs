using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LemonSpawn
{

    [System.Serializable]
    public class MenuItem
    {
        public string text;
        public string command;
        public Vector2 grid = new Vector3(3, 3);
        public List<MenuItem> menuItems = new List<MenuItem>();
    }

    public class MenuItemObject
    {
        public MenuItem mi;
        public GameObject go;
        public float focusCounter = 0, focusTarget = 0;
        private Vector3 localscale;
        private Vector2 gridPos;
        public List<MenuItemObject> mio = new List<MenuItemObject>();
        public MenuItemObject selected;
        public void Update()
        {
            float t = 0.85f;
            focusCounter = t * focusCounter + (1 - t) * focusTarget;
            go.transform.localScale = localscale * (1 + focusCounter);

            foreach (MenuItemObject mi in mio)
                mi.Unfocus();

            if (selected != null)
                selected.Focus();

            foreach (MenuItemObject mi in mio)
                mi.Update();

        }

        public void Unfocus()
        {
            focusTarget = 0;
        }

        public void Focus()
        {
            focusTarget = 1;
        }


        public MenuItemObject(MenuItem m, Vector3 pos, Vector3 scale, Quaternion rotation, Color c, Color fc, GameObject parent)
        {
            mi = m;
            go = new GameObject("Parent");
            go.transform.parent = parent.transform;
            go.transform.localPosition = pos;
            go.transform.localRotation = rotation;
            go.transform.localScale = scale;
            localscale = scale;
            gridPos = mi.grid / 2;

            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.transform.parent = go.transform;
            plane.transform.localPosition = Vector3.zero;
            plane.transform.localRotation = Quaternion.identity;
            plane.transform.localScale = Vector3.one;

            plane.GetComponent<Renderer>().material = (Material)Resources.Load("MenuMaterial");

            GameObject tm = new GameObject();
            tm.transform.parent = go.transform;
            tm.transform.localPosition = Vector3.zero;
            tm.transform.localRotation = Quaternion.Euler(-90, -180, 0);
            tm.transform.localScale = Vector3.one;

            TextMesh ttm = tm.AddComponent<TextMesh>();
            ttm.text = m.text;
            ttm.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            ttm.color = fc;
            ttm.fontSize = 26;
            ttm.alignment = TextAlignment.Center;
            ttm.anchor = TextAnchor.MiddleCenter;

            Vector2 p = Vector2.zero;
            Vector3 dir = Vector3.forward;
            foreach (MenuItem mi2 in mi.menuItems)
            {
                float sizeScale = 1f;
                Vector3 cp = (p - mi.grid * 0.5f + Vector2.one * 0.5f) / mi.grid.x;
                Vector3 actualP = new Vector3(cp.x, 0, cp.y);
                Vector3 newP = dir * sizeScale * 15 + actualP * 12;
                Vector3 newScale = Vector3.one / mi.grid.x;
                mio.Add(new MenuItemObject(mi2, newP, newScale, Quaternion.identity, c, fc, go));
                p.x += 1;
                if (p.x >= mi.grid.x)
                {
                    p.x = 0;
                    p.y += 1;
                }
            }

            //            TextMesh tm = go.AddComponent < TextMesh();>

        }

        public MenuItemObject find(Vector2 findP)
        {
            Vector2 p = Vector2.zero;
            findP.x = (int)findP.x;
            findP.y = (int)findP.y;
            foreach (MenuItemObject mi2 in mio)
            {
                if ((findP - p).magnitude < 0.01f)
                {
                    Debug.Log("FOUND");
                    return mi2;
                }
                p.x += 1;
                if (p.x >= mi.grid.x)
                {
                    p.x = 0;
                    p.y += 1;
                }

            }
            return null;
        }

        public void Move(float x, float y)
        {
            gridPos += new Vector2(x, y);

            if (gridPos.x >= mi.grid.x) gridPos.x -= mi.grid.x;
            if (gridPos.y >= mi.grid.y) gridPos.y -= mi.grid.y;
            if (gridPos.y < 0) gridPos.y += mi.grid.y;
            if (gridPos.x < 0) gridPos.x += mi.grid.x;

            selected = find(gridPos);

        }

    }

    public class MenuController : MonoBehaviour
    {

        public Color color = Color.white;
        public Color textColor = Color.white;
        public List<MenuItem> menuItems;
        public Vector3 size = new Vector3(2, 1, 1);
        public Vector3 position = new Vector3(0, 0, 0);
        SteamVR_Controller.Device device;
        SteamVR_TrackedObject trackedobj;
        private SteamVR_TrackedController _controller;




        private GameObject go;
        private List<MenuItemObject> mio = new List<MenuItemObject>();
        private float currentRot = 0;
        // Use this for initialization
        void Start()
        {
            _controller = GetComponent<SteamVR_TrackedController>();
            trackedobj = GetComponent<SteamVR_TrackedObject>();
            device = SteamVR_Controller.Input((int)trackedobj.index);
            Generate();


        }

        private void OnEnable()
        {
            _controller = GetComponent<SteamVR_TrackedController>();
            _controller.PadTouched += HandlePadTouched;
            _controller.PadUntouched += HandlePadUnTouched;
            _controller.PadClicked += HandlePadClicked;
            //            _controller.OnPadTouched += HandlePadTouched;
            //            _controller.PadClicked += HandlePadClicked;
        }

        private void HandlePadTouched(object sender, ClickedEventArgs e)
        {
            prevPos = new Vector2(device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).x, device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y);


        }
        private void HandlePadUnTouched(object sender, ClickedEventArgs e)
        {
        }
        private void HandlePadClicked(object sender, ClickedEventArgs e)
        {
            if (currentItemFocus!=null)
                if (currentItemFocus.selected!=null)
                   Command(currentItemFocus.selected.mi.command);
        }


        public void Generate()
        {
            if (go != null)
                GameObject.Destroy(go);

            go = new GameObject("Menu");
            //            go.transform.position = Vector3.zero;
            go.transform.parent = this.transform;
            go.transform.localPosition = Vector3.zero;
            //            Vector3 scale = Vector3.one * 0.002f;
            //          scale.x *= 2;


            for (int i = 0; i < menuItems.Count; i++)
            {
                float ta = i / (float)(menuItems.Count) * Mathf.PI * 2;
                float tb = (i + 1) / (float)(menuItems.Count + 1) * Mathf.PI * 2;
                float rot = ta / (Mathf.PI * 2) * 360;
                Quaternion rotation = Quaternion.Euler(0, 0, rot);
                Vector3 p = rotation * position;

                mio.Add(new MenuItemObject(menuItems[i], p, size, Quaternion.Euler(0, 0, rot + 90), color, textColor, go));


            }

        }

        // Update is called once per frame
        Vector2 prevPos;

        MenuItemObject currentItemFocus = null;

        void UpdateAllFocus()
        {
            foreach (MenuItemObject mi in mio)
                if (mi != currentItemFocus)
                    mi.Unfocus();

            if (currentItemFocus != null)
                currentItemFocus.Focus();

            foreach (MenuItemObject mi in mio)
                mi.Update();
        }

        public float accumRot = 0;

        MenuItemObject findItemFocus()
        {
            MenuItemObject winner = mio[0];
            if (mio.Count <= 1)
                return winner;
            float wd = -1;
            for (int i = 0; i < mio.Count; i++)
            {
                Vector3 cp = transform.parent.forward;
                Vector3 n = (transform.position - mio[i].go.transform.position).normalized;
                float dist = Vector3.Dot(cp, n);
                //Debug.Log(dist);
                if (dist > wd)
                {
                    wd = dist;
                    winner = mio[i];
                }
            }
            return winner;
        }

        public enum PadControllerType { Intensity, Rotation, Opacity };
        public static PadControllerType controllerType = PadControllerType.Intensity;

        public virtual void Command(string s)
        {
            if (s.ToLower() == "setintensity")
                controllerType = PadControllerType.Intensity;
            if (s.ToLower() == "setopacity")
                controllerType = PadControllerType.Opacity;
            if (s.ToLower() == "setrotation")
                controllerType = PadControllerType.Rotation;
        }

        void Update()
        {
            Vector2 currentpos = new Vector2(device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).x, device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y);


            float deltaX = currentpos.x - prevPos.x;
            float deltaY = currentpos.y - prevPos.y;

            if (_controller.padTouched && !_controller.gripped)
            {
                currentRot = -deltaX * 100;
                accumRot += currentRot;
                prevPos = currentpos;

                if (currentItemFocus != null)
                    currentItemFocus.Move(-deltaX*2, deltaY*2);

            }

            go.transform.Rotate(new Vector3(0, 0, 1), currentRot);
            currentRot *= 0.94f;

            currentItemFocus = findItemFocus();
            UpdateAllFocus();

        }
    }

}