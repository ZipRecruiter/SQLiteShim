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
        public static String InjectSQLite(String dllStoragePath = null)
        {
            string resName = String.Join(".", new[] {
                "ZipRecruiter",
                "platform",
                System.Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE").ToLower(),
                SQLiteShim.SQLiteResourceName});

            var thisAssembly = Assembly.GetExecutingAssembly();
            var res = thisAssembly.GetManifestResourceNames();
            using (var input = thisAssembly.GetManifestResourceStream(resName))
            {
                if (String.IsNullOrWhiteSpace(dllStoragePath)){
                    string tempDirectory = Path.Combine(Path.GetTempPath(),
                                                        "SQLiteShim",
                                                        thisAssembly.GetName().Version.ToString());
                    Directory.CreateDirectory(tempDirectory);
                    dllStoragePath = tempDirectory + "\\" + SQLiteShim.SQLiteResourceName;
                }
                   
                using (Stream outFile = File.Create(dllStoragePath))
                {
                    const int sz = 4096;
                    byte[] buf = new byte[sz];
                    while (true)
                    {
                        int nRead = input.Read(buf, 0, sz);
                        if (nRead < 1)
                            break;
                        outFile.Write(buf, 0, nRead);
                    }
                }

                if (File.Exists(dllStoragePath))
                {
                    try {
                        IntPtr h = LoadLibrary(dllStoragePath);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            return dllStoragePath;
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibraryW([MarshalAs(UnmanagedType.LPStr)] string lpLibFileName);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

        /// <summary>
        /// Copies a stream to a byte array and returns it.
        /// Taken from http://stackoverflow.com/questions/96732/embedding-one-dll-inside-another-as-an-embedded-resource-and-then-calling-it-fro
        /// </summary>
        private static byte[] StreamToBytes(Stream input) 
        {
            var capacity = input.CanSeek ? (int) input.Length : 0;
            using (var output = new MemoryStream(capacity))
            {
                int readLength;
                var buffer = new byte[4096];

                do
                {
                    readLength = input.Read(buffer, 0, buffer.Length);
                    output.Write(buffer, 0, readLength);
                }
                while (readLength != 0);

                return output.ToArray();
            }
        }
    }
}
