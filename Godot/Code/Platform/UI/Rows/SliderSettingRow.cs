using Godot;

namespace Deuteros.Code.UI.Rows;

[Tool]
public partial class SliderSettingRow : HBoxContainer
{
    private string _labelText = "Setting";
    private int _value;
    private int _maxValue = 10;

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
    public int Value
    {
        get => _value;
        set
        {
            _value = value;
            if (IsNodeReady()) ApplyValue();
        }
    }

    [Export]
    public int MaxValue
    {
        get => _maxValue;
        set
        {
            _maxValue = value;
            if (!IsNodeReady()) return;
            ApplyMaxValue();
            ApplyValue();
        }
    }

    public Button LabelButton { get; private set; }
    public Button DecButton { get; private set; }
    public HSlider ValueSlider { get; private set; }
    public Button IncButton { get; private set; }

    public override void _Ready()
    {
        LabelButton = GetNode<Button>("%LabelButton");
        DecButton = GetNode<Button>("%DecButton");
        ValueSlider = GetNode<HSlider>("%ValueSlider");
        IncButton = GetNode<Button>("%IncButton");

        ApplyLabelText();
        ApplyMaxValue();
        ApplyValue();

        if (Engine.IsEditorHint()) return;
        ValueSlider.ValueChanged += OnValueSliderValueChanged;
    }

    private void OnValueSliderValueChanged(double value)
    {
        _value = Mathf.RoundToInt(value);
    }

    private void ApplyLabelText()
    {
        LabelButton.Text = _labelText;
    }

    private void ApplyMaxValue()
    {
        ValueSlider.MaxValue = _maxValue;
        ValueSlider.TickCount = _maxValue + 1;
    }

    private void ApplyValue()
    {
        ValueSlider.SetValueNoSignal(_value);
    }
}
