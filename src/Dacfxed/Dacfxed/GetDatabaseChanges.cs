using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using DacFxLoadProxy;

namespace DacFxed
{
    /// <summary>
    /// <para type="synopsis">Get a list of differences between a dacpac and a database</para>
    /// <para type="description">Compares a dacpac to a database and returns a list of changes that need to be made to the database to get the database in sync with the dacpac. This is the XML deployment report returned by the DacFx.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DatabaseChanges")]
    public class GetDatabaseChanges : Cmdlet
    {
        /// <summary>
        /// <para type="description">The path to the dacpac to deploy</para>
        /// </summary>
        [Parameter(ParameterSetName = "GetDatabaseChanges", Mandatory = true, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The path of the dacpac to check against the database")]
        public string DacpacPath { get; set; }

        /// <summary>
        /// <para type="description">The path to the publish profile. A publish profile contains lots of useful info like database name and any one of the hundreds of config options for deploying. If you do not have a publish profile use New-PublishProfile to generate a template that you can modify</para>
        /// </summary>
        [Parameter(ParameterSetName = "GetDatabaseChanges", Mandatory = true, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The path of the publish profile. To generate a new publish profile use New-PublishProfile")]
        public string PublishProfilePath { get; set; }

        /// <summary>
        /// a semi-colon separated list of directories you want to load extensions from, if you have multiple you can put them under one "Extensions" folder and point to that - if you do have write access to program files then you can just write to the normal extensions paths.
        /// </summary>
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

                _dacFxed = new DacFxedModuleLoader(!string.IsNullOrEmpty(DacFxExtensionsPath) ?  DacFxExtensionsPath.Split(';').ToList() : new List<string>(), (sender, message) => _messages.AppendLine(message));
                
                _dacFxed.LoadDacpac(DacpacPath, PublishProfilePath);
            }
            catch (Exception x)
            {
                _messages.AppendLine($"Fatal Error loading the dacpac: {x.Message}");
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