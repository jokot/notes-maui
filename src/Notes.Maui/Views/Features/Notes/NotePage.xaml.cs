namespace Notes.Maui.Views.Features.Notes;

public partial class NotePage : ContentPage
{

	public NotePage(NoteViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}