namespace E_Commerce.Core.DTOs
{
    public class ForgotPasswordResultDto
    {
        public bool EmailExists { get; set; }
        public bool OtpSent { get; set; }
    }
}

