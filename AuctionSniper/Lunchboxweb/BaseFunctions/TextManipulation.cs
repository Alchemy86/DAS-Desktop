using System.Globalization;
using System.Text.RegularExpressions;

namespace Lunchboxweb.BaseFunctions
{
    public class TextManipulation : ITextManipulation
    {
        /// <summary>
        /// Reduce multiple spaces to a single space
        /// </summary>
        /// <param name="stringToModify">string to trim</param>
        /// <returns>Trimmed string</returns>
        public string TrimSpaces(string stringToModify)
        {
            return Regex.Replace(stringToModify, @"\s+", " ", RegexOptions.IgnorePatternWhitespace);
        }

        /// <summary>
        /// Titlecase a string
        /// </summary>
        /// <param name="stringToModify">string to modify</param>
        /// <returns>Titlecase string</returns>
        public string TitleCase(string stringToModify)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(stringToModify);
        }

        /// <summary>
        /// Safe instance of try parse int
        /// </summary>
        /// <param name="stringToModify">string to parse</param>
        /// <returns>0 if it fails</returns>
        public int TryParse_INT(string stringToModify)
        {
            int val;
            return int.TryParse(stringToModify, out val) ? int.Parse(stringToModify) : 0;
        }

        /// <summary>
        /// Safe instance of try parse decimal
        /// </summary>
        /// <param name="stringToModify">string to parse</param>
        /// <returns>0 if it fails</returns>
        public decimal TryParse_Decimal(string stringToModify)
        {
            decimal val;
            return decimal.TryParse(stringToModify, out val) ? decimal.Parse(stringToModify) : 0;
        }

        /// <summary>
        /// Spintx implementaion
        /// </summary>
        /// <param name="random">Random with set range</param>
        /// <param name="stringToModify">string to interperet</param>
        /// <returns>Full spingtax of text</returns>
        public string SpintaxParse(System.Random random, string stringToModify)
        {
            if (!stringToModify.Contains("{")) return stringToModify;
            var closingBracePosition = stringToModify.IndexOf('}');
            var openingBracePosition = closingBracePosition;

            while (!stringToModify[openingBracePosition].Equals('{'))
                openingBracePosition--;

            var spintaxBlock = stringToModify.Substring(openingBracePosition, closingBracePosition - openingBracePosition + 1);

            var items = spintaxBlock.Substring(1, spintaxBlock.Length - 2).Split('|');

            stringToModify = stringToModify.Replace(spintaxBlock, items[random.Next(items.Length)]);

            return SpintaxParse(random, stringToModify);
        }
    }
}