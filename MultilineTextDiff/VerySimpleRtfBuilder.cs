// MIT License
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Text;

#pragma warning disable S1144 // Unused private types or members should be removed
// public utility interface

namespace Atrip.Utils.Texts
{
  /// <summary>
  /// Very simple RTF builder class (only simple basic functions).
  /// </summary>
  [DebuggerDisplay("Text: {txt.ToString()}")]
  internal sealed class VerySimpleRtfBuilder
  {
    #region Public definitions (enumerations)

    /// <summary>
    /// Basic font formating.
    /// </summary>
    [Flags]
    public enum Styles
    {
      /// <summary>
      /// No change.
      /// </summary>
      None = 0,

      /// <summary>
      /// Make bold text = add bold flag.
      /// </summary>
      Bold = 1,

      /// <summary>
      /// Make italic text = add italic flag.
      /// </summary>
      Italic = 2,

      /// <summary>
      /// Make bold italic = add bold and italic flag.
      /// </summary>
      BoldItalic = (Bold | Italic),

      /// <summary>
      /// Make underline text = add underline flag.
      /// </summary>
      Underline = 4,

      /// <summary>
      /// Make strikeout text = add strikeout flag.
      /// </summary>
      Strikeout = 8,

      /// <summary>
      /// Makes the character as superscript.
      /// </summary>
      Superscript = 16,

      /// <summary>
      /// Makes the character as subscript.
      /// </summary>
      Subscript = 32,

      /// <summary>
      /// Invalid script option; can not combine superscript and subscript.
      /// </summary>
      InvalidScriptOption = (Superscript | Subscript),

      /// <summary>
      /// Make all characters in capitals.
      /// </summary>
      Capitals = 64,

      /// <summary><para>Clear font change flag(s).</para>
      /// <para>If used separately, clear all flags.</para>
      /// <para>Use with given flag to clear only given flag.</para></summary>
      Clear = 256
    }

    /// <summary>
    /// Font support mode for RTF building.
    /// </summary>
    /// <remarks>This is necessary because of the behaviour of font table in RTF file.
    /// When the font table is used in created RTF data, the font size changes for whole data
    /// even data with standard font (compared to the result, when no font change is used).</remarks>
    public enum FontMode
    {
      /// <summary>
      /// Fonts are allowed, but no font table is added when the user do not select any font.
      /// </summary>
      AllowFonts,

      /// <summary>
      /// Font table is always used, even when the user uses no custom font.
      /// </summary>
      AlwaysUseFontTable,

      /// <summary>
      /// Use of custom fonts is not allowed. All functions that sets font throw an exception.
      /// </summary>
      DoNotUseFonts,
    }

    /// <summary>
    /// Text alignment type.
    /// </summary>
    public enum Alignment
    {
      /// <summary>
      /// Align text to left.
      /// </summary>
      Left,

      /// <summary>
      /// Align text to right.
      /// </summary>
      Right,

      /// <summary>
      /// Center text.
      /// </summary>
      Center,

      /// <summary>
      /// Justify text.
      /// </summary>
      /// <remarks>
      /// Do not work in RichTextBox (from in .NET Framework).
      /// </remarks>
      Justify,
    }

    #endregion

    #region Public (default) configuration

    /// <summary>
    /// Default font support mode for new instances.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public static FontMode DefaultFontMode
    {
      [DebuggerStepThrough]
      get;

      [DebuggerStepThrough]
      set;
    } = FontMode.AlwaysUseFontTable;

    /// <summary>
    /// Default font size (in half-points) for new instances; null means not used.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public static int? DefaultFontSize
    {
      [DebuggerStepThrough]
      get;

      [DebuggerStepThrough]
      set;
    }

    #endregion

    #region Public constants

    /// <summary>
    /// Minimal usable font size (in half-points).
    /// </summary>
    public const byte MinimalFontSize = 4;

    /// <summary>
    /// Minimal usable font size (in half-points).
    /// </summary>
    public const short MaximalFontSize = 128;

    /// <summary>
    /// Minimal usable font size (in half-points).
    /// </summary>
    public const byte RecomendedFontSize = 22;

    #endregion

    #region Private support definitions

    /// <summary>
    /// Document paragraph (new line).
    /// </summary>
    private const string paragraph = @"\par";

    /// <summary>
    /// Document tabulator.
    /// </summary>
    private const string tab = @"\tab ";

    #endregion

    #region Support class definitions

    [DebuggerDisplay("{CurrentFontName} <{CurrentFontSize}>")]
    private class FontConfig
    {
      private VerySimpleRtfBuilder activeInstance;

      public FontConfig(VerySimpleRtfBuilder rootInstance, int? defaultFontSize)
      {
        if (rootInstance is null)
        {
          throw new ArgumentNullException(nameof(rootInstance));
        }

        this.DefaultSize = defaultFontSize;
        this.RootInstance = rootInstance;
        this.activeInstance = rootInstance;
      }

      public int? DefaultSize
      {
        [DebuggerStepThrough]
        get;
      }

