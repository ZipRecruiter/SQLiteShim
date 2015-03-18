using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ZipRecruiter
{
    /// <summary>
    /// A Shim that provides facilities to add/remove an AssemblyResolve delegate
    /// that loads an embedded version of SQLite3 in place of the specified DLL names.
    /// </summary>
    public class SQLiteShim
    {
        /// <summary>
        /// When true, attempts to store the DLL in a path near the SQLiteShim dll.
        /// Otherwise installs in a temp directory
        /// </summary>
        public static Boolean InstallLocalToDLL { get; set; }

        /// <summary>
        /// Simple accessor to retrieve this Library's and the embedded SQLite's version number.
        /// </summary>
        public static Version SQLiteVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        private readonly static String SQLiteResourceName = "sqlite3.dll";

        static SQLiteShim()
        {
            InstallLocalToDLL = false;
        }

        private SQLiteShim() { }

        /// <summary>
        /// Writes the correct embedded SQLite DLL to dllStoragePath or a temp folder.
        /// </summary>
        /// <param name="dllStoragePath">An optional specific path to write the SQLite library</param>
        /// <returns></returns>
        public static void InjectSQLite(String dllStoragePath = null)
        {
            var thisAssembly = Assembly.GetExecutingAssembly();
            var thisAssemblyFolder = new Uri(Path.GetDirectoryName(thisAssembly.CodeBase)).LocalPath;
            var desiredDll = Path.Combine(thisAssemblyFolder,
                                          "SQLiteShimResources",
                                          System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE").ToLower(),
                                          SQLiteShim.SQLiteResourceName);

            if (File.Exists(desiredDll))
            {
                IntPtr h = LoadLibrary(desiredDll);
            }
            else
            {
                throw new FileNotFoundException("Unable to locate SQLite DLL file. Please make sure your SQLiteShim distribution is complete.", desiredDll);
            }
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibraryW([MarshalAs(UnmanagedType.LPStr)] string lpLibFileName);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);
    }
}
