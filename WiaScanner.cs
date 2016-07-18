using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using WIA;

// http://geekswithblogs.net/tonyt/archive/2006/07/29/86608.aspx 

namespace CryptoInator
{
    public sealed class WiaScannerAdapter : IDisposable
    {
        private bool _disposed; // indicates if Dispose has been called
        private CommonDialog _wiaManager;

        private CommonDialog WiaManager
        {
            get { return _wiaManager; }
            set { _wiaManager = value; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        ~WiaScannerAdapter()
        {
            Dispose(false);
        }

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
        public byte[] ScanImage(System.Drawing.Imaging.ImageFormat outputFormat)
        {
            ImageFile imageObject = null;

            try
            {
                if (WiaManager == null)
                    WiaManager = new CommonDialog();

                imageObject =
                    WiaManager.ShowAcquireImage(WiaDeviceType.ScannerDeviceType,
                                                WiaImageIntent.ColorIntent, WiaImageBias.MinimizeSize,
                                                outputFormat.Guid.ToString("B"), false, true, true);

                Vector vector = imageObject.FileData;

                var ms = new MemoryStream((byte[])vector.get_BinaryData());
                var bytes = new byte[ms.Length];
                ms.Read(bytes, 0, (int)ms.Length);
                ms.Close();

                return bytes;
            }
            catch (COMException ex)
            {
                const string message = "Error scanning image";
                throw new WiaOperationException(message, ex);
            }
            finally
            {
                if (imageObject != null)
                    Marshal.ReleaseComObject(imageObject);
            }
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // no managed resources to cleanup
                }

                // cleanup unmanaged resources
                if (_wiaManager != null)
                    Marshal.ReleaseComObject(_wiaManager);

                _disposed = true;
            }
        }
    }
}