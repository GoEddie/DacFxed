using System.IO;
using System.Management.Automation;

namespace DacFxed
{
    [Cmdlet(VerbsCommon.New, "PublishProfile")]
    public class NewPublishProfile : Cmdlet
    {

        [Parameter(ParameterSetName = "PublishProfile", Mandatory = true, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The path to the new publish profile")]
        public string PublishProfilePath { get; set; }


        [Parameter(ParameterSetName = "PublishProfile", Mandatory = true, ValueFromPipeline = false, ValueFromPipelineByPropertyName = true, HelpMessage = "The database name to deploy to")]
        public string TargetDatabaseName { get; set; }


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