namespace JCS.Argon.Model.Responses
{
    /// <summary>
    ///     Generic exception response object
    /// </summary>
    public class ExceptionResponse
    {
        public ExceptionResponse(string message, string source)
        {
            Message = message;
            Source = source;
        }

        public ExceptionResponse()
        {
        }

        public int HttpResponseCode { get; set; }

        public string? Message { get; set; }

        public string? Source { get; set; }
    }
}