using System.Xml;

namespace Translator.Extensions
{
    internal static class XmlExtension
    {
        internal static void CreateAndAddAttribute(this XmlDocument doc, XmlElement parentElement, string tag, string value)
        {
            if (doc == null ||
                parentElement == null)
                return;
            XmlAttribute attrNode = doc.CreateAttribute(tag);
            attrNode.Value = value;
            parentElement.Attributes.Append(attrNode);
        }

        internal static XmlElement CreateAndAddElement(this XmlDocument doc, XmlElement parentElement, string tag, string value)
        {
            if (doc == null ||
                parentElement == null)
                return null;
            XmlElement elementNode = doc.CreateElement(tag);
            if (!string.IsNullOrEmpty(value))
                elementNode.InnerText = value;
            parentElement.AppendChild(elementNode);
            return elementNode;
        }
    }
}
