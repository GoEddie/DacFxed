using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using DacFxLoadProxy;

namespace DacFxed
{
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