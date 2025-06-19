using System.Security.Cryptography;
using System.Text;
using System.Web;
using api.DTO.SetttingsDTO;
using api.Helpers.Instances;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace api.Helpers;

public class Encrypter(IOptions<EncryptKeysDTO> encrypter, IDataProtectionProvider dataPro)
    : IEncrypter
{
    private const string Purpose = "DGX.Hotel.V1";
    private readonly IDataProtector _protector = dataPro.CreateProtector(Purpose);
    private readonly EncryptKeysDTO _encrypter = encrypter.Value;
    private readonly Func<string, byte[]> _getBytesUTF8 = (text) => Encoding.UTF8.GetBytes(text);
    private readonly Func<byte[], string> _getStringUTF8 = (bytes) =>
        Encoding.UTF8.GetString(bytes);

    public string EncryptString(string? plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return string.Empty;
        }
        byte[] plainBytes = _getBytesUTF8(plainText);
        byte[] protectedBytes = _protector.Protect(plainBytes);
        string base64Encoded = Convert.ToBase64String(protectedBytes);
        return HttpUtility.UrlEncode(base64Encoded);
    }

    public string DecryptString(string? encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
        {
            return string.Empty;
        }
        try
        {
            string urlDecoded = HttpUtility.UrlDecode(encryptedText);
            byte[] base64Decoded = Convert.FromBase64String(urlDecoded);
            byte[] unprotectedBytes = _protector.Unprotect(base64Decoded);
            return _getStringUTF8(unprotectedBytes);
        }
        catch (FormatException)
        {
            return "";
        }
        catch (CryptographicException)
        {
            return "";
        }
    }
}
