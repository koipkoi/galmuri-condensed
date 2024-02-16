using System;
using System.Collections.Generic;
using System.Linq;

namespace pfp_generator;

public static class Hangul
{
  private static readonly int HANGUL_START_INDEX = 0xAC00;

  public static readonly Dictionary<int, int> CHO_TABLE = [];
  public static readonly Dictionary<int, int> JUNG_TABLE = [];
  public static readonly Dictionary<int, int> JONG_TABLE = [];

  public static readonly string[] CHO =
  [
    "ㄱ", "ㄲ", "ㄴ", "ㄷ", "ㄸ",
    "ㄹ", "ㅁ", "ㅂ", "ㅃ", "ㅅ",
    "ㅆ", "ㅇ", "ㅈ", "ㅉ", "ㅊ",
    "ㅋ", "ㅌ", "ㅍ", "ㅎ",
  ];

  public static readonly string[] JUNG =
  [
    "ㅏ", "ㅐ", "ㅑ", "ㅒ", "ㅓ",
    "ㅔ", "ㅕ", "ㅖ", "ㅗ", "ㅘ",
    "ㅙ", "ㅚ", "ㅛ", "ㅜ", "ㅝ",
    "ㅞ", "ㅟ", "ㅠ", "ㅡ", "ㅢ",
    "ㅣ"
  ];

  public static readonly string[] JONG =
  [
    "", "ㄱ", "ㄲ", "ㄳ", "ㄴ",
    "ㄵ", "ㄶ", "ㄷ", "ㄹ", "ㄺ",
    "ㄻ", "ㄼ", "ㄽ", "ㄾ", "ㄿ",
    "ㅀ", "ㅁ", "ㅂ", "ㅄ", "ㅅ",
    "ㅆ", "ㅇ", "ㅈ", "ㅊ", "ㅋ",
    "ㅌ", "ㅍ", "ㅎ"
  ];

  static Hangul()
  {
    ForEachHangulCode(code =>
    {
      CHO_TABLE.Add(code, (code - HANGUL_START_INDEX) / 28 / 21);
      JUNG_TABLE.Add(code, (code - HANGUL_START_INDEX) / 28 % 21);
      JONG_TABLE.Add(code, (code - HANGUL_START_INDEX) % 28);
    });
  }

  public static void ForEachHangulCode(Action<int> func)
  {
    for (var code = 0xAC00; code <= 0xD7A3; code++)
      func(code);
  }
}

public sealed class HangulStrategy
{
  private static readonly string[] 쌍자음 = ["ㅃ", "ㅉ", "ㄸ", "ㄲ", "ㅆ"];

  private static readonly string[] 일반모음 = ["ㅏ", "ㅑ", "ㅓ", "ㅕ", "ㅣ"];
  private static readonly string[] 넓은모음 = ["ㅐ", "ㅔ", "ㅒ", "ㅖ"];
  private static readonly string[] 쌍자음모음 = ["ㅔ", "ㅖ", "ㅘ", "ㅚ", "ㅙ"];

  private static readonly string[] 넓은종성 = ["ㅂ", "ㅄ", "ㅆ", "ㅈ", "ㅉ", "ㅊ", "ㄹ", "ㄺ", "ㄻ", "ㄼ", "ㄽ", "ㄾ", "ㄿ", "ㅀ", "ㅌ"];

