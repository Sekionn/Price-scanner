using price_bot.Interfaces;

namespace price_bot.Models;
public class License(string email, string key) : IJSONSerializable
{
    public string Email { get; set; } = email;
    public string Key { get; set; } = key;
    public Guid? ShopId { get; set; }
}