namespace Selu383.SP24.Api.Dtos;

public class CreateUserDto
{
    public string UserName { get; set; }
    public string Password { get; set; }
    public string[] Roles { get; set; }
}