  public static readonly HangulStrategy[] STRATEGIES =
  [
    // 가 갸 거 겨 기
    new(0, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, 일반모음) && jong == 0),
    // 각 갹 걱 격 긱
    new(1, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, 일반모음) && jong > 0 && !ContainsJong(jong, 넓은종성)),
    // 갈 걀 걸 결 길
    new(2, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, 일반모음) && jong > 0 && ContainsJong(jong, 넓은종성)),

    // 개 게 걔 계
    new(3, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, 넓은모음) && jong == 0),
    // 객 겍 걕 곅
    new(4,  (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, 넓은모음) && jong > 0 && !ContainsJong(jong, 넓은종성)),
    // 갤 겔 걜 곌
    new(5, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, 넓은모음) && jong > 0 && ContainsJong(jong, 넓은종성)),

    // 과 괴 긔
    new(6, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅘ", "ㅚ", "ㅢ") && jong == 0),
    // 곽 괵 긕
    new(7, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅘ", "ㅚ", "ㅢ") && jong > 0 && !ContainsJong(jong, 넓은종성)),
    // 괄 괼 긜
    new(8, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅘ", "ㅚ", "ㅢ") && jong > 0 && ContainsJong(jong, 넓은종성)),

    // 괘
    new(9, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅙ") && jong == 0),
    // 괙
    new(10, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅙ") && jong > 0 && !ContainsJong(jong, 넓은종성)),
    // 괠
    new(11, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅙ") && jong > 0 && ContainsJong(jong, 넓은종성)),

    // 궈 귀
    new(12, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅝ", "ㅟ") && jong == 0),
    // 궉 귁
    new(13, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅝ", "ㅟ") && jong > 0 && !ContainsJong(jong, 넓은종성)),
    // 궐 귈
    new(14, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅝ", "ㅟ") && jong > 0 && ContainsJong(jong, 넓은종성)),

    // 궤
    new(15, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅞ") && jong == 0),
    // 궥
    new(16, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅞ") &&  jong > 0 &&!ContainsJong(jong, 넓은종성)),
    // 궬
    new(17, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅞ") && jong > 0 && ContainsJong(jong, 넓은종성)),

    // -----------------------------------------------------------------------
    
    // 가 갸 거 겨 기
    new(18, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, 일반모음) && jong == 0),
    // 각 갹 걱 격 긱
    new(19, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, 일반모음) && jong > 0 && !ContainsJong(jong, 넓은종성)),
    // 갈 걀 걸 결 길
    new(20, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, 일반모음) && jong > 0 && ContainsJong(jong, 넓은종성)),

    // 개 게 걔 계
    new(21, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, 넓은모음) && jong == 0),
    // 객 겍 걕 곅
    new(22,  (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, 넓은모음) && jong > 0 && !ContainsJong(jong, 넓은종성)),
    // 갤 겔 걜 곌
    new(23, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, 넓은모음) && jong > 0 && ContainsJong(jong, 넓은종성)),

    // 과 괴 긔
    new(24, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅘ", "ㅚ", "ㅢ") && jong == 0),
    // 곽 괵 긕
    new(25, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅘ", "ㅚ", "ㅢ") && jong > 0 && !ContainsJong(jong, 넓은종성)),
    // 괄 괼 긜
    new(26, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅘ", "ㅚ", "ㅢ") && jong > 0 && ContainsJong(jong, 넓은종성)),

    // 괘
    new(27, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅙ") && jong == 0),
    // 괙
    new(28, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅙ") && jong > 0 && !ContainsJong(jong, 넓은종성)),
    // 괠
    new(29, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅙ") && jong > 0 && ContainsJong(jong, 넓은종성)),

    // 궈 귀
    new(30, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅝ", "ㅟ") && jong == 0),
    // 궉 귁
    new(31, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅝ", "ㅟ") && jong > 0 && !ContainsJong(jong, 넓은종성)),
    // 궐 귈
    new(32, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅝ", "ㅟ") && jong > 0 && ContainsJong(jong, 넓은종성)),

    // 궤
    new(33, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅞ") && jong == 0),
    // 궥
    new(34, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅞ") &&  jong > 0 &&!ContainsJong(jong, 넓은종성)),
    // 궬
    new(35, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅞ") && jong > 0 && ContainsJong(jong, 넓은종성)),

    // -----------------------------------------------------------------------

    // 고 교 그
    new(36, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅗ", "ㅛ", "ㅡ") && jong == 0),
    // 곡 굑 극
    new(37, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅗ", "ㅛ", "ㅡ") && jong > 0 && !ContainsJong(jong, 넓은종성)),
    // 골 굘 글
    new(38, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅗ", "ㅛ", "ㅡ") && jong > 0 && ContainsJong(jong, 넓은종성)),

    // 구 규 
    new(39, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅜ", "ㅠ") && jong == 0),
    // 국 귝
    new(40, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅜ", "ㅠ") && jong > 0 && !ContainsJong(jong, 넓은종성)),
    // 굴 귤
    new(41, (cho, jung, jong) => !ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅜ", "ㅠ") && jong > 0 && ContainsJong(jong, 넓은종성)),

    // -----------------------------------------------------------------------

    // 고 교 그
    new(42, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅗ", "ㅛ", "ㅡ") && jong == 0),
    // 곡 굑 극
    new(43, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅗ", "ㅛ", "ㅡ") && jong > 0 && !ContainsJong(jong, 넓은종성)),
    // 골 굘 글
    new(44, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅗ", "ㅛ", "ㅡ") && jong > 0 && ContainsJong(jong, 넓은종성)),

    // 구 규 
    new(45, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅜ", "ㅠ") && jong == 0),
    // 국 귝
    new(46, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅜ", "ㅠ") && jong > 0 && !ContainsJong(jong, 넓은종성)),
    // 굴 귤
    new(47, (cho, jung, jong) => ContainsCho(cho, 쌍자음) && ContainsJung(jung, "ㅜ", "ㅠ") && jong > 0 && ContainsJong(jong, 넓은종성)),
  ];

  public int Index { get; private set; }
  public Func<int, int, int, bool> Strategy { get; private set; }

  private HangulStrategy(int index, Func<int, int, int, bool> strategy)
  {
    Index = index;
    Strategy = strategy;
  }

  private static bool ContainsCho(int code, params string[] chars)
  {
    var result = new List<int>();
    foreach (var ch in chars)
    {
      var index = 0;
      var found = false;
      foreach (var e in Hangul.CHO)
      {
        if (ch == e)
        {
          found = true;
          break;
        }
        index++;
      }
      if (found)
        result.Add(index);
    }
    return result.Contains(code);
  }

  private static bool ContainsJung(int code, params string[] chars)
  {
    var result = new List<int>();
    foreach (var ch in chars)
    {
      var index = 0;
      var found = false;
      foreach (var e in Hangul.JUNG)
      {
        if (ch == e)
        {
          found = true;
          break;
        }
        index++;
      }
      if (found)
        result.Add(index);
    }
    return result.Contains(code);
  }

  private static bool ContainsJong(int code, params string[] chars)
  {
    var result = new List<int>();
    foreach (var ch in chars)
    {
      var index = 0;
      var found = false;
      foreach (var e in Hangul.JONG)
      {
        if (ch == e)
        {
          found = true;
          break;
        }
        index++;
      }
      if (found)
        result.Add(index);
    }
    return result.Contains(code);
  }

  public static int GetChoComponentId(int cho, int jung, int jong)
  {
    var result = new List<HangulStrategy>();
    foreach (var strategy in STRATEGIES)
    {
      if (strategy.Strategy(cho, jung, jong))
        result.Add(strategy);
    }

    if (result.Count >= 2 || result.Count == 0)
      throw new NotSupportedException($"{cho}, {jung}, {jong}");

    return 0xEA00 + (STRATEGIES.Length * 30 * 0) + (result[0].Index * 30) + cho;
  }

  public static int GetJungComponentId(int cho, int jung, int jong)
  {
    var result = new List<HangulStrategy>();
    foreach (var strategy in STRATEGIES)
    {
      if (strategy.Strategy(cho, jung, jong))
        result.Add(strategy);
    }

    if (result.Count >= 2 || result.Count == 0)
      throw new NotSupportedException($"{cho}, {jung}, {jong}");

    return 0xEA00 + (STRATEGIES.Length * 30 * 1) + (result[0].Index * 30) + jung;
  }

  public static int GetJongComponentId(int cho, int jung, int jong)
  {
    var result = new List<HangulStrategy>();
    foreach (var strategy in STRATEGIES)
    {
      if (strategy.Strategy(cho, jung, jong))
        result.Add(strategy);
    }

    if (result.Count >= 2 || result.Count == 0)
      throw new NotSupportedException($"{cho}, {jung}, {jong}");

    return 0xEA00 + (STRATEGIES.Length * 30 * 2) + (result[0].Index * 30) + jong;
  }
}
