using SLOY.Shared.Models;
using SLOY.Shared.Validators;

namespace SLOY.Core.Factory;

public static class IdentityFactory
{
    public static Identity Create(string nickname, string? fio = null, byte[]? publicKey = null)
    {
        if (!NicknameValidator.IsValid(nickname))
            throw new ArgumentException("Недопустимый nickname.");

        return new Identity(nickname)
        {
            FIO = fio,
            PublicKey = publicKey
        };
    }
}