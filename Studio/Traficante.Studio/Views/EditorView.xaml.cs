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

namespace Traficante.Studio.Views
{
    public class EditorView : ReactiveUserControl<EditorViewModel>
    {
        public TextEditor TextEditor => this.FindControl<TextEditor>("TextEditor");
        public TextBlock Ln => this.FindControl<TextBlock>("Ln");
        public TextBlock Col => this.FindControl<TextBlock>("Col");
        

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
                    this.SetValue(TextProperty, x);
                });

                Observable.FromEventPattern(
                    this.TextEditor.TextArea.Caret,
                    nameof(this.TextEditor.TextArea.Caret.PositionChanged))
                .Subscribe(x =>
                {
                    this.Ln.Text = this.TextEditor.TextArea.Caret.Line.ToString();
                    this.Col.Text = this.TextEditor.TextArea.Caret.Column.ToString();
                });
                
                this.TextEditor.AddHandler(DragDrop.DropEvent, Drop);
                this.TextEditor.AddHandler(DragDrop.DragOverEvent, DragOver);
                DragDrop.SetAllowDrop(this.TextEditor, true);
                DragDrop.SetAllowDrop(this.TextEditor.TextArea, true);
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
