﻿using ClangPowerTools.DiffStyle;
using ClangPowerTools.MVVM.Interfaces;
using ClangPowerTools.MVVM.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace ClangPowerTools.MVVM.Controllers
{
  public class DiffController
  {
    #region Members

    public EventHandler ClosedWindow;

    private readonly StyleFormatter formatter;
    private EditorStyles formatStyle;
    private List<IFormatOption> formatOptions;
    private string editorInput;
    private readonly Action CreateFormatFile;

    #endregion


    #region Constructor

    public DiffController(Action CreateFormatFile)
    {
      formatter = new StyleFormatter();
      StyleDetector.StopDetection = false;
      ClosedWindow += CloseLoadingView;
      this.CreateFormatFile = CreateFormatFile;
    }

    private void CloseLoadingView(object sender, EventArgs e)
    {
      StyleDetector.StopDetection = true;
      ClosedWindow -= CloseLoadingView;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Get the found EditorStyle and FormatOptions
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public async Task<(EditorStyles matchedStyle, List<IFormatOption> matchedOptions)> GetFormatOptionsAsync(string text)
    {
      editorInput = text;

      var styleDetector = new StyleDetector();
      var (matchedStyle, matchedOptions) = await styleDetector.DetectStyleOptionsAsync(text);

      formatStyle = matchedStyle;
      formatOptions = matchedOptions;
      return (formatStyle, formatOptions);
    }

    /// <summary>
    /// Display the diffs in an html format after GetFormatOptionsAsync
    /// </summary>
    /// <returns></returns>
    public async Task ShowDiffAsync(string formatOptionFile)
    {
      string editorOutput = string.Empty;
      var diffMatchPatchWrapper = new DiffMatchPatchWrapper();
      await Task.Run(() =>
      {
        editorOutput = formatter.FormatText(editorInput, formatOptions, formatStyle);
        diffMatchPatchWrapper.Diff(editorInput, editorOutput);
        diffMatchPatchWrapper.CleanupSemantic();
      });

      DisplayDiffWindow(formatOptionFile, editorOutput, diffMatchPatchWrapper);
    }

    #endregion

    #region Private Methods

    private void DisplayDiffWindow(string formatOptionFile, string editorOutput, DiffMatchPatchWrapper diffMatchPatchWrapper)
    {
      (FlowDocument diffInput, FlowDocument diffOutput) = diffMatchPatchWrapper.DiffAsFlowDocuments(editorInput, editorOutput);
      var diffWindow = new DiffWindow(diffInput, diffOutput, formatOptionFile, CreateFormatFile);
      diffWindow.Show();
    }

    #endregion
  }
}
