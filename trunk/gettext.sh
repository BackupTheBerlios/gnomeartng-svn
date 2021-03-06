#/bin/sh

#English-German
xgettext --from-code=UTF-8 ./src/*.cs gui.glade --no-location -o ./po/de_new.po
#English-Bulgarian
xgettext --from-code=UTF-8 ./src/*.cs gui.glade --no-location -o ./po/bg_new.po
#English-Portugese
xgettext --from-code=UTF-8 ./src/*.cs gui.glade --no-location -o ./po/pt_new.po
#English-French
xgettext --from-code=UTF-8 ./src/*.cs gui.glade --no-location -o ./po/fr_new.po
#English-Swedish
xgettext --from-code=UTF-8 ./src/*.cs gui.glade --no-location -o ./po/se_new.po

#Compile the texts
msgfmt ./po/de.po -o locale/de/LC_MESSAGES/gnomeartng.mo
msgfmt ./po/bg.po -o locale/bg/LC_MESSAGES/gnomeartng.mo
msgfmt ./po/pt_BR.po -o locale/pt_BR/LC_MESSAGES/gnomeartng.mo
msgfmt ./po/fr.po -o locale/fr/LC_MESSAGES/gnomeartng.mo
msgfmt ./po/se.po -o locale/se/LC_MESSAGES/gnomeartng.mo
