namespace AgentsDM
{
    public class BaseResponse
    {
        public string Status { get; set; }
        public string Error { get; set; }

        public void UpdateStatusAndError(string status, string error = null)
        {
            Status = status;
            Error = error;
        }
    }
}
