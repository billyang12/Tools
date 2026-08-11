using YXBToolsMAUI.Services;

namespace YXBToolsMAUI.Platforms;

public class DefaultFolderPickerService : IFolderPickerService
{
    public Task<string?> PickFolderAsync()
    {
        return Task.FromResult<string?>(null);
    }
}
