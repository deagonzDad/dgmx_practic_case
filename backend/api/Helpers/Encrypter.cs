using System.Security.Cryptography;
using api.DTO.SetttingsDTO;
using api.Helpers.Instances;
using Microsoft.Extensions.Options;

namespace api.Helpers;

public class Encrypter(IOptions<EncryptKeysDTO> encrypter) : IEncrypter
{
    private readonly EncryptKeysDTO _encrypter = encrypter.Value;

    public string DecryptString(string encryptedText)
    {
        throw new NotImplementedException();
    }

    public string EncryptString(string plainText)
    {
        //https: //medium.com/@muhebollah.diu/securing-query-parameters-in-asp-net-core-encrypting-guids-for-better-security-fdfd921033ae
        if (string.IsNullOrEmpty(plainText))
        {
            throw new Exception("MissingText");
        }
        using Aes aesAlg = Aes.Create();
        throw new NotImplementedException();
    }
}
