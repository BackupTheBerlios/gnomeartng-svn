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
		private bool closeRequested;
		public Gtk.Window MainWindow {
			get {return mainWindow;}
		}
		public bool CloseRequested{
			get{ return closeRequested;}
		}
		
		//StatusWindow
		[Widget] Gtk.Label StatusHeadLabel;
		[Widget] Gtk.Label StatusMainLabel;
		[Widget] Gtk.Button StatusCancelButton;
		[Widget] Gtk.ProgressBar StatusProgressBar;
		[Widget] Gtk.Label StatusExtInfoLabel;
		[Widget] Gtk.Expander ExtendedInfoExpander;
		
		
		public string Headline{
			get{return StatusHeadLabel.Text;}
			set{
				StatusHeadLabel.Text = "<b>"+value+"</b>";
				StatusHeadLabel.UseMarkup=true;
			}
		}
		
		public bool ExpanderLabelVisible{
			get{ return ExtendedInfoExpander.Expanded;}
			set{ 
				ExtendedInfoExpander.Expanded = value;
				Invalidate();
			}
		}
		
		public string ExtInfoLabel{
			get{ return StatusExtInfoLabel.Text; }
			set{ 
				StatusExtInfoLabel.Text="<i>"+value+"</i>";
				StatusExtInfoLabel.UseMarkup=true;
			}
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
		
		public bool ButtonSensitive{
			get {return StatusCancelButton.Sensitive;}
			set{StatusCancelButton.Sensitive=value;}
		}
		
		public void Pulse(){
			StatusProgressBar.Pulse();
		}
		
		public void Invalidate(){
			while (Gtk.Application.EventsPending ())
				Gtk.Application.RunIteration ();
		}
		
		public void SetProgressStep(int MaxCount){
			if (MaxCount==0)
				MaxCount=1;
			StatusProgressBar.PulseStep=(double)(1/MaxCount);
			StatusProgressBar.Fraction=0.0;
		}
		
		public CStatusWindow(string Headline,int MaxCount,bool CloseByRequest, bool ExpandExpander, bool ShowWindow)	{
			string statusW="StatusWindow";
			Glade.XML statusXml= new Glade.XML (null, "gui.glade", statusW, null);
			statusXml.Autoconnect (this);
			mainWindow = (Gtk.Window) statusXml.GetWidget (statusW);
			this.Headline = Headline;
			if (CloseByRequest)
				StatusCancelButton.Clicked+=new EventHandler(OnCancelRequestButtonClicked);
			else
				StatusCancelButton.Clicked+=new EventHandler(OnCancelButtonClicked);
			ExpanderLabelVisible = ExpandExpander;
			SetProgressStep(MaxCount);
			if(ShowWindow)
				Show();
		}
				
		public void Close(){
			closeRequested=false;
			mainWindow.Destroy();
		}
		
		public void Show(){
			mainWindow.ShowAll();
			Invalidate();
		}
		
		private void OnCancelButtonClicked (object sender, EventArgs b){
			Close();
		}
		
		private void OnCancelRequestButtonClicked(object sender, EventArgs b){
			closeRequested=true;
		}
		
	
	}
}
