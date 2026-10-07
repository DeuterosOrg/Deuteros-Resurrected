using Godot;

namespace Deuteros.Code.UI.Rows;

[Tool]
public abstract partial class SettingRow : HBoxContainer
{
    [Signal]
    public delegate void ValueChangedEventHandler();

    private string _labelText = "Setting";
    private string _settingKey = "";
    private string _description = "";

    [Export]
    public string LabelText
    {
        get => _labelText;
        set
        {
            _labelText = value;
            if (IsNodeReady()) ApplyLabelText();
        }
    }

    [Export]
    public string SettingKey
    {
        get => _settingKey;
        set => _settingKey = value;
    }

    [Export(PropertyHint.MultilineText)]
    public string Description
    {
        get => _description;
        set => _description = value;
    }

    public Button LabelButton { get; private set; }

    public abstract Variant SettingValue { get; set; }

    public abstract string FormatValue(Variant value);

    public abstract bool StepValue(int direction);

    public override void _Ready()
    {
        LabelButton = GetNode<Button>("%LabelButton");

        ApplyLabelText();
    }

    protected void NotifyValueChanged()
    {
        EmitSignal(SignalName.ValueChanged);
    }

    private void ApplyLabelText()
    {
        LabelButton.Text = _labelText;
    }
}
