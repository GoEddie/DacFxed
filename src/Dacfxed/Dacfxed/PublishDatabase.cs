using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Management.Automation;
using DacFxLoadProxy;


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
        public string ScriptSavePath { get; set; }

        [Parameter(ParameterSetName = "PublishDatabase", Mandatory = false, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true, HelpMessage = "If you reference a deployment contributor from your publish profile then use this to tell the DacFx where to load them from. To pass multiple paths use a semi-colon seperated list")]
        public string DacFxExtensionsPath { get; set; }


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

            try
            {
                if (!string.IsNullOrEmpty(DacFxExtensionsPath) && DacFxExtensionsPath.EndsWith(";"))
                {
                    DacFxExtensionsPath = DacFxExtensionsPath.Trim(new[] { ' ', ';' });
                }

                _dacFxed = new DacFxedModuleLoader(!string.IsNullOrEmpty(DacFxExtensionsPath) ? DacFxExtensionsPath.Split(';').ToList() : new List<string>());

                _dacFxed.Message += (sender, message) => _messages.AppendLine(message);

                _dacFxed.LoadDacpac(DacpacPath, PublishProfilePath);
            }
            catch (Exception x)
            {
                _messages.AppendLine($"Fatal Error lodaing the dacpac: {x.Message}");
                _error = true;
            }
        }

        private DacFxedModuleLoader _dacFxed;

        protected override void ProcessRecord()
        {
            WriteVerbose("About to publish dacpac...");

            if (_error)
            {
                WriteVerbose("About to publish dacpac...failed see previous error(s)");
                return;
            }

            if (!string.IsNullOrEmpty(ScriptSavePath))
            {
                SaveDeployScript();
            }

            Deploy();

            WriteVerbose("About to publish dacpac...Complete");

        }

        private void SaveDeployScript()
        {
            var deployScript = _dacFxed.GenerateDeployScript();
            var scriptPath = Path.Combine(ScriptSavePath, $"DeployScript-{DateTime.UtcNow.Date.ToString("yyyy-MM-dd_hh-mm-ss")}.sql");
            WriteVerbose($"Copying deploy script to {scriptPath}");
            File.WriteAllText(scriptPath, deployScript);
        }

        void Deploy()
        {
            _dacFxed.Deploy();
        }
        
        protected override void EndProcessing()
        {
            WriteVerbose(_messages.ToString());
        }
     
    }

    [Cmdlet(VerbsCommon.Get, "DatabaseChanges")]
    public class GetDatabaseChanges : Cmdlet
    {
        [Parameter(ParameterSetName = "GetDatabaseChanges", Mandatory = true, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The path of the dacpac to check against the database")]
        public string DacpacPath { get; set; }

        [Parameter(ParameterSetName = "GetDatabaseChanges", Mandatory = true, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The path of the publish profile. To generate a new publish profile use New-PublishProfile")]
        public string PublishProfilePath { get; set; }

        [Parameter(ParameterSetName = "GetDatabaseChanges", Mandatory = false, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true, HelpMessage = "If you reference a deployment contributor from your publish profile then use this to tell the DacFx where to load them from. To pass multiple paths use a semi-colon seperated list")]
        public string DacFxExtensionsPath { get; set; }

        
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

            try
            {
                if (!string.IsNullOrEmpty(DacFxExtensionsPath) && DacFxExtensionsPath.EndsWith(";"))
                {
                    DacFxExtensionsPath = DacFxExtensionsPath.Trim(new[] {' ', ';'});
                }

                _dacFxed = new DacFxedModuleLoader(!string.IsNullOrEmpty(DacFxExtensionsPath) ?  DacFxExtensionsPath.Split(';').ToList() : new List<string>());
                _dacFxed.Message += (sender, message) => _messages.AppendLine(message);

                _dacFxed.LoadDacpac(DacpacPath, PublishProfilePath);
            }
            catch (Exception x)
            {
                _messages.AppendLine($"Fatal Error lodaing the dacpac: {x.Message}");
                _error = true;
            }
        }

        private DacFxedModuleLoader _dacFxed;

        protected override void ProcessRecord()
        {
            WriteVerbose("About to compare dacpac to database...");

            if (_error)
            {
                WriteVerbose("About to compare dacpac to database...failed see previous error(s)");
                return;
            }

            base.WriteObject(_dacFxed.GenerateDeployReport());

            WriteVerbose("About to compare dacpac to database...Complete");

        }
        
        protected override void EndProcessing()
        {
            WriteVerbose(_messages.ToString());
        }

    }
}
        