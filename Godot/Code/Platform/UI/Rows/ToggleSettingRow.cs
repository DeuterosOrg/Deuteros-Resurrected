using Godot;

namespace Deuteros.Code.UI.Rows;

[Tool]
public partial class ToggleSettingRow : SettingRow
{
    private bool _value;

    [Export]
    public bool Value
    {
        get => _value;
        set
        {
            _value = value;
            if (IsNodeReady()) ApplyValue();
        }
    }

    public Button OffButton { get; private set; }
    public Button OnButton { get; private set; }

    public override Variant SettingValue
    {
        get => _value;
        set => Value = value.AsBool();
    }

    public override string FormatValue(Variant value)
    {
        return value.AsBool() ? OnButton.Text : OffButton.Text;
    }

    public override bool StepValue(int direction)
    {
        var value = direction > 0;
        if (value == _value) return true;
        Value = value;
        NotifyValueChanged();
        return true;
    }

    public override void _Ready()
    {
        base._Ready();

        OffButton = GetNode<Button>("%OffButton");
        OnButton = GetNode<Button>("%OnButton");

        ApplyValue();

        if (Engine.IsEditorHint()) return;
        OffButton.Toggled += OnOffButtonToggled;
        OnButton.Toggled += OnOnButtonToggled;
    }

    private void OnOffButtonToggled(bool toggledOn)
    {
        if (!toggledOn) return;
        _value = false;
        NotifyValueChanged();
    }

    private void OnOnButtonToggled(bool toggledOn)
    {
        if (!toggledOn) return;
        _value = true;
        NotifyValueChanged();
    }

    private void ApplyValue()
    {
        OffButton.SetPressedNoSignal(!_value);
        OnButton.SetPressedNoSignal(_value);
    }
}
