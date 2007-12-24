// /home/neo/Projects/GnomeArtNG/GnomeArtNG/CGdmTheme.cs created with MonoDevelop
// User: neo at 12:16 24.11.2007
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.IO;
using System.Text;

namespace GnomeArtNG
{
	//Systemweiter Ordner: /usr/share/gdm/themes/<theme_name>
	
	public class CGdmTheme: CTheme
	{
		
		private string previousTheme="";
		private string gdmconf=@"/etc/gdm/gdm.conf";
		private string gdmconfcustom=@"/etc/gdm/gdm.conf-custom";
		private StringBuilder sb;
		private string tarParams="";
		private bool randomThemeActive =false;
		private bool gdmConfAvailable=false;
		private bool gdmConfCustomAvailable=false;
		private CIniWorker iworker;
		private string[] FolderName;		
		
		override protected void PreInstallation(CStatusWindow sw){
			LocalThemeFile=config.ThemesPath+Path.GetFileName(DownloadUrl);
			gdmConfAvailable=File.Exists(gdmconf);
			gdmConfCustomAvailable=File.Exists(gdmconfcustom);
			if (!gdmConfAvailable)
				throw new Exception("Installation is not possible! No gdm.conf available");
			if (!installationIsPossible) 
				throw new Exception("Installation is not possible! Only GDM is supported");
			
			//TODO: grep ersetzen durch IniWorker
			sb = config.Execute("grep","GraphicalThemeRand= "+gdmconf);
			if (sb.Length<1) 
				throw new Exception("Installation is not possible! grep returned no GraphicalThemeRand-Entry\"");
			else
				randomThemeActive=bool.Parse(sb.ToString().Split('=')[1]);
			
			//Falls nicht vorhanden wird sie erzeugt
			if (!gdmConfCustomAvailable){
				try{
					FileStream fs = File.Create("/tmp/gdm.conf-custom");
					fs.Close();
					config.Execute("gksudo","'mv /tmp/gdm.conf-custom "+gdmconfcustom);
				}
				catch {throw new Exception("Gdm.conf-custom couldn't be created, aborting!");}
			}
			sw.SetProgress("1/"+installationSteps);
			//Datei einlesen
			iworker = new CIniWorker(gdmconfcustom);
			iworker.CreateSections("daemon;security;xdmcp;gui;greeter;chooser;debug;servers",';');
			//Entpackparameter
			tarParams = "tar "+config.GetTarParams(DownloadUrl);
			//Herunterladen
			sw.Mainlabel=CConfiguration.txtDownloadTheme;
			if (!File.Exists(LocalThemeFile))
				DownloadFile(DownloadUrl, LocalThemeFile);
			sw.SetProgress("2/"+installationSteps);
			//Entpacken
			sw.Mainlabel = CConfiguration.txtExtracting;
			sb = config.Execute("gksu","'"+tarParams+LocalThemeFile+" -C "+config.GdmInstallPath+"'");
			FolderName=sb.ToString().Split('/');
			//Console.WriteLine(FolderName[0]);
			sw.SetProgress("3/"+installationSteps);
		}

		/*
		 * 2 Dateien sind wichtig:
		 * /etc/gdm/gdm.conf um herauszufinden ob u.U Multiselect gewählt ist
		 * /etc/gdm/gdm.conf-custom um die gewählten Themes herauszulesen und zu setzen
		 */
		override protected void Installation(CStatusWindow sw){
			//Eintrag "GraphicalTheme=Themename" 
			sw.Mainlabel=CConfiguration.txtInstalling;
			if (!randomThemeActive){
				//Sichern
				previousTheme = iworker.getValue("greeter","GraphicalTheme",true);
				//Console.WriteLine("PreviousTheme: "+previousTheme);
				iworker.setValue("greeter","GraphicalTheme",FolderName[0],true);
				//Kopieren an einen Ort an dem Schreibberechtigung vorhanden ist 
				iworker.Save("/tmp/gdm.conf-custom");
				//Per gksudo den Benutzer für diese Aktion zum Superuser werden lassen
				config.Execute("gksudo","'mv /tmp/gdm.conf-custom /etc/gdm/'");
			} else{
				//TODO: für Random
			}
			//GDM die Änderungen mitteilen
			config.Execute("gdmflexiserver", "--command=\"UPDATE_CONFIG greeter/GraphicalTheme\"");
		}
		
		override protected void PostInstallation(CStatusWindow sw){
			revertIsAvailable=true;
		}
		
		override public void Revert(){
			if (revertIsAvailable){
				//Eintrag " GraphicalTheme=Themename" 
				if (!randomThemeActive){
					//IWorker wurde bei der Installation schon erzeugt
					iworker.setValue("greeter","GraphicalTheme",previousTheme,true);
					//Kopieren an einen Ort an dem Schreibberechtigung vorhanden ist 
					iworker.Save("/tmp/gdm.conf-custom");
					config.Execute("gksudo","'mv /tmp/gdm.conf-custom /etc/gdm/'");
				} else{
					//Random ist aktiv :/
				}
				config.Execute("gdmflexiserver", "--command=\"UPDATE_CONFIG greeter/GraphicalTheme\"");
				revertIsAvailable=false;
			}
		}
	
		
		public CGdmTheme(CConfiguration config):base(config) {
			installationIsPossible=config.GrepIsAvailable && config.TarIsAvailable && config.SedIsAvailable;
			installationSteps=4;
		}

	}
}