      [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Internal function")]
      public VerySimpleRtfBuilder RootInstance
      {
        [DebuggerStepThrough]
        get;
      }

      [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Internal function")]
      public VerySimpleRtfBuilder ActiveInstance
      {
        [DebuggerStepThrough]
        get
        {
          return (this.activeInstance);
        }

        [DebuggerStepThrough]
        set
        {
          if (value is null)
          {
            throw new ArgumentNullException(nameof(value));
          }

          this.activeInstance = value;
        }
      }

      public string CurrentName
      {
        [DebuggerStepThrough]
        get
        {
          return (this.activeInstance.currentFontName);
        }
      }

      public int? CurrentSize
      {
        [DebuggerStepThrough]
        get
        {
          return (this.activeInstance.currentFontSize);
        }
      }
    }

    #endregion

    #region Members

    /// <summary>
    /// Parent for current operation.
    /// </summary>
    private readonly VerySimpleRtfBuilder parent;

    /// <summary>
    /// List of active formating changes.
    /// </summary>
    private readonly List<VerySimpleRtfBuilder> formatList;

    /// <summary>
    /// List of colours.
    /// </summary>
    private readonly List<Color> coloursList;

    /// <summary>
    /// List of used fonts.
    /// </summary>
    private readonly List<string> fontList;

    /// <summary>
    /// Real RTF string.
    /// </summary>
    private readonly StringBuilder rtf;

    /// <summary>
    /// Simple text string for debugging.
    /// </summary>
    private readonly StringBuilder txt;

    /// <summary>
    /// End block on style close (unregister function).
    /// </summary>
    private readonly bool endBlockOnStyleClose;

    /// <summary>
    /// Previous added character.
    /// </summary>
    private char? previousChar;

    /// <summary>
    /// Font mode for current instance.
    /// </summary>
    private readonly FontMode fontMode;

    /// <summary>
    /// Font configuration.
    /// </summary>
    private readonly FontConfig fontConfig;

    /// <summary>
    /// Font configuration.
    /// </summary>
    private readonly int? currentFontSize;

    /// <summary>
    /// Font configuration.
    /// </summary>
    private readonly string currentFontName;

    #endregion

    #region Constructors

    /// <summary>
    /// Standard constructor.
    /// </summary>
    public VerySimpleRtfBuilder()
      : this(0, DefaultFontMode, DefaultFontSize)
    {

    }

    /// <summary>
    /// Advanced constructor.
    /// </summary>
    /// <param name="capacity">The suggested starting size of this instance.</param>
    public VerySimpleRtfBuilder(int capacity)
      : this(capacity, DefaultFontMode, DefaultFontSize)
    {

    }

    /// <summary>
    /// Advanced constructor.
    /// </summary>
    /// <param name="capacity">The suggested starting size of this instance.</param>
    /// <param name="fontMode">Font using mode.</param>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder(int capacity, FontMode fontMode)
      : this(capacity, fontMode, DefaultFontSize)
    {

    }

    /// <summary>
    /// Advanced constructor.
    /// </summary>
    /// <param name="capacity">The suggested starting size of this instance.</param>
    /// <param name="fontMode">Font using mode.</param>
    /// <param name="initialFontSize">Standard font size on start in half-points. Must be in range from 4 to 128.</param>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder(int capacity, FontMode fontMode, int initialFontSize)
      : this(capacity, fontMode, (int?)initialFontSize)
    {

    }

    /// <summary>
    /// Advanced constructor.
    /// </summary>
    /// <param name="capacity">The suggested starting size of this instance.</param>
    /// <param name="fontMode">Font using mode.</param>
    /// <param name="initialFontSize">Standard font size on start in half-points.
    /// Must be in range from 4 to 128. Value null means, that no default value is set.</param>
    private VerySimpleRtfBuilder(int capacity, FontMode fontMode, int? initialFontSize)
    {
      switch (fontMode)
      {
      case FontMode.AllowFonts:
      case FontMode.AlwaysUseFontTable:
      case FontMode.DoNotUseFonts:
        this.fontMode = fontMode;
        break;

      default:
        throw new ArgumentOutOfRangeException(nameof(fontMode));
      }

      if (initialFontSize.HasValue && ((initialFontSize < MinimalFontSize) || (initialFontSize > MaximalFontSize)))
      {
        throw new ArgumentOutOfRangeException(nameof(initialFontSize));
      }

      this.rtf = new StringBuilder(250 + capacity);
      this.txt = new StringBuilder(capacity);
      this.coloursList = new List<Color>(5);
      this.fontList = new List<string>();
      this.formatList = new List<VerySimpleRtfBuilder>(10);
      this.parent = null;
      this.endBlockOnStyleClose = false;
      this.previousChar = null;
      this.fontConfig = new FontConfig(this, initialFontSize);
      this.currentFontName = null;
      this.currentFontSize = initialFontSize;
      Clear();
    }

    /// <summary>
    /// Constructor used for formating (only from current instance).
    /// </summary>
    /// <param name="src">Source instance which request format change.</param>
    /// <param name="endBlockOnDispose">End current block on dispose?</param>
    /// <param name="newFontName">New font name or null when no font change was set.</param>
    /// <param name="newFontSize">New font size or null when no font change was set.</param>
    private VerySimpleRtfBuilder(VerySimpleRtfBuilder src, bool endBlockOnDispose, string newFontName, int? newFontSize)
    {
      this.formatList = src.formatList;
      this.coloursList = src.coloursList;
      this.fontList = src.fontList;
      this.rtf = src.rtf;
      this.txt = src.txt;
      this.parent = src;
      this.endBlockOnStyleClose = endBlockOnDispose;
      this.previousChar = src.previousChar;
      this.fontMode = src.fontMode;
      this.currentFontName = newFontName ?? src.fontConfig.CurrentName;

      if (newFontSize.HasValue)
      {
        this.currentFontSize = newFontSize;
      }
      else
      {
        this.currentFontSize = src.fontConfig.CurrentSize;
      }

      this.fontConfig = src.fontConfig;
      this.fontConfig.ActiveInstance = this;
      src.Register(this);
    }

    #endregion

    #region Support functions

    /// <summary>
    /// Get a child version, with different formatting style.
    /// </summary>
    /// <returns>
    /// Child of current instance with defined formatting parameters.
    /// </returns>
    /// <param name="endBlockOnClose">End current block on dispose?</param>
    /// <param name="newFontName">New font name or null when no font change was set.</param>
    /// <param name="newFontSize">New font size or null when no font change was set.</param>
    private VerySimpleRtfBuilder GetChild(bool endBlockOnClose, string newFontName, int? newFontSize)
    {
      return (new VerySimpleRtfBuilder(this, endBlockOnClose, newFontName, newFontSize));
    }

    /// <summary>
    /// Append a single character as RTF data.
    /// </summary>
    /// <param name="c">Character to be appended.</param>
    private void AppendRtf(char c)
    {
      switch (c)
      {
      case '\t':
        this.rtf.Append(tab);
        break;

      case '\\':
      case '{':
      case '}':
        this.rtf.Append('\\').Append(c);
        break;

      case '\r':
        this.rtf.AppendLine(paragraph);
        break;

      case '\n':
        if (this.previousChar != '\r')
        {
          this.rtf.AppendLine(paragraph);
        }
        break;

      default:
        if ((c < '\0') || (c > '\x7f'))
        {
          int n = (int)c;
          if (n > 32767)
          {
            n -= 65536;
          }
          this.rtf.Append(@"\u").Append(n).Append('?');
        }
        else if (c < ' ')
        {
          this.rtf.Append('#').Append(((int)c).ToString("X2", CultureInfo.InvariantCulture));
        }
        else
        {
          this.rtf.Append(c);
        }
        break;
      }
      this.previousChar = c;
    }

    /// <summary>
    /// Append text as RTF data.
    /// </summary>
    /// <param name="text">New text value.</param>
    /// <param name="index">Start index of the string.</param>
    /// <param name="count">Count of characters to add.</param>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Internal function")]
    private void AppendRtf(string text, int index, int count)
    {
      while (count > 0)
      {
        AppendRtf(text[index]);
        count--;
        index++;
      }
    }

    #region Style processing functions

    /// <summary>
    /// Insert formating information to internal RTF string.
    /// </summary>
    /// <param name="style">Format definition.</param>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Internal function")]
    private void ProcessStyle(Styles style)
    {
      bool clear = ((style & Styles.Clear) == Styles.Clear);
      if ((style & Styles.InvalidScriptOption) == Styles.InvalidScriptOption)
      {
        if (clear)
        {
          style &= ~Styles.Subscript;
        }
        else
        {
          throw new ArgumentException("Invalid script style!", nameof(style));
        }
      }

      if (style == Styles.Clear)
      {
        this.rtf.Append(@"\i0\b0\ulnone\strike0\nosupersub\caps0");
        return;
      }

      if ((style & Styles.Italic) == Styles.Italic)
      {
        this.rtf.Append(@"\i");
        if (clear)
        {
          this.rtf.Append("0");
        }
      }

      if ((style & Styles.Bold) == Styles.Bold)
      {
        this.rtf.Append(@"\b");
        if (clear)
        {
          this.rtf.Append("0");
        }
      }

      if ((style & Styles.Underline) == Styles.Underline)
      {
        this.rtf.Append(@"\ul");
        if (clear)
        {
          this.rtf.Append("none");
        }
      }

      if ((style & Styles.Strikeout) == Styles.Strikeout)
      {
        this.rtf.Append(@"\strike");
        if (clear)
        {
          this.rtf.Append("0");
        }
      }

      if ((style & Styles.Superscript) == Styles.Superscript)
      {
        if (clear)
        {
          this.rtf.Append(@"\nosupersub");
        }
        else
        {
          this.rtf.Append(@"\super");
        }
      }

      if ((style & Styles.Subscript) == Styles.Subscript)
      {
        if (clear)
        {
          this.rtf.Append(@"\nosupersub");
        }
        else
        {
          this.rtf.Append(@"\sub");
        }
      }

      if ((style & Styles.Capitals) == Styles.Capitals)
      {
        this.rtf.Append(@"\caps");
        if (clear)
        {
          this.rtf.Append("0");
        }
      }
    }

    /// <summary>
    /// Insert font colour identification to RTF string and add new colour to colour palette.
    /// </summary>
    /// <param name="colour">New colour to be used.</param>
    private void ProcessFontColour(Color colour)
    {
      this.rtf.AppendFormat(@"\cf{0}", GetColourId(colour));
    }

    /// <summary>
    /// Insert background colour identification to RTF string and add new colour to colour palette.
    /// </summary>
    /// <param name="colour">New colour to be used.</param>
    private void ProcessHighlightColour(Color colour)
    {
      this.rtf.AppendFormat(@"\highlight{0}", GetColourId(colour));
    }

    /// <summary>
    /// Get ID of colour for given colour.
    /// </summary>
    /// <param name="colour">Colour to be found in the colour palette.</param>
    /// <returns>
    /// ID of the colour in colour palette.
    /// </returns>
    private int GetColourId(Color colour)
    {
      if (colour.IsEmpty)
      {
        return (0);
      }

      for (int position = 0; position < this.coloursList.Count; position++)
      {
        if (ColorEquals(this.coloursList[position], colour))
        {
          return (position + 1);
        }
      }

      this.coloursList.Add(colour);
      return (this.coloursList.Count);
    }

    /// <summary>
    /// Compare RGB values in two colours.
    /// </summary>
    /// <param name="a">First colour instance.</param>
    /// <param name="b">Second colour instance.</param>
    /// <returns>
    /// true when the RGB components of colours are identical; otherwise, false.
    /// </returns>
    private static bool ColorEquals(Color a, Color b)
    {
      return ((a.R == b.R) && (a.G == b.G) && (a.B == b.B));
    }

    /// <summary>
    /// Insert font name identification to RTF string and add new font to list of fonts.
    /// </summary>
    /// <param name="name">Name of the new font.</param>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Internal function")]
    private void ProcessFontName(string name)
    {
      if (!this.FontChangeAllowed)
      {
        throw new NotSupportedException("Font change is not supported in current mode!");
      }

      this.rtf.AppendFormat(@"\f{0}", GetFontId(name));
    }

    /// <summary>
    /// Get ID of font for given font name.
    /// </summary>
    /// <param name="name">Font name to be found in the list of fonts.</param>
    /// <returns>
    /// ID of the font in the list.
    /// </returns>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Internal function")]
    private int GetFontId(string name)
    {
      if (string.IsNullOrWhiteSpace(name))
      {
        return (0);
      }

      string fixedName = name.Trim();
      int position = this.fontList.IndexOf(fixedName);
      if (position >= 0)
      {
        return (position + 1);
      }

      this.fontList.Add(fixedName);
      return (this.fontList.Count);
    }

    /// <summary>
    /// Insert font size command to RTF string.
    /// </summary>
    /// <param name="halfPointsSize">New size of font in half-points.</param>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Internal function")]
    private void ProcessFontSize(int halfPointsSize)
    {
      if ((halfPointsSize < MinimalFontSize) || (halfPointsSize > MaximalFontSize))
      {
        throw new ArgumentOutOfRangeException(nameof(halfPointsSize));
      }

      this.rtf.AppendFormat(@"\fs{0}", halfPointsSize);
    }

    /// <summary>
    /// Insert stat of alignment block to RTF string.
    /// </summary>
    /// <param name="alignment">Alignment type.</param>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Internal function")]
    private void StartAlignmentBlock(Alignment alignment)
    {
      switch (alignment)
      {
      case Alignment.Left:
        this.rtf.Append(@"{\ql");
        break;

      case Alignment.Right:
        this.rtf.Append(@"{\qr");
        break;

      case Alignment.Center:
        this.rtf.Append(@"{\qc");
        break;

      case Alignment.Justify:
        this.rtf.Append(@"{\qj");
        break;

      default:
        throw new ArgumentOutOfRangeException(nameof(alignment));
      }
    }

    /// <summary>
    /// Convert value from millimeters to twips.
    /// </summary>
    /// <param name="value">Source value in millimeters.</param>
    /// <returns>
    /// Value in twips.
    /// </returns>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Internal function")]
    private static int ConvertMillimeterToTwips(int value)
    {
      if ((value > short.MaxValue) || (value < short.MinValue))
      {
        throw new ArgumentOutOfRangeException(nameof(value));
      }

      value *= 567;
      if ((value % 10) >= 5)
      {
        value += 10;
      }

      value /= 10;
      return (value);
    }

    #endregion

    #region structured (recursive) format functions

    /// <summary>
    /// Insert new format change child instance.
    /// </summary>
    /// <param name="child">New format change child instance.</param>
    private void Register(VerySimpleRtfBuilder child)
    {
      if (child is null)
      {
        throw new ArgumentNullException(nameof(child));
      }

      this.formatList.Add(child);
    }

    /// <summary>
    /// Remove all format changes up to given child instance.
    /// </summary>
    /// <param name="child">Child instance to be returned formating.</param>
    private void Unregister(VerySimpleRtfBuilder child)
    {
      if (child is null)
      {
        throw new ArgumentNullException(nameof(child));
      }

      if (this.formatList.Count == 0)
      {
        return;
      }

      int i = 0;
      for (; i < this.formatList.Count; i++)
      {
        if (ReferenceEquals(this.formatList[i], child))
        {
          break;
        }
      }

      while (i < this.formatList.Count)
      {
        if (this.formatList[i].endBlockOnStyleClose)
        {
          this.rtf.Append('}');
        }

        this.formatList.RemoveAt(i);
      }

      this.fontConfig.ActiveInstance = this;
    }

    #endregion

    #endregion

    #region Properties

    /// <summary>
    /// Length of text (without formating information).
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public int TextLength
    {
      [DebuggerStepThrough]
      get
      {
        return (this.txt.Length);
      }
    }

    /// <summary>
    /// Can the font be changed?
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public bool FontChangeAllowed
    {
      [DebuggerStepThrough]
      get
      {
        return (this.fontMode != FontMode.DoNotUseFonts);
      }
    }

    #endregion

    #region Public functions

    /// <summary>
    /// Get real RTF string.
    /// </summary>
    /// <returns>
    /// RTF formated string.
    /// </returns>
    public override string ToString()
    {
      StringBuilder finalRtf = new StringBuilder(this.rtf.Length + 100);
      finalRtf.AppendLine(@"{\rtf1\ansi\f0\pard ");
      if (this.coloursList.Count > 0)
      {
        finalRtf.AppendLine(@"{\colortbl;");
        foreach (Color colour in this.coloursList)
        {
          finalRtf.AppendFormat(@"\red{0}\green{1}\blue{2};", colour.R, colour.G, colour.B);
        }

        finalRtf.AppendLine("}");
      }

      if (this.fontList.Count > 0)
      {
        finalRtf.Append(@"{\fonttbl");
        int index = 1;
        foreach (string name in this.fontList)
        {
          finalRtf.AppendFormat(@"{{\f{0}\fnil\fcharset0{{\*\fname {1};}}{1};}}", index, name);
          index++;
        }

        finalRtf.AppendLine("}");
      }

      if (this.fontConfig.DefaultSize.HasValue)
      {
        finalRtf.AppendFormat(@"\fs{0}", this.fontConfig.DefaultSize.Value);
        finalRtf.AppendLine();
      }

      finalRtf.Append(this.rtf.ToString());

      int closeCount = this.formatList.Count;
      finalRtf.Append('}', closeCount + 1);
      return (finalRtf.ToString());
    }

    /// <summary>
    /// Get text part of data (without formatting).
    /// </summary>
    /// <returns>
    /// Text part of data (without formatting).
    /// </returns>
    [DebuggerStepThrough]
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public string GetText()
    {
      return (this.txt.ToString());
    }

    /// <summary>
    /// Clear RTF text.
    /// </summary>
    /// <returns>
    /// Current instance.
    /// </returns>
    public VerySimpleRtfBuilder Clear()
    {
      this.rtf.Length = 0;
      this.txt.Length = 0;
      this.coloursList.Clear();
      this.fontList.Clear();
      return (this);
    }

    #region Set style/font/colour functions

    /// <summary>
    /// Set new format for next text values.
    /// </summary>
    /// <param name="style">Format for new inserted text.</param>
    /// <returns>
    /// New instance used to end formating change or this if no format change is applied.
    /// </returns>
    /// <remarks>
    /// Use the 'using' syntax or Dispose() function to encapsulate format change.
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder SetStyle(Styles style)
    {
      if (style == Styles.None)
      {
        return (GetChild(false, null, null));
      }

      this.rtf.Append('{');
      ProcessStyle(style);
      this.rtf.Append(' ');
      return (GetChild(true, null, null));
    }

    /// <summary>
    /// Set new font name for new text values.
    /// </summary>
    /// <param name="fontName">Name of the font.</param>
    /// <returns>
    /// New instance used to end formating change.
    /// </returns>
    /// <remarks>
    /// Use the 'using' syntax or Dispose() function to encapsulate format change.
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder SetFontName(string fontName)
    {
      this.rtf.Append('{');
      ProcessFontName(fontName);
      this.rtf.Append(' ');
      return (GetChild(true, fontName, null));
    }

    /// <summary>
    /// Set new font size for new text values.
    /// </summary>
    /// <param name="halfPointsSize">New size of font in half-points.</param>
    /// <returns>
    /// New instance used to end formating change.
    /// </returns>
    /// <remarks>
    /// Use the 'using' syntax or Dispose() function to encapsulate format change.
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder SetFontSize(int halfPointsSize)
    {
      this.rtf.Append('{');
      ProcessFontSize(halfPointsSize);
      this.rtf.Append(' ');
      return (GetChild(true, null, halfPointsSize));
    }

    /// <summary>
    /// Set new font name for new text values.
    /// </summary>
    /// <param name="fontName">Name of the font.</param>
    /// <param name="halfPointsSize">New size of font in half-points.</param>
    /// <returns>
    /// New instance used to end formating change.
    /// </returns>
    /// <remarks>
    /// Use the 'using' syntax or Dispose() function to encapsulate format change.
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder SetFont(string fontName, int halfPointsSize)
    {
      this.rtf.Append('{');
      ProcessFontName(fontName);
      ProcessFontSize(halfPointsSize);
      this.rtf.Append(' ');
      return (GetChild(true, fontName, halfPointsSize));
    }

    /// <summary>
    /// Set new font name for new text values.
    /// </summary>
    /// <param name="fontName">Name of the font.</param>
    /// <param name="halfPointsSize">New size of font in half-points.</param>
    /// <param name="colour">New font colour to be used.</param>
    /// <returns>
    /// New instance used to end formating change.
    /// </returns>
    /// <remarks>
    /// Use the 'using' syntax or Dispose() function to encapsulate format change.
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder SetFont(string fontName, int halfPointsSize, Color colour)
    {
      this.rtf.Append('{');
      ProcessFontName(fontName);
      ProcessFontSize(halfPointsSize);
      ProcessFontColour(colour);
      this.rtf.Append(' ');
      return (GetChild(true, null, halfPointsSize));
    }

    /// <summary>
    /// Set new font name for new text values.
    /// </summary>
    /// <param name="fontName">Name of the font.</param>
    /// <param name="colour">New font colour to be used.</param>
    /// <returns>
    /// New instance used to end formating change.
    /// </returns>
    /// <remarks>
    /// Use the 'using' syntax or Dispose() function to encapsulate format change.
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder SetFont(string fontName, Color colour)
    {
      this.rtf.Append('{');
      ProcessFontName(fontName);
      ProcessFontColour(colour);
      this.rtf.Append(' ');
      return (GetChild(true, fontName, null));
    }

    /// <summary>
    /// Set new font colour for next text values.
    /// </summary>
    /// <param name="colour">New font colour to be used.</param>
    /// <returns>
    /// New instance used to end formating change.
    /// </returns>
    /// <remarks>
    /// Use the 'using' syntax or Dispose() function to encapsulate format change.
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder SetFontColour(Color colour)
    {
      this.rtf.Append('{');
      ProcessFontColour(colour);
      this.rtf.Append(' ');
      return (GetChild(true, null, null));
    }

    /// <summary>
    /// Set new highlight (text background) colour for next text values.
    /// </summary>
    /// <param name="colour">New highlight (text background) colour to be used.</param>
    /// <returns>
    /// New instance used to end formating change.
    /// </returns>
    /// <remarks>
    /// Use the 'using' syntax or Dispose() function to encapsulate format change.
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder SetHighlightColour(Color colour)
    {
      this.rtf.Append('{');
      ProcessHighlightColour(colour);
      this.rtf.Append(' ');
      return (GetChild(true, null, null));
    }

    /// <summary>
    /// Set new format and colours for next text values.
    /// </summary>
    /// <param name="fontColour">New font colour to be used.</param>
    /// <param name="highlightColour">New highlight (text background) colour to be used.</param>
    /// <returns>
    /// New instance used to end formating change.
    /// </returns>
    /// <remarks>
    /// Use the 'using' syntax or Dispose() function to encapsulate format change.
    /// </remarks>
    public VerySimpleRtfBuilder SetColours(Color fontColour, Color highlightColour)
    {
      this.rtf.Append('{');
      ProcessFontColour(fontColour);
      ProcessHighlightColour(highlightColour);
      this.rtf.Append(' ');
      return (GetChild(true, null, null));
    }

    /// <summary>
    /// Set new format and font colour for next text values.
    /// </summary>
    /// <param name="style">Format for new inserted text.</param>
    /// <param name="fontColour">New font colour to be used.</param>
    /// <returns>
    /// New instance used to end formating change.
    /// </returns>
    /// <remarks>
    /// Use the 'using' syntax or Dispose() function to encapsulate format change.
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder SetStyle(Styles style, Color fontColour)
    {
      this.rtf.Append('{');
      if (style != Styles.None)
      {
        ProcessStyle(style);
      }

      ProcessFontColour(fontColour);
      this.rtf.Append(' ');
      return (GetChild(true, null, null));
    }

    /// <summary>
    /// Set new format and colours for next text values.
    /// </summary>
    /// <param name="style">Format for new inserted text.</param>
    /// <param name="fontColour">New font colour to be used.</param>
    /// <param name="highlightColour">New highlight (text background) colour to be used.</param>
    /// <returns>
    /// New instance used to end formating change.
    /// </returns>
    /// <remarks>
    /// Use the 'using' syntax or Dispose() function to encapsulate format change.
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder SetStyle(Styles style, Color fontColour, Color highlightColour)
    {
      this.rtf.Append('{');
      if (style != Styles.None)
      {
        ProcessStyle(style);
      }

      ProcessFontColour(fontColour);
      ProcessHighlightColour(highlightColour);
      this.rtf.Append(' ');
      return (GetChild(true, null, null));
    }

    /// <summary>
    /// Set alignment format for next text values.
    /// </summary>
    /// <param name="alignment">Alignment type.</param>
    /// <returns>
    /// New instance used to end formating change.
    /// </returns>
    /// <remarks>
    /// Use the 'using' syntax or Dispose() function to encapsulate format change.
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder SetAlignment(Alignment alignment)
    {
      StartAlignmentBlock(alignment);
      this.rtf.Append(' ');
      return (GetChild(true, null, null));
    }

    /// <summary>
    /// Set alignment format for next text values.
    /// </summary>
    /// <param name="alignment">Alignment type.</param>
    /// <param name="firstLineIdent">First line indent in millimeters.</param>
    /// <returns>
    /// New instance used to end formating change.
    /// </returns>
    /// <remarks>
    /// Use the 'using' syntax or Dispose() function to encapsulate format change.
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder SetAlignment(Alignment alignment, int firstLineIdent)
    {
      StartAlignmentBlock(alignment);
      this.rtf.AppendFormat(@"\fi{0} ", ConvertMillimeterToTwips(firstLineIdent));
      return (GetChild(true, null, null));
    }

    /// <summary>
    /// Set alignment format for next text values.
    /// </summary>
    /// <param name="alignment">Alignment type.</param>
    /// <param name="leftIdent">Left indent in millimeters.</param>
    /// <param name="rightIdent">Right indent in millimeters.</param>
    /// <returns>
    /// New instance used to end formating change.
    /// </returns>
    /// <remarks>
    /// Use the 'using' syntax or Dispose() function to encapsulate format change.
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder SetAlignment(Alignment alignment, int leftIdent, int rightIdent)
    {
      StartAlignmentBlock(alignment);
      this.rtf.AppendFormat(@"\li{0}\ri{1} ", ConvertMillimeterToTwips(leftIdent), ConvertMillimeterToTwips(rightIdent));
      return (GetChild(true, null, null));
    }

    /// <summary>
    /// Set alignment format for next text values.
    /// </summary>
    /// <param name="alignment">Alignment type.</param>
    /// <param name="firstLineIdent">First line indent in millimeters.</param>
    /// <param name="leftIdent">Left indent in millimeters.</param>
    /// <param name="rightIdent">Right indent in millimeters.</param>
    /// <returns>
    /// New instance used to end formating change.
    /// </returns>
    /// <remarks>
    /// Use the 'using' syntax or Dispose() function to encapsulate format change.
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder SetAlignment(Alignment alignment, int firstLineIdent, int leftIdent, int rightIdent)
    {
      StartAlignmentBlock(alignment);
      this.rtf.AppendFormat(@"\fi{0}\li{1}\ri{2} ", ConvertMillimeterToTwips(firstLineIdent),
        ConvertMillimeterToTwips(leftIdent), ConvertMillimeterToTwips(rightIdent));
      return (GetChild(true, null, null));
    }

    /// <summary>
    /// Remove formating change associated with current instance.
    /// </summary>
    /// <remarks>
    /// This also invalidates current class functions.
    /// </remarks>
    public void CloseStyle()
    {
      if (this.parent is null)
      {
        return;
      }

      this.fontConfig.ActiveInstance = this.parent;
      this.parent.Unregister(this);
    }

    #endregion

    #region Append functions

    /// <summary>
    /// Add new text value to RTF.
    /// </summary>
    /// <param name="c">Character to append.</param>
    /// <returns>
    /// Current instance.
    /// </returns>
    public VerySimpleRtfBuilder Append(char c)
    {
      if (c == '\0')
      {
        throw new ArgumentOutOfRangeException(nameof(c));
      }

      this.txt.Append(c);
      AppendRtf(c);
      return (this);
    }

    /// <summary>
    /// Add new text value to RTF.
    /// </summary>
    /// <param name="text">New text value.</param>
    /// <returns>
    /// Current instance.
    /// </returns>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder Append(string text)
    {
      if (!string.IsNullOrEmpty(text))
      {
        this.txt.Append(text, 0, text.Length);
        AppendRtf(text, 0, text.Length);
      }

      return (this);
    }

    /// <summary>
    /// Add new text value to RTF.
    /// </summary>
    /// <param name="text">New text value.</param>
    /// <param name="index">Start index of the string.</param>
    /// <param name="count">Count of characters to add.</param>
    /// <returns>
    /// Current instance.
    /// </returns>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder Append(string text, int index, int count)
    {
      this.txt.Append(text, index, count);
      if (!string.IsNullOrEmpty(text))
      {
        AppendRtf(text, index, count);
      }

      return (this);
    }

    /// <summary>
    /// Add new formated text value to RTF.
    /// </summary>
    /// <param name="style">Style for inserted text value.</param>
    /// <param name="text">New text value.</param>
    /// <returns>
    /// Current instance.
    /// </returns>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder Append(Styles style, string text)
    {
      SetStyle(style).Append(text).CloseStyle();
      return (this);
    }

    /// <summary>
    /// Add new formated text value to RTF.
    /// </summary>
    /// <param name="style">Style for inserted text value.</param>
    /// <param name="text">New text value.</param>
    /// <param name="index">Start index of the string.</param>
    /// <param name="count">Count of characters to add.</param>
    /// <returns>
    /// Current instance.
    /// </returns>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder Append(Styles style, string text, int index, int count)
    {
      SetStyle(style).Append(text, index, count).CloseStyle();
      return (this);
    }

    /// <summary>
    /// Append formatted text as RTF data.
    /// </summary>
    /// <param name="format">Format of new formatted text.</param>
    /// <param name="args">Values to be added to formatted text.</param>
    /// <returns>
    /// Current instance.
    /// </returns>
    [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])", Justification = "Use system default")]
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder Append(string format, params object[] args)
    {
      if (string.IsNullOrWhiteSpace(format))
      {
        return (Append(format));
      }

      return (Append(string.Format(format, args)));
    }

