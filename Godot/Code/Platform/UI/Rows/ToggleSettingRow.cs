using Godot;

namespace Deuteros.Code.UI.Rows;

[Tool]
public partial class ToggleSettingRow : HBoxContainer
{
    private string _labelText = "Setting";
    private bool _value;

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
    public bool Value
    {
        get => _value;
        set
        {
            _value = value;
            if (IsNodeReady()) ApplyValue();
        }
    }

    public Button LabelButton { get; private set; }
    public Button OffButton { get; private set; }
    public Button OnButton { get; private set; }

    public override void _Ready()
    {
        LabelButton = GetNode<Button>("%LabelButton");
        OffButton = GetNode<Button>("%OffButton");
        OnButton = GetNode<Button>("%OnButton");

        ApplyLabelText();
        ApplyValue();

        if (Engine.IsEditorHint()) return;
        OffButton.Toggled += OnOffButtonToggled;
        OnButton.Toggled += OnOnButtonToggled;
    }

    private void OnOffButtonToggled(bool toggledOn)
    {
        if (toggledOn) _value = false;
    }

    private void OnOnButtonToggled(bool toggledOn)
    {
        if (toggledOn) _value = true;
    }

    private void ApplyLabelText()
    {
        LabelButton.Text = _labelText;
    }

    private void ApplyValue()
    {
        OffButton.SetPressedNoSignal(!_value);
        OnButton.SetPressedNoSignal(_value);
    }
}
