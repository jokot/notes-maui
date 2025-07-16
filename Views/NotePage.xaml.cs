namespace Notes.Views;

public partial class NotePage : ContentPage
{

	public NotePage(NoteViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}