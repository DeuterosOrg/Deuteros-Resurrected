using Godot;

namespace Deuteros.Code.UI.Rows;

[Tool]
public partial class KeybindSettingRow : HBoxContainer
{
    private string _labelText = "Setting";
    private StringName _action = new StringName();
    private string _keyText = "Key";

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
    public StringName Action
    {
        get => _action;
        set => _action = value;
    }

    [Export]
    public string KeyText
    {
        get => _keyText;
        set
        {
            _keyText = value;
            if (IsNodeReady()) ApplyKeyText();
        }
    }

    public Button LabelButton { get; private set; }
    public Label KeyLabel { get; private set; }
    public Button RebindButton { get; private set; }

    public override void _Ready()
    {
        LabelButton = GetNode<Button>("%LabelButton");
        KeyLabel = GetNode<Label>("%KeyLabel");
        RebindButton = GetNode<Button>("%RebindButton");

        ApplyLabelText();
        ApplyKeyText();
    }

    private void ApplyLabelText()
    {
        LabelButton.Text = _labelText;
    }

    private void ApplyKeyText()
    {
        KeyLabel.Text = _keyText;
    }
}
