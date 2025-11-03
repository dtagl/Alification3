using System.Security.Cryptography;
using System.Text;

public class TelegramAuthService
{
    private readonly string _botToken;

    public TelegramAuthService(IConfiguration config)
    {
        _botToken = config["Telegram:BotToken"]!;
    }

    public bool Validate(Dictionary<string, string> data)
    {
        if (!data.TryGetValue("hash", out var hash))
            return false;
        data.Remove("hash");

        var sorted = data.OrderBy(kv => kv.Key)
            .Select(kv => $"{kv.Key}={kv.Value}");
        var dataCheckString = string.Join("\n", sorted);

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SHA256Hash(_botToken)));
        var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(dataCheckString));
        var hex = BitConverter.ToString(computed).Replace("-", "").ToLower();

        return hex == hash;
    }

    private static string SHA256Hash(string input)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}