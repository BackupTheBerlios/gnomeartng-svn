#!/bin/bash
if [ $# -lt 2 ]; then
  echo "Es fehlen Informationen:";
  echo "1: Die Versionsnummer mit der GnomeArt gepackt wird";
  echo "2: Das Verzeichnis in dem die Readme von GnomeArt-NextGen liegt"
  exit 1;
fi

if [ ! -d "$2" ]; then
    echo "Pfad zu GnomeArt-NextGen passt nicht"
    exit 1
fi
vzname=GnomeArtNG-$1
#versioncommand=
version=`cat /etc/lsb-release | grep DISTRIB_CODENAME | sed s/.*=//`

echo "---Wechsele ins GnomeArtNg Verzeichnis"
cd "$2"

echo "---Kompiliere GnomeArtNg für ${version}"
./gettext.sh
./compile

if [ ! -d Releases ]; then
    echo "Creating directory Releases"
    mkdir Releases
fi

echo "---Creating Binary package(tar.gz)"
mkdir $vzname

echo "---Kopiere die benötigten Dateien in das Unterverzeichnis"
cp -r locale $vzname/
cp GnomeArtNg.exe $vzname/
cp README $vzname/
cp VERSION $vzname/
cp runGnomeArtNG.sh $vzname/
cp -r images $vzname/

echo "---Lösche alle .svn-Dateien"
cd $vzname
find . -name .svn -exec rm -Rf {} \;
cd ..

gzname=$vzname-$version.tar.gz

echo "---Packe das Verzeichnis"
tar -czf $gzname $vzname/

echo "---Lösche das Verzeichnis"
cd $vzname
rm -r *
cd ..
rmdir $vzname

echo "---Verschiebe tar-file in Releases"
mv $gzname ./Releases/
echo "###->Creating Binary package($gzname) -> finished"

echo "Creating $1 debian package"
vzname=gnomeartng_deb
destination=$vzname/usr/share/gnomeartng
versionfile=$vzname/DEBIAN/control
sed -i "s/Version:.*/Version: $1/g" $versionfile
cp -r locale $destination
cp -r images $destination
cp README $destination
cp VERSION $destination
cp TODO $destination
cp runGnomeArtNG.sh $destination
cp GnomeArtNg.exe $destination
rm -f $destination/images/GaNG-Design.svg
rm -f $destination/images/Icon.svg
rm -f $destination/software-update-icon-512x512.png
cd $vzname
find . -name .svn -exec rm -Rf {} \;
cd ..
dpkg -b gnomeartng_deb gnomeartng-$1-$version.deb 
mv gnomeartng-$1-$version.deb ./Releases/
echo "Debian-Paket wurde erstellt"

echo "--> Build finished for version $1"

