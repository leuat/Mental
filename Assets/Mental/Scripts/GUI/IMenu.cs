using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Genetic {

public class IMenu {

	public Vector2 currentPos = new Vector2(25,20);
	public Vector2 deltaPos = new Vector3(0,25);
	public static int spaceY = 5;
	public static int width = 200;
	public float widthScale = 1.2f;
	public Vector2 windowPos = new Vector2(0,0);
	public Vector2 windowSize = new Vector2(200,0);
	public int currentI = 0;
	public static Texture2D background = null;
	public string Name = "";
	public IMenu activeMenu = null;
	public IMenu parent = null;
	public List<IMenu> subMenus = new List<IMenu>();
	
	private void Reset() {
		renderBackground();
		currentI = 0;
	}		
	
	public IMenu(string n, IMenu p) {
		Name = n;
		parent = p;
		if (parent!=null)
			currentPos.x=parent.currentPos.x + (int)(IMenu.width*widthScale);
	}
	
	
	public IMenu addMenu( IMenu m ) {
		subMenus.Add (m);
		return m;
	}
	
	public Rect getRect() {
		return new Rect(currentPos.x,currentPos.y + (deltaPos.y+spaceY)*currentI++, width, deltaPos.y);
	}
	
	public List<Rect> getSplitRects(float split) {
		List<Rect> r = new List<Rect>();
		
		r.Add (new Rect(currentPos.x,currentPos.y + (deltaPos.y+spaceY)*currentI, width*split, deltaPos.y));
		r.Add (new Rect(currentPos.x + width*split,currentPos.y + (deltaPos.y+spaceY)*currentI++, width*(1-split), deltaPos.y));
		return r;
	}
	
	
	public Rect getCustomRect(int sx, int sy) {
		Rect r =  new Rect(currentPos.x,currentPos.y + (deltaPos.y + spaceY)*currentI, sx, sy);
		currentI+= (int)(sy/(deltaPos.y + spaceY)) +1;
		return r;
	}
	
	public void renderBackground() {
		if (background == null) {
			background = new Texture2D (1, 1, TextureFormat.RGBA32, false);
			Color c = new Color(0.3f,0.7f, 0.7f);
			c.a = 0.25f;
			background.SetPixel (0, 0, c);
			background.Apply ();
		}
		
		float b = 6;
		
		Rect r = new Rect(currentPos.x - b, currentPos.y -b, width + 2*b, (deltaPos.y + spaceY)*currentI + 2*b);
		GUI.DrawTexture(r, background);	
		
			
	}
	
	public virtual void Render() {
		Reset();
		//if (parent!=null)
/*		if (GUI.Button(getRect(), "Close")) 
			parent.activeMenu = null;			
*/		
		foreach (IMenu m in subMenus) {
			GUI.color = Color.white;
			if (activeMenu == m)
					GUI.color = Color.yellow;
			if (activeMenu != m) { 
				if (GUI.Button(getRect(),  m.Name + ">")) {
					activeMenu = m;
				}
			}
			else
			{
				if (GUI.Button(getRect(),  m.Name + "<")) {
					activeMenu = null;
				}
					
			}
		}
			GUI.color = Color.white;
			
		if (activeMenu!=null)
		 	activeMenu.Render();

			GUI.color = Color.white;
			
	}
	
	

}

}