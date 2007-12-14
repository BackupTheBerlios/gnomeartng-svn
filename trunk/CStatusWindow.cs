// /home/neo/Projects/Monodevelop-Projects/GnomeArtNG/CStatusWindow.cs created with MonoDevelop
// User: neo at 20:50 03.12.2007
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
	
	
	public class CStatusWindow
	{
		
		private Gtk.Window mainWindow;
		public Gtk.Window MainWindow {
			get {return mainWindow;}
		}
		
		//StatusWindow
		[Widget] Gtk.Label StatusHeadLabel;
		[Widget] Gtk.Label StatusMainLabel;
		[Widget] Gtk.Button StatusCancelButton;
		[Widget] Gtk.ProgressBar StatusProgressBar;
		
		
		public string Headline{
			get{return StatusHeadLabel.Text;}
			set{StatusHeadLabel.Text = value;
			StatusHeadLabel.UseMarkup=true;}
		}
		
		public string Mainlabel{
			get{return StatusMainLabel.Text;}
			set{
				StatusMainLabel.Text = value;
				StatusMainLabel.UseMarkup=true;
				Invalidate();
			}
		}
		
		public void SetProgress(string Text){
			//SetFraction, PulseStep, SetPulse, SetText
			StatusProgressBar.Text=Text;
			StatusProgressBar.Fraction = StatusProgressBar.Fraction+StatusProgressBar.PulseStep;
			Invalidate();
		}
		
		public void Invalidate(){
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();
		}
		
		public CStatusWindow(string Headline,bool ShowWindow,int MaxCount)	{
			string statusW="StatusWindow";
			Glade.XML statusXml= new Glade.XML (null, "gui.glade", statusW, null);
			statusXml.Autoconnect (this);
			mainWindow = (Gtk.Window) statusXml.GetWidget (statusW);
			this.Headline = Headline;
			StatusCancelButton.Clicked+=new EventHandler(OnCancelButtonClicked);
			StatusProgressBar.PulseStep=(double)1/MaxCount;
			StatusProgressBar.Fraction=0.0;
			if(ShowWindow)
				Show();
			
		}
				
		public void Close(){
			mainWindow.Destroy();
		}
		public void Show(){
			mainWindow.ShowAll();
		}
		
		private void OnCancelButtonClicked (object sender, EventArgs b){
			mainWindow.Destroy();
		}
	
	}
}
