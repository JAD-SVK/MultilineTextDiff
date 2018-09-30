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
using System.ComponentModel;
using System.Diagnostics;

namespace Atrip.MultilineTextDiff
{
  internal sealed class Worker : IDisposable
  {
    private readonly BackgroundWorker worker;

    public Worker()
    {
      this.worker = new BackgroundWorker();
      this.worker.DoWork += Worker_DoWork;
      this.worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
    }

    public event EventHandler<EditResultEventArgs> Finished;

    public bool IsBusy
    {
      [DebuggerStepThrough]
      get;

      [DebuggerStepThrough]
      private set;
    }

    public void Dispose() => this.worker.Dispose();

    public void Start(string sourrce, string modified)
    {
      if (this.IsBusy)
      {
        throw new InvalidOperationException("Comparison already running!");
      }

      this.IsBusy = true;
      this.worker.RunWorkerAsync(new[] { sourrce, modified });
    }

    private static string FixLineEndings(string text) => (text.Replace("\r\n", "\n").Replace("\r", "\n"));

    private static void Worker_DoWork(object sender, DoWorkEventArgs e)
    {
      string[] texts = (string[])e.Argument;
      var diff = new LevenstheinMethod
      {
        Source = FixLineEndings(texts[0]),
        Target = FixLineEndings(texts[1])
      };

      e.Result = diff.GetEditSequence();
    }

    private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      Finished?.Invoke(this, new EditResultEventArgs((IList<EditOperation>)e.Result));
      this.IsBusy = false;
    }
  }
}
