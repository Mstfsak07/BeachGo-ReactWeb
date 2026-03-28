using System.Security.Cryptography;

namespace BeachRehberi.API.Services;

public interface IConfirmationCodeGenerator
{
    string Generate(int length = 8);
}

public class ConfirmationCodeGenerator : IConfirmationCodeGenerator
{
    private static readonly char[] ConfirmationChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    public string Generate(int length = 8)
    {
        var codeChars = new char[length];
        for (int i = 0; i < length; i++)
        {
            codeChars[i] = ConfirmationChars[RandomNumberGenerator.GetInt32(ConfirmationChars.Length)];
        }
        return new string(codeChars);
    }
}
