sudo apt-get install wget
wget http://chewymoon.me/ci/System.zip
sudo apt-get install unzip
unzip System.zip -d LSBin

for file in LSBin/*.dll
do
    gacutil -i "$file"
done

