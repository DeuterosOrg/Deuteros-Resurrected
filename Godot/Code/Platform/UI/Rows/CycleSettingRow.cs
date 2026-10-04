using Godot;

namespace Deuteros.Code.UI.Rows;

[Tool]
public partial class CycleSettingRow : SettingRow
{
    private string[] _options = { "Value" };
    private int _selectedIndex;

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

    public Button PrevButton { get; private set; }
    public Label ValueLabel { get; private set; }
    public Button NextButton { get; private set; }

    public override Variant SettingValue
    {
        get => SelectedOption;
        set => SelectedIndex = System.Array.IndexOf(_options, value.AsString());
    }

    private string SelectedOption => _selectedIndex >= 0 && _selectedIndex < _options.Length ? _options[_selectedIndex] : "";

    public override string FormatValue(Variant value)
    {
        return value.AsString();
    }

    public override bool StepValue(int direction)
    {
        if (_options.Length == 0) return false;
        StepSelection(direction);
        return true;
    }

    public override void _Ready()
    {
        base._Ready();

        PrevButton = GetNode<Button>("%PrevButton");
        ValueLabel = GetNode<Label>("%ValueLabel");
        NextButton = GetNode<Button>("%NextButton");

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
        NotifyValueChanged();
    }

    private void ApplyValue()
    {
        ValueLabel.Text = SelectedOption;
    }
}
