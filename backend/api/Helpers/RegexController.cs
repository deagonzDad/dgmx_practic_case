using System.Text.RegularExpressions;
using api.ValidationAttributes.Interfaces;

namespace api.ValidationAttributes;

public partial class RegexController : IRegexController
{
    [GeneratedRegex("[A-Z]")]
    private static partial Regex _UpperCaseRegex();

    [GeneratedRegex("[^a-zA-Z0-9]")]
    private static partial Regex _SpecialCharacterRegex();

    [GeneratedRegex("asc|desc", RegexOptions.IgnoreCase)]
    private static partial Regex _SortAscDescRegex();

    [GeneratedRegex("", RegexOptions.IgnoreCase)]
    private static partial Regex _FilterSortBy();

    [GeneratedRegex(@"[@:]", RegexOptions.None)]
    private static partial Regex _FilterExcludeSymbols();

    public Regex UpperCaseRegex() => _UpperCaseRegex();

    public Regex SpecialCharacterRegex() => _SpecialCharacterRegex();

    public Regex FilterAscDesc() => _SortAscDescRegex();

    public Regex FilterSortBy() => _FilterSortBy();

    public Regex FilterExcludeSymbols() => _FilterExcludeSymbols();
}
