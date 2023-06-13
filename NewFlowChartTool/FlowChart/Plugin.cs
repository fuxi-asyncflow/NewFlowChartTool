using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.AST;
using FlowChart.Common;
using FlowChart.Core;
using FlowChart.Parser;

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

        public List<string> GetKeys()
        {
            return _plugins.Keys.ToList();
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
            _codeGeneratorContainer = new PluginContainer<ICodeGenerator>();
            _parserContainer = new PluginContainer<IParser>();
        }

        #region Project Factory
        public bool RegisterProjectFactory<T>(string name) where T : IProjectFactory
        {
            return _projectFactoryContainer.Add<T>(name);
        }

        public IProjectFactory? GetProjectFactory(string name)
        {
            return _projectFactoryContainer.Create(name);
        }

        private PluginContainer<IProjectFactory> _projectFactoryContainer;

        #endregion

        #region Code Generator

        public bool RegisterCodeGenerator<T>(string name) where T : ICodeGenerator
        {
            return _codeGeneratorContainer.Add<T>(name);
        }

        public ICodeGenerator? GetCodeGenerator(string name)
        {
            return _codeGeneratorContainer.Create(name);
        }

        public List<string> GetAllCodeGenerators()
        {
            return _codeGeneratorContainer.GetKeys();
        }

        private PluginContainer<ICodeGenerator> _codeGeneratorContainer;

        #endregion

        #region Parser

        public bool RegisterParser<T>(string name) where T : IParser
        {
            Logger.LOG($"register parser `{name}`");
            return _parserContainer.Add<T>(name);
        }

        public IParser? GetParser(string name)
        {
            return _parserContainer.Create(name);
        }

        private PluginContainer<IParser> _parserContainer;

        #endregion



    }
}
