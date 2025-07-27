using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using TranslatorBackend.Results;

namespace TranslatorBackend.Translation
{
    internal class LibreTranslateHttpHandler : HttpHandler<TextTranslationResult, JsonTranslationData>
    {
        public void TranslateText(TextTranslationResult result, string uri, string text, string sourceLngCode, string targetLngCode, int numAlternativesToGet)
        {
            try
            {
                string contentType = "application/json";

                JObject postData = new JObject
                {
                    { "q", text },
                    { "format", "text" },
                    { "source", sourceLngCode },
                    { "target", targetLngCode },
                    { "alternatives", numAlternativesToGet },
                    { "api_key", "" }
                };

                // Create the content 
                StringContent content = new StringContent(postData.ToString(Newtonsoft.Json.Formatting.None));

                // Set the content type header
                content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

                RunHttpTask(result, content, uri);
            }
            catch (Exception ex)
            {
                result.MarkAsError(ex.Message);
            }
        }
    }
}
