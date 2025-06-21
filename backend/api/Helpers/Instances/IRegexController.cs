using System.Text.RegularExpressions;

namespace api.Helpers.Instances
{
    public interface IRegexController
    {
        Regex UpperCaseRegex();

        Regex SpecialCharacterRegex();
        Regex FilterAscDesc();
        Regex FilterSortBy();
        Regex FilterExcludeSymbols();
    }
}
