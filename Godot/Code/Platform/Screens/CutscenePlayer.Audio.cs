using Godot;

namespace Deuteros.Code.Platform.Screens
{
	public partial class CutscenePlayer
	{
		// Start the pack's music. An empty name leaves the pack silent.
		// The stream loops; cue timing does not follow the stream, it follows the sync clock.
		void StartMusic(string name)
		{
			if (string.IsNullOrEmpty(name))
				return;

			var stream = GD.Load<AudioStream>("res://Sounds/Cutscenes/" + name);
			if (stream == null)
			{
				GD.PushError("Missing cutscene music " + name);
				return;
			}
			if (stream is AudioStreamOggVorbis ogg)
				ogg.Loop = true;
			music.Stream = stream;
			music.Play();
		}

		// Play a sample on the channels selected by the mask, or stop those channels.
		// Bit 0 is channel 0, bit 1 is channel 1, and so on. A mask of zero does nothing.
		// Slot zero, or no sample name, stops the selected channels instead of playing.
		void PlaySfx(int slot, int mask, string wav)
		{
			if (mask == 0)
				return;

			if (slot == 0 || string.IsNullOrEmpty(wav))
			{
				for (int ch = 0; ch < 4; ch++)
				{
					if ((mask & (1 << ch)) != 0)
						sfx[ch].Stop();
				}
				return;
			}

			string path = "res://Sounds/Cutscenes/" + pack + "/" + wav;
			if (!sfxCache.TryGetValue(path, out var stream))
			{
				stream = GD.Load<AudioStream>(path);
				sfxCache[path] = stream;
			}
			if (stream == null)
				return;

			for (int ch = 0; ch < 4; ch++)
			{
				if ((mask & (1 << ch)) == 0)
					continue;
				sfx[ch].Stop();
				sfx[ch].Stream = stream;
				sfx[ch].Play();
			}
		}

		void StopAudio()
		{
			music?.Stop();
			foreach (var player in sfx)
				player?.Stop();
		}
	}
}
