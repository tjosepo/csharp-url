using System.Linq;
using System.Text;

namespace URLStandard;

// Ref: https://url.spec.whatwg.org/#concept-basic-url-parser
internal static class CodePoints
{
  static private char[] Range(int from, int to)
  {
    var charList = new char[to - from];
    var index = from;
    while (index <= to)
    {
      charList[index] = (char)index;
      index += 1;
    }
    return charList;
  }

  // Ref: https://infra.spec.whatwg.org/#ascii-tab-or-newline
  static internal char[] ASCIITabOrNewline { get => new[] { (char)0x0009, (char)0x000A, (char)0x000D }; }

  // Ref: https://infra.spec.whatwg.org/#c0-control
  static internal char[] C0Control { get => Range(0x0000, 0x001F); }

  // Ref: https://infra.spec.whatwg.org/#c0-control-or-space
  static internal char[] C0ControlOrSpace { get => C0Control.Concat(new[] { (char)0x0020 }).ToArray(); }

  // Ref: https://infra.spec.whatwg.org/#ascii-alpha  
  static internal bool IsASCIIDigit(char c) => (0x0030 <= c && c <= 0x0039);
  static internal bool IsASCIIUpperAlpha(char c) => (0x0041 <= c && c <= 0x005A);
  static internal bool IsASCIILowerAlpha(char c) => (0x0061 <= c && c <= 0x007A);
  static internal bool IsASCIIAlpha(char c) => IsASCIIUpperAlpha(c) || IsASCIILowerAlpha(c);
  static internal bool IsASCIIAlphanumeric(char c) => IsASCIIDigit(c) || IsASCIIAlpha(c);

}