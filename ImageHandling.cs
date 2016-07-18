using System.ComponentModel;
using System.IO;
using System.Text;
using CryptoUtils;
using Microsoft.Win32;

namespace CryptoInator
{

    partial class MainWindow
    {
        private int _imagesize;

        /// <summary>
        /// Read the last 16 bytes of the executable to see if there is an image embedded. 
        /// If there is an image, its size is stored in _imagesize.
        /// </summary>
        /// <returns></returns>
        private bool CheckForImage()
        {
            const int numBytesToRead = 16;
            var exe = new FileStream(_currentexe, FileMode.Open, FileAccess.Read, FileShare.Read);
            exe.Seek(-numBytesToRead, SeekOrigin.End);
            var bytes = new byte[numBytesToRead];
            exe.Read(bytes, 0, numBytesToRead);
            exe.Close();

            var enc = new UTF8Encoding();
            string filedata = enc.GetString(bytes);

            //find image part of exefile
            if (filedata.IndexOf("|XYZ|") > -1)
            {
                _imagesize = int.Parse(filedata.Substring(1, 10));
                return true;
            }
            return false;
        }


        /// <summary>
        /// Gets image data from exe and decrypts it. Global _imgbytes stores the image.
        /// </summary>
        /// <param name="password"></param>
        /// <returns>True if successful decrypt</returns>
        private bool GetImage(string password)
        {
            // get image from executable
            var exe = new FileStream(_currentexe, FileMode.Open, FileAccess.Read, FileShare.Read);

            exe.Seek(exe.Length - _imagesize - 16, SeekOrigin.Begin);
            int numBytesToRead = _imagesize;

            var bytes = new byte[numBytesToRead];
            exe.Read(bytes, 0, numBytesToRead);
            exe.Close();

            var enc = new UTF8Encoding();
            string encryptet = enc.GetString(bytes);

            // decrypt image data
            _imgbytes = StringEncryption.DecryptStringByte(encryptet, password);

            // wrong password
            if (_imgbytes.Length == 1)
            {
                _imgbytes = null;
                return false;
            }

            return true;
        }


        /// <summary>
        /// Gets image data from exe and decrypts it. Global _imgbytes stores the image.
        /// </summary>
        /// <param name="password"></param>
        /// <returns>True if successful decrypt</returns>
        private void BackgroundGetImage(object s, DoWorkEventArgs e)
        {
            var password = (string) e.Argument;

            // get image from executable
            var exe = new FileStream(_currentexe, FileMode.Open, FileAccess.Read, FileShare.Read);

            exe.Seek(exe.Length - _imagesize - 16, SeekOrigin.Begin);
            int numBytesToRead = _imagesize;

            var bytes = new byte[numBytesToRead];
            exe.Read(bytes, 0, numBytesToRead);
            exe.Close();

            var enc = new UTF8Encoding();
            string encryptet = enc.GetString(bytes);

            // decrypt image data
            _imgbytes = StringEncryption.DecryptStringByte(encryptet, password);

            // wrong password
            if (_imgbytes.Length == 1)
            {
                _imgbytes = null;
                e.Cancel = true;
                return;
            }

            e.Cancel = false;
        }


        /// <summary>
        /// Copies the executable to "filename". Encrypts image data and adds it to the new executable.
        /// </summary>
        /// <param name="filename">Destination file</param>
        /// <param name="password">Password that should be used to encrypt image</param>
        private string SaveImage(string filename, string password)
        {
            string newexefile = Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename) +
                                ".exe";

            // create a new executable
            var fstream = new FileStream(newexefile, FileMode.Create);

            //Copies the current executable to the new executable
            var exe = new FileStream(_currentexe, FileMode.Open, FileAccess.Read, FileShare.Read);

            var numBytesToRead = (int) exe.Length;
            var bytes = new byte[numBytesToRead];
            exe.Read(bytes, 0, numBytesToRead);

            // Save executable data to new file
            fstream.Write(bytes, 0, bytes.Length);

            // Encrypt encryptetbytes and embed in exe file
            string encryptet = StringEncryption.EncryptByteArray(_imgbytes, password);
            byte[] encryptetbytes = Encoding.UTF8.GetBytes(encryptet);
            fstream.Write(encryptetbytes, 0, encryptetbytes.Length);

            // Save image sizedata and a recognizable string
            byte[] delim = Encoding.UTF8.GetBytes(string.Format("|{0:0000000000}|XYZ|", encryptetbytes.Length));
            fstream.Write(delim, 0, delim.Length);
            fstream.Close();

            return newexefile;
        }


        /// <summary>
        ///  Asks for filename to either new exefile (if scanned) og image file.
        /// </summary>
        /// <returns>Filename</returns>
        private string GetFileName()
        {
            var openFileDialog = new OpenFileDialog();

            if (_imagesize == 0)
                openFileDialog.Filter = "All files (*.*)|*.*|Image files|*.tif;*.bmp;*.gif;*.jpg;*.png;*.ico.*";
            else
            {
                openFileDialog.Filter = "All files (*.*)|*.*|Executables|*.exe";
                openFileDialog.CheckFileExists = false;
            }

            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog().Value)
            {
                return openFileDialog.FileName;
            }

            return "";
        }
    }
}