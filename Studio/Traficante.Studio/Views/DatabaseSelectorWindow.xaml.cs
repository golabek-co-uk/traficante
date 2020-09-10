using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reactive;
using System.Reactive.Disposables;
using Traficante.Studio.Models;
using Traficante.Studio.ViewModels;

namespace Traficante.Studio.Views
{
    public class DatabaseSelectorWindow : ReactiveWindow<DatabaseSelectorWindowViewModel>
    {
        public TreeView Objects => this.FindControl<TreeView>("Objects");
        public Button OK => this.FindControl<Button>("Ok");
        public Button Cancel => this.FindControl<Button>("Cancel");
        public DatabaseSelectorWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            this.WhenActivated(disposables =>
            {
                ViewModel.CloseInteraction.RegisterHandler(x =>
                {
                    try { this.Close(); } catch { }
                    x.SetOutput(Unit.Default);
                });
                this.BindCommand(ViewModel, x => x.OkCommand, x => x.OK)
                    .DisposeWith(disposables);
                this.BindCommand(ViewModel, x => x.CancelCommand, x => x.Cancel)
                    .DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.Objects, x => x.Objects.Items, x => (System.Collections.IEnumerable)x)
                    .DisposeWith(disposables);
                this.Bind(ViewModel, x => x.SelectedObject, x => x.Objects.SelectedItem)
                    .DisposeWith(disposables);
            });

            AvaloniaXamlLoader.Load(this);
        }
    }
}
