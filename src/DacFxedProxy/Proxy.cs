using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Extensions;

namespace DacFxedProxy
{
    public class Proxy
    {
        public delegate void ProxyMessageArgs(object sender, string message);

        private PackageOptions _unusedOptions = new PackageOptions();   //without this the Microsoft.SqlServer.Dac.Extensions.dll is not included in the build

        public Proxy(string dacPacPath, string profilePath)
        {
            Profile = DacProfile.Load(profilePath);
            Services = new DacServices(Profile.TargetConnectionString);

            Services.Message += (sender, args) => Message.Invoke(sender, $"{args.Message.Prefix}, {args.Message.MessageType}, {args.Message.Number}, {args.Message.Message}");
            Services.ProgressChanged += (sender, args) => Message.Invoke(sender, $"{args.Message}, {args.Status.ToString().Replace("Completed", "Complete")}");

            DacpacPath = dacPacPath;
        }

        public DacProfile Profile { get; set; }
        public DacServices Services { get; set; }
        public DacPackage Dacpac { get; set; }
        public string DacpacPath { get; set; }

        public event ProxyMessageArgs Message = delegate { };

        public void LoadDacpac()
        {
            Dacpac = DacPackage.Load(DacpacPath);
        }

        public string GenerateDeploymentScript()
        {
            return Services.GenerateDeployScript(Dacpac, Profile.TargetDatabaseName, Profile.DeployOptions);
        }

        public void RegisterDacpac()
        {
            Services.Register(Profile.TargetDatabaseName, Dacpac.Name, Dacpac.Version, Dacpac.Description);
        }

        public string GenerateDeployReport()
        {
            return Services.GenerateDeployReport(Dacpac, Profile.TargetDatabaseName, Profile.DeployOptions);
        }
        
        public void Deploy()
        {
            Services.Deploy(Dacpac, Profile.TargetDatabaseName, true, Profile.DeployOptions);
        }

        public string GenerateDriftReport()
        {
            return Services.GenerateDriftReport(Profile.TargetDatabaseName);
        }

        public void Unregister()
        {
            Services.Unregister(Profile.TargetDatabaseName);
        }
    }
}