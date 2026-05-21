namespace DotNet8.EasyTrip.App.Client.Services;

public class DrawerStateService
{
    public bool IsOpen { get; private set; } = true;

    public event Action? OnChange;

    public void Toggle()
    {
        IsOpen = !IsOpen;
        OnChange?.Invoke();
    }

    public void SetOpen(bool open)
    {
        if (IsOpen == open) return;
        IsOpen = open;
        OnChange?.Invoke();
    }
}
