using Deuteros.Code.Objects;
using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Deuteros.Code.Platform.Screens
{
	public partial class Training : Base.BaseSubScene
	{
		public AudioStream ButtonSound { get; set; }
		public AudioStream DoorSound { get; set; }
		private AudioStreamPlayer DoorButtonSound { get; set; }
		private AudioStreamPlayer DoorSoundPlayer;
		private readonly HashSet<AnimatedSprite2D> _movingDoors = new();

		private CanvasModulate _modulateNode;
		private bool _isDimmed = false;

		private AnimatedSprite2D ResearchDoorAnimation;
		private AnimatedSprite2D ProductionDoorAnimation;
		private AnimatedSprite2D MarinesDoorAnimation;

		private List<RepeatingButton> ResearchButtons;
		private List<RepeatingButton> ProductionButtons;
		private List<RepeatingButton> MarinesButtons;

		private const string Closed_Animation_Name = "closed";
		private const string Opening_Animation_Name = "opening";
		private const string Open_Animation_Name = "open";
		private const string Close_Animation_Name = "close";

		public override void _ExitTree()
		{
			foreach (var door in _movingDoors)
			{
				door.Stop();
				GameCore.UnLockScreen();
			}
			_movingDoors.Clear();
			DoorButtonSound?.Stop();
			DoorSoundPlayer?.Stop();
			base._ExitTree();
			if (_modulateNode != null)
				_modulateNode.Color = new Color(1, 1, 1);
			_isDimmed = false;
		}

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			base._Ready();

			// Use an existing CanvasModulate in the scene if present, otherwise create one
			_modulateNode = GetNodeOrNull<CanvasModulate>("CanvasModulate");
			if (_modulateNode == null)
			{
				_modulateNode = new CanvasModulate();
				AddChild(_modulateNode);
			}
			_modulateNode.Color = new Color(1, 1, 1);

			var researchButtonDown = GetNode<RepeatingButton>("Doors/Research/MinusButton/ResearchMinusButton");
			var researchButtonUp = GetNode<RepeatingButton>("Doors/Research/PlusButton/ResearchPlusButton");
			var productionButtonDown = GetNode<RepeatingButton>("Doors/Production/MinusButton/ProductionMinusButton");
			var productionButtonUp = GetNode<RepeatingButton>("Doors/Production/PlusButton/ProductionPlusButton");
			var marinesButtonDown = GetNode<RepeatingButton>("Doors/Marines/MinusButton/MarinesMinusButton");
			var marinesButtonUp = GetNode<RepeatingButton>("Doors/Marines/PlusButton/MarinesPlusButton");

			var lightSwitch = GetNode<RepeatingButton>("Doors/LightSwitchButton");

			researchButtonDown.Connect("pressed", new Callable(this, nameof(ResearchMinusButton_Pressed)));
			researchButtonUp.Connect("pressed", new Callable(this, nameof(ResearchPlusButton_Pressed)));
			productionButtonDown.Connect("pressed", new Callable(this, nameof(ProductionMinusButton_Pressed)));
			productionButtonUp.Connect("pressed", new Callable(this, nameof(ProductionPlusButton_Pressed)));
			marinesButtonDown.Connect("pressed", new Callable(this, nameof(MarinesMinusButton_Pressed)));
			marinesButtonUp.Connect("pressed", new Callable(this, nameof(MarinesPlusButton_Pressed)));
			lightSwitch.Connect("pressed", new Callable(this, nameof(LightSwitchButton_Pressed)));
			ResearchButtons = new List<RepeatingButton>
				{
					researchButtonDown,
					researchButtonUp
				};

			ProductionButtons = new List<RepeatingButton>
				{
					productionButtonDown,
					productionButtonUp
				};

			MarinesButtons = new List<RepeatingButton>
				{
					marinesButtonDown,
					marinesButtonUp
				};

			ResearchDoorAnimation = GetNode<Node2D>("Doors/Research/TrainingDoors").GetNode<AnimatedSprite2D>("DoorAnimation");

			ProductionDoorAnimation = GetNode<Node2D>("Doors/Production/TrainingDoors").GetNode<AnimatedSprite2D>("DoorAnimation");

			MarinesDoorAnimation = GetNode<Node2D>("Doors/Marines/TrainingDoors").GetNode<AnimatedSprite2D>("DoorAnimation");

			ResearchDoorAnimation.AnimationFinished += () => DoorAnimation_AnimationFinished(ResearchDoorAnimation);
			ProductionDoorAnimation.AnimationFinished += () => DoorAnimation_AnimationFinished(ProductionDoorAnimation);
			MarinesDoorAnimation.AnimationFinished += () => DoorAnimation_AnimationFinished(MarinesDoorAnimation);

			ResearchDoorAnimation.Animation = GameCore.Earth.TrainingData.ResearcherLocked ? Closed_Animation_Name : Open_Animation_Name;
			ProductionDoorAnimation.Animation = GameCore.Earth.TrainingData.ProductionLocked ? Closed_Animation_Name : Open_Animation_Name;
			MarinesDoorAnimation.Animation = GameCore.Earth.TrainingData.MarinesLocked ? Closed_Animation_Name : Open_Animation_Name;

			ButtonSound = (AudioStream)ResourceLoader.Load("res://Sounds/Button/sTrainingRoom_Button.wav");
			DoorSound = (AudioStream)ResourceLoader.Load("res://Sounds/Button/sTrainingRoom_Door.wav");

			DoorButtonSound = this.GetNode<AudioStreamPlayer>("SoundPlayer");
			DoorButtonSound.Stream = ButtonSound;
			DoorSoundPlayer = GetNode<AudioStreamPlayer>("DoorSoundPlayer");
			DoorSoundPlayer.Stream = DoorSound;

			DrawData();
		}

		private void DoorAnimation_AnimationFinished(AnimatedSprite2D door)
		{
			if (!_movingDoors.Remove(door)) return;
			door.Animation = door.Animation == Close_Animation_Name ? Closed_Animation_Name : Open_Animation_Name;
			GameCore.UnLockScreen();
		}

		// Called every update.
		public override void _Draw()
		{
		}

		//Triggered from gamecore
		protected override async void DayTick(uint previousDay, uint currentDay)
		{
			DrawData();
		}

		public async void DrawData()
		{
			var moving = UpdateDoor(ResearchDoorAnimation, GameCore.Earth.TrainingData.ResearcherLocked);
			moving |= UpdateDoor(ProductionDoorAnimation, GameCore.Earth.TrainingData.ProductionLocked);
			moving |= UpdateDoor(MarinesDoorAnimation, GameCore.Earth.TrainingData.MarinesLocked);
			// Original $2E4FE dispatches one cue for the simultaneous door transitions.
			if (moving) DoorSoundPlayer.Play();

			ProductionButtons.ForEach(T => T.Disabled = GameCore.Earth.TrainingData.ProductionLocked);
			MarinesButtons.ForEach(T => T.Disabled = GameCore.Earth.TrainingData.MarinesLocked);
			ResearchButtons.ForEach(T => T.Disabled = GameCore.Earth.TrainingData.ResearcherLocked);

			GetNode<Label>("TraineeCountLabel").Text = (GameCore.Earth.TrainingData.AvailableTrainees - (GameCore.Earth.TrainingData.ResearcherTrainingCount + GameCore.Earth.TrainingData.ProductionTrainingCount + GameCore.Earth.TrainingData.MarinesTrainingCount)).ToString();
			GetNode<Label>("Doors/Research/ResearchTrainingCountLabel").Text = GameCore.Earth.TrainingData.ResearcherTrainingCount.ToString();
			GetNode<Label>("Doors/Production/ProductionTrainingCountLabel").Text = GameCore.Earth.TrainingData.ProductionTrainingCount.ToString();
			GetNode<Label>("Doors/Marines/MarinesTrainingCountLabel").Text = GameCore.Earth.TrainingData.MarinesTrainingCount.ToString();
		}

		private bool UpdateDoor(AnimatedSprite2D door, bool locked)
		{
			if (door.Animation != (locked ? Open_Animation_Name : Closed_Animation_Name)) return false;
			if (_movingDoors.Add(door)) GameCore.LockScreen();
			door.Play(locked ? Close_Animation_Name : Opening_Animation_Name);
			return true;
		}

		public void ResearchMinusButton_Pressed()
		{
			if (!GameCore.Earth.TrainingData.ResearcherLocked && GameCore.Earth.TrainingData.ResearcherTrainingCount > 0)
			{
				GameCore.Earth.TrainingData.ResearcherTrainingCount--;
			}

			if (!ResearchButtons.Any(T => T.IsRepeating))
				DoorButtonSound.Play();

			DrawData();
		}

		public void ResearchPlusButton_Pressed()
		{
			var earth = GameCore.GetPlanet<Earth>(Enums.StellarBodies.earth);

			if (!GameCore.Earth.TrainingData.ResearcherLocked && GameCore.Earth.TrainingData.ResearcherTrainingMax > GameCore.Earth.TrainingData.ResearcherTrainingCount &&
			(
				earth.ResearchStaff == null
				||
				(earth.ResearchStaff.Count + GameCore.Earth.TrainingData.ResearcherTrainingCount) < GameCore.Earth.TrainingData.ResearcherMaxCount)
				)
			{
				GameCore.Earth.TrainingData.ResearcherTrainingCount++;
			}

			if (!ResearchButtons.Any(T => T.IsRepeating))
				DoorButtonSound.Play();

			DrawData();
		}

		public void ProductionMinusButton_Pressed()
		{
			if (!GameCore.Earth.TrainingData.ProductionLocked && GameCore.Earth.TrainingData.ProductionTrainingCount > 0)
			{
				GameCore.Earth.TrainingData.ProductionTrainingCount--;
			}

			if (!ProductionButtons.Any(T => T.IsRepeating))
				DoorButtonSound.Play();

			DrawData();
		}

		public void ProductionPlusButton_Pressed()
		{
			if (!GameCore.Earth.TrainingData.ProductionLocked &&
				GameCore.Earth.TrainingData.ProductionTrainingCount < GameCore.Earth.TrainingData.ProductionTrainingMax &&
				(GameCore.Earth.Factory.Builder == null ? 0 : GameCore.Earth.Factory.Builder.Count) + GameCore.Earth.TrainingData.ProductionTrainingCount < GameCore.Earth.TrainingData.ProductionMaxCount)
			{
				GameCore.Earth.TrainingData.ProductionTrainingCount++;
			}

			if (!ProductionButtons.Any(T => T.IsRepeating))
				DoorButtonSound.Play();

			DrawData();
		}

		public void MarinesMinusButton_Pressed()
		{
			if (!GameCore.Earth.TrainingData.MarinesLocked &&
				GameCore.Earth.TrainingData.MarinesTrainingCount > 0)
			{
				GameCore.Earth.TrainingData.MarinesTrainingCount--;
			}

			if (!MarinesButtons.Any(T => T.IsRepeating))
				DoorButtonSound.Play();

			DrawData();
		}

		public void MarinesPlusButton_Pressed()
		{
			if (!GameCore.Earth.TrainingData.MarinesLocked &&
				GameCore.Earth.TrainingData.MarinesTrainingMax > GameCore.Earth.TrainingData.MarinesTrainingCount)
			{
				GameCore.Earth.TrainingData.MarinesTrainingCount++;
			}

			if (!MarinesButtons.Any(T => T.IsRepeating))
				DoorButtonSound.Play();

			DrawData();
		}

		public void LightSwitchButton_Pressed()
		{
			DoorButtonSound.Play();
			_isDimmed = !_isDimmed;
			_modulateNode.Color = _isDimmed ? new Color(0.5f, 0.5f, 0.5f) : new Color(1, 1, 1);
			// Compensate UI so text/buttons stay at full brightness
			var ui = GetNode<Node2D>("Doors");
			if (ui != null)
				ui.Modulate = _isDimmed ? new Color(2f, 2f, 2f) : new Color(1f, 1f, 1f);
		}
	}
}