namespace YXBToolsMAUI.Services;

public interface IFolderPickerService
{
    Task<string?> PickFolderAsync();
}
