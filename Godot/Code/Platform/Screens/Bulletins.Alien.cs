using System.Text;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Objects.GameData;
using Godot;
using static Deuteros.Code.Enums;

public partial class Bulletins
{
    private SaveFile noticeSave;
    private int noticeStage;

    public async void DisplayTransmissionNotice(SaveFile save)
    {
        noticeSave = save;
        noticeStage = save.AlienTransmissions.Stage;
        save.TimeSkip = save.TimeSkipDay = false;
        var button = GetNode<Button>("ViewTransmission");
        button.Disabled = true;
        button.Visible = false;
        var completed = await TypeText(BulletinLabel, DepartmentText(noticeStage == 0 ? BulletinTypes.Transmission1 : BulletinTypes.Transmission2));
        if (!completed || !IsInstanceValid(this) || !IsInsideTree() || IsQueuedForDeletion()
            || !ReferenceEquals(save, GameCore.SingletonInstance.GameData.ActiveSaveFile)) return;
        button.Visible = true;
        button.Disabled = false;
        button.GrabFocus();
    }

    private void AcknowledgeTransmission()
    {
        if (noticeSave == null || !ReferenceEquals(noticeSave, GameCore.SingletonInstance.GameData.ActiveSaveFile)
            || noticeSave.AlienTransmissions.Stage != noticeStage || !noticeSave.AlienTransmissions.Ready) return;
        var save = noticeSave;
        noticeSave = null;
        DisplayAlienTransmission(save, false);
    }

    public async void DisplayAlienTransmission(SaveFile save, bool replay)
    {
        var state = save.AlienTransmissions;
        if (!ReferenceEquals(save, GameCore.SingletonInstance.GameData.ActiveSaveFile)
            || (replay ? state.LastStage < 0 : !state.BeginDisplay())) return;
        GetNode<Button>("ViewTransmission").Visible = false;
        GetNode<Button>("ViewTransmission").Disabled = true;
        save.TimeSkip = save.TimeSkipDay = false;
        if (!replay) save.News.LastBulletin = state.LastStage == 0 ? BulletinTypes.Transmission1 : BulletinTypes.Transmission2;
        BulletinLabel.Theme = GD.Load<Theme>("res://Themes/AlienThemes/WhiteFont5-Alien.tres");
        BulletinLabel.AddThemeConstantOverride("line_separation", 0);
        await TypeText(BulletinLabel, AlienText(state));
    }

    // Exact message bodies from disk 2's $26800 block; retain padding spaces because they rotate the mask.
    private static readonly string[] AlienMessages =
    {
        "greetings human.\n\nwe are monitoring all of your\ntransmissions in an attempt to\nunderstand your language\n\nthis message shall be repeated\nuntil we are able to communicate\nfluently\n\nthere is a subject of great\nimportance we must discuss\nwith you\n",
        "greetings once again, human.\n\nallow us to introduce ourselves.\nwe are a peaceful race, similar\nto yourselves from a galaxy\nsome 700000 parsecs distant.\n              \n\nwe have already made contact\nwith a race in your galaxy and\nobserve that you are both at\nwar. this is to be expected.\nwe too have found them to be\ndishonourable. this time we\nmust be certain before placing\nour TRUST in YOU.\n           \nCONTACT will follow when\nobservations are complete.\n",
        "GREETINGS, FRIEND.\n\nwe BELIEVE we CAN TRUST YOU AND\nREQUEST your ASSISTANCE IN A\nproject TO OUR MUTUAL BENEFIT.\n              \nMANY land AGO, WE transmuted A\ngift TO the METHANOIDS. A GIFT\nOF great POWER AND imPORTANce.\n            \nSADLY, THEY DISASSEMBLEd it IN\nAN ATTEMPT to understand THE\nTECHnologY, A GRAVE MISTaKE.\nOUR SCANNERS tell US THAT THE\nsegments ARE SCATTERED amomg 8\nSTARS in your GALAXY. we shall\nINFORM you of their exact\nLOCATION as we DETEct them.\n",
        "GREETINGS, FRIEND.\n                \nwe have confirmation from our\nscanners that one segment of\nour apparatus is\nlying in orbit around \0.\n          \nplease attempt to recover the\nsegment and return it to any\nof your factories.\n          \ngood luck.",
        "OUR COMPLIMENTS, FRIEND.\n            \nYOU NOW HAVE ALL SEGMENTS OF\nOUR TRANSMITTER.    \nIF YOU WISH TO USE IT PLEASE\nFOLLOW THESE INSTRUCTIONS.  \n\n1: CONSTRUCT THE TRANSMITTER\n   IN ANY OF YOUR FACTORIES. \n2: FIT THIS TO ANY STARSHIP\n   IN YOUR FLEET.\n3: ACTIVATE THE POD HOLDING\n   THE TRANSMITTER.\n\nWE WILL DO THE REST...\n\nSEE YOU SOON, HUMAN !\n"
    };

    private static string AlienText(AlienTransmissions state)
    {
        var index = state.LastStage <= 2 ? 0 : state.LastStage == 3 ? 1 : state.LastStage == 4 ? 2 : state.LastStage <= 12 ? 3 : 4;
        var parts = AlienMessages[index].Split('\0');
        var mask = state.LastMask;
        var text = DecodeAlienCase(parts[0], ref mask);
        // The original rotates this stored word in place, including during News replay.
        state.LastMask = mask;
        if (parts.Length == 1) return text;
        // $1FF08 inserts the ordinary font; the second $3A3A6 call selects the zero word at $3A380.
        uint suffixMask = 0;
        return text + "[font=res://Fonts/deuteros.ttf]" + state.LastLocation.ToScreenString(" ").ToUpperInvariant()
            + "[/font]" + DecodeAlienCase(parts[1], ref suffixMask);
    }

    private static string DecodeAlienCase(string source, ref uint mask)
    {
        var text = new StringBuilder(source.Length);
        foreach (var character in source)
        {
            if (character == ' ') mask = (mask << 1) | (mask >> 31);
            text.Append(character >= 'a' && character <= 'z' && (mask & 0x01000000) != 0
                ? (char)(character - 32) : character);
        }
        return text.ToString();
    }
}
