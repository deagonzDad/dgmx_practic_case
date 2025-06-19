using System;

namespace api.Helpers.Instances;

public interface IEncrypter
{
    string EncryptString(string? plainText);
    string DecryptString(string? encryptedText);
}

// public interface IEncryptionHelper
// {

// }
