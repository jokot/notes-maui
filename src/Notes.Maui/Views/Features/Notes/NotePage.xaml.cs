using System.ComponentModel;
using Notes.Maui.ViewModels.Features.Notes;

namespace Notes.Maui.Views.Features.Notes;

public partial class NotePage : ContentPage
{
	private readonly NoteViewModel _viewModel;
	private readonly ToolbarItem _deleteToolbarItem;

	public NotePage(NoteViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = viewModel;
		
		// Create delete toolbar item programmatically
		_deleteToolbarItem = new ToolbarItem
		{
			Text = "Delete",
			AutomationId = "DeleteButton",
			IconImageSource = new FontImageSource
			{
				Glyph = "✕",
				FontFamily = "Default",
				Color = Colors.Black,
				Size = 20,
			}
		};
		_deleteToolbarItem.SetBinding(ToolbarItem.CommandProperty, new Binding(nameof(NoteViewModel.DeleteNoteCommand)));
		
		// Subscribe to property changes
		_viewModel.PropertyChanged += OnViewModelPropertyChanged;
		
		// Set initial visibility
		UpdateDeleteButtonVisibility();
	}

	private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(NoteViewModel.IsEditMode))
		{
			UpdateDeleteButtonVisibility();
		}
	}

	private void UpdateDeleteButtonVisibility()
	{
		if (_viewModel.IsEditMode)
		{
			if (!ToolbarItems.Contains(_deleteToolbarItem))
			{
				ToolbarItems.Add(_deleteToolbarItem);
			}
		}
		else
		{
			if (ToolbarItems.Contains(_deleteToolbarItem))
			{
				ToolbarItems.Remove(_deleteToolbarItem);
			}
		}
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_viewModel.PropertyChanged -= OnViewModelPropertyChanged;
	}
}