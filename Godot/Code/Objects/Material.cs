using Godot;
using System;

namespace Deuteros.Code.Objects
{
    [Serializable]
    public partial class Material
    {
        //Total amount of resources available in current vein
        public int GroundAmount { get; set; }
        public Enums.ItemTypes MaterialType { get; set; }
        // Original known-deposit bit distinguishes an exhausted vein from a zero-delay survey.
        public const int KnownEmpty = -1;
        public bool IsSurveying => GroundAmount == 0 && SurveyTicks >= 0;
        // Remaining survey updates, or KnownEmpty until the next extraction starts a survey.
        public int SurveyTicks { get; set; }

        public Material(Enums.ItemTypes materialType, int surveyTicks)
        {
            MaterialType = materialType;
            SurveyTicks = surveyTicks;
            GroundAmount = 0;
        }
    }
}