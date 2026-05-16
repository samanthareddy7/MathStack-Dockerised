namespace MathApi.Models
{
    public class FirebaseErrorModel
    {
        public FirebaseError detail { get; set; }
        public ErrorContent error { get; set; }
    }

    public class FirebaseError
    {
        public string message { get; set; }
    }

    public class ErrorContent
    {
        public int code { get; set; }
        public string message { get; set; }
    }
}