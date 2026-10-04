using Godot;

namespace Deuteros.Code.UI.Rows;

[Tool]
public partial class CycleSettingRow : HBoxContainer
{
    private string _labelText = "Setting";
    private string[] _options = { "Value" };
    private int _selectedIndex;

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
    public string[] Options
    {
        get => _options;
        set
        {
            _options = value ?? System.Array.Empty<string>();
            if (IsNodeReady()) ApplyValue();
        }
    }

    [Export]
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            _selectedIndex = value;
            if (IsNodeReady()) ApplyValue();
        }
    }

    public Button LabelButton { get; private set; }
    public Button PrevButton { get; private set; }
    public Label ValueLabel { get; private set; }
    public Button NextButton { get; private set; }

    public override void _Ready()
    {
        LabelButton = GetNode<Button>("%LabelButton");
        PrevButton = GetNode<Button>("%PrevButton");
        ValueLabel = GetNode<Label>("%ValueLabel");
        NextButton = GetNode<Button>("%NextButton");

        ApplyLabelText();
        ApplyValue();

        if (Engine.IsEditorHint()) return;
        PrevButton.Pressed += OnPrevButtonPressed;
        NextButton.Pressed += OnNextButtonPressed;
    }

    private void OnPrevButtonPressed()
    {
        StepSelection(-1);
    }

    private void OnNextButtonPressed()
    {
        StepSelection(1);
    }

    private void StepSelection(int step)
    {
        if (_options.Length == 0) return;
        SelectedIndex = ((_selectedIndex + step) % _options.Length + _options.Length) % _options.Length;
    }

    private void ApplyLabelText()
    {
        LabelButton.Text = _labelText;
    }

    private void ApplyValue()
    {
        ValueLabel.Text = _selectedIndex >= 0 && _selectedIndex < _options.Length ? _options[_selectedIndex] : "";
    }
}
