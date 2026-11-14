using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using Godot;
using static Deuteros.Code.Enums;
using MenuControl = Deuteros.Code.Platform.MenuButton;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task RunMenuArtworkRegressions()
        {
            foreach (var body in new[] { StellarBodies.the_moon, StellarBodies.mars })
                await CheckAsync($"Damaged {body} ground services show unavailable artwork without blocking orbital services", () => DamagedBaseArtwork(body));
            await CheckAsync("Damaged ground-service controls cannot navigate through retained signals", DamagedBaseNavigation);
            await CheckAsync("Completing a shuttle repair restores open menu services on the same day", RepairRestoresMenu);
            await CheckAsync("Unbuilt bases stay blank and changing planets replaces damaged artwork", MissingBaseMenu);
            await CheckAsync("Methanoid orbit indicator follows station ownership without reopening the interior", MethanoidMenuOwnership);
            await CheckAsync("Methanoid indicator clears in transit and outside an occupied station orbit", MethanoidMenuContexts);
            await CheckAsync("Daily service availability refresh preserves the bulletin header", MenuRefreshPreservesHeader);
        }

        private MainMenu OpenBaseMenu(StellarBodies body, bool damaged = true, int parts = 2)
        {
            InitializeUi();
            Save.CurrentPlanet = body;
            var planet = Save.BaseGameData.Planets[body];
            planet.ActiveMethanoid = false;
            planet.Station.Built = true;
            planet.BaseBuildParts = parts;
            planet.BaseDamaged = damaged;
            GameCore.SingletonInstance.ChangeScene(Scenes.ShipBay, new List<SceneVariables> { SceneVariables.Orbit, SceneVariables.Shuttle });
            return ActiveScreen<MainMenu>();
        }

        private void CheckDamagedMenu(MainMenu menu)
        {
            foreach (var (path, region) in new[] { ("B5", new Rect2(122, 366, 48, 32)), ("B6", new Rect2(122, 398, 48, 32)) })
            {
                var button = menu.GetNode<MenuControl>("MainButtons/" + path);
                Equal(true, button.Disabled, path + " damaged service disabled");
                Equal(Scenes.None, button.TargetScene, path + " retains no destination");
                Equal(0, button.SceneVariables.Count, path + " retains no context");
                Equal(true, button.ClickActions == null, path + " retains no action");
                var image = button.GetNode<TextureRect>("Sprite");
                Equal(true, image.Texture is AtlasTexture, path + " shows the supplied crossed-out reference");
                var atlas = (AtlasTexture)image.Texture;
                Equal(region, atlas.Region, path + " reference excludes annotation");
                Equal(new Vector2(24, 16), image.Size, path + " keeps the menu slot size");
            }
            Equal(false, menu.GetNode<MenuControl>("MainButtons/A1").Disabled, "orbital production remains available");
            Equal(false, menu.GetNode<MenuControl>("MainButtons/B1").Disabled, "orbital stores remain available");
            Equal(false, menu.GetNode<MenuControl>("MainButtons/A5").Disabled, "surface shuttle bay remains available for repair");
        }

        private async Task DamagedBaseArtwork(StellarBodies body)
        {
            var menu = OpenBaseMenu(body);
            await InputFrames();
            CheckDamagedMenu(menu);
            if (body == StellarBodies.the_moon) await CaptureDisplayEvidence("damaged-moon-menu");
        }

        private async Task DamagedBaseNavigation()
        {
            var menu = OpenBaseMenu(StellarBodies.the_moon);
            await InputFrames();
            foreach (var path in new[] { "B5", "B6" })
            {
                var control = menu.GetNode<MenuControl>("MainButtons/" + path);
                control._Pressed();
                control.EmitSignal(BaseButton.SignalName.Pressed);
                Equal(Scenes.ShipBay, GameCore.SingletonInstance.currentScene, "disabled service must not retain a navigation callback");
            }
        }

        private async Task RepairRestoresMenu()
        {
            var menu = OpenBaseMenu(StellarBodies.the_moon);
            var ship = new Shuttle
            {
                ShipType = Ship_Types.Shuttle, PlanetLocation = StellarBodies.the_moon,
                StarLocation = StellarBodies.the_sun, ShipState = Ship_States.CrewRepairing,
                OnGround = true, StartRepairDay = Save.CurrentDay,
                Modules = new List<ShipModule> { new ShipModule { ItemStored = ItemTypes.bandaid, ItemCount = 1 } }
            };
            Save.Ships.Add(ship);
            await InputFrames();
            CheckDamagedMenu(menu);
            AdvanceTickDay();
            Equal(true, menu.GetNode<MenuControl>("MainButtons/B6").Disabled, "repair is still underway");
            AdvanceTickDay();
            Equal(false, Save.BaseGameData.Planets[StellarBodies.the_moon].BaseDamaged, "normal shuttle update completes repair");
            Equal(ItemTypes.none, ship.Modules[0].ItemStored, "one repair kit consumed");
            foreach (var path in new[] { "B5", "B6" })
            {
                Equal(false, menu.GetNode<MenuControl>("MainButtons/" + path).Disabled, "completed repair enables " + path);
                Equal(false, menu.GetNode<TextureRect>("MainButtons/" + path + "/Sprite").Texture is AtlasTexture, "repair restores the normal artwork");
            }
            menu.GetNode<MenuControl>("MainButtons/B6").EmitSignal(BaseButton.SignalName.Pressed);
            Equal(Scenes.Store, GameCore.SingletonInstance.currentScene, "restored store control navigates normally");
            Equal(true, GameCore.SingletonInstance.SceneVariables.Contains(SceneVariables.Ground), "restored destination is ground stores");
        }

        private async Task MissingBaseMenu()
        {
            var menu = OpenBaseMenu(StellarBodies.the_moon, parts: 1);
            await InputFrames();
            foreach (var path in new[] { "B5", "B6" })
            {
                var control = menu.GetNode<MenuControl>("MainButtons/" + path);
                Equal(true, control.Disabled, "incomplete base has no service");
                Equal(true, control.GetNode<TextureRect>("Sprite").Texture.ResourcePath.EndsWith("/Empty.png"), "incomplete base is blank, not damaged");
            }
            OpenBaseMenu(StellarBodies.the_moon);
            OpenBaseMenu(StellarBodies.mars, damaged: false);
            foreach (var path in new[] { "B5", "B6" })
            {
                var control = menu.GetNode<MenuControl>("MainButtons/" + path);
                Equal(false, control.Disabled, "new healthy planet has services");
                Equal(false, control.GetNode<TextureRect>("Sprite").Texture is AtlasTexture, "damage artwork does not leak across planets");
            }
        }

        private async Task<ShipInterior> OpenMethanoidOrbit()
        {
            var interior = await OpenInterior(Ship_Types.IOS);
            var planet = Save.BaseGameData.Planets[StellarBodies.jupiter];
            planet.ActiveMethanoid = true;
            planet.Station.Built = true;
            interior.Ship.PlanetLocation = StellarBodies.jupiter;
            interior.Ship.ShipState = Ship_States.UnDocked;
            GameCore.SingletonInstance.UpdateMenuButtons(false, true);
            AdvanceTickDay();
            Equal(StellarBodies.jupiter, interior.CurrentPlanet.PlanetId, "interior display follows the staged orbit");
            return interior;
        }

        private TextureRect MethanoidIndicator()
        {
            var indicator = ActiveScreen<MainMenu>().GetNodeOrNull<TextureRect>("MethanoidIndicator");
            Equal(true, indicator != null, "menu includes the Methanoid reference graphic");
            return indicator;
        }

        private async Task MethanoidMenuOwnership()
        {
            var interior = await OpenMethanoidOrbit();
            var indicator = MethanoidIndicator();
            Equal(true, indicator.Visible, "enemy-held orbital station shows the face");
            Equal(Control.MouseFilterEnum.Ignore, indicator.MouseFilter, "ownership image does not intercept controls");
            Equal(new Vector2(0, 135), indicator.Position, "face occupies the reference menu row");
            Equal(new Vector2(48, 16), indicator.Size, "face spans two slots without covering adjacent rows");
            Equal(new Rect2(74, 334, 96, 32), ((AtlasTexture)indicator.Texture).Region, "face excludes screenshot annotation");
            await CaptureDisplayEvidence("methanoid-orbit-menu");
            Save.BaseGameData.Planets[StellarBodies.jupiter].ActiveMethanoid = false;
            AdvanceTickDay();
            Equal(false, indicator.Visible, "friendly capture clears the indicator on the open interior");
            Equal(false, ActiveScreen<MainMenu>().GetNode<MenuControl>("MainButtons/A1").Disabled, "capture restores station services");
        }

        private async Task MethanoidMenuContexts()
        {
            var interior = await OpenMethanoidOrbit();
            var indicator = MethanoidIndicator();
            interior.Ship.ShipState = Ship_States.InTransit;
            GameCore.SingletonInstance.UpdateMenuButtons(false, true);
            Equal(false, indicator.Visible, "departing orbit clears the old owner");
            interior.Ship.ShipState = Ship_States.UnDocked;
            Save.BaseGameData.Planets[StellarBodies.jupiter].Station.Built = false;
            GameCore.SingletonInstance.UpdateMenuButtons(false, true);
            Equal(false, indicator.Visible, "no occupied station means no station-owner graphic");
            GameCore.SingletonInstance.ChangeScene(Scenes.Overview, new List<SceneVariables>());
            Equal(false, indicator.Visible, "overview does not retain an interior's owner graphic");
            var shuttleInterior = await OpenInterior(Ship_Types.Shuttle);
            Save.BaseGameData.Planets[shuttleInterior.Ship.PlanetLocation].ActiveMethanoid = true;
            GameCore.SingletonInstance.UpdateMenuButtons(true, false);
            Equal(false, indicator.Visible, "ground shuttle is not an orbital encounter");
        }

        private async Task MenuRefreshPreservesHeader()
        {
            var menu = OpenBaseMenu(StellarBodies.the_moon, damaged: false);
            Save.BaseGameData.BulletinTexts[BulletinTypes.Matter_Transmitter].BulletinText = "Report.";
            GameCore.SingletonInstance.ShowBulletin(BulletinTypes.Matter_Transmitter);
            await FinishBulletin(ActiveScreen<Bulletins>());
            Equal("News Bulletins", menu.Location.Text, "bulletin owns its header");
            Save.BaseGameData.Planets[StellarBodies.the_moon].BaseDamaged = true;
            AdvanceTickDay();
            CheckDamagedMenu(menu);
            Equal("News Bulletins", menu.Location.Text, "service availability cannot overwrite the current screen title");
        }
    }
}
