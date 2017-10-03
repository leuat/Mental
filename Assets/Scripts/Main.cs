using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {


    public GameObject cube;
    public Material myMaterial;

    public List<GameObject> balls = new List<GameObject>();

	// Use this for initialization
	void Start () {

        int N = 200;

        for (int i=0;i<N;i++)
        {

            GameObject b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Vector3 pos = new Vector3((Random.value - 0.5f) * 50, (Random.value - 0.5f) * 20, (Random.value - 0.5f) * 50);
            b.transform.position = pos;

            b.GetComponent<Renderer>().material = myMaterial;

            b.AddComponent<Rigidbody>();




        }
		
	}
	
	// Update is called once per frame
	void Update () {
//        GameObject cube = GameObject.Find("MyCube");
        float s = 20;
        cube.transform.rotation = Quaternion.Euler(Time.time*s, -0.64f * Time.time*s, Time.time * 1.524f * s);

        myMaterial.SetFloat("myTime", Time.time);

	}

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 100), "privet!"))
        {
            
        }
    }
}
