using System.ComponentModel;
using Notes.Maui.ViewModels.Features.Notes;

namespace Notes.Maui.Views.Features.Notes;

public partial class NotePage : ContentPage
{
	private readonly NoteViewModel _viewModel;
	private readonly ToolbarItem _deleteToolbarItem;
	private bool _suppressToggleAnimations;
    private double _dragStartY;
    private double _dragStartTranslationY;
    private const double DragDismissThreshold = 120;
    private double _halfScreenHeight;

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

		if (e.PropertyName == nameof(NoteViewModel.IsBackgroundColorBottomSheetVisible))
		{
			_ = ToggleBackgroundSheetAsync(_viewModel.IsBackgroundColorBottomSheetVisible);
		}

		if (e.PropertyName == nameof(NoteViewModel.IsTagsBottomSheetVisible))
		{
			_ = ToggleTagsSheetAsync(_viewModel.IsTagsBottomSheetVisible);
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

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		
		// Load tags when the page appears
		if (_viewModel.Note != null && !_viewModel.IsTagsLoaded)
		{
			await _viewModel.LoadTagsCommand.ExecuteAsync(null);
		}

		// Wire scrim tap to close any open sheet
		var tap = new TapGestureRecognizer();
		tap.Tapped += (_, __) =>
		{
			_viewModel.HideBackgroundColorBottomSheetCommand.Execute(null);
			_viewModel.HideTagsBottomSheetCommand.Execute(null);
		};
		OverlayScrim.GestureRecognizers.Clear();
		OverlayScrim.GestureRecognizers.Add(tap);

        // Cache half screen height for tags sheet max height
        _halfScreenHeight = this.Height > 0 ? this.Height / 2.0 : DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density / 2.0;
        TagsScroll.MaximumHeightRequest = _halfScreenHeight - 40; // account for padding/title
	}

    private void OnBackgroundSheetPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        HandleSheetPanUpdated(BackgroundSheet, e, isTagsSheet: false);
    }

    private void OnTagsSheetPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        HandleSheetPanUpdated(TagsSheet, e, isTagsSheet: true);
    }

    private async void HandleSheetPanUpdated(VisualElement sheet, PanUpdatedEventArgs e, bool isTagsSheet)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _dragStartY = e.TotalY;
                _dragStartTranslationY = sheet.TranslationY;
                break;

            case GestureStatus.Running:
                var delta = Math.Max(0, _dragStartTranslationY + e.TotalY - _dragStartY);
                sheet.TranslationY = delta;
                var scrimTarget = Math.Clamp(1.0 - (delta / 400.0), 0.0, 1.0);
                OverlayScrim.Opacity = scrimTarget;
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                if (sheet.TranslationY > DragDismissThreshold)
                {
                    if (isTagsSheet)
                    {
                        _viewModel.HideTagsBottomSheetCommand.Execute(null);
                    }
                    else
                    {
                        _viewModel.HideBackgroundColorBottomSheetCommand.Execute(null);
                    }
                }
                else
                {
                    await Task.WhenAll(
                        OverlayScrim.FadeTo(1.0, 120, Easing.CubicOut),
                        sheet.TranslateTo(0, 0, 150, Easing.CubicOut)
                    );
                }
                break;
        }
    }

	private async Task ToggleBackgroundSheetAsync(bool show)
	{
		if (show)
		{
			// Ensure tags sheet is closed without conflicting animations
			if (TagsSheet.IsVisible || _viewModel.IsTagsBottomSheetVisible)
			{
				_suppressToggleAnimations = true;
				_viewModel.IsTagsBottomSheetVisible = false;
				TagsSheet.IsVisible = false;
				TagsSheet.TranslationY = 500;
				_suppressToggleAnimations = false;
			}

			BottomSheetOverlay.IsVisible = true;
			BackgroundSheet.IsVisible = true;
			BackgroundSheet.TranslationY = 500;
			await Task.WhenAll(
				OverlayScrim.FadeTo(1.0, 150, Easing.CubicOut),
				BackgroundSheet.TranslateTo(0, 0, 200, Easing.CubicOut)
			);
		}
		else
		{
			if (_suppressToggleAnimations)
			{
				BackgroundSheet.IsVisible = false;
				BackgroundSheet.TranslationY = 500;
			}
			else
			{
				await Task.WhenAll(
					OverlayScrim.FadeTo(TagsSheet.IsVisible ? 1.0 : 0.0, 150, Easing.CubicIn),
					BackgroundSheet.TranslateTo(0, 500, 200, Easing.CubicIn)
				);
				BackgroundSheet.IsVisible = false;
			}

			BottomSheetOverlay.IsVisible = TagsSheet.IsVisible;
		}
	}

	private async Task ToggleTagsSheetAsync(bool show)
	{
		if (show)
		{
			// Ensure background sheet is closed without conflicting animations
			if (BackgroundSheet.IsVisible || _viewModel.IsBackgroundColorBottomSheetVisible)
			{
				_suppressToggleAnimations = true;
				_viewModel.IsBackgroundColorBottomSheetVisible = false;
				BackgroundSheet.IsVisible = false;
				BackgroundSheet.TranslationY = 500;
				_suppressToggleAnimations = false;
			}

			BottomSheetOverlay.IsVisible = true;
			TagsSheet.IsVisible = true;
			TagsSheet.TranslationY = 500;
            // Let content wrap but cap height at half screen
            TagsScroll.MaximumHeightRequest = _halfScreenHeight - 40;
			await Task.WhenAll(
				OverlayScrim.FadeTo(1.0, 150, Easing.CubicOut),
				TagsSheet.TranslateTo(0, 0, 200, Easing.CubicOut)
			);
		}
		else
		{
			if (_suppressToggleAnimations)
			{
				TagsSheet.IsVisible = false;
				TagsSheet.TranslationY = 500;
			}
			else
			{
				await Task.WhenAll(
					OverlayScrim.FadeTo(BackgroundSheet.IsVisible ? 1.0 : 0.0, 150, Easing.CubicIn),
					TagsSheet.TranslateTo(0, 500, 200, Easing.CubicIn)
				);
				TagsSheet.IsVisible = false;
			}

			BottomSheetOverlay.IsVisible = BackgroundSheet.IsVisible;
		}
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_viewModel.PropertyChanged -= OnViewModelPropertyChanged;
	}

	protected override bool OnBackButtonPressed()
	{
		// Close any open bottom sheet instead of navigating back
		if (BackgroundSheet.IsVisible || TagsSheet.IsVisible)
		{
			_viewModel.HideBackgroundColorBottomSheetCommand.Execute(null);
			_viewModel.HideTagsBottomSheetCommand.Execute(null);
			return true;
		}

		return base.OnBackButtonPressed();
	}
}