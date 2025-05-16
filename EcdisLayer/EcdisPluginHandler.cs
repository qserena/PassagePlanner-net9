using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Reflection;

namespace EcdisLayer
{
    public class EcdisPluginHandler
    {
        [ImportMany]
        IEnumerable<Lazy<IEcdisPlugin, IEcdisName>> ecdisPlugins;

        private CompositionContainer _container;
        private List<string> _ecdisPluginNames;
        private FileVersionInfo _fileVersionInfo;
        private AssemblyName _assemblyName;
        private string _pluginCatalog;
        private string _assemblyVersion;

        public EcdisPluginHandler()
        {
            // An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();

            string executablePath = Path.GetDirectoryName(Application.ExecutablePath);

            _pluginCatalog = Path.Combine(executablePath, "EcdisPlugins");

            string debugPluginCatalog = Path.Combine(executablePath, @"..\..\..\EcdisLayer\EcdisPlugins");

            if (Directory.Exists(_pluginCatalog))
            {
                catalog.Catalogs.Add(new DirectoryCatalog(_pluginCatalog));
            }
            else if (Directory.Exists(debugPluginCatalog))
            {
                catalog.Catalogs.Add(new DirectoryCatalog(debugPluginCatalog));
                _pluginCatalog = debugPluginCatalog;
            }


            // Create the CompositionContainer with the parts in the catalog
            _container = new CompositionContainer(catalog);

            // Fill the imports of this object
            try
            {
                this._container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                Console.WriteLine(compositionException.ToString());
            }

            _ecdisPluginNames = new List<string>();

            try
            {
                foreach (Lazy<IEcdisPlugin, IEcdisName> plugin in ecdisPlugins)
                {
                    _ecdisPluginNames.Add(plugin.Metadata.EcdisName);
                }
            }
            catch (Exception ex)
            {
                throw(ex);
            }

            _ecdisPluginNames.Sort();

            // Find out assembly version to be showed in GUI.
            try
            {
                foreach (string file in Directory.GetFiles(_pluginCatalog, "*.dll", SearchOption.AllDirectories))
                {
                    if (Path.GetFileName(file).Equals("EcdisPlugins.dll"))
                    {
                        Assembly assembly = Assembly.UnsafeLoadFrom(file);
                        _assemblyVersion = assembly.GetName().Version.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return _assemblyVersion;
            }
        }

        public List<string> GetAvailablePlugins()
        {
            return _ecdisPluginNames;
        }

        public IEcdisPlugin GetPlugin(string pluginName)
        {
            foreach (Lazy<IEcdisPlugin, IEcdisName> ecdisPlugin in ecdisPlugins)
            {
                if (ecdisPlugin.Metadata.EcdisName.Equals(pluginName))
                {
                    return ecdisPlugin.Value;
                } 
            }
            // If not found
            return null;
        }


        

    }
}
