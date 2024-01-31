using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace pfp_generator;

public static class Program
{
  public static int Main(string[] args)
  {
    if (!Font.Initialize(args.Length == 1 ? args[0] : "../../../../imgs/7x9.png"))
    {
      Console.WriteLine("자소 이미지 로딩 실패");
      return -1;
    }

    if (!TryWirteOutput())
    {
      Console.WriteLine("pfp 파일 기록 오류");
      return -1;
    }

    // successful
    return 0;
  }

  private static bool TryWirteOutput()
  {
    try
    {
      var index = 0xE000;
      var glyphs = new JsonArray();
      GenerateGlyphs(ref index, glyphs);
      GenerateComponents(glyphs);

      File.WriteAllText(
        "galmuri9-condensed.pfp",
        JsonSerializer.Serialize(
          new JsonObject
          {
            ["version"] = 1,
            ["attr"] = new JsonObject
            {
              ["name"] = "galmuri9-condensed",
              ["author"] = "koipkoi",
              ["widthType"] = "monospace",
              ["fixedWidth"] = 8,
              ["spaceWidth"] = 8,
              ["descent"] = 0,
              ["ascent"] = 9,
              ["offsetX"] = 0,
              ["offsetY"] = 0,
              ["lineGap"] = 0,
              ["maxWidth"] = 8,
            },
            ["glyphs"] = glyphs,
          }
        )
      );

      return true;
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.ToString());
    }
    return false;
  }

  private static void GenerateGlyphs(ref int index, JsonArray glyphs)
  {
    var list = new List<List<List<List<string>>>>
    {
      Font.Cho,
      Font.Jung,
      Font.Jong
    };

    foreach (var l in list)
    {
      foreach (var e in l)
      {
        foreach (var lines in e)
        {
          var arr = new JsonArray();
          foreach (var line in lines)
            arr.Add(line);

          glyphs.Add(
            new JsonObject
            {
              ["unicode"] = index,
              ["data"] = arr,
            }
          );

          index++;
        }
      }
    }
  }

  private static void GenerateComponents(JsonArray glyphs)
  {
    var index = 0;
    foreach (var code in Hangul.UNICODE_2350)
    {
      var choId = Hangul.CHO_TABLE[code];
      var jungId = Hangul.JUNG_TABLE[code];
      var jongId = Hangul.JONG_TABLE[code];

      var components = new JsonArray
      {
        HangulStrategy.GetChoComponentId(choId, jungId, jongId),
        HangulStrategy.GetJungComponentId(choId, jungId, jongId)
      };

      if (jongId != 0)
        components.Add(HangulStrategy.GetJongComponentId(choId, jungId, jongId));

      glyphs.Add(
        new JsonObject
        {
          ["name"] = Hangul.CHARACTERS_2350[index],
          ["unicode"] = code,
          ["components"] = components,
        }
      );

      index++;
    }
  }
}
