using System;
using System.IO;
using System.Xml;

namespace TranslatorBackend.LanguageCodes
{
    internal static class LanguageCodeParser
    {
        private const string RootXmlTag = "languagecodes";
        private const string LanguageCodeXmlTag = "languagecode";
        private const string DisplayAttributeTag = "display";
        private const string TranslationAttributeTag = "translation";
        private const string OcrAttributeTag = "ocr";

        public static bool ParseLanguagesFile(this LanguageCodeContainer container, string path)
        {
            if (string.IsNullOrWhiteSpace(path) ||
                container == null)
                return false;

            bool success;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                success = ParseLanguageCodeFile(doc, container);
            }
            catch (Exception ex)
            {
                string msg = "Exception loading/parsing language codes xml file:\n" + path;
                msg += "\n\n" + ex.Message + "\n\n" + ex.StackTrace;
                System.Diagnostics.Debug.WriteLine(msg);
                return false;
            }
            return success;
        }

        public static bool ParseLanguagesFile(this LanguageCodeContainer container, Stream stream)
        {
            if (container == null ||
                stream == null)
                return false;

            bool success;
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(stream);
                success = ParseLanguageCodeFile(doc, container);
            }
            catch (Exception ex)
            {
                string msg = "Exception loading/parsing language codes xml file:\n";
                msg += "\n\n" + ex.Message + "\n\n" + ex.StackTrace;
                System.Diagnostics.Debug.WriteLine(msg);
                return false;
            }
            return true;
        }

        private static bool ParseLanguageCodeFile(XmlDocument doc, LanguageCodeContainer container)
        {
            if (doc == null ||
                container == null)
                return false;

            try
            {
                XmlElement rootXml = doc.DocumentElement;
                if (rootXml != null && string.Equals(rootXml.Name, RootXmlTag, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (XmlNode node in rootXml.ChildNodes)
                    {
                        switch (node.Name.ToLower())
                        {
                            case LanguageCodeXmlTag:
                                LanguageCode languageCode = new LanguageCode();
                                if (languageCode.ParseLanguageCode(node))
                                    container.AddLanguageCode(languageCode);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Exception loading/parsing language config file:\n" + doc.Name;
                msg += "\n\n" + ex.Message + "\n\n" + ex.StackTrace;
                System.Diagnostics.Debug.WriteLine(msg);
                return false;
            }
            return true;
        }

        private static bool ParseLanguageCode(this LanguageCode languageCode, XmlNode node)
        {
            if (languageCode == null ||
                node == null ||
                !string.Equals(node.Name, LanguageCodeXmlTag, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            try
            {
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    switch (attribute.Name.ToLower())
                    {
                        case DisplayAttributeTag:
                            languageCode.DisplayName = attribute.Value;
                            break;
                        case TranslationAttributeTag:
                            languageCode.TranslationCode = attribute.Value;
                            break;
                        case OcrAttributeTag:
                            languageCode.OcrCode = attribute.Value;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = "Exception parsing language code from XML:\n";
                msg += "\n\n" + ex.Message + "\n\n" + ex.StackTrace;
                System.Diagnostics.Debug.WriteLine(msg);
                return false;
            }
            return true;
        }
    }
}
