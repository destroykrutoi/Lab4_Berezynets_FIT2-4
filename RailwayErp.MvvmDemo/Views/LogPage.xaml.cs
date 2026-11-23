using RailwayErp.MvvmDemo.ViewModels;

namespace RailwayErp.MvvmDemo.Views;

public partial class LogPage : ContentPage
{
    public LogPage(LogViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
