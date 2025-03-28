using System;
using System.Text.RegularExpressions;

namespace api.ValidationAttributes.Interfaces;

public interface IRegexController
{
    Regex UpperCaseRegex();

    Regex SpecialCharacterRegex();
    Regex FilterAscDesc();
    Regex FilterSortBy();
}
