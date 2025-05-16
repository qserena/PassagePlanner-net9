using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MultiEcdisPlugin
{

    /// <summary>
    /// Allows the manipulation of bytes into structures.
    /// </summary>
    public static class ByteCaster
    {
        /// <summary>
        /// Attempts to get a byte array as a structure.
        /// </summary>
        /// <typeparam name="T">The type of structure we wish to represent the bytes as.</typeparam>
        /// <param name="bytes">The bytes to convert to a structure.</param>
        /// <param name="structure">The structure we wish to populate.</param>
        /// <returns>True if the operation succeeded, false if there was an error.</returns>
        public static bool TryGetStructure<T>(this byte[] bytes, out T structure) where T : struct
        {
            try
            {
                structure = GetStructure<T>(bytes);
                return true;
            }
            catch
            {
                structure = default(T);
                return false;
            }
        }

        /// <summary>
        /// Gets a strucure representation of a byte array.
        /// </summary>
        /// <typeparam name="T">The type of structure we wish to represent the bytes as.</typeparam>
        /// <param name="bytes">The bytes to convert to a structure.</param>
        /// <exception cref="ArgumentException">The structureType parameter layout is not sequential or explicit.
        /// -or-
        /// The structureType parameter is a generic type.</exception>
        /// <returns>The structure populated with the data from the byte array.</returns>
        public static T GetStructure<T>(this byte[] bytes) where T : struct
        {
            GCHandle gch = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            IntPtr ptr = gch.AddrOfPinnedObject();
            T result;
            try
            {
                result = (T)Marshal.PtrToStructure(ptr, typeof(T));
            }
            finally
            {
                gch.Free();
            }
            return result;
        }

        public static T ConvertByteArrayToObject<T>(this byte[] bytes) where T : class
        {
            GCHandle gch = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            IntPtr ptr = gch.AddrOfPinnedObject();
            T result;
            try
            {
                result = (T)Marshal.PtrToStructure(ptr, typeof(T));
            }
            finally
            {
                gch.Free();
            }
            return result;
        }
    }

}
