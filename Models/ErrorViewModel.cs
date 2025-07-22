public class ErrorVM
{
    private string? requestId;
    public string? RequestId { get => requestId; set => requestId = value; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
