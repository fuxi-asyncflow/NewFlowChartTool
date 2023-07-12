using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlowChart.Common;
using FlowChart.Core;

namespace ProjectFactory.DefaultProjectFactory
{
    public interface ISaveForLang
    {
        public void SaveEventDefine(Project Project, List<string> lines);
        public void SaveNodeFunc(TextNode textNode, List<string> lines);
        public string SaveGenerateFile(Project project, string belongRoot, string outputFile, List<string> yamlLines,
            List<string> codeLines);

        public string SaveAllFlowChartsFile(List<string> lines);

        public string LineComment { get; }
    }

    public class SaveForDefault : ISaveForLang
    {
        public void SaveEventDefine(Project Project, List<string> lines)
        {
            
        }

        public void SaveNodeFunc(TextNode textNode, List<string> lines)
        {
            
        }

        public string SaveGenerateFile(Project project, string belongRoot, string outputFile, List<string> yamlLines,
            List<string> codeLines)
        {
            return string.Empty;
        }

        public string SaveAllFlowChartsFile(List<string> lines)
        {
            return string.Empty;
        }

        public string LineComment => "//";
    }

    public class SaveForLua : ISaveForLang
    {
        public string LineComment => "--";
        public void SaveEventDefine(Project Project, List<string> lines)
        {
            var luaLines = new List<string>();
            luaLines.Add("if asyncflow and asyncflow.EventId then");
            foreach (var ev in Project.EventDict.Values.ToList())
            {
                if (ev.EventId == 0)
                    continue;
                if (string.IsNullOrEmpty(ev.Description))
                    luaLines.Add($"  asyncflow.EventId.{ev.Name} = {ev.EventId}");
                else
                    luaLines.Add($"  asyncflow.EventId.{ev.Name} = {ev.EventId}    -- {ev.Description}");
            }
            luaLines.Add("end");
            luaLines.Add("");

            luaLines.Add("local str = [[");
            luaLines.AddRange(lines);
            luaLines.Add("]]");
            luaLines.Add("asyncflow.import_event(str)");

            FileHelper.Save(Path.Combine(Project.Path, Project.Config.Output, "asyncflow_events.lua"), luaLines);
        }

        public void SaveNodeFunc(TextNode textNode, List<string> genLines)
        {
            var normalUidStr = textNode.Uid.ToString("N");
            genLines.Add($"-- {textNode.Text}");
            genLines.Add($"local function f_{normalUidStr}(self)");
            foreach (var c in textNode.Content.Contents)
            {
                if (c is string s)
                    genLines.Add($"    {c.ToString()}");
            }
            genLines.Add("end");
            var funcName = $"{textNode.OwnerGraph.Path}.{normalUidStr}";
            genLines.Add($"asyncflow.set_node_func(\"{funcName}\", f_{normalUidStr})");
            genLines.Add("");
        }

        public string SaveGenerateFile(Project project, string belongRoot, string outputFile, List<string> yamlLines,
            List<string> codeLines)
        {
            codeLines.Add("");
            codeLines.Add("local str = [[");
            codeLines.AddRange(yamlLines);
            codeLines.Add("]]");
            codeLines.Add("");
            codeLines.Add("asyncflow.import_charts(str)");

            return $"  {{ \"{belongRoot}\", \"{outputFile.Replace('\\', '/')}\"}},";
        }

        public string SaveAllFlowChartsFile(List<string> lines)
        {
            lines.Insert(0, "return {");
            lines.Add("}");
            return "all_flowcharts.lua";
        }
    }

    public class SaveForPython : ISaveForLang
    {
        public string LineComment => "#";
        public void SaveEventDefine(Project project, List<string> lines)
        {
            var pyLines = new List<string>();
            //pyLines.Add("import asyncflow");
            WriteHeader(project, pyLines);
            pyLines.Add("");

            pyLines.Add("class EventId:");
            foreach (var ev in project.EventDict.Values.ToList())
            {
                if (ev.EventId == 0)
                    continue;
                if (string.IsNullOrEmpty(ev.Description))
                    pyLines.Add($"    {ev.Name} = {ev.EventId}");
                else
                    pyLines.Add($"    {ev.Name} = {ev.EventId}    # {ev.Description}");
            }

            pyLines.Add("");
            pyLines.Add("asyncflow.EventId = EventId");

            pyLines.Add("str = r'''");
            pyLines.AddRange(lines);
            pyLines.Add("'''");
            pyLines.Add("asyncflow.import_event(str)");

            FileHelper.Save(Path.Combine(project.Path, project.Config.Output, "asyncflow_events.py"), pyLines);
        }

        void WriteHeader(Project project, List<string> lines)
        {
            if (lines.Count > 0)
                return;
            var header = project.Config?.CustomHeader;
            if (header != null)
            {
                lines.AddRange(header);
                lines.Add("");
            }

            // lines.Add("import asyncflow");
        }

        public void SaveNodeFunc(TextNode textNode, List<string> genLines)
        {
            WriteHeader(textNode.OwnerGraph.Project, genLines);

            var normalUidStr = textNode.Uid.ToString("N");
            genLines.Add($"# {textNode.Text}");
            genLines.Add($"def f_{normalUidStr}(self):");
            foreach (var c in textNode.Content.Contents)
            {
                if (c is string s)
                    genLines.Add($"    {c.ToString()}");
            }
            genLines.Add("");
            var funcName = $"{textNode.OwnerGraph.Path}.{normalUidStr}";
            genLines.Add($"asyncflow.set_node_func(\"{funcName}\", f_{normalUidStr})");
            genLines.Add("");
        }

        public string SaveGenerateFile(Project project, string belongRoot, string outputFile, List<string> yamlLines,
            List<string> codeLines)
        {
            WriteHeader(project, codeLines);

            codeLines.Add("");
            codeLines.Add("str = r'''");
            codeLines.AddRange(yamlLines);
            codeLines.Add("'''");
            codeLines.Add("");
            codeLines.Add("asyncflow.import_charts(str)");

            return $"  [ \"{belongRoot}\", \"{outputFile.Replace('\\', '/')}\"],";
        }

        public string SaveAllFlowChartsFile(List<string> lines)
        {
            lines.Insert(0, "file_list = [");
            lines.Add("]");
            return "all_flowcharts.py";
        }
    }
}
