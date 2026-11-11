using Godot;

namespace Deuteros.Code.Platform.Screens
{
	public partial class CutscenePlayer
	{
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
	}
}
