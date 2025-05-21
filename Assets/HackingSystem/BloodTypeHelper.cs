using System.Collections.Generic;

public static class BloodTypeHelper
{
    private static readonly Dictionary<BloodType, string> BloodTypeLabels = new()
    {
        { BloodType.A_Pos, "A+" },
        { BloodType.A_Neg, "A-" },
        { BloodType.B_Pos, "B+" },
        { BloodType.B_Neg, "B-" },
        { BloodType.AB_Pos, "AB+" },
        { BloodType.AB_Neg, "AB-" },
        { BloodType.O_Pos, "O+" },
        { BloodType.O_Neg, "O-" }
    };

    public static string GetLabel(BloodType type) => BloodTypeLabels[type];
}
