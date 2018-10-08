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

namespace Atrip.MultilineTextDiff
{
  internal sealed class LevenstheinMethod
  {
    [Flags]
    private enum Operations
    {
      None,

      Insert = 1,

      Remove = 2,

      Edit = 4,

      Copy = 8
    }

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    public int CopyCost
    {
      [DebuggerStepThrough]
      get;

      [DebuggerStepThrough]
      set;
    } = 1;

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    public int EditCost
    {
      [DebuggerStepThrough]
      get;

      [DebuggerStepThrough]
      set;
    } = 2;

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    public int InsertCost
    {
      [DebuggerStepThrough]
      get;

      [DebuggerStepThrough]
      set;
    } = 1;

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    public int RemoveCost
    {
      [DebuggerStepThrough]
      get;

      [DebuggerStepThrough]
      set;
    } = 1;

    [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
    public int WhiteSpacePreference
    {
      [DebuggerStepThrough]
      get;

      [DebuggerStepThrough]
      set;
    } = 2;

    public string Source
    {
      [DebuggerStepThrough]
      get;

      [DebuggerStepThrough]
      set;
    }

    public string Target
    {
      [DebuggerStepThrough]
      get;

      [DebuggerStepThrough]
      set;
    }

    public List<EditOperation> GetEditSequence()
    {
      return (ProcessEditSequence(this.Source, this.Target, this.InsertCost, this.RemoveCost, this.EditCost, this.CopyCost, this.WhiteSpacePreference));
    }

    [SuppressMessage("Microsoft.Performance", "CA1814:PreferJaggedArraysOverMultidimensional", MessageId = "Body", Justification = "Better code readability")]
    private static List<EditOperation> ProcessEditSequence(string source, string target,
      int insertCost, int removeCost, int editCost, int copyCost, int whiteSpacePreference)
    {
      // Forward: building score matrix

      // Best operation (among insert, edit/copy, remove) to perform
      Operations[,] nextOperation = new Operations[source.Length + 1, target.Length + 1];

      // Minimum cost so far
      int[,] pathCost = new int[source.Length + 1, target.Length + 1];

      // Edge: all removes
      for (int i = 1; i <= source.Length; i++)
      {
        nextOperation[i, 0] = Operations.Remove;
        pathCost[i, 0] = removeCost * i;
      }

      // Edge: all inserts
      for (int i = 1; i <= target.Length; i++)
      {
        nextOperation[0, i] = Operations.Insert;
        pathCost[0, i] = insertCost * i;
      }

      // fill the cost and operation table
      for (int i = 1; i <= source.Length; i++)
      {
        char sourceCharacter = source[i - 1];
        for (int j = 1; j <= target.Length; j++)
        {
          // here we choose the operation with the least cost
          char targetCharacter = target[j - 1];
          bool copy = (sourceCharacter == targetCharacter);
          int insert = pathCost[i, j - 1] + insertCost;
          int remove = pathCost[i - 1, j] + removeCost;
          int edit = pathCost[i - 1, j - 1] + ((copy) ? (copyCost) : (editCost));

          if ((!char.IsWhiteSpace(sourceCharacter)) && (!char.IsWhiteSpace(targetCharacter)) && !copy)
          {
            insert *= whiteSpacePreference;
            remove *= whiteSpacePreference;
            edit *= whiteSpacePreference;
          }

          int min = Math.Min(Math.Min(insert, remove), edit);

          if (min == insert)
          {
            nextOperation[i, j] |= Operations.Insert;
          }

          if (min == remove)
          {
            nextOperation[i, j] |= Operations.Remove;
          }

          if (min == edit)
          {
            nextOperation[i, j] |= ((copy) ? (Operations.Copy) : (Operations.Edit));
          }

          pathCost[i, j] = min;
        }
      }

      // Backward: knowing costs and operations let's building edit sequence (in reverse order, from end to start)
      List<EditOperation> result = new List<EditOperation>(source.Length + target.Length);

      Operations previousOperation = Operations.None;
      for (int x = target.Length, y = source.Length; (x > 0) || (y > 0);)
      {
        EditOperationKind op = GetNextOperation(nextOperation[y, x], insertCost, removeCost, editCost, copyCost, ref previousOperation);

        switch (op)
        {
        case EditOperationKind.Insert:
          x--;
          result.Add(new EditOperation(target[x], op));
          break;

        case EditOperationKind.Remove:
          y--;
          result.Add(new EditOperation(source[y], op));
          break;

        default: // EditOperationKind.Edit, EditOperationKind.Copy
          x--;
          y--;
          result.Add(new EditOperation(target[x], op));
          Debug.Assert((op == EditOperationKind.Edit) || (op == EditOperationKind.Copy));
          break;
        }
      }

      result.Reverse();
      return (result);
    }

    private static EditOperationKind GetNextOperation(Operations nextOperation, int insertCost, int removeCost,
      int editCost, int copyCost, ref Operations previousOperation)
    {
      if ((previousOperation != Operations.None) && ((nextOperation & previousOperation) == previousOperation))
      {
        switch (previousOperation)
        {
        case Operations.Insert:
          return EditOperationKind.Insert;

        case Operations.Remove:
          return EditOperationKind.Remove;

        case Operations.Edit:
          return EditOperationKind.Edit;

        case Operations.Copy:
          return EditOperationKind.Copy;

        default:
          throw new ArgumentOutOfRangeException(nameof(previousOperation));
        }
      }

      int min = int.MaxValue;
      EditOperationKind operation = EditOperationKind.Edit;
      if ((nextOperation & Operations.Copy) != Operations.None)
      {
        min = copyCost;
        operation = EditOperationKind.Copy;
        previousOperation = Operations.Copy;
      }
      else if ((nextOperation & Operations.Edit) != Operations.None)
      {
        min = editCost;
        previousOperation = Operations.Edit;
      }

      if ((min >= removeCost) && ((nextOperation & Operations.Remove) != Operations.None))
      {
        min = removeCost;
        operation = EditOperationKind.Remove;
        previousOperation = Operations.Remove;
      }

      if ((min >= insertCost) && ((nextOperation & Operations.Insert) != Operations.None))
      {
        operation = EditOperationKind.Insert;
        previousOperation = Operations.Insert;
      }

      return (operation);
    }
  }
}
