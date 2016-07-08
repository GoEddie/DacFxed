using System;
using System.IO;
using System.Text;
using System.Management.Automation;
using Microsoft.SqlServer.Dac;

namespace DacFxed
{
    [Cmdlet(VerbsData.Publish, "Database")]
    public class PublishDatabase : Cmdlet
    {
        [Parameter(ParameterSetName = "PublishDatabase",Mandatory = true,ValueFromPipeline = false,ValueFromPipelineByPropertyName = true,HelpMessage = "The path of the dacpac to deploy")]
        public string DacpacPath { get; set; }

        [Parameter(ParameterSetName = "PublishDatabase", Mandatory = true, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The path of the publish profile. To generate a new publis profile use New-PublishProfile")]
        public string PublishProfilePath { get; set; }

        [Parameter(ParameterSetName = "PublishDatabase", Mandatory = false, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true, HelpMessage = "If this is specified then a copy of the script used to deploy the changes is copied to this location")]
        public string AuditScriptPath { get; set; }

        bool _error = false;
        private readonly StringBuilder _messages = new StringBuilder();

        protected override void BeginProcessing()
        {
            WriteVerbose("Validating dacpac path");

            if (!File.Exists(DacpacPath))
            {
                WriteError(new ErrorRecord(new FileNotFoundException(), "The Dacpac referenced by DacpacPath was not found", ErrorCategory.InvalidArgument, this));
                _error = true;
                WriteVerbose("Validating dacpac path - file missing");
            }
            else
            { 
                WriteVerbose("Validating dacpac path - file exists");
            }

            WriteVerbose("Validating pubish profile path");

            if (!File.Exists(PublishProfilePath))
            {
                WriteError(new ErrorRecord(new FileNotFoundException(), "The Publish profile referenced by PublishProfilePath was not found", ErrorCategory.InvalidArgument, this));
                _error = true;
                WriteVerbose("Validating pubish profile path - file missing");
            }
            else
            {
                WriteVerbose("Validating pubish profile path - file exists");
            }
        }
        
        protected override void ProcessRecord()
        {
            WriteVerbose("About to publish dacpac...");
            if (_error)
            {
                WriteVerbose("About to publish dacpac...failed see previous error(s)");
                return;
            }
            
            var profile = GetProfile();

            var services = GetDacServices(profile);
            
            var package = LoadDacpac();

            Deploy(services, package, profile);
        }

        private void GenerateAuditScript(DacServices services, DacPackage package, DacProfile profile)
        {
            WriteVerbose("Generating deploy script...");
            var script = services.GenerateDeployScript(package, profile.TargetDatabaseName, profile.DeployOptions);
            File.WriteAllText(AuditScriptPath, script);
            WriteVerbose("Generating deploy script...done");
        }

        private void Deploy(DacServices services, DacPackage package, DacProfile profile)
        {
            WriteVerbose("About to deploy...");
            services.Deploy(package, profile.TargetDatabaseName, true, profile.DeployOptions);
            WriteVerbose("About to deploy...done");
        }

        private DacPackage LoadDacpac()
        {
            WriteDebug("Getting Dac Services...");
            var package = DacPackage.Load(DacpacPath);
            WriteDebug("Getting Dac Services...Done");
            return package;
        }

        private DacServices GetDacServices(DacProfile profile)
        {
            WriteDebug("Getting Dac Services...");
            var services = new DacServices(profile.TargetConnectionString);
            services.Message += (sender, e) => _messages.Append($"{e.Message}\r\nVERBOSE: ");
            services.ProgressChanged += (sender, e) => _messages.Append($"{e.Message} is {e.Status.ToString().Replace("Completed", "Complete")}\r\nVERBOSE: ");
            WriteDebug("Getting Dac Services...Done");
            return services;
        }
        
        protected override void EndProcessing()
        {
            WriteVerbose(_messages.ToString());
        }
        
        private DacProfile GetProfile()
        {
            try
            {
                WriteDebug("Getting Profile...");
                var profile = DacProfile.Load(PublishProfilePath);
                WriteDebug("Getting Profile...Done");
                return profile;
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "Error reading the publish profile xml", ErrorCategory.InvalidData, this));
                throw;
            }
        }
    }
}
        