    /// <summary>
    /// Add new formated text value to RTF.
    /// </summary>
    /// <param name="style">Style for inserted text value.</param>
    /// <param name="format">Format of new formatted text.</param>
    /// <param name="args">Values to be added to formatted text.</param>
    /// <returns>
    /// Current instance.
    /// </returns>
    [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])", Justification = "Use system default")]
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder Append(Styles style, string format, params object[] args)
    {
      if (!string.IsNullOrEmpty(format))
      {
        SetStyle(style).Append(string.Format(format, args)).CloseStyle();
      }

      return (this);
    }

    #endregion

    #region AppendLine functions

    /// <summary>
    /// End current paragraph.
    /// </summary>
    /// <returns>
    /// Current instance.
    /// </returns>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder AppendLine()
    {
      this.rtf.AppendLine(paragraph);
      this.txt.AppendLine();
      return (this);
    }

    /// <summary>
    /// Add new text value to RTF and end current paragraph.
    /// </summary>
    /// <param name="text">New text value.</param>
    /// <returns>
    /// Current instance.
    /// </returns>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder AppendLine(string text)
    {
      if (!string.IsNullOrEmpty(text))
      {
        AppendRtf(text, 0, text.Length);
      }

      this.rtf.AppendLine(paragraph);
      this.txt.AppendLine(text);
      return (this);
    }

