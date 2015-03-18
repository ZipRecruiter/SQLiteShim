using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NUnit;
using NUnit.Framework;
using FluentAssertions;
using System.Reflection;

namespace ZipRecruiter
{
    [TestFixture]
    public class Tests
    {
        [TestFixtureSetUp]
        public void Init()
        {
            SQLiteShim.InjectSQLite();
        }

        [Test]
        public void CallImportedSQLiteMethod()
        {
            var libversion = SQLite3Consumer.LibVersionNumber();
            libversion.Should().BeGreaterThan(0);
        }

        [Test]
        public void CheckSQLiteVersionAgainstShimVersion()
        {
            var libVersionStrPtr = SQLite3Consumer.LibVersion();
            var libVersionStr = Marshal.PtrToStringAnsi(libVersionStrPtr);
            libVersionStr.Should().BeSameAs(SQLiteShim.SQLiteVersion.ToString());
        }
    }

    public class SQLite3Consumer
    {
        [DllImport(@"sqlite3", EntryPoint = "sqlite3_libversion_number", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LibVersionNumber();

        [DllImport(@"sqlite3", EntryPoint = "sqlite3_libversion", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr LibVersion();
    }
}
