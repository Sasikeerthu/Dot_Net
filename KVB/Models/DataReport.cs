namespace KVB.Models
{
    public class DataReport
    {
        public string Date { get; set; }
        public int Enrollment { get; set; }
        public int EnrollmentFailure { get; set; }
        public string LoginAttempt { get; set; }
        public string LoginSuccessthroughDirectVoiceAuthentication { get; set; }
        public string LoginSuccessThroughAIModel { get; set; }
        public int TotalSuccess { get; set; }
        public int LoginFailed  { get; set; }
    }
}
