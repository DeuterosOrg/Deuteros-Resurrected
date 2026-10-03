using Godot;
using System;

namespace Deuteros.Code
{
    public partial class Enums
    {
        [Serializable]
        public enum StellarBodies
        {
            none = 0,
            mercury = 100,
            venus = 200,
            earth = 300,
            mars = 400,
            asteroids = 500,
            jupiter = 600,
            saturn = 700,
            uranus = 800,
            neptune = 900,
            pluto = 1000,
            decuria = 1100,
            atlantic = 1200,
            pacific = 1300,
            chiron = 1400,
            cercops = 1500,
            cerberus = 1600,
            creon = 1700,
            mycenae = 1800,
            tyre = 1900,
            thebes = 2000,
            pompeii = 2100,
            jericho = 2200,
            crete = 2300,
            mari = 2400,
            nero = 2500,
            julius = 2600,
            romulus = 2700,
            remus = 2800,
            helios = 2900,
            lithos = 3000,
            burah = 3100,
            sulfurum = 3200,
            titanes = 3300,
            zargun = 3400,
            osme = 3500,
            radius = 3600,
            cambrian = 3700,
            cainozoic = 3800,
            paleozoic = 3900,
            alpha = 4000,
            beta = 4100,
            gamma = 4200,
            epsilon = 4300,
            zeta = 4400,
            //Moons
            the_moon = 5000,
            phobos = 5100,
            deimos = 5200,
            amalthea = 5300,
            io = 5400,
            europa = 5500,
            ganymede = 5600,
            callisto = 5700,
            leda = 5800,
            himalia = 5900,
            elara = 6000,
            pasiphae = 6100,
            mimas = 6200,
            encaladus = 6300,
            tethys = 6400,
            dione = 6500,
            rhea = 6600,
            titan = 6700,
            hyperion = 6800,
            iapetus = 6900,
            phoebe = 7000,
            miranda = 7100,
            ariel = 7200,
            umbriel = 7300,
            titania = 7400,
            oberon = 7500,
            triton = 7600,
            neried = 7700,
            nthree = 7800,
            nfour = 7900,
            charon = 8000,
            barent = 8100,
            baltic = 8200,
            circe = 8300,
            chimaera = 8400,
            cronus = 8500,
            chloe = 8600,
            calchas = 8700,
            cadmus = 8800,
            cybele = 8900,
            cupid = 9000,
            ur = 9100,
            tanis = 9200,
            memphis = 9300,
            karnak = 9400,
            gizeh = 9500,
            calah = 9600,
            noria = 9700,
            abydos = 9800,
            saqqara = 9900,
            petra = 10000,
            palmyra = 10100,
            babylon = 10200,
            troy = 10300,
            carthage = 10400,
            knossos = 10500,
            delphi = 10600,
            ephesus = 10700,
            corinth = 10800,
            athens = 10900,
            olympia = 11000,
            cuzco = 11100,
            septimus = 11200,
            augustus = 11300,
            claudius = 11400,
            hadrian = 11500,
            alumen = 11600,
            silex = 11700,
            chloros = 11800,
            argos = 11900,
            calx = 12000,
            vanadis = 12100,
            chronos = 12200,
            selene = 12300,
            bromos = 12400,
            kryptos = 12500,
            rubidos = 12600,
            niobe = 12700,
            kadmeia = 12800,
            tellus = 12900,
            iodes = 13000,
            xenos = 13100,
            caesius = 13200,
            rhenus = 13300,
            iris = 13400,
            platina = 13500,
            aurum = 13600,
            thallos = 13700,
            astatos = 13800,
            aktis = 13900,
            protos = 14000,
            prasios = 14100,
            tertiary = 14200,
            paleocene = 14300,
            eocene = 14400,
            oligocene = 14500,
            miocene = 14600,
            pliocene = 14700,
            silurian = 14800,
            delta = 14900,
            theta = 15000,
            iota = 15100,
            kappa = 15200,
            lambda = 15300,
            mu = 15400,
            nu = 15500,
            xi = 15600,
            omicron = 15700,
            pi = 15800,
            rho = 15900,
            sigma = 16000,
            upsilon = 16100,
            phi = 16200,
            chi = 16300,
            psi = 16400,
            omega = 16500,
            //Stars
            the_sun = 100000,
            proxima = 200000,
            centauri = 300000,
            barnard = 400000,
            lalande = 500000,
            sirius = 600000,
            cygni = 700000,
            procyon = 800000,
            tau_ceti = 900000
        }

        [Serializable]
        public enum PlanetColor
        {
            blue = 1,
            green = 2,
            red = 3,
            white = 4,
            white_blue = 5,
            white_green = 6,
            yellow = 7,
            asteroid
        }

        [Serializable]
        public enum PlanetStyle
        {
            lines = 1,
            whirl = 2,
            giant = 3,
            rings = 4,
            moon = 5,
            massive = 6,
            massive_rings = 7,
            asteroid = 8
        }

        [Serializable]
        public enum StationType
        {
            earth = 1,
            sol = 2,
            milkyway = 3
        }

