using UnityEngine;
using System.Collections;

namespace Genetic {
	
	public class PopulationMenu : IMenu {
		
		public PopulationMenu(string n, IMenu p) : base(n,p) {
			subMenus.Add(new NodeMenu("Nodes", this));			
		}
		
		
		public override void Render ()
		{
			base.Render ();
			//main.pop.RenderSelection(this);
			
		}
		
		
	}

	public class NodeMenu : IMenu {
		
		public NodeMenu(string n, IMenu p) : base(n,p) {
			
		}
		
		public override void Render ()
		{
			base.Render ();
			//main.pop.RenderNodeSystem(this);
			
		}
		
		
	}

	public class StateMenu : IMenu {
		
		public StateMenu(string n, IMenu p) : base(n,p) {
			
		}
		
		public override void Render ()
		{
			base.Render ();
			//main.stateManager.Render(this);
			
		}
		
		
	}
	
			
}
