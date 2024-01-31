using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace pfp_generator;

public static class Font
{
  public static readonly int ROWS = 27 * 3;
  public static readonly int COLUMNS = 30;

  private static readonly int ROW_COUNT = 9;

  public readonly static List<List<List<string>>> Cho = [];
  public readonly static List<List<List<string>>> Jung = [];
  public readonly static List<List<List<string>>> Jong = [];

  public static bool Initialize(string fontFilename)
  {
    try
    {
      var image = (Image.FromFile(fontFilename) as Bitmap)!;

      for (var row = 0; row < ROWS; row++)
      {
        var list = new List<List<string>>();
        for (var column = 0; column < COLUMNS; column++)
        {
          var baseX = 8 + column * 8;
          var baseY = row * 16;

          var list2 = new List<string>();
          for (var y = 0; y < 9; y++)
          {
            var sb = new StringBuilder();
            for (var x = 0; x < 8; x++)
            {
              var color = image.GetPixel(baseX + x, baseY + y);
              sb.Append(color.ToArgb() == Color.White.ToArgb() ? '.' : '#');
            }
            list2.Add(sb.ToString());
          }

          list.Add(list2);
        }

        switch (row / ROW_COUNT)
        {
          case 0:
            Cho.Add(list);
            break;
          case 1:
            Jung.Add(list);
            break;
          default:
            Jong.Add(list);
            break;
        }
      }

      return true;
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.ToString());
    }
    return false;
  }
}
