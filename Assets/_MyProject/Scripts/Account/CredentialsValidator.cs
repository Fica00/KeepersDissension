public static class CredentialsValidator
{
    private const int MINIMUM_EMAIL_LENGTH = 10;
    private const int MINIMUM_PASSWORD_LENGTH = 6;

    public static bool ValidateCredentials(string _email, string _password)
    {
        if (!VerifyEmail(_email))
        {
            return false;
        }

        if (!VerifyPassword(_password))
        {
            return false;
        }
        return true;
    }
    
    public static bool VerifyEmail(string _email)
    {
        if (string.IsNullOrEmpty(_email))
        {
            DialogsManager.Instance.ShowOkDialog("Please enter email");
            return false;
        }

        if (_email.Length < MINIMUM_EMAIL_LENGTH)
        {
            DialogsManager.Instance.ShowOkDialog($"Email must contain at least {MINIMUM_EMAIL_LENGTH} characters");
            return false;
        }

        if (!_email.Contains("@"))
        {
            DialogsManager.Instance.ShowOkDialog("Please enter valid email");
            return false;
        }

        return true;
    }
    
    private static bool VerifyPassword(string _password)
    {
        if (string.IsNullOrEmpty(_password))
        {
            DialogsManager.Instance.ShowOkDialog("Please enter password");
            return false;
        }
        if (_password.Length < MINIMUM_PASSWORD_LENGTH)
        {
            DialogsManager.Instance.ShowOkDialog($"Password must contain at least {MINIMUM_PASSWORD_LENGTH} characters");
            return false;
        }

        return true;
    }
    
    public static bool ValidateName(string _name)
    {
        if (string.IsNullOrEmpty(_name))
        {
            DialogsManager.Instance.ShowOkDialog("Please enter name");
            return false;
        }

        if (_name.Length is < 4 or > 10)
        {
            DialogsManager.Instance.ShowOkDialog("Name must be between 4 and 10 characters");
            return false;
        }
        
        return true;
    }
}