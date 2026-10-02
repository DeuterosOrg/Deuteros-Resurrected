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

	public override void _ExitTree()
	{
		CancelTyping();
		base._ExitTree();
	}

	private void CancelTyping()
	{
		typingGeneration++;
		if (IsInstanceValid(TypeSound)) TypeSound.Stop();
		if (ownsScreenLock)
		{
			ownsScreenLock = false;
			GameCore.UnLockScreen();
		}
	}

	private async Task TypeText(RichTextLabel label, string fullText)
	{
		CancelTyping();
		var generation = typingGeneration;
		GameCore.LockScreen();
		ownsScreenLock = true;
		label.Text = "";
		try
		{
			for (int i = 0; i < fullText.Length; i++)
			{
				// A scene change can free the label while its timer is pending.
				if (generation != typingGeneration) return;
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
		}
		finally
		{
			if (generation == typingGeneration) CancelTyping();
		}
	}

	private async Task WaitMs(int ms)
	{
		await ToSignal(
			GetTree().CreateTimer(ms / 1000.0),
			SceneTreeTimer.SignalName.Timeout
		);
	}

	public async void DisplayBulletin(BulletinTypes bulletin)
	{
		string bulletinText = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.BulletinTexts[bulletin].BulletinText;

		GameCore.SingletonInstance.GameData.ActiveSaveFile.TimeSkip = false;

		bulletinText = "[color=ff0000]Special Bulletin.[/color]\r\n" +
			"From: \r\n" +
			(GameCore.Earth.ResearchStaff?.Leader ?? "Research Department") + "\r\n" +
			"Head of research.\r\n \r\n" + bulletinText + "\r\n \r\nMessage ends.";
		await TypeText(BulletinLabel, bulletinText);
	}
}

/*

"greetings once again, human.\n"+
" \n"+
"allow us to introduce ourselves.\n"+
"we are a peaceful race, similar\n"+
"to yourselves from a galaxy\n"+
"some 700000 parsecs distant.\n"+
" \n"+
"we have already made contact\n"+
"with a race in your galaxy and\n"+
"observe that you are both at\n"+
"war. this is to be expected.\n"+
" \n"+
"we too have found them to be\n"+
"dishonourable. this time we\n"+
"must be certain before placing\n"+
"our TRUST in YOU.\n"+
" \n"+
"CONTACT will follow when\n"+
"observations are complete.";



"GREETINGS, FRIEND.\n"+
" \n"+
"we BELIEVE we CAN TRUST YOU AND\n"+
"REQUEST your ASSISTANCE IN A\n"+
"project TO OUR MUTUAL BENEFIT.\n"+
" \n"+
"MANY land AGO, WE transmuted A\n"+
"gift TO the METHANOIDS.A GIFT\n"+
"OF great POWER AND imPORTANce.\n"+
" \n"+
"SADLY, THEY DISASSEMBLEd it IN\n"+
"AN ATTEMPT to understand THE\n"+
"TECHnologY, A GRAVE MISTaKE.\n"+
" \n"+
"OUR SCANNERS tell US THAT THE\n"+
"segments ARE SCATTERED amomg 8\n"+
"STARS in your GALAXY. we shall\n"+
"INFORM you of their exact\n"+
"LOCATION as we DETEct them.";


"GREETINGS, FRIEND.\n"+
" \n"+
"we have confirmation from our\n"+
"scanners that one segment of\n"+
"our apparatus is\n"+
"lying in orbit around\n"+
"{0}.\n"+
" \n"+
"please attempt to recover the\n"+
"segment and return it to any\n"+
"of your factories.\n"+
" \n"+
"good luck.\n";


"OUR COMPLIMENTS, FRIEND.\n"+
"YOU NOW HAVE ALL SEGMENTS OF\n"+
"OUR TRANSMITTER\n"+
" \n"+
"IF YOU WISH TO USE IT PLEASE\n"+
"FOLLOW THESE INSTRUCTIONS.\n"+
" \n"+
"1: CONSTRUCT THE TRANSMITTER\n"+
"   IN ANY OF YOUR FACTORIES\n"+
" \n"+
"2: FIT THIS TO ANY STARSHIP\n"+
"   IN YOUR FLEET\n"+
"3: ACTIVATE THE POD HOLDING\n"+
"   THE TRANSMITTER.\n"+
" \n"+
"WE WILL DO THE REST.\n"+
" \n"+
"SEE YOU SOON , HUMAN!\n";
					*/
