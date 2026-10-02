using Godot;
using System;
using System.Linq;

namespace Deuteros.Code.Objects
{
    [Serializable]
    public partial class Staff
    {
        public string Leader { get; set; }
        private int ActionsTaken { get; set; }
        public int Count { get; set; }
        public int AttritionCountdown { get; set; }
        public Enums.StaffType Type { get; set; }

        public Staff()
        {
            ActionsTaken = 0;
        }

        public void AddAction()
        {
			if (Type == Enums.StaffType.Research)
			{
                if (ActionsTaken == 5)
                    GameCore.SingletonInstance.GameData.ActiveSaveFile.News.AddNews(Leader.PadRight(9, ' ') + "New Rank:Doctor");
                else if (ActionsTaken == 8)
                    GameCore.SingletonInstance.GameData.ActiveSaveFile.News.AddNews(Leader.PadRight(9, ' ') + "New Rank:Professor");
			}
			else if (Type == Enums.StaffType.Production)
			{
				if (ActionsTaken == 5)
					GameCore.SingletonInstance.GameData.ActiveSaveFile.News.AddNews(Leader.PadRight(9, ' ') + "New Rank:Engineer");
				else if (ActionsTaken == 11)
					GameCore.SingletonInstance.GameData.ActiveSaveFile.News.AddNews(Leader.PadRight(9, ' ') + "New Rank:Expert");
			}
			else if (Type == Enums.StaffType.Marines)
			{
				if (ActionsTaken == 9)
					GameCore.SingletonInstance.GameData.ActiveSaveFile.News.AddNews(Leader.PadRight(9, ' ') + "New Rank:Captain");
				else if (ActionsTaken == 39)
					GameCore.SingletonInstance.GameData.ActiveSaveFile.News.AddNews(Leader.PadRight(9, ' ') + "New Rank:Admiral");
			}

			ActionsTaken++;
        }

		public void AddAction(int count)
		{
			ActionsTaken += count;
		}
		
        public int GetLevel()
        {
            if (Type == Enums.StaffType.Research)
            {
                if (ActionsTaken >= 6 && ActionsTaken < 9)
                    return (int)Enums.StaffLevel_Researcher.Doctor;
                else if (ActionsTaken >= 9)
                    return (int)Enums.StaffLevel_Researcher.Professor;
                else
                    return (int)Enums.StaffLevel_Researcher.Technician;
            }
            else if (Type == Enums.StaffType.Production)
            {
                if (ActionsTaken >= 6 && ActionsTaken < 12)
                    return (int)Enums.StaffLevel_Production.Engineer;
                else if (ActionsTaken >= 12)
                    return (int)Enums.StaffLevel_Production.Expert;
                else
                    return (int)Enums.StaffLevel_Production.Apprentice;
            }
            else if (Type == Enums.StaffType.Marines)
            {
                if (ActionsTaken >= 10 && ActionsTaken < 40)
                    return (int)Enums.StaffLevel_Marines.Captain;
                else if (ActionsTaken >= 40)
                    return (int)Enums.StaffLevel_Marines.Admiral;
                else
                    return (int)Enums.StaffLevel_Marines.Pilot;
            }

            return 0;
        }

        public string GetLevelString(bool artisan=false)
        {
            if (Type == Enums.StaffType.Research)
            {
                if (ActionsTaken >= 6 && ActionsTaken < 9)
                    return Enums.StaffLevel_Researcher.Doctor.ToScreenString();
                else if (ActionsTaken >= 9)
                    return Enums.StaffLevel_Researcher.Professor.ToScreenString();
                else
                    return Enums.StaffLevel_Researcher.Technician.ToScreenString();
            }
            else if (Type == Enums.StaffType.Production)
            {
                if (artisan) return "Artisan";

                if (ActionsTaken >= 6 && ActionsTaken < 12)
                    return Enums.StaffLevel_Production.Engineer.ToScreenString();
                else if (ActionsTaken >= 12)
                    return Enums.StaffLevel_Production.Expert.ToScreenString();
                else
                    return Enums.StaffLevel_Production.Apprentice.ToScreenString();
            }
            else if (Type == Enums.StaffType.Marines)
            {
                return ((Enums.StaffLevel_Marines)GetLevel()).ToScreenString();
            }

            return "";
        }

        public string GetTypeText2()
        {
            switch(Type)
            {
                case Enums.StaffType.Production:
                    return "Artisans";
                case Enums.StaffType.Marines:
                    return "Mariners";
            }
            return "";
        }
    }
}