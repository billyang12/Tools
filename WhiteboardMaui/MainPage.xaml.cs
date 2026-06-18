using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Shapes;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System;
using System.IO;

namespace WhiteboardMaui;

public partial class MainPage : ContentPage
{
    WhiteboardDrawable drawable;

    public MainPage()
    {
        InitializeComponent();

        drawable = new WhiteboardDrawable();
        Canvas.Drawable = drawable;

        // handle gesture on the graphics view - simplified for .NET 10
        // Touch handling will be implemented via GraphicsView interactions
        // For now, drawing happens through button interactions

        // update properties when UI changes
        ToolPicker.SelectedIndexChanged += (s, e) => {
            drawable.CurrentTool = (Tool)ToolPicker.SelectedIndex;
        };
        LineWidthSlider.ValueChanged += (s, e) => {
            drawable.LineWidth = (float)e.NewValue;
        };

        ToolPicker.SelectedIndex = 0;
        ColorPicker.SelectedIndex = 0;
        BgPicker.SelectedIndex = 0;
    }

    // Touch/gesture handling can be added in future versions
    // GraphicsView in .NET 10 MAUI uses different input handling


    private void NewClicked(object sender, EventArgs e)
    {
        drawable.Clear(Microsoft.Maui.Graphics.Colors.White);
        Canvas.Invalidate();
    }

    private async void LoadImageClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Select image", FileTypes = FilePickerFileType.Images });
            if (result == null) return;
            using var stream = await result.OpenReadAsync();
            var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            drawable.LoadImage(ms.ToArray());
            Canvas.Invalidate();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OpenClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Open image", FileTypes = FilePickerFileType.Images });
            if (result == null) return;
            using var stream = await result.OpenReadAsync();
            var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            drawable.LoadFromImage(ms.ToArray());
            Canvas.Invalidate();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void SaveClicked(object sender, EventArgs e)
    {
        try
        {
            var img = drawable.Rasterize((int)Canvas.Width, (int)Canvas.Height);
            if (img == null)
            {
                await DisplayAlert("Info", "Image rasterization not yet fully supported", "OK");
                return;
            }
            // Note: In .NET 10 MAUI, image saving would require platform-specific implementations
            // var bytes = img.AsPng();
            // await FileSaver.Default.SaveAsync("whiteboard.png", new MemoryStream(bytes));
            await DisplayAlert("Info", "Image saving requires platform-specific implementation", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void SaveAsClicked(object sender, EventArgs e)
    {
        SaveClicked(sender, e);
    }

    private void ClearClicked(object sender, EventArgs e)
    {
        if (BgPicker.SelectedIndex == 1)
            drawable.Clear(Colors.Transparent);
        else
            drawable.Clear(Colors.White);
        Canvas.Invalidate();
    }

    private void ColorPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        switch (ColorPicker.SelectedIndex)
        {
            case 0: drawable.CurrentColor = Colors.Black; break;
            case 1: drawable.CurrentColor = Colors.Red; break;
            case 2: drawable.CurrentColor = Colors.Blue; break;
            case 3: drawable.CurrentColor = Colors.Green; break;
        }
    }

    private void BgPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        switch (BgPicker.SelectedIndex)
        {
            case 0: drawable.Background = Colors.White; break;
            case 1: drawable.Background = Colors.Transparent; break;
        }
        Canvas.Invalidate();
    }
}
