using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace SADeckManager;

public static class AppDialogs
{
    public static async Task ShowErrorAsync(Window owner, string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(
            "Error",
            message,
            ButtonEnum.Ok,
            Icon.Error
        );

        await box.ShowWindowDialogAsync(owner);
    }

    /// <returns><c>true</c> if user clicked Yes.</returns>
    public static async Task<bool> ShowYesNoAsync(Window owner, string title, string message)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(
            title,
            message,
            ButtonEnum.YesNo,
            Icon.Question
        );

        var result = await box.ShowWindowDialogAsync(owner);
        return result == ButtonResult.Yes;
    }
}