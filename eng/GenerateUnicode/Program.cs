using System.Globalization;
using System.Text.Json;
var start=new List<int>();var part=new List<int>();
for(int code=0;code<=0xffff;code++)
{
 var category=char.GetUnicodeCategory((char)code);
 bool first=code=='_' || category is UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter or UnicodeCategory.TitlecaseLetter or UnicodeCategory.ModifierLetter or UnicodeCategory.OtherLetter or UnicodeCategory.LetterNumber;
 bool next=first || category is UnicodeCategory.DecimalDigitNumber or UnicodeCategory.ConnectorPunctuation or UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark or UnicodeCategory.Format;
 if(first)start.Add(code);if(next)part.Add(code);
}
Console.WriteLine(JsonSerializer.Serialize(new { start,part,runtime=Environment.Version.ToString(),culture=CultureInfo.InvariantCulture.Name }));
