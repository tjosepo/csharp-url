using System.Text;
using URLStandard;
using Xunit;

public class PercentEncodingTest
{

  [Theory]
  [InlineData(0x23, "%23")]
  [InlineData(0x7F, "%7F")]
  public void PercentEncode(byte input, string expected)
  {
    Assert.Equal(expected, PercentEncoding.PercentEncode(input));
  }

  [Fact]
  public void PercentDecode_Bytes()
  {
    var input = Encoding.UTF8.GetBytes("%25%s%1G");
    var expected = Encoding.UTF8.GetBytes("%%s%1G");
    Assert.Equal(expected, PercentEncoding.PercentDecode(input));
  }

  [Fact]
  public void PercentDecode_String()
  {
    var input = "‽%25%2E";
    var expected = new byte[] { 0xE2, 0x80, 0xBD, 0x25, 0x2E };
    Assert.Equal(expected, PercentEncoding.PercentDecode(input));
  }
}