    /// <summary>
    /// Add new text value to RTF and end current paragraph.
    /// </summary>
    /// <param name="format">Format of new formatted text.</param>
    /// <param name="args">Values to be added to formatted text.</param>
    /// <returns>
    /// Current instance.
    /// </returns>
    [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])", Justification = "Use system default")]
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder AppendLine(string format, params object[] args)
    {
      if (string.IsNullOrWhiteSpace(format))
      {
        return (AppendLine());
      }

      return (AppendLine(string.Format(format, args)));
    }

    /// <summary>
    /// Add new formated text value to RTF and end current paragraph.
    /// </summary>
    /// <param name="style">Format for inserted text value.</param>
    /// <param name="format">Format of new formatted text.</param>
    /// <param name="args">Values to be added to formatted text.</param>
    /// <returns>
    /// Current instance.
    /// </returns>
    [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object[])", Justification = "Use system default")]
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder AppendLine(Styles style, string format, params object[] args)
    {
      if (string.IsNullOrWhiteSpace(format))
      {
        return (AppendLine());
      }

      SetStyle(style).AppendLine(string.Format(format, args)).CloseStyle();
      return (this);
    }

    #endregion

    #region Tabulators

    /// <summary>
    /// Clear custom tabulator settings.
    /// </summary>
    /// <returns>
    /// Current instance.
    /// </returns>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder SetTabs()
    {
      this.rtf.Append("\\pard ");
      return (this);
    }

