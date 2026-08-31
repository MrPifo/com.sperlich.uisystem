using UnityEngine;

namespace Sperlich.UISystem {
	public interface ISubMenu : IMenu {

		/// <summary>
		/// The Parent Menu
		/// </summary>
		public IMenu ParentMenu { get; set; }
		public int MenuOrder { get; set; }

		public void Enable();
		public void Disable();
	}
}