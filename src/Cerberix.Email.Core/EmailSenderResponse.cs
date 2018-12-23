namespace Cerberix.Email.Core
{
    public class EmailSenderResponse
    {
        public EmailSenderResponseStatusCode StatusCode { get; }

        public EmailSenderResponse(
            EmailSenderResponseStatusCode statusCode
            )
        {
            StatusCode = statusCode;
        }
    }
}
