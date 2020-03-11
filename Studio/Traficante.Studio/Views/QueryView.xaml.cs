using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System.Reactive.Disposables;
using Traficante.Studio.ViewModels;

namespace Traficante.Studio.Views
{
    public class QueryView : ReactiveUserControl<QueryViewModel>
    {
        public Grid Grid => this.FindControl<Grid>("Grid");
        public Button Run => this.FindControl<Button>("Run");
        public TextBox Text => this.FindControl<TextBox>("Text");
        public TabControl Results => this.FindControl<TabControl>("Results");
        public DataGrid ResultsData => this.FindControl<DataGrid>("ResultsData");

        public TextBlock ResultsError => this.FindControl<TextBlock>("ResultsError");
        public TextBlock ResultsMessage => this.FindControl<TextBlock>("ResultsMessage");
        public GridSplitter ResultsSplitter => this.FindControl<GridSplitter>("ResultsSplitter");
        
        public QueryView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables =>
            {
                this.Bind(ViewModel, x => x.Text, x => x.Text.Text)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.RunCommand, x => x.Run)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.ResultsData, x => x.ResultsData.Items, x => (System.Collections.IEnumerable)x)
                    .DisposeWith(disposables);
                this.Bind(ViewModel, x => x.ResultsError, x => x.ResultsError.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel, x => x.ResultsMessage, x => x.ResultsMessage.Text)
                    .DisposeWith(disposables);
                this.Bind(ViewModel, x => x.ResultsAreVisible, x => x.Results.IsVisible)
                    .DisposeWith(disposables);
                this.Bind(ViewModel, x => x.ResultsAreVisible, x => x.ResultsSplitter.IsVisible)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.ResultsAreVisible, x => x.Grid.RowDefinitions[3].Height, x => x ? new GridLength(1, GridUnitType.Star) : new GridLength(0, GridUnitType.Auto))
                    .DisposeWith(disposables);
                this.ViewModel.ResultsDataColumns = this.ResultsData.Columns;

                //var resultRow = this.Grid.RowDefinitions[3].Height.IsAbsolute;

                //this.OneWayBind(ViewModel, x => x.ResultsColumns, x => x.Results.Columns)
                //    .DisposeWith(disposables);
            });
            AvaloniaXamlLoader.Load(this);
        }
    }
}
