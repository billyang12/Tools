using System.Text.RegularExpressions;
using HierarchicalNotes.Core.Interfaces;

namespace HierarchicalNotes.Core.Models;

public enum TextSearchMethodEnum
{
    Strict,
    And,
    Or
}

public class SearchCriteria : ISearchCriteria
{
    private bool _isCaseSensitive = false;
    private TextSearchMethodEnum _searchMethod = TextSearchMethodEnum.Strict;
    private string? _pattern;
    private string[]? _patterns;

    public SearchCriteria()
    {
        SearchFromDateTime = null;
        SearchToDateTime = null;
        _isCaseSensitive = false;
        _searchMethod = TextSearchMethodEnum.Strict;
        PreProcessParameters();
    }

    public string? Pattern
    {
        get => _pattern;
        set
        {
            _pattern = value;
            PreProcessParameters();
        }
    }

    public bool IsCaseSensitive
    {
        get => _isCaseSensitive;
        set => _isCaseSensitive = value;
    }

    public bool IsRegularExpression { get; set; }

    public TextSearchMethodEnum SearchMethod
    {
        get => _searchMethod;
        set
        {
            _searchMethod = value;
            PreProcessParameters();
        }
    }

    public DateTime? SearchFromDateTime { get; set; }
    public DateTime? SearchToDateTime { get; set; }

    private void PreProcessParameters()
    {
        _pattern = Pattern ?? string.Empty;
        _patterns = _searchMethod == TextSearchMethodEnum.Strict
            ? null
            : _pattern.Split(new[] { ',', ' ', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
    }

    public bool HasMatches(string textSource)
    {
        return SearchMethod switch
        {
            TextSearchMethodEnum.Strict => HasSubString(textSource, _pattern),
            TextSearchMethodEnum.And => _patterns?.All(p => HasSubString(textSource, p)) == true,
            TextSearchMethodEnum.Or => _patterns?.Any(p => HasSubString(textSource, p)) == true,
            _ => true
        };
    }

    public bool HasMatches(string textSource, DateTime? itemDateTime)
    {
        if (!itemDateTime.HasValue)
        {
            return HasMatches(textSource);
        }

        if (SearchFromDateTime.HasValue && SearchToDateTime.HasValue)
        {
            return itemDateTime >= SearchFromDateTime && itemDateTime <= SearchToDateTime && HasMatches(textSource);
        }

        if (SearchFromDateTime.HasValue)
        {
            return itemDateTime >= SearchFromDateTime && HasMatches(textSource);
        }

        if (SearchToDateTime.HasValue)
        {
            return itemDateTime <= SearchToDateTime && HasMatches(textSource);
        }

        return HasMatches(textSource);
    }

    private bool HasSubString(string? src, string? patt)
    {
        if (src == null || patt == null)
        {
            return false;
        }

        if (!IsRegularExpression)
        {
            return _isCaseSensitive
                ? src.Contains(patt, StringComparison.CurrentCulture)
                : src.Contains(patt, StringComparison.CurrentCultureIgnoreCase);
        }

        return Regex.IsMatch(src, patt, _isCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
    }
}
