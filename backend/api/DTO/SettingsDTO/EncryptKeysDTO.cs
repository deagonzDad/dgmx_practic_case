using System;

namespace api.DTO.SettingsDTO;

public class EncryptKeysDTO
{
    public required string SecretKey { get; set; }
    public required string InitVector { get; set; }
}
