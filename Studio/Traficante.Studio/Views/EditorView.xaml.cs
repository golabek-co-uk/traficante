using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using AvaloniaEdit;
using ReactiveUI;
using System.Linq;
using System.Reactive.Linq;
using Traficante.Studio.ViewModels;
using ReactiveUI.Fody.Helpers;
using AvaloniaEdit.Highlighting;
using System.IO;
using System.Xml;
using AvaloniaEdit.Highlighting.Xshd;
using Traficante.Studio.Highlighting;

namespace Traficante.Studio.Views
{
    public class EditorView : ReactiveUserControl<EditorViewModel>
    {
        public TextEditor TextEditor => this.FindControl<TextEditor>("TextEditor");


        public static readonly AvaloniaProperty<string> TextProperty = AvaloniaProperty.Register<EditorView, string>("Text");
        public string Text
        {
            get { return this.TextEditor.Text; }
            set 
            {
                this.TextEditor.Text = value;
                this.SetValue(TextProperty, value);
            }
        }

        public static readonly AvaloniaProperty<int> LineProperty = AvaloniaProperty.Register<EditorView, int>("Line");
        public int Line
        {
            get => this.GetValue(LineProperty);
            set => this.SetValue(LineProperty, value);
        }

        public static readonly AvaloniaProperty<int> ColumnProperty = AvaloniaProperty.Register<EditorView, int>("Column");
        public int Column
        {
            get => this.GetValue(ColumnProperty);
            set => this.SetValue(ColumnProperty, value);
        }

        public static readonly AvaloniaProperty<string> SelectedTextProperty = AvaloniaProperty.Register<EditorView, string>("SelectedText");
        public string SelectedText
        {
            get { return this.TextEditor.SelectedText; }
            set { this.SetValue(SelectedTextProperty, value); }
        }

        public EditorView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables =>
            {
                Observable.FromEventPattern(
                    this.TextEditor,
                    nameof(this.TextEditor.TextChanged))
                .Subscribe(x =>
                {
                    this.SetValue(TextProperty, this.TextEditor.Text);
                });

                Observable.FromEventPattern(
                    this.TextEditor.TextArea.Caret,
                    nameof(this.TextEditor.TextArea.Caret.PositionChanged))
                .Subscribe(x =>
                {
                    Line = this.TextEditor.TextArea.Caret.Line;
                    Column = this.TextEditor.TextArea.Caret.Column;
                });

                Observable.FromEventPattern(
                    this.TextEditor.TextArea,
                    nameof(this.TextEditor.TextArea.SelectionChanged))
                .Subscribe(x =>
                {
                    this.SelectedText = this.TextEditor.SelectedText;
                });

                this.TextEditor.AddHandler(DragDrop.DropEvent, Drop);
                this.TextEditor.AddHandler(DragDrop.DragOverEvent, DragOver);
                DragDrop.SetAllowDrop(this.TextEditor, true);
                DragDrop.SetAllowDrop(this.TextEditor.TextArea, true);

                this.TextEditor.SyntaxHighlighting = new HighlightingHelper().LoadHighlightingDefinition("TSQL");

                //new AvaloniaEdit.Indentation.CSharp.CSharpIndentationStrategy()
                //this.TextEditor
            });
            
            AvaloniaXamlLoader.Load(this);
        }

        private void DragOver(object sender, DragEventArgs e)
        {

            // Only allow Copy or Link as Drop Operations.
            e.DragEffects = e.DragEffects & (DragDropEffects.Copy | DragDropEffects.Link);

            // Only allow if the dragged data contains text or filenames.
            if (!e.Data.Contains(DataFormats.Text) && !e.Data.Contains(DataFormats.FileNames))
                e.DragEffects = DragDropEffects.None;


            var point = e.GetPosition(this.TextEditor);
            var position = this.TextEditor.GetPositionFromPoint(point);
            if (position.HasValue)
            {
                var offset = this.TextEditor.TextArea.Document.GetOffset(position.Value.Line, position.Value.Column);
                this.TextEditor.Select(offset, 0);
                this.TextEditor.Focus();
            }
        }

        private void Drop(object sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.Text))
            {
                var point = e.GetPosition(this.TextEditor);
                var position = this.TextEditor.GetPositionFromPoint(point);
                if (position.HasValue)
                {
                    var text = e.Data.GetText();
                    this.TextEditor.TextArea.Selection.ReplaceSelectionWithText(text);
                    this.TextEditor.Focus();
                }

            }
        }
    }
}
