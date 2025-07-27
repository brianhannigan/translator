
using System;
using System.IO;
using System.Xml;
using Translator.Extensions;
using Translator.Interfaces;

namespace Translator.Palette
{
    internal static class PaletteFileGeneration
    {
        #region XmlTags
        private const string GenerationInfoXmlTag = "generationInfo";
        private const string GenerationTimeXmlTag = "timeStamp";
        private const string PaletteRootXmlTag = "palette";
        private const string BrushesXmlTag = "brushes";
        private const string BrushXmlTag = "brush";
        private const string BrushNameXmlTag = "name";
        private const string BrushValueXmlTag = "value";
        #endregion

        #region Generate Implementation
        /// <summary>
        /// Generate a file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="paletteContainer"></param>
        /// <returns></returns>
        public static bool GeneratePaletteFile(string path, IPaletteContainer paletteContainer)
        {
            if (paletteContainer == null)
            {
                string msg = "Error saving Palette file:\n";
                msg += "No palette container specified";
                System.Diagnostics.Debug.WriteLine(msg);
                return false;
            }
            try
            {
                string dir = Path.GetDirectoryName(path);
                DirectoryInfo dirInfo = Directory.CreateDirectory(dir);
                if (dirInfo.Exists)
                {
                    XmlDocument doc = new XmlDocument();
                    XmlNode rootNode = doc.CreateElement(PaletteRootXmlTag);
                    XmlElement generationInfoElement = doc.CreateElement(GenerationInfoXmlTag);
                    doc.CreateAndAddElement(generationInfoElement, GenerationTimeXmlTag, DateTime.Now.ToString());
                    rootNode.AppendChild(generationInfoElement);

                    XmlNode brushesNode = paletteContainer.GeneratePaletteContainerData(doc);
                    if (brushesNode != null)
                        rootNode.AppendChild(brushesNode);

                    doc.AppendChild(rootNode);
                    doc.Save(path);
                }
            }
            catch (Exception ex)
            {
                string msg = "Exception saving palette to file:\n";
                msg += "\n\n" + ex.Message + "\n\n" + ex.StackTrace;
                System.Diagnostics.Debug.WriteLine(msg);
            }
            return true;
        }

        /// <summary>
        /// Geernate the list of brushes
        /// </summary>
        /// <param name="paletteContainer"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static XmlNode GeneratePaletteContainerData(this IPaletteContainer paletteContainer, XmlDocument doc)
        {
            if (paletteContainer == null)
                return null;

            XmlElement brushesNode = null;
            try
            {
                brushesNode = doc.CreateElement(string.Empty, BrushesXmlTag, string.Empty);
                foreach (IPaletteBrush brush in paletteContainer.Brushes)
                {
                    brushesNode.AppendChild(brush.GenerateBrushEntry(doc));
                }
            }
            catch(Exception ex)
            {
                string msg = "Exception converting brushes to XML:\n"
                            + "\n\n"
                            + ex.Message
                            + "\n\n"
                            + ex.StackTrace;
                System.Diagnostics.Debug.WriteLine(msg);
            }
            return brushesNode;
        }

        /// <summary>
        /// Generate a brush entry
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private static XmlNode GenerateBrushEntry(this IPaletteBrush brush, XmlDocument doc)
        {
            if (brush == null)
                return null;
            XmlElement brushNode = null;
            try
            {
                brushNode = doc.CreateElement(string.Empty, BrushXmlTag, string.Empty);
                doc.CreateAndAddAttribute(brushNode, BrushNameXmlTag, brush.BrushName);
                doc.CreateAndAddAttribute(brushNode, BrushValueXmlTag, brush.Value);
            }
            catch (Exception ex)
            {
                string msg = "Exception converting brush to XML:\n"
                             + "\n\n"
                             + ex.Message
                             + "\n\n"
                             + ex.StackTrace;
                System.Diagnostics.Debug.WriteLine(msg);
            }
            return brushNode;
        }
        #endregion

        #region Load Implementation
        /// <summary>
        /// Parse the file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IPaletteContainer ParsePaletteFile(string path)
        {
            IPaletteContainer paletteContainer = null;
            try
            {
                XmlDocument xd = new XmlDocument();
                xd.Load(path);
                paletteContainer = ParsePaletteFile(new PaletteContainer(), xd);
            }
            catch (Exception ex)
            {
                string msg = "Exception loading/parsing palette file:\n" + path;
                msg += "\n\n" + ex.Message + "\n\n" + ex.StackTrace;
                System.Diagnostics.Debug.WriteLine(msg);
            }
            return paletteContainer;
        }

        /// <summary>
        /// Parse the palette container
        /// </summary>
        /// <param name="paletteContainer"></param>
        /// <param name="xd"></param>
        /// <returns></returns>
        private static IPaletteContainer ParsePaletteFile(PaletteContainer paletteContainer, XmlDocument xd)
        {
            if (paletteContainer == null)
                return paletteContainer;

            try
            {
                XmlElement rootXml = xd.DocumentElement;
                if (rootXml != null && rootXml.Name == PaletteRootXmlTag)
                {
                    foreach (XmlNode node in rootXml.ChildNodes)
                    {
                        switch (node.Name)
                        {
                            case BrushesXmlTag:
                                paletteContainer.ParseBrushes(node);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Exception parsing brushes from XML:\n";
                msg += "\n\n" + ex.Message + "\n\n" + ex.StackTrace;
                System.Diagnostics.Debug.WriteLine(msg);
            }
            return paletteContainer;
        }

        /// <summary>
        /// Parse the brushes element
        /// </summary>
        /// <param name="paletteContainer"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private static bool ParseBrushes(this PaletteContainer paletteContainer, XmlNode node)
        {
            if (paletteContainer == null ||
                node == null ||
                node.Name != BrushesXmlTag)
                return false;

            try
            {
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    switch (childNode.Name)
                    {
                        case BrushXmlTag:
                            PaletteBrush brush = new PaletteBrush();
                            if (brush.ParseBrush(childNode))
                                paletteContainer.AddBrush(brush);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Exception parsing brushes from XML:\n";
                msg += "\n\n" + ex.Message + "\n\n" + ex.StackTrace;
                System.Diagnostics.Debug.WriteLine(msg);
            }
            return true;
        }

        /// <summary>
        /// Parse the brush element
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private static bool ParseBrush(this PaletteBrush brush, XmlNode node)
        {
            if (brush == null ||
                node == null ||
                node.Name != BrushXmlTag)
                return false;
            try
            {
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    switch (attribute.Name)
                    {
                        case BrushNameXmlTag:
                            brush.BrushName = attribute.Value;
                            break;
                        case BrushValueXmlTag:
                            brush.Value = attribute.Value;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Exception parsing brush from XML:\n";
                msg += "\n\n" + ex.Message + "\n\n" + ex.StackTrace;
                System.Diagnostics.Debug.WriteLine(msg);
                return false;
            }
            return true;
        }
        #endregion
    }
}
