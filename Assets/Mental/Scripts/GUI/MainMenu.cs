using UnityEngine;
using System.Collections;

namespace Genetic {

public class MainMenu : IMenu {

	public Vector3 orgGravity = Physics.gravity;
		
		
	public MainMenu(string n, IMenu p) : base(n,p) {
	
		addMenu( new PopulationMenu("Population", this));
		addMenu( new StateMenu("States", this));
			
	}
		
				
	public override void Render ()
		{
			base.Render ();
			if (GUI.Button(getRect(), "Save")) {
				//main.pop.Save();
			}
			if (GUI.Button(getRect(), "Load")) {
				//main.pop.Load();
			}
			if (GUI.Button(getRect(), "Gravity")) {
					if (Physics.gravity.magnitude<0.001)
						Physics.gravity = orgGravity;
					else
						Physics.gravity = new Vector3(0,0,0);
				}
				
			
			
			
		}


}

}
