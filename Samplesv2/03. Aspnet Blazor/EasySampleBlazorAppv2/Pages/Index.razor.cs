using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using EasySampleBlazorLib;
using BlazorMonaco;

namespace EasySampleBlazorAppv2.Pages
{
    public partial class Index : ComponentBase
    {
        [Inject]
        protected ILogger<Counter> _logger { get; set; }

        private MonacoEditor _editor { get; set; }
        private string ValueToSet { get; set; }

        private StandaloneEditorConstructionOptions EditorConstructionOptions(MonacoEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                Language = "json",
                GlyphMargin = true,
                Value = @"{
                    ""Logging"": {
                        ""LogLevel"": {
                            ""Default"": ""Information"",
                            ""Microsoft"": ""Warning"",
                            ""Microsoft.Hosting.Lifetime"": ""Information""
                        }
                    }
                }"
            };
        }

        private async Task EditorOnDidInit(MonacoEditorBase editor)
        {
            await _editor.AddCommand((int)KeyMode.CtrlCmd | (int)KeyCode.KEY_H, (editor, keyCode) =>
            {
                Console.WriteLine("Ctrl+H : Initial editor command is triggered.");
            });

            var newDecorations = new ModelDeltaDecoration[]
            {
                new ModelDeltaDecoration
                {
                    Range = new BlazorMonaco.Range(3,1,3,1),
                    Options = new ModelDecorationOptions
                    {
                        IsWholeLine = true,
                        ClassName = "decorationContentClass",
                        GlyphMarginClassName = "decorationGlyphMarginClass"
                    }
                }
            };

            decorationIds = await _editor.DeltaDecorations(null, newDecorations);
            // You can now use 'decorationIds' to change or remove the decorations
        }

        private string[] decorationIds;

        private void OnContextMenu(EditorMouseEvent eventArg)
        {
            Console.WriteLine("OnContextMenu : " + System.Text.Json.JsonSerializer.Serialize(eventArg));
        }

        private async Task ChangeTheme(ChangeEventArgs e)
        {
            Console.WriteLine($"setting theme to: {e.Value.ToString()}");
            await MonacoEditor.SetTheme(e.Value.ToString());
        }

        private async Task SetValue()
        {
            Console.WriteLine($"setting value to: {ValueToSet}");
            await _editor.SetValue(ValueToSet);
        }

        private async Task GetValue()
        {
            var val = await _editor.GetValue();
            Console.WriteLine($"value is: {val}");
        }

        private async Task AddCommand()
        {
            await _editor.AddCommand((int)KeyMode.CtrlCmd | (int)KeyCode.Enter, (editor, keyCode) =>
            {
                Console.WriteLine("Ctrl+Enter : Editor command is triggered.");
            });
        }

        private async Task AddAction()
        {
            await _editor.AddAction("testAction", "Test Action", new int[] { (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_D, (int)KeyMode.CtrlCmd | (int)KeyCode.KEY_B }, null, null, "navigation", 1.5, (editor, keyCodes) =>
            {
                Console.WriteLine("Ctrl+D : Editor action is triggered.");
            });
        }
    }
}