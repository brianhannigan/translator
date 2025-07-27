namespace TranslatorBackend
{
    public static class OcrPropertyNames
    {
        // Ocr Data
        public const string TextPropertyName = "text";
        public const string WordInfoPropertyName = "words_info";

        // Word Info
        public const string WordPropertyName = "word";
        public const string ConfidencePropertyName = "confidence";
        public const string BoundingBoxPropertyName = "bounding_box";

        // Bounding Box
        public const string BoundingBoxX1PropertyName = "x1";
        public const string BoundingBoxX2PropertyName = "x2";
        public const string BoundingBoxY1PropertyName = "y1";
        public const string BoundingBoxY2PropertyName = "y2";
    }

    public static class TranslationPropertyNames
    {
        public const string TextPropertyName = "translatedText";
        public const string AlternativesPropertyName = "alternatives";
    }
}
