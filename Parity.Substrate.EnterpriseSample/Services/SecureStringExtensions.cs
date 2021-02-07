using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Parity.Substrate.EnterpriseSample.Services
{
    public static class SecureStringExtensions
    {
        /// <summary>
        /// Converts the string to an unsecure string.
        /// </summary>
        /// <remarks>
        /// How to properly convert SecureString to String:
        /// https://blogs.msdn.microsoft.com/fpintos/2009/06/12/how-to-properly-convert-securestring-to-string/
        /// </remarks>
        /// <param name="securePassword">The secure password.</param>
        /// <returns>The specified SecureString as a String.</returns>
        public static string ToUnsecureString(this SecureString securePassword)
        {
            if (securePassword == null)
            {
                throw new ArgumentNullException("securePassword");
            }

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}
