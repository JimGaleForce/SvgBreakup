using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Svg;

namespace SvgBreakup
{
    class Program
    {
        static void Main(string[] args)
        {
            args = new string[] { "svg.svg" };

            Console.WriteLine("SvgBreakup v1.00");
            Console.WriteLine("");
            Console.WriteLine(" Usage:");
            Console.WriteLine("  SvgBreakup <filename.svg>");
            Console.WriteLine("  (will create filename1.svg, filename2.svg...)");
            Console.WriteLine("");

            if (args.Length == 0)
            {
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine(args[0] + " doesn't exist");
                return;
            }

            var svgmgr = new SvgManager();
            svgmgr.Split(args[0]);
        }
    }

    public class SvgManager
    {
        public void Split(string filename)
        {
            var file = File.ReadAllText(filename);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(file);

            var topG = doc.DocumentElement.SelectSingleNode("//*[local-name()='g']");
            var children = topG.SelectNodes("*[local-name()='g']");
            while (children.Count == 1)
            {
                children = children[0].SelectNodes("*[local-name()='g']");
            }

            if (children.Count > 1)
            {
                var count = 0;
                foreach(XmlNode child in children)
                {
                    var newDoc = new XmlDocument();

                    var findNode = FindNode(doc, "XmlDeclaration");
                    if (findNode != null)
                    {
                        newDoc.AppendChild(newDoc.ImportNode(findNode, true));
                    }

                    findNode = FindNode(doc, "DocumentType");
                    if (findNode != null)
                    {
                        newDoc.AppendChild(newDoc.ImportNode(findNode, true));
                    }

                    var ancestors = GetAncestors(child);
                    var node = newDoc as XmlNode;
                    foreach(XmlNode parent in ancestors)
                    {
                        node = node.AppendChild(newDoc.ImportNode(parent, false));
                    }

                    node.AppendChild(newDoc.ImportNode(child, true));

                    //Transpose(newDoc);

                    var newFilename = filename.Replace(".", ++count + ".");
                    newDoc.Save(newFilename);

                    var svg = SvgDocument.Open(newFilename);
                    var g = newDoc.SelectSingleNode("//*[local-name()='g']");
                    var attr = newDoc.CreateAttribute("transform");
                    attr.Value = $"translate({-svg.Bounds.X},{-svg.Bounds.Y})";
                    g.Attributes.Append(attr);
                    newDoc.Save(newFilename);
                }
            }
        }

        private XmlNode FindNode(XmlNode node, string name)
        {
            foreach(XmlNode child in node.ChildNodes)
            {
                if (child.NodeType.ToString() == name)
                {
                    return child;
                }
            }

            return null;
        }

        private List<XmlNode> GetAncestors(XmlNode child)
        {
            var results = new List<XmlNode>();
            var parent = child.ParentNode;
            while (parent != null)
            {
                results.Insert(0, parent);
                parent = parent.ParentNode;
            }

            results.RemoveAt(0);

            return results;
        }
    }
}
