namespace KeyBuddy.Models;

public class KeyMappingRule
{
    public string TriggerKey { get; set; } = string.Empty; // e.g. "O", "2", "A"
    public string OutputKey { get; set; } = string.Empty;  // e.g. "P", "4", "@"
    public bool IsEnabled { get; set; } = true;
    public string Description { get; set; } = string.Empty;

    public KeyMappingRule() { }

    public KeyMappingRule(string trigger, string output, string description = "")
    {
        TriggerKey = trigger.ToUpperInvariant();
        OutputKey = output;
        Description = string.IsNullOrWhiteSpace(description) ? $"Hold '{TriggerKey}' to type '{OutputKey}'" : description;
    }
}
