namespace api.Helpers.Instances;

public interface IHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}
