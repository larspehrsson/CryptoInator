# CryptoInator
Self contained encrypted images

CryptoInator was developed when the Danish government decided that Denmark should have a single login solution (NemID). The solution was based on a cardboard card with a series of one-time codes. Since I did not want to walk around with a paperboard cards, I decided to make this solution is a self-contained, self-encrypting, self-decrypting and self-viewing executable image viewer. 

The program can either download images from a WIA scanner or from a file. If you open eg c:\images\nemid.jpg the program will create c:\images\nemid.exe which will include both program and image. If you open the image from a scanner, the program will overwrite itself. 

The image is encryptet using AES-256 bit encryption. 

You can zoom the picture with scroll-wheeler on the mouse by holding down the Control key.

