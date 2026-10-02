using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Deuteros.Code.Objects;
using Deuteros.Code.Objects.GameData;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using static Deuteros.Code.Enums;

namespace Deuteros.Code.Utility
{
    // Versioned local saves. Only known model subtypes may be constructed from file data.
    public sealed class SaveStorage
    {
        public const int SlotCount = 5;
        private const int MaxBytes = 16 * 1024 * 1024;
        private readonly string directory;

        public SaveStorage(string directory) => this.directory = directory;

        private sealed class Document
        {
            [JsonProperty(Required = Required.Always)] public int Version { get; set; }
            [JsonProperty(Required = Required.Always)] public SaveFile Game { get; set; }
        }

        private sealed class ModelBinder : ISerializationBinder
        {
            private static readonly Type[] Types = { typeof(Planet), typeof(Earth), typeof(Shuttle), typeof(IOS),
                typeof(SCG), typeof(EnemyFleet), typeof(Asteroid), typeof(UnknownItem) };
            public Type BindToType(string assemblyName, string typeName)
            {
                if (assemblyName != null) throw new JsonSerializationException("Assembly names are not valid save types.");
                return Types.SingleOrDefault(t => t.Name == typeName)
                    ?? throw new JsonSerializationException("Unknown save type: " + typeName);
            }
            public void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                if (!Types.Contains(serializedType)) throw new JsonSerializationException("Unsupported model type.");
                assemblyName = null;
                typeName = serializedType.Name;
            }
        }

