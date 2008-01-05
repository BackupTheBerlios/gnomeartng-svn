// /home/neo/Projects/Monodevelop-Projects/GnomeArtNG/CAboutWindow.cs created with MonoDevelop
// User: neo at 17:50 13.12.2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using Gtk;
using Gdk;
using Glade;
using GnomeArtNG;

namespace GnomeArtNG
{
	
	
	public class CAboutWindow
	{
		
		private Gtk.Window mainWindow;
		public Gtk.Window MainWindow {
			get {return mainWindow;}
		}
		
		//AboutWindow
		//[Widget] Gtk.Label AboutHeadLabel;
		[Widget] Gtk.Label AboutVersion;
		[Widget] Gtk.Button AboutCloseButton;
		[Widget] Gtk.Image AboutImage;
		
		public CAboutWindow(string Version,bool ShowWindow)	{
			string aboutW="AboutWindow";
			Glade.XML aboutXml= new Glade.XML (null, "gui.glade", aboutW, null);
			aboutXml.Autoconnect (this);
			mainWindow = (Gtk.Window) aboutXml.GetWidget (aboutW);
			AboutVersion.Text=Version;
			AboutCloseButton.Clicked+=new EventHandler(OnAboutCloseButtonClicked);
			AboutImage.Pixbuf = new Gdk.Pixbuf("./gnome.png");
			if(ShowWindow)
				mainWindow.ShowAll();
		}
		
		private void OnAboutCloseButtonClicked (object sender, EventArgs b){
			mainWindow.Destroy();
		}
	}
}
