namespace UserAPI.Models.DTOs
{
    public class ResponseDto
    {
        public bool Status { get; set; }
        public string Message { get; set; }
         public  User? Users { get; set; }
}
}