        private sealed class ModelContract : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization serialization)
            {
                // Avoid computed getters which consult the running game (including ship travel times).
                var properties = base.CreateProperties(type, serialization).Where(p => p.Writable).ToList();
                if (type == typeof(BaseData))
                    properties.RemoveAll(p => p.PropertyName == nameof(BaseData.ModuleFrameTexts)
                        || p.PropertyName == nameof(BaseData.BulletinTexts) || p.PropertyType == typeof(Godot.Color));
                var privateName = type == typeof(Staff) ? "ActionsTaken" : type == typeof(News) ? "NewsItems" : null;
                if (privateName != null)
                {
                    var property = base.CreateProperty(type.GetProperty(privateName, BindingFlags.Instance | BindingFlags.NonPublic), serialization);
                    property.Readable = property.Writable = true;
                    property.Required = Required.Always;
                    properties.Add(property);
                }
                foreach (var property in properties)
                {
                    // Version-1 saves written before engine damage have no flag; default healthy.
                    if (typeof(Ship).IsAssignableFrom(type) && property.PropertyName == nameof(Ship.EngineDamaged))
                        property.Required = Required.DisallowNull;
                    // Original allocation starts at zero; do not replay losses when upgrading a save.
                    else if (type == typeof(Staff) && property.PropertyName == nameof(Staff.AttritionCountdown))
                        property.Required = Required.DisallowNull;
                    else if (type == typeof(SpaceStation) && property.PropertyName == nameof(SpaceStation.SdmCountdown))
                        property.Required = Required.DisallowNull;
                    else if (type == typeof(SaveFile) && property.PropertyName == nameof(SaveFile.SdmTimerRemainder))
                        property.Required = Required.DisallowNull;
                    else if (type == typeof(SaveFile) && property.PropertyName == nameof(SaveFile.AlienTransmissions))
                        property.Required = Required.DisallowNull;
                    else if (property.Required == Required.Default) property.Required = Required.AllowNull;
                }
                return properties;
            }

            protected override JsonObjectContract CreateObjectContract(Type type)
            {
                var contract = base.CreateObjectContract(type);
                // Default creators let Json.NET register an object before reading children/$ref cycles.
                if (type == typeof(Planet)) contract.DefaultCreator = () => new Planet(StellarBodies.none, 0);
                if (type == typeof(Earth)) contract.DefaultCreator = () => new Earth(StellarBodies.earth, 0);
                if (type == typeof(SpaceStation)) contract.DefaultCreator = () => new SpaceStation(StellarBodies.none);
                if (type == typeof(Star)) contract.DefaultCreator = () => new Star(StellarBodies.none);
                if (type == typeof(PlanetResource)) contract.DefaultCreator = () => new PlanetResource(new List<Material>());
                if (type == typeof(ProductionItem)) contract.DefaultCreator = () => new ProductionItem(new Item { Research = new ResearchItem() });
                if (type == typeof(Material)) contract.DefaultCreator = () => new Material(ItemTypes.none, 0);
                if (type == typeof(UnknownItem)) contract.DefaultCreator = () => new UnknownItem(default);
                return contract;
            }
        }

        private static JsonSerializerSettings Settings() => new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            SerializationBinder = new ModelBinder(),
            ContractResolver = new ModelContract(),
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            MissingMemberHandling = MissingMemberHandling.Error,
            MaxDepth = 100,
        };

        public static string Serialize(SaveFile save)
        {
            Validate(save);
            return JsonConvert.SerializeObject(new Document { Version = 1, Game = save }, Settings());
        }

        public static SaveFile Deserialize(string json)
        {
            if (json == null || Encoding.UTF8.GetByteCount(json) > MaxBytes)
                throw new InvalidDataException("Save file is too large.");
            var document = JsonConvert.DeserializeObject<Document>(json, Settings());
            if (document == null || document.Version != 1)
                throw new InvalidDataException("Unsupported save version.");
            Validate(document.Game);
            var save = document.Game;
            ArtifactRecovery.RestoreLegacy(save);
            // Every legacy system was assigned at startup; a missing location means it was collected.
            // Keep those assignments and held cargo instead of spawning replacement segments.
            save.AlienTransmissions ??= new AlienTransmissions { AssignedStars = save.BaseGameData.Stars.Keys.ToList() };
            // Definitions are supplied by this game version, rather than embedded executable/UI data.
            save.BaseGameData.ModuleFrameTexts = CoreData.StaticGameData.ModuleFrameTexts;
            save.BaseGameData.BulletinTexts = CoreData.StaticGameData.BulletinTexts;
            foreach (var item in save.BaseGameData.ItemList.Where(i => string.IsNullOrWhiteSpace(i.ShortName)))
                item.ShortName = CoreData.StaticGameData.ItemList.FirstOrDefault(i => i.ItemType == item.ItemType)?.ShortName;
            save.TimeSkip = save.TimeSkipDay = false;
            save.TimeSkipStart = 0;
            return save;
        }

        public string SlotPath(int slot)
        {
            if (slot < 1 || slot > SlotCount) throw new ArgumentOutOfRangeException(nameof(slot));
            return Path.Combine(directory, $"slot-{slot}.json");
        }

        public bool Exists(int slot) => File.Exists(SlotPath(slot));

        public SaveFile Read(int slot)
        {
            var path = SlotPath(slot);
            if (new FileInfo(path).Length > MaxBytes) throw new InvalidDataException("Save file is too large.");
            return Deserialize(File.ReadAllText(path));
        }

        public void Write(int slot, SaveFile save)
        {
            var path = SlotPath(slot);
            var bytes = Encoding.UTF8.GetBytes(Serialize(save));
            if (bytes.Length > MaxBytes) throw new InvalidDataException("Save file is too large.");
            Directory.CreateDirectory(directory);
            var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    stream.Write(bytes);
                    stream.Flush(true);
                }
                if (File.Exists(path)) File.Replace(temporary, path, path + ".bak");
                else File.Move(temporary, path);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }

        public static void Validate(SaveFile save)
        {
            void Require(bool condition, string field)
            {
                if (!condition) throw new InvalidDataException("Invalid save: " + field + ".");
            }
            Require(save?.BaseGameData?.Planets != null && save.BaseGameData.Stars != null, "world");
            Require(double.IsFinite(save.SdmTimerRemainder) && save.SdmTimerRemainder >= 0 && save.SdmTimerRemainder < 1, "SDM timer phase");
            var data = save.BaseGameData;
            Require(data.Planets.TryGetValue(StellarBodies.earth, out var earthPlanet) && earthPlanet is Earth, "Earth");
            Require(data.Planets.Keys.OrderBy(k => k).SequenceEqual(CoreData.StaticGameData.Planets.Keys.OrderBy(k => k))
                && data.Stars.Keys.OrderBy(k => k).SequenceEqual(CoreData.StaticGameData.Stars.Keys.OrderBy(k => k)), "world locations");
            Require(data.Stars.All(p => p.Value != null && p.Value.StarId == p.Key), "stars");
            if (save.AlienTransmissions is { } transmissions)
            {
                Require(transmissions.AssignedStars != null && transmissions.PendingLocations != null, "alien transmission lists");
                Require(transmissions.AssignedStars.Contains(StellarBodies.the_sun)
                    && transmissions.AssignedStars.Distinct().Count() == transmissions.AssignedStars.Count
                    && transmissions.AssignedStars.All(data.Stars.ContainsKey), "assigned artifact systems");
                Require(transmissions.PendingLocations.All(location => data.Planets.TryGetValue(location, out var planet)
                        && planet != null && planet.ParentStar != StellarBodies.the_sun
                        && transmissions.AssignedStars.Contains(planet.ParentStar))
                    && transmissions.PendingLocations.Select(location => data.Planets[location].ParentStar).Distinct().Count()
                        == transmissions.PendingLocations.Count, "pending artifact locations");
            }
            Require(save.EnemyStarCursor >= 0 && save.EnemyStarCursor < data.Stars.Count, "enemy scheduling cursor");
            Require(data.Planets.ContainsKey(save.CurrentPlanet), "current planet");
            Require(save.GameConfig != null && save.News != null && save.Unlocks != null && save.Ships != null, "game state");
            Require(data.ItemList != null && data.ItemList.All(i => i != null)
                && data.ItemList.Select(i => i.ItemType).Distinct().Count() == data.ItemList.Count, "items");
            Require(data.PersonNames?.Count > 0 && save.NextPersonIndex >= 0 && save.NextPersonIndex <= data.PersonNames.Count, "staff names");
            Require(data.ResourceRate_Per_Derrick != null && data.ResourceLevels_Survey_Multiplier != null, "mining rules");
            void CheckFactory(Factory factory)
            {
                Require(factory?.ProductionQueue != null && factory.ProductionQueue.All(p => p?.Product != null
                    && data.ItemList.Contains(p.Product)), "factory queue");
                Require(factory.ProductionQueue.Count(p => p.Active) <= 1, "active production");
            }
            void CheckResource(Deuteros.Code.Platform.Resource resource)
            {
                Require(resource?.Staff?.Length == 4 && resource.Stores?.Items != null
                    && resource.Stores.MTX?.SendItems != null && resource.Stores.MTX.BalanceItems != null, "inventory");
                Require(resource.Stores.Items.All(pair => Enum.IsDefined(typeof(ItemTypes), pair.Key) && pair.Value >= 0), "stock quantities");
            }
            foreach (var pair in data.Planets)
            {
                var planet = pair.Value;
                Require(planet != null && planet.PlanetId == pair.Key && data.Stars.ContainsKey(planet.ParentStar), "planet identity");
                CheckResource(planet.PlanetResources);
                Require(planet.PlanetResources.Materials != null && planet.PlanetResources.Materials.All(m => m != null
                    && data.ResourceRate_Per_Derrick.ContainsKey(m.MaterialType)
                    && data.ResourceLevels_Survey_Multiplier.ContainsKey(m.MaterialType)), "deposits");
                Require(planet.Station != null && planet.Station.PlanetId == pair.Key, "station identity");
                Require(planet.Station.SdmCountdown >= 0 && planet.Station.SdmCountdown <= 255, "SDM countdown");
                CheckResource(planet.Station.Resources);
                CheckFactory(planet.Station.Factory);
            }
            var earth = (Earth)earthPlanet;
            Require(earth.TrainingData != null, "training");
            CheckFactory(earth.Factory);
            Require(earth.CurrentResearchItem == null || data.ItemList.Any(i => ReferenceEquals(i.Research, earth.CurrentResearchItem)), "current research");
            Require(save.Ships.All(s => s != null) && save.Ships.Select(s => s.ShipID).Distinct().Count() == save.Ships.Count, "ship identities");
            foreach (var ship in save.Ships)
            {
                Require(ship is Shuttle || ship is IOS || ship is SCG, "ship type");
                Require(data.Planets.ContainsKey(ship.PlanetLocation), "ship location");
                Require(ship.DestinationPlanetLocation == StellarBodies.none || data.Planets.ContainsKey(ship.DestinationPlanetLocation), "ship destination");
                // Enemy fleets intentionally have no cargo modules/ACC in the current model.
                if (ship is EnemyFleet) continue;
                Require((ship is Shuttle && ship.ShipType == Ship_Types.Shuttle)
                    || (ship is IOS && ship.ShipType == Ship_Types.IOS)
                    || (ship is SCG && ship.ShipType == Ship_Types.SCG), "hull type");
                Require(Enum.IsDefined(typeof(Ship_States), ship.ShipState), "ship state");
                Require(ship is not InterStellarShip || ship.ShipState != Ship_States.InTransit
                    || data.Planets.ContainsKey(ship.DestinationPlanetLocation), "in-flight destination");
                Require(ship.Modules != null && ship.Modules.All(m => m != null && m.ItemCount >= 0), "ship modules");
                Require(ship.ACC == null || (ReferenceEquals(ship, ship.ACC.Ship) && ship.ACC.SourceItems != null && ship.ACC.DestinationItems != null), "ACC owner");
                if (ship.ACC != null)
                    Require(ship.ACC.CurrentSource >= ItemTypes.iron && ship.ACC.CurrentSource <= ItemTypes.hed_fuel
                        && ship.ACC.CurrentDestination >= ItemTypes.iron && ship.ACC.CurrentDestination <= ItemTypes.hed_fuel, "ACC cycle cursor");
            }
        }
    }
}
