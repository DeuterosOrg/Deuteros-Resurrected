using Deuteros.Code.Platform.Base;
using Godot;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Deuteros.Code.Platform.Screens.ModuleScenes
{
	public partial class ModuleTextFrame : BaseSubScene
	{
		private readonly CancellationTokenSource lifetime = new CancellationTokenSource();

		public override void _ExitTree()
		{
			lifetime.Cancel();
			TypeSound?.Stop();
			base._ExitTree();
		}
		public RichTextLabel Text { get; set; }
		public Label WindowNumber { get; set; }

		[Export] public AudioStreamPlayer TypeSound;

		public int LetterDelayMs { get; set; }
		public int WordDelayMs { get; set; }

		public override void _Ready()
		{
			Text = GetNode<RichTextLabel>("Window/Text");

			WindowNumber = GetNode<Node2D>("Window").GetNode<Label>("Background/Number");

			LetterDelayMs = 75;
			WordDelayMs = 200;

			base._Ready();
		}

		public async Task PlayText(Objects.ModuleTextFrame.TextFrame textFrame, List<string> dynamicProperties, int windowNumber, int letterDelayMs = 75, int wordDelayMs = 200)
		{
			LetterDelayMs = letterDelayMs;
			WordDelayMs = wordDelayMs;

			WindowNumber.Text = windowNumber.ToString();

			Text.Text = "";

			foreach (var line in textFrame.Lines)
			{
				if (line.Pause)
				{
					await WaitMs(line.Length);
				}
				else
				{
					await TypeText(Text, line.GetText(dynamicProperties), line.DelayEachWord);
					await WaitMs(line.Length);
				}
			}
		}

		public async Task PlayRFrameThreeLabels(int frameCount)
		{

		}

		//public async Task PlayMethanoidText()
		//{
		//	Text01.Text = "";
		//	Text02.Text = "";
		//	Text03.Text = "";
		//	Text04.Text = "";

		//	await TypeText(Text01, MethanoidText,true);
		//	await WaitMs(5000);
		//}

		//public async Task PlayMethanoidText2()
		//{
		//	Text01.Text = "";
		//	Text02.Text = "";
		//	Text03.Text = "";
		//	Text04.Text = "";

		//	await TypeText(Text01, MethanoidText2, true);
		//	await WaitMs(5000);
		//}

		private async Task TypeText(RichTextLabel label, string fullText, bool wordDelayOnly)
		{
			for (int i = 0; i < fullText.Length; i++)
			{
				//Instantly print and skip color tags
				if (fullText[i] == '[' && (fullText.Substring(i, 6) == "[color" || fullText.Substring(i, 7) == "[/color"))
				{
					label.Text += fullText.Substring(i, fullText.IndexOf("]", i) + 1 - i);

					i = fullText.IndexOf("]", i);

					continue;
				}

				label.Text += fullText[i];

				if (!wordDelayOnly)
				{
					if (fullText[i] != ' ' && TypeSound != null)
					{
						TypeSound.Stop(); // restarts the sound cleanly
						TypeSound.Play();
					}
					
					await WaitMs(LetterDelayMs);
				}
				else
				{
					if (fullText[i] == ' ')
						await WaitMs(WordDelayMs);
					if (fullText[i] == '\n')
						await WaitMs(WordDelayMs*2);
				}
			}
		}

		private async Task WaitMs(int ms)
		{
			lifetime.Token.ThrowIfCancellationRequested();
			var timer = GetTree().CreateTimer(ms / 1000.0);
			var completion = new TaskCompletionSource<bool>();
			void Finish() => completion.TrySetResult(true);
			timer.Timeout += Finish;
			using (lifetime.Token.Register(() => completion.TrySetCanceled(lifetime.Token)))
			{
				try { await completion.Task; }
				finally { if (IsInstanceValid(timer)) timer.Timeout -= Finish; }
			}
		}
	}
}
