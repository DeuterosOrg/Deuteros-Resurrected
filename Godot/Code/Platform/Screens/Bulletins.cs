using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Objects.Bulletins;
using Deuteros.Code.Objects.Interfaces;
using Deuteros.Code.Platform.Base;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Platform.Screens;
using Godot;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.Emit;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static Deuteros.Code.Enums;

public partial class Bulletins : BaseSubScene
{
	RichTextLabel BulletinLabel { get; set; }
	AudioStreamPlayer TypeSound { get; set; }
	public int LetterDelayMs { get; set; }

	private bool _typing;
	private bool _skipRequested;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		LetterDelayMs = 75;

		BulletinLabel = GetNode<RichTextLabel>("Labels/BulletinLabel");
		TypeSound = GetNode<AudioStreamPlayer>("TypeSound");

		base._Ready();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Input(InputEvent @event)
	{
		if (!_typing || @event is not InputEventMouseButton mouseEvent || !mouseEvent.Pressed || mouseEvent.ButtonIndex != MouseButton.Left)
			return;

		if (!SettingsManager.Instance.GetSetting("modern/bulletin_skip", false).AsBool())
			return;

		_skipRequested = true;
		GetViewport().SetInputAsHandled();
	}

	private async Task TypeText(RichTextLabel label, string fullText)
	{
		GameCore.LockScreen();
		label.Text = "";
		_typing = true;
		_skipRequested = false;

		for (int i = 0; i < fullText.Length; i++)
		{
			if (_skipRequested)
			{
				label.Text = fullText;
				break;
			}

			//Instantly print and skip color tags
			if (fullText[i] == '[' && (fullText.Substring(i, 6) == "[color" || fullText.Substring(i, 7) == "[/color"))
			{
				label.Text += fullText.Substring(i, fullText.IndexOf("]", i) + 1 - i);

				i = fullText.IndexOf("]", i);

				continue;
			}

			label.Text += fullText[i];

			// Optional: don't blip on spaces
			if (fullText[i] != ' ' && TypeSound != null)
			{
				TypeSound.Stop(); // restarts the sound cleanly
				TypeSound.Play();
			}

			await WaitMs(LetterDelayMs);
		}
		_typing = false;
		GameCore.UnLockScreen();
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
		BulletinLabel.RemoveThemeFontOverride("normal_font");

		string bulletinText = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.BulletinTexts[bulletin].BulletinText;

		GameCore.SingletonInstance.GameData.ActiveSaveFile.TimeSkip = false;

		bulletinText = "[color=ff0000]Special Bulletin.[/color]\r\n" +
			"From: \r\n" +
			GameCore.Earth.ResearchStaff.Leader + "\r\n" +
			"Head of research.\r\n \r\n" + bulletinText + "\r\n \r\nMessage ends.";

		await TypeText(BulletinLabel, bulletinText);
	}
	public async void DisplayAlienMessage(BulletinTypes introBulletinType,BulletinTypes alienBulletinType)
	{
        BulletinLabel.RemoveThemeFontOverride("normal_font");

        string bulletinText = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.BulletinTexts[introBulletinType].BulletinText;

        GameCore.SingletonInstance.GameData.ActiveSaveFile.TimeSkip = false;

        bulletinText = "[color=ff0000]Special Bulletin.[/color]\r\n" +
			"From: \r\n" +
			GameCore.Earth.ResearchStaff.Leader + "\r\n" +
			"Head of research.\r\n \r\n" + bulletinText + "\r\n \r\nMessage ends.";
       
		await TypeText(BulletinLabel, bulletinText);

		string alienMessageText = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.BulletinTexts[alienBulletinType].BulletinText;

        long messageKey = 0;

        switch (GameCore.SingletonInstance.GameData.ActiveSaveFile.AlienTransmissionsReceived)
		{
			case 1:
				messageKey = 0;
				break;
			case 2:
				messageKey = 0x08945949;
				break;
			default:
                messageKey = ((uint)(Random.Shared.Next(0x10000) << 16 )) | (uint)(Random.Shared.Next(0x10000));
				break;
        }

		StringBuilder newMessage = new StringBuilder();
		for (int i = 0; i < alienMessageText.Length; i++)
		{
			if (alienMessageText[i] == ' ')
			{
				messageKey = ((messageKey << 1) & 0xffffffff) | (messageKey >> 31);
			}

			if ((alienMessageText[i] >= 'a') && (alienMessageText[i] <= 'z'))
			{
				if ((messageKey & 0x1000000) != 0)
				{
					newMessage.Append(alienMessageText[i].ToString().ToUpper());
				}
				else
				{
					newMessage.Append(alienMessageText[i]);
				}

			}
			else
			{
				newMessage.Append(alienMessageText[i]);
			}

		}

		alienMessageText = "[color=99aa77]" + newMessage.ToString() + "[/color]";

		BulletinLabel.AddThemeFontOverride("normal_font",ResourceLoader.Load<Font>("res://Fonts/deuteros-alien.ttf"));
		await TypeText(BulletinLabel, alienMessageText);
	}
}

