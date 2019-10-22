using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using System.Reactive.Disposables;
using Traficante.Studio.ViewModels;

namespace Traficante.Studio.Views
{
    public class QueryView : ReactiveUserControl<QueryViewModel>
    {
        public Button Run => this.FindControl<Button>("Run");
        public DropDown Objects => this.FindControl<DropDown>("Objects");
        public TextBox Text => this.FindControl<TextBox>("Text");
        public DataGrid Results => this.FindControl<DataGrid>("Results");
        

        public QueryView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel, x => x.Objects, x => x.Objects.Items, x => (System.Collections.IEnumerable)x)
                    .DisposeWith(disposables);
                this.Bind(ViewModel, x => x.SelectedObject, x => x.Objects.SelectedItem)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.Text, x => x.Text.Text)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.RunCommand, x => x.Run)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.Results, x => x.Results.Items, x => (System.Collections.IEnumerable)x)
                    .DisposeWith(disposables);
                
            });
            AvaloniaXamlLoader.Load(this);
        }
    }
}
