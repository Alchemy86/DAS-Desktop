using System;

namespace Lunchboxweb.BaseFunctions
{
    public interface ITextManipulation
    {
        string TrimSpaces(string stringToModify);
        string TitleCase(string stringToModify);
        int TryParse_INT(string stringToModify);
        decimal TryParse_Decimal(string stringToModify);
        string SpintaxParse(Random random, string stringToModify);
    }
}