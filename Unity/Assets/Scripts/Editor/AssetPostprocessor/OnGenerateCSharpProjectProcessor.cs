using System.IO;
using System.Xml;
using UnityEditor;

namespace Chaos
{
    public sealed class OnGenerateCSharpProjectProcessor : AssetPostprocessor
    {
        /// <summary>
        /// https://learn.microsoft.com/zh-cn/visualstudio/gamedev/unity/extensibility/customize-project-files-created-by-vstu#%E6%A6%82%E8%A7%88
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string OnGeneratedCSProject(string path, string content)
        {
            var buildType = EditorUserBuildSettings.development ? BuildType.Debug : BuildType.Release;

            if (buildType == BuildType.Release)
            {
                content = content.Replace("<Optimize>false</Optimize>", "<Optimize>true</Optimize>");
                // content = content.Replace(";DEBUG;", ";");
            }

            if (path.EndsWith("Unity.Foundation.csproj"))
            {
                return GenerateProject(content);
            }

            if (path.EndsWith("Unity.Model.csproj") || path.EndsWith("Unity.Hotfix.csproj") || path.EndsWith("Unity.ModelView.csproj") || path.EndsWith("Unity.HotfixView.csproj"))
            {
                return GenerateProject(content);
            }

            return content;
        }

        private static string GenerateProject(string content)
        {
            XmlDocument srcDocument = new();
            srcDocument.LoadXml(content);
            var dstDocument = (XmlDocument)srcDocument.Clone();
            var rootNode = dstDocument.GetElementsByTagName("Project")[0];

            // 添加分析器
            {
                var itemGroup = dstDocument.CreateElement("ItemGroup", dstDocument.DocumentElement!.NamespaceURI);
                var projectReference = dstDocument.CreateElement("ProjectReference", dstDocument.DocumentElement.NamespaceURI);
                projectReference.SetAttribute("Include", @"..\Server\Analyzer.Unity\Analyzer.Unity.csproj");
                projectReference.SetAttribute("OutputItemType", "Analyzer");
                projectReference.SetAttribute("ReferenceOutputAssembly", "false");

                var project = dstDocument.CreateElement("Project", dstDocument.DocumentElement.NamespaceURI);
                projectReference.AppendChild(project);

                var name = dstDocument.CreateElement("Name", dstDocument.DocumentElement.NamespaceURI);
                name.InnerText = "Analyzer";
                projectReference.AppendChild(name);

                itemGroup.AppendChild(projectReference);
                rootNode.AppendChild(itemGroup);
            }

            using StringWriter stringWriter = new();
            using XmlTextWriter xmlWriter = new(stringWriter);
            xmlWriter.Formatting = Formatting.Indented;
            dstDocument.WriteTo(xmlWriter);
            xmlWriter.Flush();
            return stringWriter.GetStringBuilder().ToString();
        }
    }
}