    /// <summary>
    /// Set positions for tabulators from string.
    /// </summary>
    /// <param name="tabList">List of tabulator positions (in cm) separated by semicolon.</param>
    /// <returns>
    /// Current instance.
    /// </returns>
    /// <remarks>
    /// Position of tabulators are set in cm (not pixels, points nor inch).
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder SetTabs(string tabList)
    {
      return (SetTabs(tabList, CultureInfo.CurrentCulture));
    }

    /// <summary>
    /// Set positions for tabulators from string.
    /// </summary>
    /// <param name="tabList">List of tabulator positions (in cm) separated by semicolon.</param>
    /// <param name="provider">Culture information</param>
    /// <returns>
    /// Current instance.
    /// </returns>
    /// <remarks>
    /// Position of tabulators are set in cm (not pixels, points nor inch).
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder SetTabs(string tabList, CultureInfo provider)
    {
      if (provider is null)
      {
        provider = CultureInfo.CurrentCulture;
      }

      if (string.IsNullOrEmpty(tabList))
      {
        return (SetTabs());
      }

      string[] tabsStr = tabList.Split(';');
      float[] tabPos = new float[tabsStr.Length];
      for (int i = 0; i < tabsStr.Length; i++)
      {
        tabPos[i] = float.Parse(tabsStr[i], provider);
      }

      return (SetTabs(tabPos));
    }

