using Deuteros.Code;
using Deuteros.Code.Platform.Base;
using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Deuteros.Code.Platform.Screens
{
	// Plays one cutscene pack at a time, on a fixed 50 Hz tick.
	//
	// A pack is either a still picture held on screen for a set number of ticks,
	// or a scripted stage. The stage is a list of actors. Each actor walks its
	// own command list: show a frame, move, wait, play a sound, wait for the
	// music. Every frame comes from one shared atlas. The atlas stores a colour
	// pen rather than a colour, and a shader looks that pen up in a 16-colour palette.
	public partial class CutscenePlayer : BaseSubScene
	{
		// Stage size in pixels. Script positions are in this space.
		const int ScreenW = 320;
		const int ScreenH = 200;
		// Length of one script tick. A long render frame runs several ticks to catch up.
		const double Tick = 0.02;

		// What an actor is showing. Kept on the actor as a single value.
		const ushort Hide = 0x00FF;      // draw nothing
		const ushort ShowText = 0x00FE;  // draw a text block; the block id is stored on the actor
		const ushort Restore = 0xFFFF;   // redraw the picture this actor previously froze
		// Top bit set: the low byte is a frame number, and pen 0 is left transparent.
		const ushort Cut = 0x8000;
		// Both top bits set: draw that frame, keep it as the frozen picture, then
		// switch the actor to Restore so later ticks redraw the frozen picture.
		const ushort Freeze = 0xC000;

		// Low byte of an actor's mode. Zero means the actor is idle.
		const int Waiting = 3;      // counting down a delay
		const int Cueing = 5;       // waiting for the music clock, or for a frame number
		const int Sliding = 6;      // moving vertically, one step at a time
		const int WaitClick = 0x14; // waiting until a mouse click has been recorded

		// Indexed-colour shader. The atlas stores the pen in the alpha channel as
		// (240 + pen) / 255, so pen 0 reads as alpha 240 and pen 15 as alpha 255.
		// The texture's RGB is ignored.
		// A cell is an atlas slice, so UV 0..1 covers that cell only.
		// Glyphs are one bit: any non-zero pen uses the foreground pen, zero uses the background.
		// Other sprites look the pen up in the palette. Cut-out sprites can discard pen 0.
		const string PaletteShader = @"
shader_type canvas_item;
uniform vec4 palette[16];
uniform bool transparent_zero = false;
uniform bool is_glyph = false;
uniform int glyph_fg = 15;
uniform int glyph_bg = 0;

void fragment() {
	vec4 tex = texture(TEXTURE, UV);
	int pen = clamp(int(round(tex.a * 255.0)) - 240, 0, 15);
	if (is_glyph) {
		COLOR = palette[pen != 0 ? glyph_fg : glyph_bg];
	} else if (transparent_zero && pen == 0) {
		discard;
	} else {
		COLOR = palette[pen];
	}
}
";

		AudioStreamPlayer music;
		// Four effect channels. A script selects them with a bit mask.
		AudioStreamPlayer[] sfx = new AudioStreamPlayer[4];
		Dictionary<string, AudioStream> sfxCache = new Dictionary<string, AudioStream>();

		ColorRect backdrop;
		// Full-screen still, used by packs that are a single picture rather than a script.
		Sprite2D still;
		// Parent of every actor. Later actors are drawn on top.
		Node2D stage;
		Shader shader;
		// Opaque sprites, and sprites that punch out pen 0. Both share the shader.
		ShaderMaterial copyMat;
		ShaderMaterial cutMat;
		// Every text block's material, so a palette change can recolour text already on screen.
		List<ShaderMaterial> textMats = new List<ShaderMaterial>();
		Godot.Collections.Array<Color> palette = new Godot.Collections.Array<Color>();
		bool paletteDirty;

		Texture2D atlas;
		// One slice per frame in the pack. A missing frame is a null entry.
		List<AtlasTexture> cellTextures = new List<AtlasTexture>();
		// Glyph slices, built the first time each character is drawn.
		Dictionary<long, AtlasTexture> glyphTextures = new Dictionary<long, AtlasTexture>();
		// Last frame an actor froze, and where it was drawn.
		// Restore draws this cell again, which is how a frozen picture then slides.
		int snapFrame = -1;
		int snapX;
		int snapRows;

		CueFile cue;
		string pack = "title";
		// Time not yet consumed by script ticks.
		double accum;
		// Ticks left on a still-picture pack.
		int stillTicks;
		// Set when a preview starts from the title or the credits, so the preview returns there.
		bool goBackToTitle;
		bool closed;

		List<int[]> frames;
		// Font sheet rectangle inside the atlas, and how far the cursor steps per character.
		int[] fontRect;
		int fontAdvance = 8;
		// Random table for this pack, and the running position within it.
		byte[] rngBank;
		ushort rng;
		// Frame counter mixed into the random index. Steps by 4 and wraps at 65536.
		uint clock;
		// Colour cycle. While this is non-zero, every tick writes a new pair of colours
		// into palette entries 10 and 11. copperIdx walks the pack's list of pairs.
		int copperOn;
		ushort copperIdx;
		List<Actor> actors = new List<Actor>();
		// While clicks are on, a mouse button ends the pack (the title has its own rules).
		// click is set by the left button and read by actors that are waiting for a click.
		bool clicksOn;
		ushort click;
		// An actor ran the exit command. The pack ends once the gap between steps has elapsed.
		bool exiting;
		// Frames remaining until actors run again, and how long that gap is for this pack.
		int hold;
		int packHold = 1;
		// Frames since this pack started. A cue can wait until this reaches a number.
		int tick;

		// Music sync clock. The audio file is only sound; this clock walks the song's
		// pattern layout so a cue can wait for a position in the song.
		// A pattern is 64 rows. musicRow sits at 64 between patterns, so the next row
		// advance loads one. musicPos is one ahead of the sequence slot now playing.
		bool musicOk;
		uint musicPos;
		ushort musicRow = 0x40;
		ushort musicSpeed = 6;
		ushort musicDiv;
		int musicSeq = -1;
		int musicAt;
		int musicPat;
		bool musicHave;
		List<int> musicSequence;
		List<Dictionary<string, List<int[]>>> musicPatterns;

		public override void _Ready()
		{
			base._Ready();
			// The in-game menu stays hidden for the whole cutscene.
			GameCore.SingletonInstance.SetPlayfieldMenuVisible(false);

			for (int i = 0; i < 16; i++)
				palette.Add(Colors.Black);

			shader = new Shader { Code = PaletteShader };
			copyMat = new ShaderMaterial { Shader = shader };
			copyMat.SetShaderParameter("transparent_zero", false);
			copyMat.SetShaderParameter("is_glyph", false);
			cutMat = new ShaderMaterial { Shader = shader };
			cutMat.SetShaderParameter("transparent_zero", true);
			cutMat.SetShaderParameter("is_glyph", false);

			backdrop = new ColorRect();
			backdrop.Size = new Vector2(ScreenW, ScreenH);
			backdrop.Color = Colors.Black;
			backdrop.MouseFilter = Control.MouseFilterEnum.Stop;
			backdrop.GuiInput += OnInput;
			AddChild(backdrop);

			still = new Sprite2D();
			still.Centered = false;
			still.Visible = false;
			still.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
			AddChild(still);

			stage = new Node2D();
			AddChild(stage);

			music = new AudioStreamPlayer { Bus = "Music" };
			AddChild(music);

			for (int i = 0; i < sfx.Length; i++)
			{
				sfx[i] = new AudioStreamPlayer { Bus = "Effects" };
				AddChild(sfx[i]);
			}

			PlayPack(GameCore.SingletonInstance.CutscenePack);
		}

		public override void _ExitTree()
		{
			base._ExitTree();

			if (IsInstanceValid(GameCore.SingletonInstance))
				GameCore.SingletonInstance.SetPlayfieldMenuVisible(true);
		}

		public override void _Process(double delta)
		{
			if (cue == null || closed)
				return;

			// Catch up, but never more than 32 ticks in one render frame.
			accum += delta;

			int n = 0;

			while (accum >= Tick && n < 32)
			{
				accum -= Tick;
				n++;

				if (!string.IsNullOrEmpty(cue.StaticPng))
				{
					stillTicks--;

					if (stillTicks <= 0)
					{
						Finish();

						return;
					}

					continue;
				}

				TickFrame();

				if (closed)
					return;
			}
		}

		// Jump to another pack. A jump made from the title or the credits
		// returns to the title when that pack ends.
		public bool DebugPreview(string packName)
		{
			if (pack == "title" || pack == "credits")
				goBackToTitle = true;

			PlayPack(packName);

			return true;
		}

		void OnInput(InputEvent ev)
		{
			if (ev is not InputEventMouseButton mouse || !mouse.Pressed)
				return;

			if (mouse.ButtonIndex != MouseButton.Left && mouse.ButtonIndex != MouseButton.Right)
				return;

			GetViewport().SetInputAsHandled();

			if (cue == null || !string.IsNullOrEmpty(cue.StaticPng) || !clicksOn)
				return;

			if (mouse.ButtonIndex == MouseButton.Left)
				click = 1;

			// Title screen: the right button starts the flyby, the left button leaves.
			if (cue.Selector == 0)
			{
				if (mouse.ButtonIndex == MouseButton.Right)
				{
					goBackToTitle = false;
					PlayPack("flyby");
				}
				else
					Finish();

				return;
			}

			Finish();
		}

		// Tear down whatever is playing and start this pack from the beginning.
		void PlayPack(string name)
		{
			StopAudio();
			sfxCache.Clear();
			pack = name;
			accum = 0;
			cue = LoadCue(name);

			if (cue == null)
			{
				GD.PushError("Cutscene pack '" + name + "' is missing.");
				Callable.From(Finish).CallDeferred();

				return;
			}

			if (!string.IsNullOrEmpty(cue.StaticPng))
			{
				stage.Visible = false;
				still.Texture = GD.Load<Texture2D>("res://Sprites/Cutscenes/" + pack + "/" + cue.StaticPng);
				still.Visible = true;
				stillTicks = Math.Max(1, cue.Blanks);

				return;
			}

			still.Visible = false;
			stage.Visible = true;
			BindBank();
			StartMusic(cue.Music);
			TickFrame();
		}

		// Build the stage from the loaded pack and clear leftover state from the previous one.
		void BindBank()
		{
			frames = cue.Frames ?? new List<int[]>();
			fontRect = cue.Font != null ? cue.Font.Rect : null;
			fontAdvance = cue.Font != null && cue.Font.Advance > 0 ? cue.Font.Advance : 8;
			atlas = string.IsNullOrEmpty(cue.Atlas) ? null : GD.Load<Texture2D>("res://Sprites/Cutscenes/" + pack + "/" + cue.Atlas);
			rngBank = string.IsNullOrEmpty(cue.RngB64) ? Array.Empty<byte>() : Convert.FromBase64String(cue.RngB64);

			clicksOn = true;
			click = 0;
			exiting = false;
			hold = 0;
			packHold = cue.Hold > 0 ? cue.Hold : 1;
			tick = 0;
			clock = 0;
			rng = 0;
			copperIdx = 0;
			snapFrame = -1;
			snapX = 0;
			snapRows = 0;
			copperOn = cue.CopperFlag;
			ResetMusic();

			foreach (Node child in stage.GetChildren())
				child.Free();

			textMats.Clear();
			actors.Clear();
			cellTextures.Clear();
			glyphTextures.Clear();
			BuildCellTextures();

			LoadPalette(0);

			if (cue.Actors == null)
				return;

			foreach (var def in cue.Actors)
			{
				var slot = new Node2D();
				// Band sits behind the sprite and is only shown while restoring a frozen picture.
				var band = new ColorRect();

				band.Size = new Vector2(ScreenW, 0);
				band.Visible = false;
				band.MouseFilter = Control.MouseFilterEnum.Ignore;

				var sprite = new Sprite2D();

				sprite.Centered = false;
				sprite.Visible = false;
				sprite.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
				sprite.Material = copyMat;

				var text = new Node2D();

				text.Visible = false;

				slot.AddChild(band);
				slot.AddChild(sprite);
				slot.AddChild(text);
				stage.AddChild(slot);

				actors.Add(new Actor
				{
					Image = (ushort)def.Image,
					X = (short)def.X,
					Y = (short)def.Y,
					Mode = (ushort)def.Mode,
					Param = (ushort)def.Param,
					Pc = def.Pc,
					Active = def.Commands != null && def.Commands.Count > 0,
					Commands = def.Commands ?? new List<CueOp>(),
					Band = band,
					Sprite = sprite,
					Text = text,
					DrawnText = -1
				});
			}

			ApplyPalette();
		}

		// Slice every frame rectangle out of the atlas. Rectangles are x, y, width, height.
		void BuildCellTextures()
		{
			if (atlas == null || frames == null)
				return;

			for (int i = 0; i < frames.Count; i++)
			{
				var rect = frames[i];

				if (rect == null || rect.Length < 4)
				{
					cellTextures.Add(null);

					continue;
				}

				var cell = new AtlasTexture
				{
					Atlas = atlas,
					Region = new Rect2(rect[0], rect[1], rect[2], rect[3]),
					FilterClip = true
				};

				cellTextures.Add(cell);
			}
		}

		// One 8x8 glyph from the font sheet. Glyphs are stored in rows of 16.
		AtlasTexture GlyphTexture(int g)
		{
			if (atlas == null || fontRect == null || fontRect.Length < 4)
				return null;

			long key = g;

			if (glyphTextures.TryGetValue(key, out var cached))
				return cached;

			var cell = new AtlasTexture
			{
				Atlas = atlas,
				Region = new Rect2(fontRect[0] + (g % 16) * 8, fontRect[1] + (g / 16) * 8, 8, 8),
				FilterClip = true
			};

			glyphTextures[key] = cell;

			return cell;
		}

		// Leave this pack. A return-to-title preview plays the title again.
		// A pack can name another pack to play next. Otherwise the game is told
		// the cutscene is over, which either resumes the previous screen or starts the game.
		void Finish()
		{
			if (goBackToTitle)
			{
				goBackToTitle = false;
				PlayPack("title");

				return;
			}

			if (!string.IsNullOrEmpty(cue?.Next))
			{
				PlayPack(cue.Next);

				return;
			}

			if (closed)
				return;

			closed = true;
			StopAudio();

			Callable.From(() =>
			{
				if (IsInstanceValid(GameCore.SingletonInstance))
					GameCore.SingletonInstance.CutsceneFinished();
			}).CallDeferred();
		}

		static CueFile LoadCue(string name)
		{
			var path = "res://Cutscenes/" + name + ".json";
			using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);

			if (file == null)
			{
				GD.PushError("Cannot read " + path);

				return null;
			}

			return JsonConvert.DeserializeObject<CueFile>(file.GetAsText());
		}

		// One thing on the stage, and the point it has reached in its command list.
		class Actor
		{
			public ushort Image;
			public short X; // horizontal position, in 16-pixel columns
			public short Y;
			// What the actor is doing. The low byte is a wait kind; zero means idle.
			public ushort Mode;
			// Wait counter, slide steps remaining, or the music position being waited for.
			public ushort Param;
			// Vertical step of a slide, or the music row a cue must pass.
			public ushort ParamHi;
			// Slide timing, the text-block id, or which kind of cue this wait is.
			public uint Extra;
			// Index of the next command, and where a call command returns to.
			public int Pc;
			public int Ret;
			public bool Active;
			public List<CueOp> Commands;
			public ColorRect Band;
			public Sprite2D Sprite;
			public Node2D Text;
			// Text block currently built under Text. Skips rebuilding it every tick.
			public int DrawnText;
		}

		// One pack file: either a still picture, or the stage, pictures, and scripts.
		class CueFile
		{
			[JsonProperty("pack")] public string Pack { get; set; }
			// Zero is the title screen, which treats the two mouse buttons differently.
			[JsonProperty("selector")] public int Selector { get; set; }
			// Ticks between script steps. The colour cycle and the music clock still run every tick.
			[JsonProperty("hold")] public int Hold { get; set; }
			// Set on a pack that is only a picture. Blanks is how many ticks to leave it up.
			[JsonProperty("static_png")] public string StaticPng { get; set; }
			[JsonProperty("blanks")] public int Blanks { get; set; }
			// Pack to start when this one ends. Empty means the cutscene is finished.
			[JsonProperty("next")] public string Next { get; set; }
			[JsonProperty("atlas")] public string Atlas { get; set; }
			[JsonProperty("music")] public string Music { get; set; }
			// Non-zero starts the colour cycle as soon as the pack does.
			[JsonProperty("copper_flag")] public int CopperFlag { get; set; }
			// Four pairs of colours. The cycle writes one pair into pens 10 and 11 each tick.
			[JsonProperty("copper")] public List<List<int[]>> Copper { get; set; }
			[JsonProperty("font")] public CueFont Font { get; set; }
			[JsonProperty("texts")] public List<List<CueText>> Texts { get; set; }
			// Random table, base64-encoded bytes.
			[JsonProperty("rng_b64")] public string RngB64 { get; set; }
			// Song layout used to time cues. Playback itself uses the music file.
			[JsonProperty("module")] public CueModule Module { get; set; }
			[JsonProperty("actors")] public List<CueActor> Actors { get; set; }
			[JsonProperty("palettes")] public List<List<int[]>> Palettes { get; set; }
			// Frame rectangles in the atlas: x, y, width, height.
			[JsonProperty("frames")] public List<int[]> Frames { get; set; }
		}

		// sequence is the order of pattern numbers.
		// Each pattern maps a row number to effects; an effect is a pair of kind, value.
		class CueModule
		{
			[JsonProperty("sequence")] public List<int> Sequence { get; set; }
			[JsonProperty("patterns")] public List<Dictionary<string, List<int[]>>> Patterns { get; set; }
		}

		class CueFont
		{
			[JsonProperty("rect")] public int[] Rect { get; set; }
			[JsonProperty("advance")] public int Advance { get; set; }
		}

		// Starting state of one actor, plus the commands it runs.
		// pc can start part way through the list.
		class CueActor
		{
			[JsonProperty("slot")] public int Slot { get; set; }
			[JsonProperty("pc")] public int Pc { get; set; }
			[JsonProperty("image")] public int Image { get; set; }
			[JsonProperty("x")] public int X { get; set; }
			[JsonProperty("y")] public int Y { get; set; }
			[JsonProperty("mode")] public int Mode { get; set; }
			[JsonProperty("param")] public int Param { get; set; }
			[JsonProperty("commands")] public List<CueOp> Commands { get; set; }
		}

		// One command. Only the fields that command uses are filled in.
		// next, when set, is the command index to run afterwards; otherwise the list continues in order.
		class CueOp
		{
			[JsonProperty("op")] public string Op { get; set; }
			[JsonProperty("f")] public int F { get; set; }
			[JsonProperty("x")] public int X { get; set; }
			[JsonProperty("y")] public int Y { get; set; }
			[JsonProperty("dx")] public int Dx { get; set; }
			[JsonProperty("dy")] public int Dy { get; set; }
			[JsonProperty("n")] public int N { get; set; }
			[JsonProperty("pos")] public int? Pos { get; set; }
			[JsonProperty("row")] public int? Row { get; set; }
			[JsonProperty("p")] public int P { get; set; }
			[JsonProperty("delay")] public int Delay { get; set; }
			[JsonProperty("reload")] public int Reload { get; set; }
			[JsonProperty("to")] public int To { get; set; }
			[JsonProperty("ret")] public int Ret { get; set; }
			[JsonProperty("mask")] public int Mask { get; set; }
			[JsonProperty("slot")] public int Slot { get; set; }
			[JsonProperty("wav")] public string Wav { get; set; }
			[JsonProperty("flag")] public int Flag { get; set; }
			[JsonProperty("id")] public int Id { get; set; }
			[JsonProperty("limit")] public int Limit { get; set; }
			[JsonProperty("targets")] public List<int> Targets { get; set; }
			[JsonProperty("arm")] public bool Arm { get; set; }
			[JsonProperty("param")] public int Param { get; set; }
			[JsonProperty("next")] public int? Next { get; set; }
		}

		// One step of a text block: move the cursor, set a pen, remember a margin, or draw a string.
		class CueText
		{
			[JsonProperty("op")] public string Op { get; set; }
			[JsonProperty("x")] public int X { get; set; }
			[JsonProperty("y")] public int Y { get; set; }
			[JsonProperty("p")] public int P { get; set; }
			[JsonProperty("s")] public string S { get; set; }
		}

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
		void HidePic(Actor actor)
		{
			actor.Band.Visible = false;
			actor.Sprite.Visible = false;
			actor.Text.Visible = false;
		}

		// Show whatever this actor's picture value currently means.
		// Idle actors and hidden actors draw nothing, even if a frame number is still stored.
		void DrawActor(Actor actor)
		{
			ushort word = actor.Image;

			if ((actor.Mode & 0xFF) == 0 || word == Hide)
			{
				HidePic(actor);

				return;
			}

			if (word == ShowText)
			{
				actor.Band.Visible = false;
				actor.Sprite.Visible = false;
				DrawText(actor, (int)actor.Extra);

				return;
			}

			actor.Text.Visible = false;

			// Restore is the picture value with this bit set. Redraw the frozen cell,
			// on a full-width band of the background colour, at the actor's current row.
			bool restore = (word & 0x2000) != 0;

			if (restore)
			{
				if (snapFrame < 0 || snapFrame >= cellTextures.Count || cellTextures[snapFrame] == null)
				{
					HidePic(actor);

					return;
				}

				var snap = cellTextures[snapFrame];

				actor.Band.Position = new Vector2(0, actor.Y);
				actor.Band.Size = new Vector2(ScreenW, snapRows > 0 ? snapRows : snap.Region.Size.Y);
				actor.Band.Color = palette[0];
				actor.Band.Visible = true;
				actor.Sprite.Material = copyMat;
				actor.Sprite.Texture = snap;
				actor.Sprite.Position = new Vector2(snapX, actor.Y);
				actor.Sprite.Visible = true;

				return;
			}

			// Freeze has both top bits set. Cut has only the top bit, and leaves pen 0 transparent.
			bool freeze = (word & 0xC000) == Freeze;
			bool cut = (word & 0x8000) != 0 && (word & 0x4000) == 0;
			// Cut and freeze keep the frame number in the low byte. A plain picture is the number itself.
			int index = (word & 0x8000) != 0 ? word & 0xFF : word;

			if (index < 0 || index >= cellTextures.Count || cellTextures[index] == null)
			{
				HidePic(actor);

				return;
			}

			var cell = cellTextures[index];
			int px = actor.X * 16;
			int py = actor.Y;

			actor.Band.Visible = false;
			actor.Sprite.Material = cut ? cutMat : copyMat;
			actor.Sprite.Texture = cell;
			actor.Sprite.Position = new Vector2(px, py);
			actor.Sprite.Visible = true;

			// Remember this cell, then switch the actor to Restore so the next tick
			// redraws the frozen picture instead of playing a new frame.
			if (freeze)
			{
				snapFrame = index;
				snapX = px;
				snapRows = (int)cell.Region.Size.Y;
				actor.Image = Restore;
			}
		}

		// Build the sprites for one text block. Rebuilding is skipped when this actor
		// is already showing that same block.
		// The cursor starts at the origin. "at" places it, "mark" remembers a margin,
		// and a new line drops eight pixels and returns to that margin.
		// Characters start at space. Only the first 112 have glyphs, in rows of 16,
		// but every character still advances the cursor.
		// Glyphs in the block share one material, so the foreground and background
		// pens that end up showing are the last ones the block set.
		void DrawText(Actor actor, int id)
		{
			if (cue.Texts == null || id < 0 || id >= cue.Texts.Count || fontRect == null || fontRect.Length < 4)
			{
				actor.Text.Visible = false;

				return;
			}

			actor.Text.Visible = true;

			if (actor.DrawnText == id)
				return;

			actor.DrawnText = id;

			foreach (Node child in actor.Text.GetChildren())
				child.Free();

			var mat = new ShaderMaterial { Shader = shader };

			mat.SetShaderParameter("transparent_zero", false);
			mat.SetShaderParameter("is_glyph", true);
			mat.SetShaderParameter("palette", palette);
			textMats.Add(mat);

			int cx = 0, cy = 0, lx = 0, ly = 0;

			foreach (var op in cue.Texts[id])
			{
				switch (op.Op)
				{
					case "at":
						cx = op.X;
						cy = op.Y;
						break;
					case "fg":
						mat.SetShaderParameter("glyph_fg", op.P & 0xF);
						break;
					case "bg":
						mat.SetShaderParameter("glyph_bg", op.P & 0xF);
						break;
					case "mark":
						lx = cx;
						ly = cy;
						break;
					case "nl":
						ly += 8;
						cx = lx;
						cy = ly;
						break;
					case "str":
						if (string.IsNullOrEmpty(op.S))
							break;

						foreach (char ch in op.S)
						{
							int g = ch - 0x20;

							if (g >= 0 && g < 112)
							{
								var cell = GlyphTexture(g);

								if (cell == null)
									continue;

								var glyph = new Sprite2D();

								glyph.Centered = false;
								glyph.Texture = cell;
								glyph.Position = new Vector2(cx, cy);
								glyph.TextureFilter = CanvasItem.TextureFilterEnum.Nearest;
								glyph.Material = mat;
								actor.Text.AddChild(glyph);
							}

							cx += fontAdvance;
						}

						break;
				}
			}
		}

		// Copy palette index into the 16 pens. RGB values in the pack are 0..255.
		// The new colours are pushed to the sprites at the end of the tick.
		void LoadPalette(int index)
		{
			if (cue?.Palettes == null || index < 0 || index >= cue.Palettes.Count)
				return;

			paletteDirty = true;

			var record = cue.Palettes[index];

			for (int c = 0; c < 16 && c < record.Count; c++)
			{
				var rgb = record[c];

				palette[c] = new Color(rgb[0] / 255f, rgb[1] / 255f, rgb[2] / 255f, 1f);
			}
		}

		// One step of the colour cycle. The pack supplies four pairs; each tick
		// uses the next pair, wrapping around, and writes it into pens 10 and 11.
		void CycleCopper()
		{
			if (copperOn == 0 || cue?.Copper == null || cue.Copper.Count < 4)
				return;

			copperIdx++;

			var pair = cue.Copper[copperIdx & 3];

			if (pair == null || pair.Count < 2)
				return;

			palette[10] = new Color(pair[0][0] / 255f, pair[0][1] / 255f, pair[0][2] / 255f, 1f);
			palette[11] = new Color(pair[1][0] / 255f, pair[1][1] / 255f, pair[1][2] / 255f, 1f);
			paletteDirty = true;
		}

		// Push the current pens to everything that samples them.
		void ApplyPalette()
		{
			paletteDirty = false;
			backdrop.Color = palette[0];
			copyMat.SetShaderParameter("palette", palette);
			cutMat.SetShaderParameter("palette", palette);

			for (int i = 0; i < textMats.Count; i++)
				textMats[i].SetShaderParameter("palette", palette);

			for (int i = 0; i < actors.Count; i++)
			{
				if (actors[i].Band.Visible)
					actors[i].Band.Color = palette[0];
			}
		}

		// One tick of a scripted pack.
		// Actors only step every packHold ticks. Drawing happens on those same steps.
		// The colour cycle, the music clock, and the frame counters advance every tick,
		// so a slow script still animates and stays in time with the song.
		void TickFrame()
		{
			if (hold <= 0)
			{
				for (int i = 0; i < actors.Count; i++)
					StepActor(actors[i]);

				hold = packHold;

				for (int i = 0; i < actors.Count; i++)
					DrawActor(actors[i]);
			}

			hold--;
			CycleCopper();
			MusicTick();
			tick++;
			clock = (clock + 4) & 0xFFFF;

			if (paletteDirty)
				ApplyPalette();

			// End on a step boundary. The title stays up while idle; every other pack
			// ends once every actor has stopped, or as soon as one of them exits.
			if (hold <= 0 && (exiting || (cue.Selector != 0 && AllIdle())))
				Finish();
		}

		// Start the sync clock just before the first pattern, at the default tempo of 6 ticks per row.
		void ResetMusic()
		{
			musicSequence = cue?.Module?.Sequence;
			musicPatterns = cue?.Module?.Patterns;
			musicOk = musicSequence != null && musicSequence.Count > 0 && musicPatterns != null;
			musicPos = 0;
			musicRow = 0x40;
			musicSpeed = 6;
			musicDiv = 0;
			musicSeq = -1;
			musicAt = 0;
			musicPat = 0;
			musicHave = false;
		}

		// Advance the sync clock by one tick. Scripts read this clock before it advances,
		// so a cue sees the position from the previous tick.
		void MusicTick()
		{
			if (!musicOk)
				return;

			musicDiv++;

			if (musicSpeed > musicDiv)
				return;

			musicDiv = 0;
			musicRow++;

			// Row 64 means "between patterns". Crossing it loads the next one and restarts at row 1.
			if (musicRow == 0x41)
				MusicNextPattern();

			if (!musicHave || musicAt >= 64)
				return;

			string key = musicAt.ToString();

			musicAt++;

			if (musicPat < 0 || musicPat >= musicPatterns.Count)
				return;

			if (!musicPatterns[musicPat].TryGetValue(key, out var effects) || effects == null)
				return;

			for (int i = 0; i < effects.Count; i++)
			{
				var eff = effects[i];

				if (eff == null || eff.Length < 2)
					continue;

				int kind = eff[0];
				int param = eff[1];

				// End this pattern: park the row so the next advance loads the following pattern.
				if (kind == 4)
					musicRow = 0x40;
				// Jump in the sequence. The position is stored one step early, and the row is
				// parked, so the next advance lands on the requested slot.
				else if (kind == 5)
				{
					musicPos = (uint)(param - 1);
					musicSeq = param - 1;
					musicRow = 0x40;
				}
				// Change how many ticks make up one row.
				else if (kind == 8)
					musicSpeed = (ushort)param;
			}
		}

		// Step to the next sequence slot and start its pattern at row 1.
		// Running off the end wraps back to the first slot.
		void MusicNextPattern()
		{
			while (true)
			{
				musicPos++;

				if (musicPos > (uint)musicSequence.Count)
				{
					musicPos = 0;
					musicSeq = -1;

					continue;
				}

				musicRow = 1;
				musicSeq++;

				if (musicSeq >= 0 && musicSeq < musicSequence.Count)
				{
					musicHave = true;
					musicPat = musicSequence[musicSeq];
				}
				else
				{
					musicHave = false;
					musicPat = 0;
				}

				musicAt = 0;

				return;
			}
		}

		// True when the sequence slot matches and the row has moved past rowAbove.
		// Slots are counted from zero. Rows are counted from 1.
		bool CueMatches(ushort pos, ushort rowAbove)
		{
			if (!musicOk)
				return false;

			ushort ready = (ushort)((musicPos - 1) & 0xFFFF);

			return ready == pos && musicRow > rowAbove;
		}

		// True when no actor is in the middle of a wait, a slide, a cue, or a click.
		bool AllIdle()
		{
			for (int i = 0; i < actors.Count; i++)
			{
				if ((actors[i].Mode & 0xFF) != 0)
					return false;
			}

			return true;
		}

		// Run this actor until it reaches something that takes time, or it stops.
		// Commands that don't wait are chained on the same step.
		// The cap stops a list that never waits from spinning inside one tick.
		void StepActor(Actor actor)
		{
			for (int n = 0; n < 32; n++)
			{
				if (!actor.Active)
					return;

				int mode = actor.Mode & 0xFF;

				if (mode == Waiting)
				{
					// A count of zero means the delay is over and the list continues.
					// Otherwise this step uses up one count.
					if (actor.Param == 0)
					{
						RunOps(actor);

						continue;
					}

					actor.Param--;

					return;
				}

				if (mode == Sliding)
				{
					// Low byte counts down between moves. At zero, reload it from the
					// high half, move vertically, and use up one remaining move.
					ushort delay = (ushort)(actor.Extra & 0xFFFF);

					if ((delay & 0xFF) != 0)
					{
						actor.Extra = (actor.Extra & 0xFFFF0000) | (ushort)(delay - 1);

						return;
					}

					actor.Extra = (actor.Extra & 0xFFFF0000) | (actor.Extra >> 16);
					actor.Y += (short)actor.ParamHi;

					if (actor.Param == 0)
					{
						RunOps(actor);

						continue;
					}

					actor.Param--;

					return;
				}

				if (mode == Cueing)
				{
					// Extra distinguishes a music cue from a plain frame-count cue.
					bool hit = actor.Extra != 0
						? CueMatches(actor.Param, actor.ParamHi)
						: tick >= actor.Param;

					if (hit)
					{
						RunOps(actor);

						continue;
					}

					return;
				}

				if (mode == WaitClick)
				{
					// Resumes once a click was recorded and click handling has since been turned off.
					if (!clicksOn && click != 0)
					{
						RunOps(actor);

						continue;
					}

					return;
				}

				StopActor(actor);

				return;
			}

			StopActor(actor);
		}

		// Run commands until one of them waits or the actor stops.
		// The cap is the same guard as StepActor, for a long run of instant commands.
		void RunOps(Actor actor)
		{
			for (int n = 0; n < 200; n++)
			{
				if (!actor.Active || actor.Pc < 0 || actor.Pc >= actor.Commands.Count)
				{
					StopActor(actor);

					return;
				}

				var cmd = actor.Commands[actor.Pc];

				actor.Pc = cmd.Next ?? (actor.Pc + 1);

				switch (cmd.Op)
				{
					case "end":
						// This actor is finished. The pack keeps going.
						StopActor(actor);

						return;
					case "exit":
						// This actor is finished, and the pack ends on the next step boundary.
						exiting = true;
						StopActor(actor);

						return;
					case "hide":
						actor.Image = Hide;
						break;
					case "restore":
						actor.Image = Restore;
						break;
					case "image":
						// Opaque frame. No flag bits are set, so the stored value is the frame number.
						actor.Image = (ushort)(cmd.F & 0xFF);
						break;
					case "cut":
						// Same frame, but pen 0 is transparent.
						actor.Image = (ushort)(Cut | (cmd.F & 0xFF));
						break;
					case "freeze":
						// Draw the frame once, then keep redrawing that frozen picture.
						actor.Image = (ushort)(Freeze | (cmd.F & 0xFF));
						break;
					case "at":
						// x is a 16-pixel column. y is a pixel row.
						actor.X = (short)cmd.X;
						actor.Y = (short)cmd.Y;
						break;
					case "dx":
						actor.X += (short)cmd.Dx;
						break;
					case "dy":
						actor.Y += (short)cmd.Dy;
						break;
					case "wait":
						// Pause for N script steps. The step that issues the wait counts as the first.
						// Zero does not pause.
						actor.Mode = Waiting;
						actor.Param = (ushort)cmd.N;

						return;
					case "palette":
						LoadPalette(cmd.P);
						break;
					case "cue":
						// With a song position: wait until that slot is playing and the row has passed.
						// Without one: wait until the pack's frame counter reaches N.
						actor.Mode = Cueing;

						if (cmd.Pos.HasValue)
						{
							actor.Param = (ushort)cmd.Pos.Value;
							actor.ParamHi = (ushort)(cmd.Row ?? 0);
							actor.Extra = 1;
						}
						else
						{
							actor.Param = (ushort)cmd.N;
							actor.Extra = 0;
						}

						return;
					case "slide":
						// Move by dy pixels, N times. delay counts down between moves and
						// is reloaded after each one. Both countdowns start on this same step.
						actor.Mode = Sliding;
						actor.Param = (ushort)cmd.N;
						actor.ParamHi = (ushort)(short)cmd.Dy;
						actor.Extra = ((uint)(ushort)cmd.Reload << 16) | (ushort)cmd.Delay;

						return;
					case "loop":
						actor.Pc = cmd.To;
						break;
					case "wait_rand":
						// Pause for a random number of steps, trimmed by the mask.
						actor.Mode = Waiting;
						actor.Param = (ushort)(NextRand() & cmd.Mask);

						return;
					case "sfx":
						PlaySfx(cmd.Slot, cmd.Mask, cmd.Wav);
						break;
					case "call":
						actor.Ret = cmd.Ret;
						actor.Pc = cmd.To;
						break;
					case "ret":
						actor.Pc = actor.Ret;
						break;
					case "copper":
						// Turn the palette colour cycle on or off.
						copperOn = cmd.Flag & 0xFF;
						break;
					case "text":
						actor.Extra = (uint)cmd.Id;
						actor.Image = ShowText;
						break;
					case "switch":
						// Jump to one of the listed commands. The low four bits of a random word choose which.
						int pick = NextRand() & 0xF;

						if (cmd.Targets != null && pick < cmd.Targets.Count)
							actor.Pc = cmd.Targets[pick];
						else
							StopActor(actor);

						break;
					case "clicks":
						clicksOn = cmd.Arm;
						break;
					case "wait_button":
						actor.Mode = WaitClick;
						actor.Param = (ushort)cmd.Param;

						return;
					default:
						StopActor(actor);

						return;
				}
			}

			StopActor(actor);
		}

		static void StopActor(Actor actor)
		{
			actor.Mode = 0;
			actor.Active = false;
		}

		// Next word from this pack's random table.
		// The index mixes the running state with the frame counter, stays on an even
		// byte, and stays inside the first 16384 bytes. That word is then increased
		// by 14 and added into the running state. The increased word is the result.
		// A missing table reads as zero, so the result is 14.
		ushort NextRand()
		{
			int index = (rng + (ushort)clock) & 0x3FFE;
			ushort word = 0;

			if (rngBank != null && index + 2 <= rngBank.Length)
				word = (ushort)((rngBank[index] << 8) | rngBank[index + 1]);

			word += 14;
			rng += word;

			return word;
		}
	}
}
