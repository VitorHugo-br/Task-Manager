using System.Net.Mail;

namespace Task_Manager.Helpers;

public static class EmailVerification
{
    public static bool IsValid(string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}