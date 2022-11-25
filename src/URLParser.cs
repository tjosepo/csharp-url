using System.Diagnostics;
using System.Text;
using System.Web;

namespace URLStandard;

internal enum State
{
  SchemeStartState,
  SchemeState,
  NoSchemeState,
  SpecialRelativeOrAuthorityState,
  PathOrAuthorityState,
  RelativeState,
  RelativeSlashState,
  SpecialAuthoritySlashesState,
  SpecialAuthorityIgnoreSlashesState,
  AuthorityState,
  HostState,
  HostnameState,
  PortState,
  FileState,
  FileSlashState,
  FileHostState,
  PathStartState,
  PathState,
  OpaquePathState,
  QueryState,
  FragmentState,
}

// Ref: https://url.spec.whatwg.org/#concept-basic-url-parser
internal static class BasicURLParser
{
  static internal (URLRecord? url, bool failure) Parse(string input, URLRecord baseURL = null)
  {
    var validationError = false;
    // No URL param simplifies the code a bit
    var url = new URLRecord();
    input.Trim(CodePoints.C0ControlOrSpace);
    foreach (var codePoint in CodePoints.ASCIITabOrNewline)
    {
      input.Replace(codePoint.ToString(), "");
    }

    var state = State.SchemeStartState;
    var encoding = Encoding.UTF8;
    var buffer = string.Empty;
    bool atSignSeen = false, insideBrackets = false, passwordTokenSeen = false;
    var pointer = new Pointer(input);

    var index = 0;
    while (index < input.Length)
    {
      char c = input[index];
      var remaining = input.Substring(index + 1);

      switch (state)
      {
        // Ref: https://url.spec.whatwg.org/#scheme-start-state
        case State.SchemeStartState:
          if (CodePoints.IsASCIIAlpha(c)) buffer += char.ToLower(c);
          else /* no state override */
          {
            state = State.NoSchemeState;
            index -= 1;
          }
          /* no else, no state override */
          break;

        // Ref: https://url.spec.whatwg.org/#scheme-state
        case State.SchemeState:
          if (CodePoints.IsASCIIAlphanumeric(c) || c is '+' || c is '-' || c is 'c') buffer += char.ToLower(c);
          else if (c is ':')
          {
            /* no state override */
            url.Scheme = buffer;
            /* no state override */
            buffer = string.Empty;

            if (url.Scheme is "file")
            {
              if (remaining.StartsWith("//") is false) validationError = true;
              state = State.FileState;
            }
            else if (url.IsSpecial && baseURL is not null && baseURL.Scheme == url.Scheme)
            {
              Debug.Assert(baseURL.IsSpecial);
              state = State.SpecialRelativeOrAuthorityState;
            }
            else if (url.IsSpecial) state = State.SpecialAuthoritySlashesState;
            else if (remaining.StartsWith('/')) state = State.PathOrAuthorityState;
            else
            {
              url.Path = string.Empty;
              state = State.OpaquePathState;
            }
          }
          else
          {
            validationError = true;
            return (null, true);
          }
          break;

        // Ref: https://url.spec.whatwg.org/#no-scheme-state
        case State.NoSchemeState:
          if (baseURL is null || baseURL.HasAnOpaquePath && c is not '#')
          {
            validationError = true;
            return (null, true);
          }
          else if (baseURL.HasAnOpaquePath && c is '#')
          {
            url.Scheme = baseURL.Scheme;
            url.Path = baseURL.Path;
            url.Query = baseURL.Query;
            url.Fragment = string.Empty;
            state = State.FragmentState;
          }
          else if (baseURL.Scheme is not "file")
          {
            state = State.RelativeState;
            index -= 1;
          }
          else
          {
            state = State.FileState;
            index -= 1;
          }
          break;

        case State.SpecialRelativeOrAuthorityState:
          if (c is '/' && remaining.StartsWith('/'))
          {
            state = State.SpecialAuthorityIgnoreSlashesState;
            index += 1;
          }
          else
          {
            validationError = true;
            state = State.RelativeState;
            index -= 1;
          }
          break;

        // Ref: https://url.spec.whatwg.org/#path-or-authority-state
        case State.PathOrAuthorityState:
          if (c is '/') state = State.AuthorityState;
          else
          {
            state = State.PathState;
            index -= 1;
          }
          break;

        // Ref: https://url.spec.whatwg.org/#relative-state
        case State.RelativeState:
          Debug.Assert(baseURL!.Scheme is not "file");
          url.Scheme = baseURL.Scheme;
          if (c is '/') state = State.RelativeSlashState;
          else if (url.IsSpecial && c is '\\')
          {
            validationError = true;
            state = State.RelativeSlashState;
          }
          else
          {
            url.Username = baseURL.Username;
            url.Password = baseURL.Password;
            url.Host = baseURL.Host;
            url.Port = baseURL.Port;
            url.Path = baseURL.Path; // TODO: Should be a clone
            url.Query = baseURL.Query;

            if (c is '?')
            {
              url.Query = string.Empty;
              state = State.QueryState;
            }
            else if (c is '#')
            {
              url.Fragment = string.Empty;
              state = State.FragmentState;
            }
            else if (c != 0x0000)
            {
              url.Query = null;
              url.ShortenUrlPath();
              state = State.PathState;
              index -= 1;
            }
          }
          break;

        // Ref: https://url.spec.whatwg.org/#relative-slash-state
        case State.RelativeSlashState:
          if (url.IsSpecial && c is '/' or '\\')
          {
            if (c is '\\') validationError = true;
            state = State.SpecialAuthorityIgnoreSlashesState;
          }
          else if (c is '/') state = State.AuthorityState;
          else
          {
            url.Username = baseURL!.Username;
            url.Password = baseURL.Password;
            url.Host = baseURL.Host;
            url.Port = baseURL.Port;
            state = State.PathState;
            index -= 1;
          }
          break;

        // Ref: https://url.spec.whatwg.org/#special-authority-slashes-state
        case State.SpecialAuthoritySlashesState:
          if (c is '/' && remaining.StartsWith('/'))
          {
            state = State.SpecialAuthorityIgnoreSlashesState;
            index += 1;
          }
          else
          {
            validationError = true;
            state = State.SpecialAuthorityIgnoreSlashesState;
            index -= 1;
          }
          break;

        // Ref: https://url.spec.whatwg.org/#special-authority-ignore-slashes-state        
        case State.SpecialAuthorityIgnoreSlashesState:
          if (c is not '/' or '\\')
          {
            state = State.AuthorityState;
            index -= 1;
          }
          else validationError = true;
          break;

        // Ref: https://url.spec.whatwg.org/#authority-state
        case State.AuthorityState:
          if (c is '@')
          {
            validationError = true;
            atSignSeen = true;
            foreach (var codePoint in buffer)
            {
              if (codePoint is ':' && passwordTokenSeen is false)
              {
                passwordTokenSeen = true;
                continue;
              }

              var encodedCodePoints = PercentEncoding.PercentEncode((byte)codePoint);
              // TODO
            }
          }
          break;
      }

      index += 1;
    }
    return (url, false);
  }
}