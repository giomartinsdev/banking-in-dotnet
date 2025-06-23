namespace BankingProject.Domain.Context.CustomerAggregate.ValueObjects;
using System.Security.Cryptography;
using System.Text;

public class Password
{
    public string Salt { get; set; }
    public string Hash { get; set; }
    public string Pepper { get; set; }
    public int Iteration { get; set; }
    
    public Password(string salt, string hash, string pepper, int iteration)
    {
        Salt = salt;
        Hash = hash;
        Pepper = pepper;
        Iteration = iteration;
    }
    
    public static string GenerateSalt()
    {
        using var rng = RandomNumberGenerator.Create();
        var byteSalt = new byte[16];
        rng.GetBytes(byteSalt);

        return Convert.ToBase64String(byteSalt);
    }

    public static string ComputeHash(string salt, string password, string pepper, int iteration)
    {
        if(iteration <= 0) return password;
        using var sha256 = SHA256.Create();
        var passwordSaltPepper = $"{password}{salt}{pepper}";
        var byteValue = Encoding.UTF8.GetBytes(passwordSaltPepper);
        var byteHash = sha256.ComputeHash(byteValue);
        var hash = Convert.ToBase64String(byteHash);

        return ComputeHash(hash, salt, pepper, iteration - 1);
    }

}