using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

// http://geekswithblogs.net/tonyt/archive/2006/07/29/86608.aspx

namespace CryptoInator
{
    public enum WiaScannerError : uint
    {
        LibraryNotInstalled = 0x80040154,
        OutputFileExists = 0x80070050,
        ScannerNotAvailable = 0x80210015,
        OperationCancelled = 0x80210064
    }

    [Serializable]
    public class WiaOperationException : Exception
    {
        private WiaScannerError _errorCode;

        public WiaOperationException(WiaScannerError errorCode)
        {
            ErrorCode = errorCode;
        }

        public WiaOperationException(string message, WiaScannerError errorCode)
            : base(message)
        {
            ErrorCode = errorCode;
        }

        public WiaOperationException(string message, Exception innerException)
            : base(message, innerException)
        {
            var comException = innerException as COMException;

            if (comException != null)
                ErrorCode = (WiaScannerError) comException.ErrorCode;
        }

        public WiaOperationException(string message, Exception innerException, WiaScannerError errorCode)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
        }

        protected WiaOperationException(SerializationInfo info,
                                        StreamingContext context)
            : base(info, context)
        {
            info.AddValue("ErrorCode", (uint) _errorCode);
        }

        public WiaScannerError ErrorCode
        {
            get { return _errorCode; }
            private set { _errorCode = value; }
        }

        public override void GetObjectData(SerializationInfo info,
                                           StreamingContext context)
        {
            base.GetObjectData(info, context);
            ErrorCode = (WiaScannerError) info.GetUInt32("ErrorCode");
        }
    }
}