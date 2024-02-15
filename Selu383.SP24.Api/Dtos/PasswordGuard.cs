namespace Selu383.SP24.Api.Dtos;

public class PasswordGuard
{
    public string? Password
    {
        get => null;
        set => throw new Exception("You returned a password, don't do this!");
    }

    public string? PasswordHash
    {
        get => null;
        set => throw new Exception("You returned a password, don't do this!");
    }
}