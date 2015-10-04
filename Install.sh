sudo apt-get install wget
sudo apt-get install unzip
wget http://chewymoon.me/ci/System.zip
unzip System.zip -d LSBin

for file in LSBin/*.dll
do
    sudo cp "$file" "/usr/lib/mono/4.5/"
done

