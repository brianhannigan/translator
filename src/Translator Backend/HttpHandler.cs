using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TranslatorBackend.Results;

namespace TranslatorBackend
{
    internal abstract class HttpHandler<R, D>
        where R : ResultBase
        where D : BaseJsonData
    {
        protected void RunHttpTask(R result, HttpContent content, string uri)
        {
            // start the translation task
            Task<PostResult> httpRequestTask = HttpPostRequest(content, uri);

            // Process the result
            PostResult postResult = httpRequestTask.Result;

            // Get the json repsonse
            string jsonResponse = postResult.Response;

            // Process translation response if we get a good result.
            if (postResult.Success)
            {
                // Parse json response
                try
                {
                    JObject jsonObj = JObject.Parse(jsonResponse);
                    if (jsonObj != null && jsonObj.HasValues)
                    {
                        D dataObj = jsonObj.ToObject<D>();
                        result.TryMarkAsSuccess(dataObj);
                    }
                    else
                    {
                        result.MarkAsError("Failure to process json response with no values.");
                    }
                }
                catch (Exception ex)
                {
                    result.MarkAsError("Failure to process json: " + ex.Message);
                }
            }
            else
            {
                result.MarkAsError(jsonResponse);
            }
        }

        /// <summary>
        /// Task to make a post call to a uri to translate text
        /// </summary>
        /// <param name="postData"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        protected async Task<PostResult> HttpPostRequest(HttpContent content, string uri)
        {
            bool success = false;
            string response = null;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage responseMsg = await client.PostAsync(uri, content);
                    //responseMsg.EnsureSuccessStatusCode();
                    response = await responseMsg.Content.ReadAsStringAsync();
                    success = responseMsg.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                response = "Http Error: " + ex.Message;
                success = false;
            }
            return new PostResult(success, response);
        }
    }
}
