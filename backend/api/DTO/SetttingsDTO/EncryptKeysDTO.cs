using System;

namespace api.DTO.SetttingsDTO;

public class EncryptKeysDTO
{
    public required string SecretKey { get; set; }
    public required string InitVector { get; set; }
}
