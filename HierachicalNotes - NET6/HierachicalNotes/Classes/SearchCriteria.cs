using HierachicalNotes.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HierachicalNotes.Classes
{
    public class SearchCriteria : ISearchCriteria
    {
        private bool _isCaseSensitive = false;
        private TextSearchMethodEnum _searchMethod = TextSearchMethodEnum.Strict;
        private string? _pattern = null;
        private string[]? _patterns = null;

        public SearchCriteria()
        {
            SearchFromDateTime = null;
            SearchToDateTime = null;
            _isCaseSensitive = false;
            _searchMethod = TextSearchMethodEnum.Strict;
            PreProcessParameters();
        }
        public string? Pattern  //if SearchMethod is And, Or, the words in the pattern is comma or space or semicolon, or CR seperated
        {
            get
            {
                return _pattern;
            }
            set
            {
                _pattern = value;
                PreProcessParameters();
            }
        }
        public bool IsCaseSensitive
        {
            get
            {
                return _isCaseSensitive;
            }
            set
            {
                _isCaseSensitive = value;
            }
        }
        public bool IsRegularExpression
        {
            get; set;
        }
        public TextSearchMethodEnum SearchMethod
        {
            get
            {
                return _searchMethod;
            }
            set
            {
                _searchMethod = value;
                PreProcessParameters();
            }
        }
        public DateTime? SearchFromDateTime { get; set; }
        public DateTime? SearchToDateTime { get; set; }
        void PreProcessParameters()
        {
            _pattern = (Pattern == null ? "" : Pattern);
            if (_searchMethod != TextSearchMethodEnum.Strict)
            {
                _patterns = _pattern.Split(new char[] { ',', ' ', ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public virtual bool HasMatches(string? textSource)
        {
            switch (SearchMethod)
            {
                case TextSearchMethodEnum.Strict:
                    return hasSubString(textSource, _pattern);
                case TextSearchMethodEnum.And:
                    foreach (string p in _patterns)
                    {
                        if (!hasSubString(textSource, p)) return false;
                    }
                    return true;
                case TextSearchMethodEnum.Or:
                    foreach (string p in _patterns)
                    {
                        if (hasSubString(textSource, p)) return true;
                    }
                    return false;
            }
            return true;
        }

        public virtual bool HasMatches(string textSource, DateTime? itemDateTime)
        {
            if(!itemDateTime.HasValue) return HasMatches(textSource); //default search
            if (SearchFromDateTime.HasValue && SearchToDateTime.HasValue)
            {
                if (itemDateTime >= SearchFromDateTime && itemDateTime <= SearchToDateTime)
                {
                    return HasMatches(textSource);
                }
                else
                {
                    return false;
                }
            }
            else if (SearchFromDateTime.HasValue && !SearchToDateTime.HasValue)
            {
                if (itemDateTime >= SearchFromDateTime)
                {
                    return HasMatches(textSource);
                }
                else
                {
                    return false;
                }
            }
            else if (!SearchFromDateTime.HasValue && SearchToDateTime.HasValue)
            {
                if (itemDateTime <= SearchToDateTime)
                {
                    return HasMatches(textSource);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return HasMatches(textSource);
            }
        }
        bool hasSubString(string? src, string? patt)
        {
            if (src == null) return false;
            if (patt == null) return false;
            if (!IsRegularExpression)
            {
                if (!_isCaseSensitive)
                {
                    return src.IndexOf(patt, StringComparison.CurrentCultureIgnoreCase) >= 0;
                }
                else
                {
                    return src.IndexOf(patt) >= 0;
                }
            }
            else
            {
                return Regex.IsMatch(src, patt);
            }
        }
    }
    public enum TextSearchMethodEnum
    {
        Strict,
        And,
        Or
    }
}
