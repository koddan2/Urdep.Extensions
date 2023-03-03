namespace TrackingCopyTool.Config;

internal class TargetElement
{
    public TargetElement(string name)
    {
        Name = name;
    }

    public string Name { get; init; }
    public bool Create { get; init; } = false;
}
