using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Objects.Interfaces;
using Deuteros.Code.Platform.Base;
using Deuteros.Code.Platform.Screens;
using Godot;
using Newtonsoft.Json;
using System;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using static Deuteros.Code.Enums;

public partial class Bulletins : BaseSubScene
{
	RichTextLabel BulletinLabel { get; set; }

	AudioStreamPlayer TypeSound { get; set; }
	public int LetterDelayMs { get; set; }
	private int typingGeneration;
	private bool ownsScreenLock;
	private bool skipRequested;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LetterDelayMs = 75;

		BulletinLabel = GetNode<RichTextLabel>("Labels/BulletinLabel");
		TypeSound = GetNode<AudioStreamPlayer>("TypeSound");
		GetNode<Button>("ViewTransmission").Pressed += AcknowledgeTransmission;

		base._Ready();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _ExitTree()
	{
		noticeSave = null;
		CancelTyping();
		base._ExitTree();
	}

	public override void _Input(InputEvent input)
	{
		if (ownsScreenLock && input is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }
			&& Deuteros.Code.Platform.Helpers.SettingsManager.Instance.GetSetting("modern/bulletin_skip", false).AsBool())
		{
			skipRequested = true;
			GetViewport().SetInputAsHandled();
		}
	}

	private void CancelTyping()
	{
		typingGeneration++;
		skipRequested = false;
		if (IsInstanceValid(TypeSound)) TypeSound.Stop();
		if (ownsScreenLock)
		{
			ownsScreenLock = false;
			GameCore.UnLockScreen();
		}
	}

	private async Task<bool> TypeText(RichTextLabel label, string fullText)
	{
		CancelTyping();
		var generation = typingGeneration;
		GameCore.LockScreen();
		ownsScreenLock = true;
		label.Text = fullText;
		label.VisibleCharacters = 0;
		var parsedText = label.GetParsedText();
		var characterCount = label.GetTotalCharacterCount();
		try
		{
			for (int i = 0; i < characterCount; i++)
			{
				if (generation != typingGeneration) return false;
				if (skipRequested)
				{
					label.VisibleCharacters = -1;
					break;
				}
				label.VisibleCharacters = i + 1;
				if (i < parsedText.Length && !char.IsWhiteSpace(parsedText[i]) && TypeSound != null)
				{
					TypeSound.Stop();
					TypeSound.Play();
				}
				if (LetterDelayMs > 0) await WaitMs(LetterDelayMs);
			}
			return generation == typingGeneration;
		}
		finally
		{
			if (generation == typingGeneration) CancelTyping();
		}
	}

	private async Task WaitMs(int ms)
	{
		await ToSignal(
			GetTree().CreateTimer(ms / 1000.0, false),
			SceneTreeTimer.SignalName.Timeout
		);
	}

	public async void DisplayBulletin(BulletinTypes bulletin)
	{
		GameCore.SingletonInstance.GameData.ActiveSaveFile.TimeSkip = false;
		await TypeText(BulletinLabel, DepartmentText(bulletin));
	}

	private string DepartmentText(BulletinTypes bulletin)
	{
		var bulletinText = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.BulletinTexts[bulletin].BulletinText;
		if (bulletin == BulletinTypes.Mining_Dump)
		{
			var news = GameCore.SingletonInstance.GameData.ActiveSaveFile.News;
			bulletinText = string.Format(bulletinText, news.MiningDumpLocation.ToScreenString(" "), news.MiningDumpResource.ToScreenString());
		}
		return "[color=ff0000]Special Bulletin.[/color]\r\n" +
			"From: \r\n" +
			(GameCore.Earth.ResearchStaff?.Leader ?? "Research Department") + "\r\n" +
			"Head of research.\r\n \r\n" + bulletinText + "\r\n \r\nMessage ends.";
	}
}
