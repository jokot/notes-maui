namespace Notes.Maui.Views.Features.Notes;

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