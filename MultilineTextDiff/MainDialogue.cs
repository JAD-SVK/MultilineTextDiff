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
using System.Windows.Forms;

using Atrip.MultilineTextDiff.Properties;
using Atrip.Utils;
using Atrip.Utils.Texts;

namespace Atrip.MultilineTextDiff
{
  internal partial class MainDialogue : Form
  {
    private readonly Timer timer;
    private readonly Worker worker;

    [SuppressMessage("Microsoft.Mobility", "CA1601:DoNotUseTimersThatPreventPowerStateChanges", Justification = "Slow response for a interactive application")]
    [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly")]
    public MainDialogue()
    {
      InitializeComponent();

      this.Icon = Resources.smallIcon;
      this.timer = new Timer();
      this.timer.Tick += Timer_Tick;
      this.timer.Enabled = false;
      this.timer.Interval = 150;
      this.worker = new Worker();
      this.worker.Finished += Worker_Finished;

#if DEBUG
      this.originalRichTextBox.Text = DebugValues.source;
      this.modifiedRichTextBox.Text = DebugValues.modified;
#endif
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        this.timer.Dispose();
        this.worker.Dispose();

#pragma warning disable RCS1146 // Use conditional access.
        // conditional access will cause another code analyser warning
        if (components != null)
        {
          components.Dispose();
        }
#pragma warning restore RCS1146 // Use conditional access.
      }
      base.Dispose(disposing);
    }

    private void Worker_Finished(object sender, EditResultEventArgs e)
    {
      if (this.changeRichTextBox.InvokeRequired)
      {
        this.changeRichTextBox.BeginInvoke((MethodInvoker)delegate
        {
          Worker_Finished(sender, e);
        });
      }

      this.changeRichTextBox.FullClear();
      this.changeRichTextBox.Rtf = GenerateRtf(e.Sequence);
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
      if (this.worker.IsBusy)
      {
        return;
      }

      this.timer.Stop();
      this.worker.Start(this.originalRichTextBox.Text, this.modifiedRichTextBox.Text);
    }

    private void MainDialogue_Load(object sender, EventArgs e)
    {
      // only to remove code analyser warnings: S1186 and CC0091
      this.changeRichTextBox.FullClear();
#if DEBUG
      Timer_Tick(sender, e);
#endif
    }

    private static string GenerateRtf(IList<EditOperation> sequence)
    {
      EditOperationKind previousState = EditOperationKind.Copy;
      VerySimpleRtfBuilder rtf = new VerySimpleRtfBuilder(sequence.Count + 2048);
      VerySimpleRtfBuilder style = null;

      foreach (EditOperation operation in sequence)
      {
        if (previousState != operation.Operation)
        {
          previousState = operation.Operation;
          style?.CloseStyle();
          switch (previousState)
          {
          case EditOperationKind.Copy:
            style = null;
            break;

          case EditOperationKind.Edit:
            style = rtf.SetStyle(VerySimpleRtfBuilder.Styles.BoldItalic | VerySimpleRtfBuilder.Styles.Underline, Color.Empty, Color.Gray);
            break;

          case EditOperationKind.Insert:
            style = rtf.SetStyle(VerySimpleRtfBuilder.Styles.Bold | VerySimpleRtfBuilder.Styles.Underline, Color.Empty, Color.LightGreen);
            break;

          case EditOperationKind.Remove:
            style = rtf.SetStyle(VerySimpleRtfBuilder.Styles.Strikeout, Color.Empty, Color.LightPink);
            break;

          default:
            Debug.Fail("Incorrect operation type!");
            style = rtf.SetColours(Color.DarkRed, Color.LightPink);
            break;
          }
        }

        if ((previousState == EditOperationKind.Remove) && (operation.Value == '\n'))
        {
          rtf.Append('¶');
        }
        else
        {
          rtf.Append(operation.Value);
        }
      }

      return (rtf.ToString());
    }

    private void Source_TextChanged(object sender, EventArgs e)
    {
      // restart timer after change
      this.timer.Stop();
      this.timer.Start();
    }
  }
}
