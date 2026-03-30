namespace HierarchicalNotes.Core.Interfaces;

public interface ISearchCriteria
{
    bool HasMatches(string textSource);
    bool HasMatches(string textSource, DateTime? itemDateTime);
}
