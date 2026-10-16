namespace CSNitrogen.Parsing;
/// <summary>C# foundation trivia. Unterminated comments are diagnosed separately.</summary>
internal static class CSharpTrivia
{
    internal static bool IsNewline(char value) => value is '\r' or '\n' or '\u0085' or '\u2028' or '\u2029';
    internal static int Skip(ReadOnlySpan<char> text, int position)
    {
        int i = position;
        if (i == 0 && text.Length > 0 && text[0] == '\uFEFF') i++;
        while (i < text.Length)
        {
            if (char.IsWhiteSpace(text[i])) { i++; continue; }
            if (text[i] == '/' && i + 1 < text.Length)
            {
                if (text[i + 1] == '/')
                {
                    i += 2;
                    while (i < text.Length && !IsNewline(text[i])) i++;
                    continue;
                }
                if (text[i + 1] == '*')
                {
                    int close = text[(i + 2)..].IndexOf("*/");
                    i = close < 0 ? text.Length : i + 2 + close + 2;
                    continue;
                }
            }
            break;
        }
        return i;
    }
}
