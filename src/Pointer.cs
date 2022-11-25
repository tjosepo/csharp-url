internal class Pointer
{
  private string Input { get; init; }
  internal int Index { get; set; } = -1;
  internal char? C
  {
    get
    {
      if (Index == -1) throw new System.Exception("Cannot use Pointer.C when pointing to nowhere");
      if (Index >= Input.Length) return null;
      return Input[Index];
    }
  }

  internal string Remaining
  {
    get => Input.Substring(Index + 1);
  }

  internal Pointer(string input)
  {
    this.Input = input;
  }
}