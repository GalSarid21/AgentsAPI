namespace AgentsDM
{
    public class BaseResponse
    {
        /// <summary>
        /// The status of POST method. Can be SUCCEEDED or FAILED.
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// The error message caught by catch block. If the POST succeeded Error will be null.
        /// </summary>
        public string Error { get; set; }

        public void UpdateStatusAndError(string status, string error = null)
        {
            Status = status;
            Error = error;
        }
    }
}
