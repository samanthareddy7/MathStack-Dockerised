namespace MathApi_Client.Models;

// This matches the "Shape" of the JSON Google sends back
public class FirebaseErrorModel
{
    public FirebaseError error { get; set; }
}

public class FirebaseError
{
    public string message { get; set; }
}