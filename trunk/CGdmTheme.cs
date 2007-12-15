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
	//Ordner /usr/share/gdm/themes/<theme_name>
	
	public class CGdmTheme: CTheme
	{
		

		private string previousTheme="";
		private string gdmconfig=@"/etc/gdm/gdm.conf";
		private StringBuilder sb;
		private string tarParams="";
		private bool randomThemeActive =false;
		private bool gdmConfAvailable=false;
		private bool customGdmConfAvailable=false;
		
		/*
		 * 2 Dateien sind wichtig:
		 * /etc/gdm/gdm.conf um herauszufinden ob u.u Multiselect gewählt ist
		 * /etc/gdm/gdm.conf-custom um die gewählten Themes herauszulesen und zu setzen
		 */
		override public void Install(){
			LocalThemeFile=config.ThemesPath+Path.GetFileName(DownloadUrl);
			gdmConfAvailable=File.Exists(gdmconfig);
			customGdmConfAvailable=File.Exists(gdmconfig+"-custom");
			if (!installationIsPossible) 
				throw new Exception("Installation is not possible! Please check your environment");
			if (gdmConfAvailable){
				sb = config.Execute("grep","GraphicalThemeRand= "+gdmconfig);
				if (sb.Length<1) 
					throw new Exception("Installation is not possible! grep returned no GraphicalThemeRand-Entry\"");
				else
					randomThemeActive=bool.Parse(sb.ToString().Split('=')[1]);
				
				if (customGdmConfAvailable){
					
					//Entpackparameter
					tarParams = "tar "+config.GetTarParams(DownloadUrl);
					
					//Herunterladen
					if (!File.Exists(LocalThemeFile))
						DownloadFile(DownloadUrl, LocalThemeFile);
					
					//Entpacken
					sb = config.Execute("gksu","'"+tarParams+LocalThemeFile+" -C "+config.GdmInstallPath+"'");
					string[] FolderName=sb.ToString().Split('/');
					//Console.WriteLine(FolderName[0]);

					//Eintrag " GraphicalTheme=Themename" 
					if (!randomThemeActive){
						sb = config.Execute("grep","GraphicalTheme= "+gdmconfig+"-custom");
						previousTheme=sb.ToString();
						//Console.WriteLine("PreviousTheme: "+previousTheme);
						config.Execute("gksu", @"'sed -i 's/"+sb.ToString()+@"/GraphicalTheme="+FolderName[0]+@"/' "+gdmconfig+"-custom'");
						//GDM Die Änderungen mitteilen
						config.Execute("gdmflexiserver", "--command=\"UPDATE_CONFIG greeter/GraphicalTheme\"");
						//Console.WriteLine("Executed command: gksu 'sed -i 's/"+sb.ToString()+"/GraphicalTheme="+FolderName[0]+"/' "+gdmconfig+"-custom'")
					} else{
						//Random ist aktiv :/
						config.Execute("gdmflexiserver", "--command=\"UPDATE_CONFIG greeter/GraphicalThemes\"");	
					}
					revertIsAvailable=true;
				}
			} else
				throw new Exception("Installation is not possible! no gdm.conf found\"");
			
		}

		override public void Revert(){
			if (revertIsAvailable){
				if (customGdmConfAvailable){
					//Eintrag " GraphicalTheme=Themename" 
					if (!randomThemeActive){
						sb = config.Execute("grep","GraphicalTheme= "+gdmconfig+"-custom");
						Console.WriteLine("PreviousTheme: "+previousTheme);
						config.Execute("gksu", "sed -i 's/"+sb.ToString()+"/GraphicalTheme="+previousTheme+"/' "+gdmconfig+"-custom");
					} else{
						//Random ist aktiv :/
						
					}
					revertIsAvailable=false;
				}
			}
		}
	
		
		public CGdmTheme(CConfiguration config):base(config) {
			installationIsPossible=config.GrepIsAvailable && config.TarIsAvailable && config.SedIsAvailable;
		}

	}
}
