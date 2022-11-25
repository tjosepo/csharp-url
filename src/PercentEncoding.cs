using System;
using System.Collections.Generic;
using System.Text;

namespace URLStandard;

// Ref: https://url.spec.whatwg.org/#percent-encoded-bytes
static internal class PercentEncoding
{
  // Ref: https://url.spec.whatwg.org/#percent-encode
  static internal string PercentEncode(byte b) => "%" + BitConverter.ToString(new[] { b });

  // Ref: https://url.spec.whatwg.org/#percent-decode
  static internal byte[] PercentDecode(byte[] input)
  {
    var output = new List<byte>();

    var index = 0;
    while (index < input.Length)
    {
      var b = input[index];

      if (b != '%') output.Add(b);
      else if (b == '%' &&
       input[index + 1] is not (>= 0x30 and <= 0x39) and not (>= 0x41 and <= 0x46) and not (>= 0x61 and <= 0x66) |
       input[index + 2] is not (>= 0x30 and <= 0x39) and not (>= 0x41 and <= 0x46) and not (>= 0x61 and <= 0x66)
      ) output.Add(b);
      else
      {
        var segment = new ArraySegment<byte>(input, index + 1, 2);
        var hexString = Encoding.UTF8.GetString(segment);
        var bytePoint = Convert.ToByte(hexString, 16);
        output.Add(bytePoint);
        index += 2;
      }
      index += 1;
    }

    return output.ToArray();
  }

  // Ref: https://url.spec.whatwg.org/#string-percent-decode
  static internal byte[] PercentDecode(string input)
  {
    var bytes = Encoding.UTF8.GetBytes(input);
    return PercentDecode(bytes);
  }

  // static internal UTF8PercentEncode()

}