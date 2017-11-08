using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LemonSpawn;
using Genetic;

public class MainMenuController : MonoBehaviour {

    // Use this for initialization
    MainMenu mainMenu = new MainMenu("Main Menu", null);
	void Start () {
		
	}
	
    void OnGUI()
    {
        //mainMenu.Render();
        float dy = 0.2f;
        float w = 0.1f;
        float h = 0.04f;
        float x = (0.5f - w)*Screen.width;
        float y = (0.5f - h) ;
        int yy = -1;
        GUI.Button(new Rect(x, (y + yy * dy)*Screen.height, 2*w*Screen.width, 2*h*Screen.height), "3D Viewer");
        yy++;
        GUI.Button(new Rect(x, (y + yy * dy) * Screen.height, 2 * w * Screen.width, (2 * h) * Screen.height), "Node Tools");
        yy++;
        GUI.Button(new Rect(x, (y + yy * dy) * Screen.height, 2 * w * Screen.width, (2 * h) * Screen.height), "VR World");


    }
    // Update is called once per frame
    void Update () {
		
	}
}
