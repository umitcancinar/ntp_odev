namespace ChatApp.Core;

public sealed record ClientInfo(string User, string Endpoint);
public sealed record ChatPacket(string Type, string? User = null, string? Text = null,
    DateTimeOffset? Timestamp = null, ClientInfo[]? Clients = null);

public static class ChatRules
{
    public static string User(string user)
    {
        ArgumentNullException.ThrowIfNull(user);
        user = user.Trim();
        if (user.Length is < 1 or > 32 || user.Any(char.IsControl))
            throw new ArgumentException("Kullanıcı adı 1–32 karakter olmalı; kontrol karakteri içeremez.");
        return user;
    }
    public static string Message(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (string.IsNullOrWhiteSpace(text) || text.Length > 2000)
            throw new ArgumentException("Mesaj 1–2000 karakter olmalıdır.");
        return text;
    }
}