    /// <summary>
    /// Set positions for tabulators from list of positions.
    /// </summary>
    /// <param name="tabPositions">List of tabulator positions (in cm).</param>
    /// <returns>
    /// Current instance.
    /// </returns>
    /// <remarks>
    /// Position of tabulators are set in cm (not pixels, points nor inch).
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder SetTabs(params float[] tabPositions)
    {
      if ((tabPositions is null) || (tabPositions.Length < 1))
      {
        return (SetTabs());
      }

      int[] tabIntPos = new int[tabPositions.Length];
      for (int i = 0; i < tabPositions.Length; i++)
      {
        tabIntPos[i] = (int)(tabPositions[i] * 568);
      }

      return (SetTabs(tabIntPos));
    }

    /// <summary>
    /// Set positions for tabulators from list of positions.
    /// </summary>
    /// <param name="tabPositions">List of tabulator positions (in local format value).</param>
    /// <returns>
    /// Current instance.
    /// </returns>
    /// <remarks>
    /// Position of tabulators are set in local values. 568 = 1 cm.
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Public user interface")]
    public VerySimpleRtfBuilder SetTabs(params int[] tabPositions)
    {
      if ((tabPositions is null) || (tabPositions.Length < 1))
      {
        return (SetTabs());
      }

      this.rtf.Append("\\pard");
      foreach (int pos in tabPositions)
      {
        this.rtf.AppendFormat(CultureInfo.InvariantCulture, "\\tx{0}", pos);
      }

      this.rtf.Append(" ");
      return (this);
    }

    #endregion

    #endregion
  }
}
