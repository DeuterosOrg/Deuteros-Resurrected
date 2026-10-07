using Godot;

namespace Deuteros.Code.UI.Rows;

[Tool]
public partial class SliderSettingRow : SettingRow
{
    private int _value;
    private int _maxValue = 10;

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

    public Button DecButton { get; private set; }
    public HSlider ValueSlider { get; private set; }
    public Button IncButton { get; private set; }

    public override Variant SettingValue
    {
        get => _value;
        set => Value = value.AsInt32();
    }

    public override string FormatValue(Variant value)
    {
        return value.AsInt32().ToString();
    }

    public override bool StepValue(int direction)
    {
        ValueSlider.Value += ValueSlider.Step * direction;
        return true;
    }

    public override void _Ready()
    {
        base._Ready();

        DecButton = GetNode<Button>("%DecButton");
        ValueSlider = GetNode<HSlider>("%ValueSlider");
        IncButton = GetNode<Button>("%IncButton");

        ApplyMaxValue();
        ApplyValue();

        if (Engine.IsEditorHint()) return;
        ValueSlider.ValueChanged += OnValueSliderValueChanged;
        DecButton.Pressed += OnDecButtonPressed;
        IncButton.Pressed += OnIncButtonPressed;
    }

    private void OnValueSliderValueChanged(double value)
    {
        _value = Mathf.RoundToInt(value);
        NotifyValueChanged();
    }

    private void OnDecButtonPressed()
    {
        ValueSlider.Value -= ValueSlider.Step;
    }

    private void OnIncButtonPressed()
    {
        ValueSlider.Value += ValueSlider.Step;
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
