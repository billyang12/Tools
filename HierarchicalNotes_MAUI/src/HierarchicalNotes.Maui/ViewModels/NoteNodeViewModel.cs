using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HierarchicalNotes.Core.Models;

namespace HierarchicalNotes.Maui.ViewModels;

public sealed class NoteNodeViewModel : INotifyPropertyChanged
{
    private readonly MainViewModel _owner;
    private HNote _note;
    private NoteNodeViewModel? _parent;
    private bool _isExpanded;

    public event PropertyChangedEventHandler? PropertyChanged;

    internal NoteNodeViewModel(MainViewModel owner, HNote note, NoteNodeViewModel? parent = null)
    {
        _owner = owner;
        _note = note;
        _parent = parent;
        Children = new ObservableCollection<NoteNodeViewModel>();
        Children.CollectionChanged += ChildrenOnCollectionChanged;
        _isExpanded = parent is null;
    }

    public ObservableCollection<NoteNodeViewModel> Children { get; }

    public NoteNodeViewModel? Parent
    {
        get => _parent;
        internal set
        {
            if (_parent == value)
            {
                return;
            }

            _parent = value;
            NotifyHierarchyChanged();
        }
    }

    public HNote Note => _note;

    public string Name
    {
        get => _note.Name ?? string.Empty;
        set
        {
            if (value == (_note.Name ?? string.Empty))
            {
                return;
            }

            _note.Name = value;
            Touch();
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(DisplayTitle));
            OnPropertyChanged(nameof(DisplaySummary));
            OnPropertyChanged(nameof(Path));
            _owner.NotifyNodeEdited(this);
        }
    }

    public string Content
    {
        get => _note.Content ?? string.Empty;
        set
        {
            if (value == (_note.Content ?? string.Empty))
            {
                return;
            }

            _note.Content = value;
            Touch();
            OnPropertyChanged(nameof(Content));
            OnPropertyChanged(nameof(DisplaySummary));
            _owner.NotifyNodeEdited(this);
        }
    }

    public string Password
    {
        get => _note.Password ?? string.Empty;
        set
        {
            if (value == (_note.Password ?? string.Empty))
            {
                return;
            }

            _note.Password = value;
            Touch();
            OnPropertyChanged(nameof(Password));
            _owner.NotifyNodeEdited(this);
        }
    }

    public DateTime CreateTime
    {
        get => _note.CreateTime;
        set
        {
            if (_note.CreateTime == value)
            {
                return;
            }

            _note.CreateTime = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CreateTimeText));
        }
    }

    public DateTime UpdateTime
    {
        get => _note.UpdateTime;
        set
        {
            if (_note.UpdateTime == value)
            {
                return;
            }

            _note.UpdateTime = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(UpdateTimeText));
        }
    }

    public string CreateTimeText => CreateTime.ToString("yyyy-MM-dd HH:mm:ss");
    public string UpdateTimeText => UpdateTime.ToString("yyyy-MM-dd HH:mm:ss");

    public string DisplayTitle => _note.BriefName;
    public string DisplaySummary => _note.BriefSummary;
    public string Path => BuildPath();

    public int Depth => Parent?.Depth + 1 ?? 0;

    public Thickness Indent => new(Depth * 18, 0, 0, 0);

    public bool HasChildren => Children.Count > 0;

    public string ExpandGlyph => !HasChildren ? string.Empty : (IsExpanded ? "▾" : "▸");

    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded == value)
            {
                return;
            }

            _isExpanded = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(ExpandGlyph));
            _owner.NotifyTreeStructureChanged();
        }
    }

    public void ReplaceWith(HNote note)
    {
        _note = note;
        Children.Clear();
        if (note.SubNodes.Count > 0)
        {
            foreach (var child in note.SubNodes)
            {
                Children.Add(FromNote(_owner, child, this));
            }
        }

        Touch();
        NotifyAllLeafProperties();
        _owner.NotifyTreeStructureChanged();
    }

    public HNote ToNote()
    {
        var copy = _note.DeepClone();
        copy.SubNodes = Children.Select(x => x.ToNote()).ToList();
        return copy;
    }

    public static NoteNodeViewModel FromNote(MainViewModel owner, HNote note, NoteNodeViewModel? parent = null)
    {
        var vm = new NoteNodeViewModel(owner, note, parent);
        if (note.SubNodes.Count > 0)
        {
            foreach (var child in note.SubNodes)
            {
                vm.Children.Add(FromNote(owner, child, vm));
            }
        }

        vm.NotifyAllLeafProperties();
        return vm;
    }

    public void NotifyAllLeafProperties()
    {
        OnPropertyChanged(nameof(DisplayTitle));
        OnPropertyChanged(nameof(DisplaySummary));
        OnPropertyChanged(nameof(Path));
        OnPropertyChanged(nameof(Depth));
        OnPropertyChanged(nameof(Indent));
        OnPropertyChanged(nameof(HasChildren));
        OnPropertyChanged(nameof(ExpandGlyph));
        OnPropertyChanged(nameof(CreateTimeText));
        OnPropertyChanged(nameof(UpdateTimeText));
    }

    public void NotifyHierarchyChanged()
    {
        NotifyAllLeafProperties();
        foreach (var child in Children)
        {
            child.NotifyHierarchyChanged();
        }
    }

    private void Touch()
    {
        _note.UpdateTime = DateTime.Now;
        UpdateTime = _note.UpdateTime;
    }

    private string BuildPath()
    {
        var parts = new List<string>();
        NoteNodeViewModel? current = this;
        while (current != null)
        {
            parts.Add(current.DisplayTitle);
            current = current.Parent;
        }

        parts.Reverse();
        return string.Join("\\", parts);
    }

    private void ChildrenOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasChildren));
        OnPropertyChanged(nameof(ExpandGlyph));
        _owner.NotifyTreeStructureChanged();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
