using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DacFxedProxy;

namespace DacFxLoadProxy
{
    public class DacFxedModuleLoader
    {
        private readonly Dictionary<string, Assembly> _dacAssemblies = new Dictionary<string, Assembly>();
        private readonly string[] _dlls = new[] { "Microsoft.SqlServer.Dac.dll", "Microsoft.Data.Tools.Schema.Sql.dll", "Microsoft.Data.Tools.Utilities.dll", "Microsoft.SqlServer.Dac.Extensions.dll", "Microsoft.SqlServer.TransactSql.ScriptDom.dll", "Microsoft.SqlServer.Types.dll", "DacFxedProxy.dll" };
        
        private string _localPath;

        public DacFxedModuleLoader(IList<string> extensionSources, MessageArgs messages, MessageArgs debugs, string privateLocalPath = null)
        {
            if (string.IsNullOrEmpty(privateLocalPath))
            {
                privateLocalPath = Path.Combine(new FileInfo(Assembly.GetAssembly(typeof(DacFxedModuleLoader)).Location).DirectoryName, @"bin\dll");
            }
            
            if (!Directory.Exists(privateLocalPath))
            {
                Directory.CreateDirectory(privateLocalPath);
            }

            var extensionsDir = privateLocalPath;

            var dir = new DirectoryInfo(extensionsDir);
            if (dir.Name.Equals("extensions", StringComparison.OrdinalIgnoreCase))
            {
                extensionsDir = Path.Combine(privateLocalPath, "Extensions");
            }
            
            if (!Directory.Exists(extensionsDir))
            {
                Directory.CreateDirectory(extensionsDir);

            }

            Message += messages;

            if(extensionsDir.Length == 0)
                Message.Invoke(this, "There are no extensions to use");

            foreach (var source in extensionSources)
            {
                Message.Invoke(this, $"Getting extensions from directory: {source}");
                CopyDir(source, extensionsDir);
            }

            LoadAssemblies(privateLocalPath);
            _localPath = privateLocalPath;

            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ResolveAssemblies;
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssemblies;
            
        }

        public delegate void MessageArgs(object sender, string message);

        public event MessageArgs Message = delegate { };
        public event MessageArgs Debug = delegate { };

        private Proxy _dacFxedProxy;

        public void LoadDacpac(string dacPacPath, string profilePath)
        {
            _dacFxedProxy = new Proxy(dacPacPath, profilePath);
            _dacFxedProxy.Message += (sender, message) => { Message.Invoke(sender, message); };
            _dacFxedProxy.LoadDacpac();
        }

        public void Deploy()
        {
            _dacFxedProxy.Deploy();

            LoadAssemblies(_localPath);
        }

        public string GenerateDeployScript()
        {
            return _dacFxedProxy.GenerateDeploymentScript();
        }

        public void RegisterDacpac()
        {
            _dacFxedProxy.RegisterDacpac();
        }

        public string GenerateDeployReport()
        {
            return _dacFxedProxy.GenerateDeployReport();
        }

        public string GenerateDriftReport()
        {
            return _dacFxedProxy.GenerateDriftReport();
        }

        public void Unregister()
        {
            _dacFxedProxy.Unregister();
        }
        
        private Assembly ResolveAssemblies(object sender, ResolveEventArgs args)
        {
            Debug.Invoke(this, $"Attempting to Resolve Assembly {args.Name}");

            if (_dacAssemblies.ContainsKey(args.Name))
            {
                Debug.Invoke(this, $"Resolving Assembly {args.Name}");
                return _dacAssemblies[args.Name];
            }

            return null;
        }

        private void LoadAssemblies(string privateLocalPath)
        {
            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                Debug.Invoke(this, $"Already have loaded: {ass.FullName}");
            }

            foreach (var dll in _dlls)
            {
                Debug.Invoke(this, $"Loading Assembly: {Path.Combine(privateLocalPath, dll)}");
                var assembly = System.Reflection.Assembly.LoadFile( Path.Combine(privateLocalPath, dll));
                _dacAssemblies[assembly.FullName] = assembly;
            }
        }

        private void CopyDir(string source, string target)
        {
            if (source.EndsWith("\\") && !target.EndsWith("\\"))
                target += "\\";

            if (!source.EndsWith("\\") && target.EndsWith("\\"))
                source += "\\";


            foreach (var path in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
            {
                if(!Directory.Exists(path.Replace(source, target)))
                    Directory.CreateDirectory(path.Replace(source, target));
            }

            foreach (var path in Directory.GetFiles(source, "*.*", SearchOption.AllDirectories))
            {
                if (!File.Exists(path.Replace(source, target)))
                {
                    File.Copy(path, path.Replace(source, target), true);
                    Message.Invoke(this, $"Extension: Copying {path} to {path.Replace(source, target)}");
                }
            }
        }
    }
}
