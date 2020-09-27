using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Collections.Generic;
using System.Reactive.Disposables;
using Traficante.Studio.ViewModels;
using AvaloniaEdit;
using Avalonia.Input;
using System.Reactive.Linq;
using System.Linq;
using DynamicData.Binding;
using Avalonia.VisualTree;
using System.Diagnostics;
using Traficante.Connect;

namespace Traficante.Studio.Views
{
    public class QueryView : ReactiveUserControl<QueryViewModel>
    {
        public Grid Grid => this.FindControl<Grid>("Grid");
        public Button Run => this.FindControl<Button>("Run");
        public Button Cancel => this.FindControl<Button>("Cancel");
        public Button DatabaseSelector => this.FindControl<Button>("DatabaseSelector");
        public EditorView Text => this.FindControl<EditorView>("Text");
        public TabControl Results => this.FindControl<TabControl>("Results");
        public DataGrid ResultsData => this.FindControl<DataGrid>("ResultsData");
        public TextBox ResultsError => this.FindControl<TextBox>("ResultsError");
        public TextBox ResultsMessage => this.FindControl<TextBox>("ResultsMessage");
        public GridSplitter ResultsSplitter => this.FindControl<GridSplitter>("ResultsSplitter");
        public MenuItem ExportResultsAs => this.FindControl<MenuItem>("ExportResultsAs");
        public TextBlock ResultsCount => this.FindControl<TextBlock>("ResultsCount");
        public TextBlock Ln => this.FindControl<TextBlock>("Ln");
        public TextBlock Col => this.FindControl<TextBlock>("Col");
        public TextBlock SelectedObject => this.FindControl<TextBlock>("SelectedObject");
        public TextBlock SelectedLanguage => this.FindControl<TextBlock>("SelectedLanguage");

        public QueryView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables =>
            {
                this.Bind(ViewModel, x => x.Query.Text, x => x.Text.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel, x => x.SelectedText, x => x.Text.SelectedText)
                    .DisposeWith(disposables);
                this.WhenAnyValue(x => x.Text.Line)
                    .Subscribe(x => this.Ln.Text = x.ToString());
                this.WhenAnyValue(x => x.Text.Column)
                    .Subscribe(x => this.Col.Text = x.ToString());

                this.BindCommand(ViewModel, x => x.RunCommand, x => x.Run)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.CancelCommand, x => x.Cancel)
                    .DisposeWith(disposables);
                this.ViewModel
                    .RunCommand
                    .IsExecuting
                    .Subscribe(isExecuting =>
                    {
                        this.Run.IsVisible = !isExecuting;
                        this.Cancel.IsVisible = isExecuting;

                        if (isExecuting && this.Results.IsVisible == false)
                        {
                            this.Results.IsVisible = true;
                            this.ResultsSplitter.IsVisible = true;
                            this.Grid.RowDefinitions[3].Height = new GridLength(1, GridUnitType.Star);
                        }
                    })
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.DatabaseSelectorCommand, x => x.DatabaseSelector)
                    .DisposeWith(disposables);

                this.ViewModel.ResultsDataColumns = this.ResultsData.Columns;
                this.OneWayBind(ViewModel, x => x.ResultsData, x => x.ResultsData.Items, x => (System.Collections.IEnumerable)x)
                    .DisposeWith(disposables);
                this.WhenAnyValue(x => x.ViewModel.ResultsDataColumns)
                    .Subscribe(columns =>
                    {
                        columns.ToObservableChangeSet().Subscribe(column =>
                        {
                            this.ResultsData.IsVisible = columns != null && columns.Count > 0;
                        });
                    })
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.ResultsError)
                    .Subscribe(x =>
                    {
                        this.ResultsError.Text = x;
                        this.ResultsError.IsVisible = !string.IsNullOrEmpty(x);
                        this.Results.SelectedIndex = string.IsNullOrEmpty(x) ? 0 : 1;
                    })
                    .DisposeWith(disposables);
                this.WhenAnyValue(x => x.ViewModel.ResultsMessage)
                    .Subscribe(x =>
                    {
                        this.ResultsMessage.IsVisible = !string.IsNullOrEmpty(x);
                        this.ResultsMessage.Text = x;
                    })
                    .DisposeWith(disposables);
      
                this.BindCommand(ViewModel, x => x.SaveResultsAsCommand, x => x.ExportResultsAs);

                this.Bind(ViewModel, x => x.ResultsCount, x => x.ResultsCount.Text)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.Query.SelectedLanguageId)
                    .Subscribe(x =>
                    {
                        this.SelectedLanguage.Text = QueryLanguage.All.FirstOrDefault(y => y.Id == x)?.Name;
                    })
                    .DisposeWith(disposables);
                this.WhenAnyValue(x => x.ViewModel.Query.SelectedObjectPath)
                    .Subscribe(x =>
                    {
                        if (x != null && x.Length > 0)
                            this.SelectedObject.Text = string.Join(".", x);
                        else
                            this.SelectedObject.Text = new NotSelectedDatabaseModel().Title;
                    })
                    .DisposeWith(disposables);

            });
            AvaloniaXamlLoader.Load(this);


        }
    }
}
