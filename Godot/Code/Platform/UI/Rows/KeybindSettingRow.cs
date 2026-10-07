using Godot;

namespace Deuteros.Code.UI.Rows;

[Tool]
public partial class KeybindSettingRow : SettingRow
{
    private static KeybindSettingRow _listeningRow;

    private StringName _action = new StringName();
    private string _keyText = "Key";
    private Key _key = Key.None;

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

    public Label KeyLabel { get; private set; }
    public Button RebindButton { get; private set; }

    public override Variant SettingValue
    {
        get => (long)_key;
        set
        {
            _key = (Key)value.AsInt64();
            KeyText = FormatValue(value);
        }
    }

    public override string FormatValue(Variant value)
    {
        var key = (Key)value.AsInt64();
        return key == Key.None ? "" : OS.GetKeycodeString(key);
    }

    public override bool StepValue(int direction)
    {
        return false;
    }

    public override void _Ready()
    {
        base._Ready();

        KeyLabel = GetNode<Label>("%KeyLabel");
        RebindButton = GetNode<Button>("%RebindButton");

        ApplyKeyText();
        SetProcessInput(false);

        if (Engine.IsEditorHint()) return;
        RebindButton.Toggled += OnRebindButtonToggled;
    }

    public override void _ExitTree()
    {
        if (_listeningRow == this)
            _listeningRow = null;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey keyEvent || !keyEvent.Pressed || keyEvent.Echo) return;

        GetViewport().SetInputAsHandled();

        if (keyEvent.IsActionPressed("ui_cancel"))
        {
            StopListening();
            return;
        }

        _key = keyEvent.Keycode != Key.None ? keyEvent.Keycode : keyEvent.PhysicalKeycode;
        _keyText = FormatValue((long)_key);
        StopListening();
        NotifyValueChanged();
    }

    private void OnRebindButtonToggled(bool toggledOn)
    {
        if (toggledOn)
            StartListening();
        else
            StopListening();
    }

    private void StartListening()
    {
        if (_listeningRow != null && _listeningRow != this)
            _listeningRow.StopListening();

        _listeningRow = this;
        KeyLabel.Text = "...";
        SetProcessInput(true);
    }

    private void StopListening()
    {
        if (_listeningRow == this)
            _listeningRow = null;

        SetProcessInput(false);
        RebindButton.SetPressedNoSignal(false);
        ApplyKeyText();
    }

    private void ApplyKeyText()
    {
        KeyLabel.Text = _keyText;
    }
}
