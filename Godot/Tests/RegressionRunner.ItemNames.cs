using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Utility;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task ScgItemNames(bool legacy)
        {
            InitializeUi();
            // A loaded save has its own item objects; do not mutate the definition table.
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            var names = new[] { (ItemTypes.g_chassis, "SCG Chassis"), (ItemTypes.star_drive, "SCG Drive") };
            foreach (var (type, _) in names)
            {
                var item = GameCore.SingletonInstance.GameData.GetItem(type);
                item.Locked = item.Research.Locked = false;
                item.Research.Researched = true;
                item.Research.ResearchOrder = Save.BaseGameData.ItemList.Where(i => i.Research != null).Max(i => i.Research.ResearchOrder) + 1;
                GameCore.Earth.Station.Resources.Stores[type] = 7;
                if (legacy) item.ShortName = type == ItemTypes.g_chassis ? null : "";
            }
            GameCore.SingletonInstance.GameData.GetItem(ItemTypes.derrick).ShortName = "Saved Rig";
            if (legacy) GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Equal("Saved Rig", GameCore.SingletonInstance.GameData.GetItem(ItemTypes.derrick).ShortName, "loading preserves an existing display name");
            GameCore.Earth.Station.Built = GameCore.Earth.Station.MtxInstalled = true;
            GameCore.Earth.Station.BuildParts = 8;
            foreach (var (type, name) in names)
            {
                var item = GameCore.SingletonInstance.GameData.GetItem(type);
                Equal(true, item.Research.Researched, "name repair preserves research");
                Equal(7, GameCore.Earth.Station.Resources.Stores[type], "name repair preserves inventory");
                var queue = GameCore.Earth.Station.Factory.ProductionQueue;
                queue.Clear();
                queue.Add(new ProductionItem(item) { Active = true, Production_Complete = 1 });
                GameCore.SingletonInstance.ChangeScene(Scenes.Production, new List<SceneVariables> { SceneVariables.Orbit });
                var production = ActiveScreen<Production>();
                production.DrawData();
                Equal(name, production.GetNode<Label>("Labels/ProductionNameLabel").Text, "active SCG product has a visible name");
                await InputFrames();
                if (type == ItemTypes.g_chassis) await CaptureDisplayEvidence("scg-product-name-" + legacy);
                GameCore.SingletonInstance.ChangeScene(Scenes.Station, new List<SceneVariables> { SceneVariables.Orbit });
                Equal("Product\n" + name, ActiveScreen<Station>().GetNode<Label>("Labels/Product").Text, "station does not report active construction as None");
                GameCore.Earth.Station.Resources.Stores.AlternativeView = true;
                GameCore.SingletonInstance.ChangeScene(Scenes.Store, new List<SceneVariables> { SceneVariables.Orbit });
                var store = ActiveScreen<Deuteros.Code.Platform.Screens.Store>();
                Equal(true, store.ResourceListLabel.Text.Contains(name), "Stores names the stock row");
                var row = store.MTX.GetNode<BoxContainer>("Config/ResourceList/ResourceContainer").GetChildren().OfType<MTXRow>().Single(r => r.ItemType == type);
                Equal(name, row.RowName.Text, "MTX names the transfer row");
            }
        }
    }
}
