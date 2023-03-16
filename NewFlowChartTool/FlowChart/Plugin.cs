using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Core;
using NLog.LayoutRenderers.Wrappers;

namespace FlowChart.Plugin
{
    public interface IPlugin
    {
        bool Register(IPluginManager manager);
    }

    public class IPluginManager
    {
        
    }

    public class PluginContainer<I>
    {
        public PluginContainer()
        {
            _plugins = new Dictionary<string, System.Type>();
        }
        public bool Add<T>(string name) where T : I
        {
            if (_plugins.ContainsKey(name))
                return false;
            _plugins.Add(name, typeof(T));
            return true;
        }

        public I? Create(string name)
        {
            if (!_plugins.ContainsKey(name))
                return default(I);
            var type = _plugins[name];
            return (I)Activator.CreateInstance(type);
        }

        private Dictionary<string, System.Type> _plugins;
    }

    public class PluginManager : IPluginManager
    {
        static PluginManager()
        {
            Inst = new PluginManager();
        }

        public static PluginManager Inst;

        public PluginManager()
        {
            _projectFactoryContainer = new PluginContainer<IProjectFactory>();
        }
        public bool RegisterProjectFactory<T>(string name) where T : IProjectFactory
        {
            return _projectFactoryContainer.Add<T>(name);
        }

        public IProjectFactory? GetProjectFactory(string name)
        {
            return _projectFactoryContainer.Create(name);
        }

        private PluginContainer<IProjectFactory> _projectFactoryContainer;
    }
}
