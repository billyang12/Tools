namespace YXBToolsMAUI.Services;

public interface IFilePickerService
{
    Task<string?> PickFileAsync(string? title = null);
}
