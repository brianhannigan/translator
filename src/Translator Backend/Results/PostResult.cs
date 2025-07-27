namespace TranslatorBackend
{
    internal struct PostResult
    {
        public bool Success { get; }
        public string Response { get; }
        public PostResult(bool success, string response)
        {
            Success = success;
            Response = response;
        }
    }
}
