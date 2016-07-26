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
        
        public void Init()
        {
            //Get a clean sqllocaldb instance...
            RunWithNotification("powershell", TestContext.CurrentContext.TestDirectory + "\\Deploy\\Setup.ps1 DacFxed");

            try
            {
                using (
                    var con =
                        new SqlConnection("server=(localdb)\\DacFxed;initial catalog=master;integrated security=sspi;"))
                {
                    con.Open();
                    var cmd = con.CreateCommand();
                    cmd.CommandText = "drop database Eddie";
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
            }
        }


        [Test]
        public void Using_PublishProfile()
        {

            Init();

            

            var profilePath =  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "WindowsPowerShell\\Modules\\DacFxed");

            var moduleArgs = GetFullPath("..\\..\\..\\DacFxed\\DacFxed\\UpdateModuleVersion.ps1") + " 99.9 " +
                             GetFullPath("..\\..\\..\\DacFxed\\DacFxed\\bin\\debug\\") + " " + profilePath;

            RunWithNotification("powershell", moduleArgs);

            

            var args = TestContext.CurrentContext.TestDirectory + "\\Deploy\\Deploy.ps1 " +
                       GetFullPath(TestContext.CurrentContext.TestDirectory +
                                   "..\\..\\..\\..\\TestDacPac\\bin\\Debug\\TestDacPac.dacpac") + " " +
                       GetFullPath(TestContext.CurrentContext.TestDirectory +
                                   "..\\..\\..\\..\\TestDacPac\\TestDacPac.publish.xml")
                       + " " +
                       Path.Combine(GetFullPath(TestContext.CurrentContext.TestDirectory + "..\\..\\..\\..\\DacFxed\\DacFxed\\bin\\debug\\"), "*.*");

            RunWithNotification("powershell",args);

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
            return new FileInfo(Path.Combine(TestContext.CurrentContext.TestDirectory , path)).FullName;
        }

        private void RunWithNotification(string process, string args)
        {
            
            var si = new ProcessStartInfo();
            si.CreateNoWindow = true;
            si.RedirectStandardError = true;
            si.RedirectStandardOutput = true;
            si.UseShellExecute = false;
            si.FileName = process;
            si.Arguments = args;
            var p = new Process();
            p.StartInfo = si;
            p.ErrorDataReceived += (sender, eventArgs) => Debug.WriteLine(eventArgs.Data);
            p.OutputDataReceived += (sender, eventArgs) => Debug.WriteLine(eventArgs.Data);
            p.EnableRaisingEvents = true;
            if (p.Start())
                p.WaitForExit();

            Console.WriteLine(p.StandardError.ReadToEnd());
            Console.WriteLine(p.StandardOutput.ReadToEnd());
        }
    }
}
