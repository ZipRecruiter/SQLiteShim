# SQLiteShim
A mechanism to load plain 32/64bit SQLite DLLs in .Net without a lot of fuss

The end goal is to make it as painless as possible to use projects that utilize plain SQLite, like the excelent sqlite-net project. Some environments, like ASP.net, make it hard to juggle sqlite DLLs due to the fact IISExpress runs projects in 32bit mode, but you typically will release in 64bit mode.

Example:
    
    using ZipRecruiter;
    
    public class MyCode
    {
        static void Main(string[] args)
        {
            //Stores embedded SQLite in a temp folder and loads it using the Win32 API
            SQLiteShim.InjectSQLite();
            //Works even though we didn't explicitly include sqlite3.dll in our App's folder
            var libversion = SQLite3Consumer.LibVersionNumber();
            Console.WriteLine(String.Format("Your SQLite Version is {0}",libversion));
        }
    }
    
    public class SQLite3Consumer
    {
        [DllImport(@"sqlite3", EntryPoint = "sqlite3_libversion_number", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LibVersionNumber();

        [DllImport(@"sqlite3", EntryPoint = "sqlite3_libversion", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr LibVersion();
    }
