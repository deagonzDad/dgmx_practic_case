using System;
using System.Text.RegularExpressions;
using api.ValidationAttributes.Interfaces;

namespace api.ValidationAttributes;

public partial class RegexController : IRegexController
{
    [GeneratedRegex("[A-Z]")]
    private static partial Regex _UpperCaseRegex();

    [GeneratedRegex("[^a-zA-Z0-9]")]
    private static partial Regex _SpecialCharacterRegex();

    public Regex UpperCaseRegex() => _UpperCaseRegex();

    public Regex SpecialCharacterRegex() => _SpecialCharacterRegex();
}
