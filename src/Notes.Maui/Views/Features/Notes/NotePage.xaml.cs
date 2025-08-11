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
				Color = Application.Current?.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black,
				Size = 20,
			}
		};
		
		// Subscribe to theme changes to update delete button color
		Application.Current!.RequestedThemeChanged += OnThemeChanged;
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

	private void OnThemeChanged(object? sender, AppThemeChangedEventArgs e)
	{
		// Update delete button color when theme changes
		if (_deleteToolbarItem.IconImageSource is FontImageSource fontImageSource)
		{
			fontImageSource.Color = e.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;
		}
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_viewModel.PropertyChanged -= OnViewModelPropertyChanged;
		Application.Current!.RequestedThemeChanged -= OnThemeChanged;
	}
}