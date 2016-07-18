# CryptoInator
Self contained encrypted images

CryptoInator was developed when the Danish government decided that Denmark should have a single login solution (NemID). The solution was based on a cardboard card with a series of one-time codes. Since I did not want to walk around with a paperboard cards, I decided to make this solution is a self-contained, self-encrypting, self-decrypting and self-viewing executable image viewer. 

The program can either download images from a WIA scanner or from a file. If you open eg c:\images\nemid.jpg the program will create c:\images\nemid.exe which will include both program and image. If you open the image from a scanner, the program will overwrite itself. 

The image is encryptet using AES-256 bit encryption. 

You can zoom the picture with scroll-wheeler on the mouse by holding down the Control key.

Credits go to:  
* [Oxygen Team for the ICON](https://www.iconfinder.com/icons/8794/cryptography_key_lock_log_in_login_password_security_unlock_icon#size=128)
* [Anthony Trudeau for the WIA scanning code](http://geekswithblogs.net/tonyt/archive/2006/07/29/86608.aspx)
* [Phira for the password quality meter](https://phiras.wordpress.com/2007/04/08/password-strength-meter-a-jquery-plugin/)
* [boogerjones for the Encryption part](http://www.neowin.net/forum/topic/967710-c-best-encryption-method-for-in-house-data/)
* [Heinz Doofenshmirtz for the Inator part](http://phineasandferb.wikia.com/wiki/List_of_Doofenshmirtz's_schemes_and_inventions)

