using System;
using System.Net.Http;
using System.Net.Http.Headers;
using TranslatorBackend.Results;

namespace TranslatorBackend.Ocr
{
    internal class TesseractHttpHandler : HttpHandler<ImageOcrResult, JsonOcrData>
    {
        public void OCRAnImage(ImageOcrResult result, string uri, byte[] imageBytes, string languageCode)
        {
            try
            {
                using (var content = new MultipartFormDataContent())
                {
                    ByteArrayContent imageContent = new ByteArrayContent(imageBytes);
                    imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

                    content.Add(imageContent, "image", "uploaded_image.jpg"); // Set a valid filename
                    content.Add(new StringContent(languageCode), "lang");  // Specify the OCR language

                    RunHttpTask(result, content, uri);
                }
            }
            catch (Exception ex)
            {
                result.MarkAsError(ex.Message);
            }
        }
    }
}
