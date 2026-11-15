namespace Deuteros.Code.Platform.Screens
{
	public partial class CutscenePlayer
	{
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
