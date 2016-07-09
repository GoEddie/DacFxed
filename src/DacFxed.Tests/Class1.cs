using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DacFxed.Tests
{
    [TestFixture]
    public class Can_Deploy_Dacpac
    {
        [OneTimeSetUp]
        public void Init()
        {
            //Get a clean sqllocaldb instance...
            Process.Start("powershell", TestContext.CurrentContext.TestDirectory + "\\Deploy\\Setup.ps1 DacFxed");
        }


        [Test]
        public void Using_PublishProfile()
        {
            var args = TestContext.CurrentContext.TestDirectory + "\\Deploy\\Deploy.ps1 " +
                       GetFullPath(TestContext.CurrentContext.TestDirectory +
                                   "..\\..\\..\\..\\TestDacPac\\bin\\Debug\\TestDacPac.dacpac") + " " +
                       GetFullPath(TestContext.CurrentContext.TestDirectory +
                                   "..\\..\\..\\..\\TestDacPac\\TestDacPac.publish.xml")
                       + " " +
                       Path.Combine(
                           GetFullPath(TestContext.CurrentContext.TestDirectory + "..\\..\\..\\..\\DacFxed\\DacFxed\\bin\\debug\\"),
                           "*.*");

            Process.Start("powershell",args);

            using (var con = new SqlConnection("server=(localdb)\\DacFxed;initial catalog=Eddie;integrated security=sspi;"))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = "select count(*) from this_is_a_table";
                cmd.ExecuteNonQuery();
            }
        }

        private string GetFullPath(string path)
        {
            return new FileInfo(path).FullName;
        }
    }
}