        [Serializable]
        public enum ItemTypes
        {
            none = 0,
            iron = 1,
            titanium = 2,
            aluminium = 3,
            carbon = 4,
            copper = 5,
            hydrogen = 6,
            deuterium = 7,
            methane = 8,
            helium = 9,
            paladium = 10,
            platinum = 11,
            silver = 12,
            gold = 13,
            silica = 14,
            meh_fuel = 15,
            hed_fuel = 16,
            derrick = 17,
            s_chassis = 18,
            s_drive = 19,
            of_frame = 20,
            supply_pod = 21,
            tool_pod = 22,
            cryo_pod = 23,
            i_chassis = 24,
            i_drive = 25,
            a__c__c = 26,
            a__o__c = 27,
            bandaid = 28,
            s__d__m = 29,
            grapple = 30,
            d__f__c__c = 31,
            a__m__a = 32,
            hyperlight = 33,
            m__t__x = 34,
            m__f__l = 35,
            r_frame = 36,
            prejudice_torpedo_launcher = 37,
            commspod = 38,
            ios_drone = 39,
            g_chassis = 40,
            star_drive = 41,
            p__t__l = 42,
            star_drone = 43,
            prison_pod = 44,
            sonic_blaster = 45,
            pulse_blaster_laser = 46,
            alien_artifact = 47,
        }

        [Serializable]
        public enum GrappleItemTypes
        {
            Asteroid,
            UnknownItem
        }

        [Serializable]
        public enum UnknownItemTypes
        {
            AlienArtifact,
            CommsPod,
            Blazer
        }

        [Serializable]
        public enum StaffType
        {
            Research,
            Production,
            Marines
        }

        [Serializable]
        public enum ItemCategory
        {
            resource,
            item,
            //Does not show up in stores at all
            hidden
        }

        //Underscores in scene names represent a folder
        [Serializable]
        public enum Scenes
        {
            Earth_Ground,
            Earth_Research,
            Earth_Training,
            GroundMaterials,
            Production,
            SaveScreen,
            News,
            Store,
            StoreMTX,
            ShipBay,
            ShipInterior,
            ResourceMap,
            Bulletins,
            Overview,
            None,
            Station,
            IntroScreen,
            SelfDestruct
        }

        //Variables to pass to scenes to notify them of button types
        //E.G. The shuttle button can be for the ground, or in orbit
        [Serializable]
        public enum SceneVariables
        {
            Ground = 100,
            Orbit = 200,
            Shuttle = 300,
            Ship = 400,
        }

        [Serializable]
        public enum BackgroundSound
        {
            Earth_Ground,
            Earth_Training,
            Production,
            Resource,
            Research,
            ShuttleBay,
            Store
        }

        public enum StaffLevel_Researcher
        {
            Technician = 1,
            Doctor = 2,
            Professor = 3
        }

        public enum StaffLevel_Production
        {
            Apprentice = 1,
            Engineer = 2,
            Expert = 3
        }

        public enum StaffLevel_Marines
        {
            Pilot = 1,
            Captain = 2,
            Admiral = 3,
            Warlord = 4
        }

        public enum Game_Unlocks
        {
            First_Station_Segment,
            Shuttle_Unlock,
            Space_Stations,
            IOS_Attachments,
            Mass_Tranceiver,
            Self_Destruct,
            D_F_C_C,
            CommsPod,
            Interstellar_Travel
        }

        public enum SidePanel_Button_State_Animations
        {
            Static_Locked = 100,
            Static_Red = 200,
            Static_Yellow = 300,
            Static_Green = 400,
            Red = 500,
            Yellow = 600,
            Green = 700
        }

        public enum Ship_Types
        {
            Shuttle = 100,
            IOS = 200,
            SCG = 300
        }

        public enum Ship_States
        {
            Docking = 100,
            InTransit = 200,
            Landing = 300,
            TakingOff = 400,
            Launching = 500,
            Docked = 600,
            UnDocked = 700,
            CrewRepairing = 800
        }

        public enum Module_Types
        {
            None = 0,
            Tool = 100,
            Supply = 200,
            Cryo = 300
        }

        public enum Menu_Buttons
        {
            Empty = 100,
            Production = 200,
            Research = 300,
            Shuttle = 400,
            GroundMaterials = 500,
            Planet_Left = 600,
            Planet_Right = 700,
            SDM = 800,
            Ship_Bay = 900,
            Shuttle_Bay = 1000,
            Station_Left = 1100,
            Station_Right = 1200,
            Store = 1300,
            Training = 1400,
            GroundMaterials_Damaged = 1500,
            Store_Damaged = 1600
        }

        public enum BulletinTypes
        {
            IOS,
            IOS_Attachments,
            Methanoid_Laser,
            Self_Destruct,
            Matter_Transmitter,
            SCG_Drone,
            Hyperlight_Speed,
            Fuel_Weapon,
            Drone_Ships,
            Sol_Cleared,
            Rogue_Ship,
            Mutiny,
            Transmission1,
            Transmission2,
            Storm,
            Storm_Over,
            Mining_Dump,
            Meteor_Warning,
            Meteor_Strike,
            Sonic_Weapon,
            Eureka,
			None
		}

		public enum ModuleFrameText
        {
            Station_Deploy,
            Station_Deploy_Complete,
            RFrame_Deploy,
            RFrame_Deploy_Complete,
			Methanoid_Intro,
			Methanoid_Intro_With_Grapple,
            Methanoid_DeclareWar,
            Methanoid_TradeQuestion,
            Methanoid_Trade1,
            Methanoid_Trade2,
            Methanoid_No_Cargo,
            Methanoid_No_Trade,
            Methanoid_Trade_Too_Slow,
            Methanoid_War_Warning,
        }
        public enum BattleState
        {
            NotStarted = 1,
            FleetsAdvancing = 2,
            FleetsInBattle = 3,
            BattleEnded = 4
        }

        public enum PTLState
        {
            NotFired = 0,
            Firing = 1,
            Fired = 2
        }
    }
}
