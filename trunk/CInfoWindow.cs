// /home/neo/Projects/Monodevelop-Projects/GnomeArtNG/CInfoWindow.cs created with MonoDevelop
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
	
	
	public class CInfoWindow
	{
		
		private Gtk.Window mainWindow;
		public Gtk.Window MainWindow {
			get {return mainWindow;}
		}
		
		//InfoWindow
		[Widget] Gtk.Label InfoHeadLabel;
		//[Widget] Gtk.Label InfoDescriptionLabel;
		[Widget] Gtk.TextView InfoDescription;
		[Widget] Gtk.Button InfoOkButton;
		[Widget] Gtk.Image InfoImage;
		
		
		public string Headline{
			get{
				return InfoHeadLabel.Text;
			}
			set{
				InfoHeadLabel.Text = "<b>"+value+"</b>";
				InfoHeadLabel.UseMarkup=true;
			}
		}
		
		public string Description{
			get{return InfoDescription.Buffer.Text;}
			set{InfoDescription.Buffer.Text = InfoDescription.Buffer.Text+value;}
		}
		
		public void ClearDescription(){
			InfoDescription.Buffer.Clear();
		}
		
		public CInfoWindow(string Headline,string Description, string StockIcon, bool ShowWindow)	{
			string infoW="InfoWindow";
			Glade.XML infoXml= new Glade.XML (null, "gui.glade", infoW, null);
			infoXml.Autoconnect (this);
			mainWindow = (Gtk.Window) infoXml.GetWidget (infoW);
			this.Headline = Headline;
			this.Description = Description;
			InfoOkButton.Clicked+=new EventHandler(OnOkButtonClicked);
			InfoImage.Stock= StockIcon;
			if(ShowWindow)
				Show();
		}
				
		public void Close(){
			mainWindow.Destroy();
		}
		
		public void Invalidate(){
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();
		}
		
		public void Show(){
			mainWindow.ShowAll();
			Invalidate();
		}
		
		private void OnOkButtonClicked (object sender, EventArgs b){
			Close();
		}
	}
}
