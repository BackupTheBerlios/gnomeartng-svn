// /home/neo/Projects/Monodevelop-Projects/GnomeArtNG/CPreviewWindow.cs created with MonoDevelop
// User: neo at 22:09 02.12.2007
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
	
	
	public class CPreviewWindow
	{
		private Gtk.Window mainWindow;
		public Gtk.Window MainWindow {
			get {return mainWindow;}
		}
		
		//PreviewWindow
		[Widget] Gtk.Image PreviewMainImage;
		[Widget] Gtk.Label PreviewHeadLabel;
		[Widget] Gtk.Button PreviewCloseButton;
		
		public Gdk.Pixbuf MainImagePixbuf{
			get{return PreviewMainImage.Pixbuf;}
			set{PreviewMainImage.Pixbuf = value;}
		}

		public string Headline{
			get{return PreviewHeadLabel.Text;}
			set{PreviewHeadLabel.Text = value;}
		}
		
		public CPreviewWindow()	{
			string prevW="PreviewWindow";
			Glade.XML previewXml= new Glade.XML (null, "gui.glade", prevW, null);
			previewXml.Autoconnect (this);
			mainWindow = (Gtk.Window) previewXml.GetWidget (prevW);
			PreviewCloseButton.Clicked+=new EventHandler(OnCloseButtonClicked);
		}
				
		public static CPreviewWindow CreateWin(string Headline,string previewFile,bool Show){
			CPreviewWindow prevObj=new CPreviewWindow(); 
			prevObj.MainImagePixbuf=new Gdk.Pixbuf(previewFile);
			prevObj.Headline = Headline;
			if(Show)
				prevObj.MainWindow.ShowAll();
			return prevObj;
			
		}
	
		private void OnCloseButtonClicked (object sender, EventArgs b){
			mainWindow.Destroy();
		}
	
	}
}
