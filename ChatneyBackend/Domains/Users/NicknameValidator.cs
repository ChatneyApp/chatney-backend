using System.Text.RegularExpressions;
using ChatneyBackend.Infra;

namespace ChatneyBackend.Domains.Users;

public static class NicknameValidator
{
    public const int MaxLength = 20;

    private static readonly Regex NicknamePattern = new(@"^[a-zA-Z0-9_]+$", RegexOptions.Compiled);

    public static string NormalizeAndValidate(string nickname)
    {
        var trimmed = nickname.Trim();
        if (string.IsNullOrEmpty(trimmed) ||
            trimmed.Length > MaxLength ||
            !NicknamePattern.IsMatch(trimmed))
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowInvalidNickname();
        }

        return trimmed;
    }

    public static async Task EnsureUnique(AppRepos repos, string nickname, Guid? excludeUserId = null)
    {
        if (await UserLookup.IsNicknameTaken(repos, nickname, excludeUserId))
        {
            ChatneyBackend.Infra.ErrorCodes.ThrowNicknameTaken();
        }
    }
}
