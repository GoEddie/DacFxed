using System.IO;
using System.Management.Automation;

namespace DacFxed
{
    /// <summary>
    /// <para type="synopsis">The New-PublishProfile CmdLet generates a standard publish profile xml file that can be used by the DacFx (this and everything else) to control the deployment of your dacpac</para>
    /// </summary>

    [Cmdlet(VerbsCommon.New, "PublishProfile")]
    public class NewPublishProfile : Cmdlet
    {
        /// <summary>
        /// <para type="description">The path you would like to save the profile xml file</para>
        /// </summary>
        [Parameter(ParameterSetName = "PublishProfile", Mandatory = true, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The path to the new publish profile")]
        public string PublishProfilePath { get; set; }

        /// <summary>
        /// <para type="description">The database name you are targetting</para>
        /// </summary>
        [Parameter(ParameterSetName = "PublishProfile", Mandatory = true, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The database name to deploy to")]
        public string TargetDatabaseName { get; set; }

        /// <summary>
        /// <para type="description">The connection string to the database you are upgrading</para>
        /// </summary>
        [Parameter(ParameterSetName = "PublishProfile", Mandatory = true, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The connection string of the target you want to deploy to")]
        public string TargetConnectionString { get; set; }

        protected override void ProcessRecord()
        {
            File.WriteAllText(PublishProfilePath, string.Format(ProfileTemplate, TargetDatabaseName, TargetConnectionString));
        }

        private const string ProfileTemplate =
@"<?xml version=""1.0"" ?>
<Project ToolsVersion=""14.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup>
    <TargetDatabaseName>{0}</TargetDatabaseName>
    <TargetConnectionString>{1}</TargetConnectionString>
    <ProfileVersionNumber>1</ProfileVersionNumber>
  </PropertyGroup>
</Project>";

    }
}