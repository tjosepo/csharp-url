using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace URLStandard;

// Ref: https://url.spec.whatwg.org/#concept-url
internal class URLRecord
{
  internal string Scheme { set; get; } = string.Empty;
  internal string Username { set; get; } = string.Empty;
  internal string Password { set; get; } = string.Empty;
  internal string? Host { set; get; } = null;
  internal UInt16? Port { set; get; } = null;
  internal object Path { set; get; } = " "; // List<string> or string
  internal string? Query { set; get; } = null;
  internal string? Fragment { set; get; } = null;
  // Blob URL entry

  // Ref: https://url.spec.whatwg.org/#is-special
  internal bool IsSpecial { get => this.Scheme is "ftp" or "file" or "http" or "https" or "ws" or "wss"; }

  // Ref: https://url.spec.whatwg.org/#include-credentials
  internal bool IncludesCredentials { get => this.Username != string.Empty && this.Password != string.Empty; }

  // Ref: https://url.spec.whatwg.org/#url-opaque-path
  internal bool HasAnOpaquePath { get => this.Path is string; }

  // Ref: https://url.spec.whatwg.org/#cannot-have-a-username-password-port
  internal bool CannotHaveAUsernamePasswordPort { get => this.Host is null || this.Host == string.Empty || this.Scheme is "file"; }

  // Ref: https://url.spec.whatwg.org/#shorten-a-urls-path
  internal void ShortenUrlPath()
  {
    Debug.Assert(this.HasAnOpaquePath is false);
    var path = (List<string>)this.Path;
    if (this.Scheme is "file" && path.Count is 1 && IsANormalizedWindowsDriveLetter(path[0])) return;
    path.RemoveAt(path.Count - 1);
  }

  // Ref: https://url.spec.whatwg.org/#windows-drive-letter
  internal static bool IsAWindowsDriveLetter(string codePoints)
  {
    if (codePoints.Length < 2) return false;
    return CodePoints.IsASCIIAlpha(codePoints[0]) && codePoints[1] is ':' or '|';
  }

  // Ref: https://url.spec.whatwg.org/#normalized-windows-drive-letter
  internal static bool IsANormalizedWindowsDriveLetter(string codePoints)
  {
    return IsAWindowsDriveLetter(codePoints) && codePoints[1] is ':';
  }

  // Ref: https://url.spec.whatwg.org/#start-with-a-windows-drive-letter
  internal static bool StartsWithAWindowsDriveLetter(string str)
  {
    return (
      str.Length >= 2 &&
      IsAWindowsDriveLetter(str) &&
      (str.Length is 2 || str[2] is '/' or '\\' or '?' or '#')
     );
  }
}