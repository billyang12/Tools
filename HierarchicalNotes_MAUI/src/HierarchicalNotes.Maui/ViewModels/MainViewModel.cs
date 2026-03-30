using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using CommunityToolkit.Maui.Storage;
using HierarchicalNotes.Core.Interfaces;
using HierarchicalNotes.Core.Models;
using HierarchicalNotes.Core.Services;
using HierarchicalNotes.Maui.Models;

namespace HierarchicalNotes.Maui.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private const string AppTitle = "Hierarchical Notes";

    private NoteNodeViewModel? _selectedNode;
    private string? _currentFileName;
    private HNoteFile _currentNoteFile = new();
    private string? _currentPassword;
    private string _previewHtml = HtmlExporter.ExportSingleNote(new HNote
    {
        Name = "Welcome",
        Content = "Create, open, or edit your hierarchical notes here.",
        CreateTime = DateTime.Now,
        UpdateTime = DateTime.Now
    });
    private string _contentSearchPattern = string.Empty;
    private readonly List<int> _contentSearchMatches = new();
    private int _contentSearchIndex = -1;
    private int _treeStructureNotificationSuspendCount;
    private bool _treeStructureChangedWhileSuspended;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<NoteNodeViewModel?>? SelectionRequested;
    public event Action<int, int>? ContentSelectionRequested;
    public event EventHandler? TreeStructureChanged;

    public MainViewModel()
    {
        RootNodes = new ObservableCollection<NoteNodeViewModel>();
        VisibleNodes = new ObservableCollection<NoteNodeViewModel>();
        SearchResults = new ObservableCollection<SearchResultItem>();

        ToggleExpandCommand = new Command<NoteNodeViewModel?>(ToggleExpand);
        SearchTreeCommand = new Command(ExecuteTreeSearch);
        SearchContentCommand = new Command(() => SearchCurrentContent(ContentSearchPattern));
        ContentFirstCommand = new Command(ContentFirst);
        ContentPreviousCommand = new Command(ContentPrevious);
        ContentNextCommand = new Command(ContentNext);
        ContentLastCommand = new Command(ContentLast);

        NewFile();
    }

    public ObservableCollection<NoteNodeViewModel> RootNodes { get; }
    public ObservableCollection<NoteNodeViewModel> VisibleNodes { get; }
    public ObservableCollection<SearchResultItem> SearchResults { get; }

    public Command<NoteNodeViewModel?> ToggleExpandCommand { get; }
    public Command SearchTreeCommand { get; }
    public Command SearchContentCommand { get; }
    public Command ContentFirstCommand { get; }
    public Command ContentPreviousCommand { get; }
    public Command ContentNextCommand { get; }
    public Command ContentLastCommand { get; }

    public string? CurrentFileName
    {
        get => _currentFileName;
        private set
        {
            if (_currentFileName == value)
            {
                return;
            }

            _currentFileName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(WindowTitle));
        }
    }

    public string WindowTitle
    {
        get
        {
            var suffix = string.IsNullOrWhiteSpace(CurrentFileName) ? string.Empty : " - " + Path.GetFileName(CurrentFileName);
            var dirty = IsDirty ? " *" : string.Empty;
            return AppTitle + suffix + dirty;
        }
    }

    public bool IsDirty => _currentNoteFile?.IsChangedSinceLastSave() == true;

    public NoteNodeViewModel? SelectedNode
    {
        get => _selectedNode;
        set
        {
            if (_selectedNode == value)
            {
                return;
            }

            _selectedNode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanEditSelectedNode));
            UpdatePreview();
            UpdateContentSearchForSelectedNode();
            SelectionRequested?.Invoke(this, value);
        }
    }

    public bool CanEditSelectedNode => SelectedNode != null;

    public HNoteFile CurrentNoteFile => _currentNoteFile;

    public string PreviewHtml
    {
        get => _previewHtml;
        private set
        {
            if (_previewHtml == value)
            {
                return;
            }

            _previewHtml = value;
            OnPropertyChanged();
        }
    }

    public string SearchPattern { get; set; } = string.Empty;
    public bool SearchOnlyCurrentNode { get; set; } = false;
    public bool SearchCaseSensitive { get; set; } = false;
    public bool SearchRegularExpression { get; set; } = false;
    public TextSearchMethodEnum SearchMethod { get; set; } = TextSearchMethodEnum.Strict;
    public bool SearchFromEnabled { get; set; } = true;
    public DateTime SearchFromDate { get; set; } = DateTime.Now.AddYears(-1);
    public bool SearchToEnabled { get; set; } = true;
    public DateTime SearchToDate { get; set; } = DateTime.Now;

    public string ContentSearchPattern
    {
        get => _contentSearchPattern;
        set
        {
            if (_contentSearchPattern == value)
            {
                return;
            }

            _contentSearchPattern = value;
            OnPropertyChanged();
        }
    }

    public string ContentSearchStatus { get; private set; } = "Enter a search pattern to find matches.";
    public Array SearchMethods { get; } = Enum.GetValues<TextSearchMethodEnum>();

    public void NewFile()
    {
        _currentNoteFile = new HNoteFile
        {
            IsEncrypted = false,
            Notes = new HNoteCollection()
        };

        _currentNoteFile.Notes.RootNodes.Clear();

        var note = new HNote
        {
            Name = "Note 1",
            Content = "This is the first note",
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now,
            SubNodes = new List<HNote>()
        };

        LoadFromNotes(new HNoteCollection { RootNodes = { note } }, null, null, false);
        _currentNoteFile.SaveLatestNotesJson();
    }

    public void LoadFromNotes(HNoteCollection notes, string? fileName, string? currentPassword, bool encrypted)
    {
        using (SuspendTreeStructureNotifications())
        {
            _currentPassword = currentPassword;
            _currentNoteFile = new HNoteFile
            {
                IsEncrypted = encrypted,
                Notes = encrypted ? null : notes,
                HNoteJsonStr = encrypted ? JsonSerializer.Serialize(notes, new JsonSerializerOptions { WriteIndented = true }) : null
            };

            RootNodes.Clear();
            foreach (var root in notes.RootNodes)
            {
                RootNodes.Add(NoteNodeViewModel.FromNote(this, root, null));
            }

            CurrentFileName = fileName;
            _selectedNode = null;
            SelectedNode = RootNodes.FirstOrDefault();
            NotifyTreeStructureChanged();
        }

        _currentNoteFile.SaveLatestNotesJson();
    }

    public string BuildCurrentFileJson()
    {
        GatherCurrentNoteFileBeforeSave();
        return JsonSerializer.Serialize(_currentNoteFile, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    public HNoteCollection BuildCurrentNoteCollection()
    {
        return new HNoteCollection
        {
            RootNodes = RootNodes.Select(x => x.ToNote()).ToList()
        };
    }

    public void GatherCurrentNoteFileBeforeSave()
    {
        var notes = BuildCurrentNoteCollection();
        if (_currentNoteFile == null)
        {
            _currentNoteFile = new HNoteFile();
        }

        if (!_currentNoteFile.IsEncrypted)
        {
            _currentNoteFile.Notes = notes;
            _currentNoteFile.HNoteJsonStr = null;
        }
        else
        {
            _currentNoteFile.Notes = null;
            _currentNoteFile.HNoteJsonStr = StringCipher.Encrypt(JsonSerializer.Serialize(notes), CurrentPasswordForEncryption);
        }
    }

    public string? CurrentPasswordForEncryption
    {
        get => _currentPassword;
        private set => _currentPassword = value;
    }

    public void SetCurrentPassword(string? password)
    {
        CurrentPasswordForEncryption = password;
    }

    public void SetCurrentFileName(string? fileName)
    {
        CurrentFileName = fileName;
    }

    public void NotifyNodeEdited(NoteNodeViewModel node)
    {
        if (node == SelectedNode)
        {
            UpdatePreview();
            UpdateContentSearchForSelectedNode();
        }

        OnPropertyChanged(nameof(WindowTitle));
    }

    public void NotifyTreeStructureChanged()
    {
        if (_treeStructureNotificationSuspendCount > 0)
        {
            _treeStructureChangedWhileSuspended = true;
            return;
        }

        ApplyTreeStructureChanged();
    }

    internal IDisposable SuspendTreeStructureNotifications()
    {
        _treeStructureNotificationSuspendCount++;
        return new TreeStructureNotificationScope(this);
    }

    private void ResumeTreeStructureNotifications()
    {
        if (_treeStructureNotificationSuspendCount == 0)
        {
            return;
        }

        _treeStructureNotificationSuspendCount--;
        if (_treeStructureNotificationSuspendCount == 0 && _treeStructureChangedWhileSuspended)
        {
            _treeStructureChangedWhileSuspended = false;
            ApplyTreeStructureChanged();
        }
    }

    private void ApplyTreeStructureChanged()
    {
        RefreshVisibleNodes();
        UpdatePreview();
        OnPropertyChanged(nameof(WindowTitle));
        TreeStructureChanged?.Invoke(this, EventArgs.Empty);
    }

    public void RefreshVisibleNodes()
    {
        var currentSelection = SelectedNode;
        VisibleNodes.Clear();
        foreach (var node in RootNodes)
        {
            AddVisible(node);
        }

        if (currentSelection != null)
        {
            // preserve selection reference if it is still in the tree
            _selectedNode = currentSelection;
            OnPropertyChanged(nameof(SelectedNode));
        }
    }

    private void AddVisible(NoteNodeViewModel node)
    {
        node.NotifyHierarchyChanged();
        VisibleNodes.Add(node);
        if (!node.IsExpanded)
        {
            return;
        }

        foreach (var child in node.Children)
        {
            AddVisible(child);
        }
    }

    public void ToggleExpand(NoteNodeViewModel? node)
    {
        if (node == null || !node.HasChildren)
        {
            return;
        }

        node.IsExpanded = !node.IsExpanded;
    }

    public void SelectNode(NoteNodeViewModel? node)
    {
        if (node == null)
        {
            SelectedNode = null;
            return;
        }

        var parent = node.Parent;
        while (parent != null)
        {
            parent.IsExpanded = true;
            parent = parent.Parent;
        }

        SelectedNode = node;
    }

    public void AddNewNode()
    {
        AddSibling(new HNote { CreateTime = DateTime.Now, UpdateTime = DateTime.Now }, insertBefore: false);
    }

    public void AddNewNodeBefore()
    {
        AddSibling(new HNote { CreateTime = DateTime.Now, UpdateTime = DateTime.Now }, insertBefore: true);
    }

    public void AddNewNodeAfter()
    {
        AddSibling(new HNote { CreateTime = DateTime.Now, UpdateTime = DateTime.Now }, insertBefore: false);
    }

    public void AddNewChildNode()
    {
        var newNode = CreateNode(new HNote
        {
            CreateTime = DateTime.Now,
            UpdateTime = DateTime.Now
        }, SelectedNode);

        if (SelectedNode == null)
        {
            RootNodes.Add(newNode);
        }
        else
        {
            SelectedNode.Children.Add(newNode);
            newNode.Parent = SelectedNode;
            SelectedNode.IsExpanded = true;
        }

        SelectAndRefresh(newNode);
    }

    public void DuplicateSelectedNode()
    {
        if (SelectedNode == null)
        {
            return;
        }

        var parentCollection = GetSiblingCollection(SelectedNode);
        var index = parentCollection.IndexOf(SelectedNode);
        var duplicate = NoteNodeViewModel.FromNote(this, SelectedNode.ToNote(), SelectedNode.Parent);
        parentCollection.Insert(index + 1, duplicate);
        SelectAndRefresh(duplicate);
    }

    public void ReverseChildrenOfSelectedNode()
    {
        if (SelectedNode == null || SelectedNode.Children.Count == 0)
        {
            return;
        }

        var items = SelectedNode.Children.ToList();
        SelectedNode.Children.Clear();
        foreach (var item in items.AsEnumerable().Reverse())
        {
            item.Parent = SelectedNode;
            SelectedNode.Children.Add(item);
        }
        NotifyTreeStructureChanged();
    }

    public bool RemoveSelectedNode()
    {
        if (SelectedNode == null)
        {
            return false;
        }

        var parentCollection = GetSiblingCollection(SelectedNode);
        var index = parentCollection.IndexOf(SelectedNode);
        if (index < 0)
        {
            return false;
        }

        NoteNodeViewModel? nextSelection = null;
        if (index > 0)
        {
            nextSelection = parentCollection[index - 1];
        }
        else if (index < parentCollection.Count - 1)
        {
            nextSelection = parentCollection[index + 1];
        }
        else
        {
            nextSelection = SelectedNode.Parent ?? parentCollection.FirstOrDefault() ?? RootNodes.FirstOrDefault();
        }

        parentCollection.RemoveAt(index);
        SelectedNode = nextSelection;

        NotifyTreeStructureChanged();
        return true;
    }

    public void MoveSelectedUp()
    {
        if (SelectedNode == null)
        {
            return;
        }

        var siblings = GetSiblingCollection(SelectedNode);
        var index = siblings.IndexOf(SelectedNode);
        if (index <= 0)
        {
            return;
        }

        siblings.Move(index, index - 1);
        NotifyTreeStructureChanged();
    }

    public void MoveSelectedDown()
    {
        if (SelectedNode == null)
        {
            return;
        }

        var siblings = GetSiblingCollection(SelectedNode);
        var index = siblings.IndexOf(SelectedNode);
        if (index < 0 || index >= siblings.Count - 1)
        {
            return;
        }

        siblings.Move(index, index + 1);
        NotifyTreeStructureChanged();
    }

    public void IndentSelected()
    {
        if (SelectedNode == null)
        {
            return;
        }

        var siblings = GetSiblingCollection(SelectedNode);
        var index = siblings.IndexOf(SelectedNode);
        if (index <= 0)
        {
            return;
        }

        var newParent = siblings[index - 1];
        siblings.RemoveAt(index);
        SelectedNode.Parent = newParent;
        newParent.Children.Add(SelectedNode);
        newParent.IsExpanded = true;
        NotifyTreeStructureChanged();
    }

    public void OutdentSelected()
    {
        if (SelectedNode == null || SelectedNode.Parent == null)
        {
            return;
        }

        var currentParent = SelectedNode.Parent;
        var grandParentCollection = currentParent.Parent == null ? RootNodes : currentParent.Parent.Children;
        currentParent.Children.Remove(SelectedNode);
        var parentIndex = grandParentCollection.IndexOf(currentParent);
        if (parentIndex < 0)
        {
            grandParentCollection.Add(SelectedNode);
        }
        else
        {
            grandParentCollection.Insert(parentIndex + 1, SelectedNode);
        }
        SelectedNode.Parent = currentParent.Parent;
        NotifyTreeStructureChanged();
    }

    public string? GetSelectedNodeJson()
    {
        if (SelectedNode == null)
        {
            return null;
        }

        return JsonSerializer.Serialize(SelectedNode.ToNote(), new JsonSerializerOptions { WriteIndented = true });
    }

    public bool PasteNodeFromJson(string json, bool asChild)
    {
        if (SelectedNode == null)
        {
            return false;
        }

        var note = JsonSerializer.Deserialize<HNote>(json);
        if (note == null)
        {
            return false;
        }

        if (asChild)
        {
            var child = NoteNodeViewModel.FromNote(this, note, SelectedNode);
            SelectedNode.Children.Add(child);
            SelectedNode.IsExpanded = true;
            SelectAndRefresh(child);
        }
        else
        {
            SelectedNode.ReplaceWith(note);
            SelectedNode.NotifyHierarchyChanged();
            UpdatePreview();
        }

        NotifyTreeStructureChanged();
        return true;
    }

    public bool PasteNodeAsChildFromJson(string json)
    {
        return PasteNodeFromJson(json, asChild: true);
    }

    public void SearchCurrentContent(string pattern)
    {
        ContentSearchPattern = pattern ?? string.Empty;
        _contentSearchMatches.Clear();
        _contentSearchIndex = -1;

        if (SelectedNode == null || string.IsNullOrWhiteSpace(ContentSearchPattern))
        {
            ContentSearchStatus = "Enter a search pattern to find matches.";
            OnPropertyChanged(nameof(ContentSearchStatus));
            return;
        }

        var text = SelectedNode.Content ?? string.Empty;
        var start = 0;
        while (start < text.Length)
        {
            var index = text.IndexOf(ContentSearchPattern, start, StringComparison.CurrentCultureIgnoreCase);
            if (index < 0)
            {
                break;
            }

            _contentSearchMatches.Add(index);
            start = index + Math.Max(1, ContentSearchPattern.Length);
        }

        if (_contentSearchMatches.Count == 0)
        {
            ContentSearchStatus = "No matches found.";
            OnPropertyChanged(nameof(ContentSearchStatus));
            return;
        }

        _contentSearchIndex = 0;
        ContentSearchStatus = $"Total found: {_contentSearchMatches.Count}, current: 1";
        OnPropertyChanged(nameof(ContentSearchStatus));
        OnPropertyChanged(nameof(ContentSearchPattern));
        ContentSelectionRequested?.Invoke(_contentSearchMatches[_contentSearchIndex], ContentSearchPattern.Length);
    }

    public void ContentFirst()
    {
        if (_contentSearchMatches.Count == 0)
        {
            return;
        }

        _contentSearchIndex = 0;
        RaiseContentSelection();
    }

    public void ContentPrevious()
    {
        if (_contentSearchMatches.Count == 0)
        {
            return;
        }

        _contentSearchIndex = _contentSearchIndex <= 0 ? _contentSearchMatches.Count - 1 : _contentSearchIndex - 1;
        RaiseContentSelection();
    }

    public void ContentNext()
    {
        if (_contentSearchMatches.Count == 0)
        {
            return;
        }

        _contentSearchIndex = _contentSearchIndex >= _contentSearchMatches.Count - 1 ? 0 : _contentSearchIndex + 1;
        RaiseContentSelection();
    }

    public void ContentLast()
    {
        if (_contentSearchMatches.Count == 0)
        {
            return;
        }

        _contentSearchIndex = _contentSearchMatches.Count - 1;
        RaiseContentSelection();
    }

    public void ExecuteTreeSearch()
    {
        SearchResults.Clear();

        var criteria = BuildSearchCriteria();

        var nodes = SearchOnlyCurrentNode && SelectedNode != null
            ? SelectedNode.Children
            : RootNodes;

        SearchTree(nodes, criteria, SearchResults);
        OnPropertyChanged(nameof(SearchResults));
    }

    public SearchCriteria BuildSearchCriteria()
    {
        var criteria = new SearchCriteria
        {
            Pattern = SearchPattern,
            IsCaseSensitive = SearchCaseSensitive,
            IsRegularExpression = SearchRegularExpression,
            SearchMethod = SearchMethod,
            SearchFromDateTime = SearchFromEnabled ? SearchFromDate.Date : null,
            SearchToDateTime = SearchToEnabled ? SearchToDate.Date.AddDays(1).AddTicks(-1) : null
        };

        return criteria;
    }

    public string ExportHtml() => HtmlExporter.ExportToHtml(RootNodes.Select(x => x.ToNote()));

    public async Task<string?> ReadClipboardTextAsync()
    {
        return await Clipboard.Default.GetTextAsync();
    }

    public async Task SetClipboardTextAsync(string text)
    {
        await Clipboard.Default.SetTextAsync(text);
    }

    private void SearchTree(IEnumerable<NoteNodeViewModel> nodes, ISearchCriteria criteria, ObservableCollection<SearchResultItem> results)
    {
        foreach (var node in nodes)
        {
            var note = node.Note;
            if (criteria.HasMatches(note.Name ?? string.Empty, note.UpdateTime) || criteria.HasMatches(note.Content ?? string.Empty, note.UpdateTime))
            {
                results.Add(new SearchResultItem { Node = node });
            }

            if (node.Children.Count > 0)
            {
                SearchTree(node.Children, criteria, results);
            }
        }
    }

    private void RaiseContentSelection()
    {
        if (_contentSearchIndex < 0 || _contentSearchIndex >= _contentSearchMatches.Count)
        {
            return;
        }

        ContentSearchStatus = $"Total found: {_contentSearchMatches.Count}, current: {_contentSearchIndex + 1}";
        OnPropertyChanged(nameof(ContentSearchStatus));
        ContentSelectionRequested?.Invoke(_contentSearchMatches[_contentSearchIndex], ContentSearchPattern.Length);
    }

    private void UpdateContentSearchForSelectedNode()
    {
        if (string.IsNullOrWhiteSpace(ContentSearchPattern))
        {
            ContentSearchStatus = "Enter a search pattern to find matches.";
            OnPropertyChanged(nameof(ContentSearchStatus));
            return;
        }

        SearchCurrentContent(ContentSearchPattern);
    }

    private void UpdatePreview()
    {
        PreviewHtml = SelectedNode == null
            ? HtmlExporter.ExportSingleNote(new HNote { Name = "No selection", Content = "Select a note to preview it.", CreateTime = DateTime.Now, UpdateTime = DateTime.Now })
            : HtmlExporter.ExportSingleNote(SelectedNode.ToNote());
    }

    private void AddSibling(HNote note, bool insertBefore)
    {
        var newNode = CreateNode(note, SelectedNode?.Parent);
        if (SelectedNode == null)
        {
            RootNodes.Add(newNode);
            SelectAndRefresh(newNode);
            return;
        }

        var siblings = GetSiblingCollection(SelectedNode);
        var index = siblings.IndexOf(SelectedNode);
        if (insertBefore)
        {
            siblings.Insert(Math.Max(0, index), newNode);
        }
        else
        {
            siblings.Insert(index < 0 ? siblings.Count : index + 1, newNode);
        }

        newNode.Parent = SelectedNode.Parent;
        SelectAndRefresh(newNode);
    }

    private NoteNodeViewModel CreateNode(HNote note, NoteNodeViewModel? parent)
    {
        note.SubNodes ??= new List<HNote>();
        note.CreateTime = note.CreateTime == default ? DateTime.Now : note.CreateTime;
        note.UpdateTime = note.UpdateTime == default ? DateTime.Now : note.UpdateTime;
        var node = NoteNodeViewModel.FromNote(this, note, parent);
        return node;
    }

    private ObservableCollection<NoteNodeViewModel> GetSiblingCollection(NoteNodeViewModel node)
    {
        return node.Parent?.Children ?? RootNodes;
    }

    private void SelectAndRefresh(NoteNodeViewModel node)
    {
        node.IsExpanded = true;
        var parent = node.Parent;
        while (parent != null)
        {
            parent.IsExpanded = true;
            parent = parent.Parent;
        }

        SelectNode(node);
        NotifyTreeStructureChanged();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private sealed class TreeStructureNotificationScope(MainViewModel owner) : IDisposable
    {
        private MainViewModel? _owner = owner;

        public void Dispose()
        {
            var owner = _owner;
            if (owner == null)
            {
                return;
            }

            _owner = null;
            owner.ResumeTreeStructureNotifications();
        }
    }
}
