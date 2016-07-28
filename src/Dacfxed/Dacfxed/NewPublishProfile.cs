using System.IO;
using System.Management.Automation;

namespace DacFxed
{
    /// <summary>
    /// <para type="synopsis">The New-PublishProfile CmdLet generates a standard publish profile xml file that can be used by the DacFx (this and everything else) to control the deployment of your dacpac</para>
    /// <para type="description">This generates a standard template XML which is enough to dpeloy a dacpac but it is highly recommended that you add additional options to the publish profile. If you use Visual Studio you can open a publish.xml file and use the ui to edit the file (right click on an SSDT project, choose "Publish" then "Load Profile" and load your profile or create a new one. Once you have loaded it in Visual Studio, clicking advanced shows you the list of options available to you.</para>
    /// <para type="description">For a full list of options that you can add to the profile, google "sqlcmd.exe command line switches" and for each option in the format /p:Option=Value create a element called &lt;Option&gt;Value&lt;/Option&gt;</para>
    /// 
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