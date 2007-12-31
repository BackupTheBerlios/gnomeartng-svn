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
using Mono.Unix;

namespace GnomeArtNG
{
	
	
	public class CPreviewWindow
	{
		private Gtk.Window mainWindow;
		private CTheme theme;
		public Gtk.Window MainWindow {
			get {return mainWindow;}
		}
		
		//PreviewWindow
		[Widget] Gtk.Image PreviewMainImage;
		[Widget] Gtk.Label PreviewHeadLabel;
		[Widget] Gtk.Button PreviewCloseButton;
		[Widget] Gtk.Button PreviewInstallButton;
		
		public Gdk.Pixbuf MainImagePixbuf{
			get{return PreviewMainImage.Pixbuf;}
			set{
				double vfac;
				Gdk.Pixbuf pix=value;
				if ((pix.Width<=640) & (pix.Height<=480))
					PreviewMainImage.Pixbuf = pix;
				else if (pix.Height>pix.Width){
					//Verkleinerungsfaktor
					vfac= (double)480/pix.Height;
					//Console.WriteLine("VFaktor: "+vfac+" Bildbreite: "+pix.Width+" Bildhöhe: "+pix.Height);
					PreviewMainImage.Pixbuf = pix.ScaleSimple((int)(vfac*pix.Width),480,Gdk.InterpType.Bilinear);
				} 
				else {
					//Verkleinerungsfaktor
					vfac= (double)640/pix.Width;
					//Console.WriteLine("VFaktor: "+vfac+" Bildbreite: "+pix.Width+" Bildhöhe: "+pix.Height);
					PreviewMainImage.Pixbuf = pix.ScaleSimple(640,(int)(vfac*pix.Height),Gdk.InterpType.Bilinear);
				}
			}
		}

		public string Headline{
			get{return PreviewHeadLabel.Text;}
			set{PreviewHeadLabel.Text = value;}
		}
		
		public CPreviewWindow(CTheme theme,bool Show)	{
			string prevW="PreviewWindow";
			this.theme = theme;
			Glade.XML previewXml= new Glade.XML (null, "gui.glade", prevW, null);
			previewXml.Autoconnect (this);
			mainWindow = (Gtk.Window) previewXml.GetWidget (prevW);
			PreviewCloseButton.Clicked+=new EventHandler(OnCloseButtonClicked);
			PreviewInstallButton.Clicked+=new EventHandler(OnInstallButtonClicked);
			MainImagePixbuf = new Gdk.Pixbuf(theme.LocalPreviewFile);
			Headline = Catalog.GetString("Previewing \"")+ theme.Name+"\"";
			if(Show)
				MainWindow.ShowAll();
		}

	
		private void OnCloseButtonClicked (object sender, EventArgs b){
			mainWindow.Destroy();
		}
		
		private void OnInstallButtonClicked (object sender, EventArgs b){
			theme.StartInstallation();
			mainWindow.Destroy();
		}
		
	
	}
}
