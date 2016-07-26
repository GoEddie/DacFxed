using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Management.Automation;
using DacFxLoadProxy;


namespace DacFxed
{
    /// <summary>
    /// <para type="synopsis">The Publish-Database CmdLet takes a dacpac which is the output from an SSDT project and publishes it to a database. Changing the schema to match the dacpac and also to run any scripts in the dacpac (pre/post deploy scripts)</para>
    /// <para type="description">Deploying a dacpac uses the DacFx which historically needed to be installed on a machine prior to use. In 2016 the DacFx was supplied by Microsoft as a nuget package and this uses that nuget package.</para>
    /// <para type="description">One of the main features of the DacFx is that when deploying your dacpac's you have the ability to use deployment contributors which is awesome, however these have to be installed into the visual studio or sql server directory under program files which people may not have access to when deploying dacpacs. This module works around that and lets you specify the folder from where you want to load extensions.</para>
    ///<para type="description">It will also hopefully stop people from having to write the same boilerpoint code to deploy a dacpac</para>
    /// </summary>
    [Cmdlet(VerbsData.Publish, "Database")]
    public class PublishDatabase : Cmdlet
    {
        /// <summary>
        /// <para type="description">The path to the dacpac to deploy</para>
        /// </summary>
        [Parameter(ParameterSetName = "PublishDatabase",Mandatory = true,ValueFromPipeline = false,ValueFromPipelineByPropertyName = true,HelpMessage = "The path of the dacpac to deploy")]
        public string DacpacPath { get; set; }

        /// <summary>
        /// <para type="description">The path to the publish profile. A publish profile contains lots of useful info like database name and any one of the hundreds of config options for deploying. If you do not have a publish profile use New-PublishProfile to generate a template that you can modify</para>
        /// </summary>
        [Parameter(ParameterSetName = "PublishDatabase", Mandatory = true, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The path of the publish profile. To generate a new publis profile use New-PublishProfile")]
        public string PublishProfilePath { get; set; }

        /// <summary>
        /// <para type="description">The path where you would like to save a copy of the deployment script to. The way this works is that we do a compare from the dacpac to the database, generate a script that will likely be used to upgrade the database but we do a separate compare/deploy for the actual deploy. The reason is that we want tp save the script but not run it manually. If someone edits the database between comparing and deploying then it could be different to the actual script run</para>
        /// </summary>
        [Parameter(ParameterSetName = "PublishDatabase", Mandatory = false, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true, HelpMessage = "If this is specified then a copy of the script used to deploy the changes is copied to this location")]
        public string ScriptSavePath { get; set; }

        /// <summary>
        /// a semi-colon separated list of directories you want to load extensions from, if you have multiple you can put them under one "Extensions" folder and point to that - if you do have write access to program files then you can just write to the normal extensions paths.
        /// </summary>
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
            try
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
            catch (Exception e)
            {
                var ex = e;
                while (ex.InnerException != null)
                {
                    WriteError(new ErrorRecord(ex.InnerException, "DacFx Error", ErrorCategory.NotSpecified, this));
                    ex = ex.InnerException;
                }

                WriteVerbose(_messages.ToString());
                _messages.Clear();
                throw;
            }
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
        