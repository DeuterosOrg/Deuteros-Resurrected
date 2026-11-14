using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Helpers;
using Godot;
using System.Linq;

namespace Deuteros.Code.Platform
{
    public partial class MenuButton : SceneChangeButton
    {
        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            base._Ready();
        }

        public void SetButtonType(Enums.Menu_Buttons buttonType)
        {
            var MenuButtonImageTextureRect = GetNode<TextureRect>("Sprite");
            var extension = buttonType == Enums.Menu_Buttons.GroundMaterials_Damaged || buttonType == Enums.Menu_Buttons.Store_Damaged ? ".tres" : ".png";
            MenuButtonImageTextureRect = SpriteManager.LoadImageToTextureRect("Sprites/Buttons/MainMenu/" + buttonType.ToString() + extension, MenuButtonImageTextureRect);
        }

        public override void _Pressed()
        {
            if (ClickActions != null && ClickActions.Count > 0)
                foreach (var item in ClickActions)
                    item.Invoke();

            base._Pressed();
        }
    }
}
