using Notes.ViewModels;

namespace Notes.Views;

public partial class AllNotesPage : ContentPage
{
    public AllNotesPage(AllNotesViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        await ((AllNotesViewModel)BindingContext).GetNotesAsync();
    }
}