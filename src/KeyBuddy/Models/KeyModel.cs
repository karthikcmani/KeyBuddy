namespace KeyBuddy.Models;

public enum KeyType
{
    Standard,
    Modifier,
    Lock,
    Action,
    Navigation,
    Numpad,
    Function
}

public class KeyModel
{
    public string Id { get; set; } = string.Empty;
    public string DisplayPrimary { get; set; } = string.Empty;
    public string? DisplaySecondary { get; set; }
    public ushort VirtualKey { get; set; }
    public ushort? ShiftVirtualKey { get; set; }
    public ushort ScanCode { get; set; }
    public bool IsExtended { get; set; }
    public KeyType KeyType { get; set; } = KeyType.Standard;
    public double WidthUnits { get; set; } = 1.0;
    public string? ToolTip { get; set; }
    public bool CanRepeat { get; set; } = true;
    public string? SpecialAction { get; set; }

    public static KeyModel Create(
        string id,
        string primary,
        ushort vk,
        double width = 1.0,
        KeyType type = KeyType.Standard,
        string? secondary = null,
        bool isExtended = false,
        bool canRepeat = true,
        string? toolTip = null,
        string? specialAction = null)
    {
        return new KeyModel
        {
            Id = id,
            DisplayPrimary = primary,
            DisplaySecondary = secondary,
            VirtualKey = vk,
            KeyType = type,
            WidthUnits = width,
            IsExtended = isExtended,
            CanRepeat = canRepeat,
            ToolTip = toolTip ?? primary,
            SpecialAction = specialAction
        };
    }
}
