namespace ApplicationLayer.Models
{
    public class ErrorResponse
    {
        public string Message { get; set; }
        public string Details { get; set; }
        public string TraceId { get; set; }

        public ErrorResponse(string message, string details = null)
        {
            Message = message;
            Details = details;
            TraceId = Guid.NewGuid().ToString();
        }
    }
}