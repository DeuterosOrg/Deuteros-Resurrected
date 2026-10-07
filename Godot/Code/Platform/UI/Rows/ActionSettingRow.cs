using Godot;

namespace Deuteros.Code.UI.Rows;

[Tool]
public partial class ActionSettingRow : SettingRow
{
    private string _actionText = "Run";

    [Export]
    public string ActionText
    {
        get => _actionText;
        set
        {
            _actionText = value;
            if (IsNodeReady()) ApplyActionText();
        }
    }

    public Button ActionButton { get; private set; }

    public override Variant SettingValue
    {
        get => "";
        set { }
    }

    public override string FormatValue(Variant value)
    {
        return "";
    }

    public override bool StepValue(int direction)
    {
        return false;
    }

    public override void _Ready()
    {
        base._Ready();

        ActionButton = GetNode<Button>("%ActionButton");

        ApplyActionText();
    }

    private void ApplyActionText()
    {
        ActionButton.Text = _actionText;
    }
}
