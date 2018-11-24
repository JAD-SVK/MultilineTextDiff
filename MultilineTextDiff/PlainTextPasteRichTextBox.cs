using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace Atrip.Utils.GUI
{
  internal class PlainTextPasteRichTextBox : RichTextBox
  {
    // Not a standard way how the paste functionality is used by this control. Only for safety reasons.
    private const int WM_PASTE = 0x0302;

    // Correct keys combination for paste.
    private const Keys paste1_ok = (Keys.V | Keys.Control);
    private const Keys paste2_ok = (Keys.Insert | Keys.Shift);

    // The following key combinations are should not paste data,
    // but unfortunately the base control process these combinations as valid.
    private const Keys paste1_error = (Keys.V | Keys.Control | Keys.Shift);
    private const Keys paste2_error = (Keys.Insert | Keys.Control | Keys.Shift);

    [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "false positive, not a public class")]
    protected override void WndProc(ref Message m)
    {
      if (m.Msg == WM_PASTE)
      {
        // Not a standard way how the paste functionality is used by this control. Only for safety reasons.
        PasteText();
      }
      else
      {
        base.WndProc(ref m);
      }
    }

    [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", Justification = "false positive, not a public class")]
    protected override void OnKeyDown(KeyEventArgs e)
    {
      switch (e.KeyData)
      {
      case paste1_ok:
      case paste2_ok:
        e.Handled = PasteText();
        base.OnKeyDown(e);
        break;

      case paste1_error:
      case paste2_error:
        e.Handled = true;
        base.OnKeyDown(e);
        break;

      default:
        base.OnKeyDown(e);
        break;
      }
    }

    [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Can not fall to the system")]
    private bool PasteText()
    {
      if (this.ReadOnly)
      {
        return (false);
      }

      try
      {
        if (Clipboard.ContainsText())
        {
          this.SelectedText = Clipboard.GetText();
        }
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.Message);
      }

      return (true);
    }
  }
}
