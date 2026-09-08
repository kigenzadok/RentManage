using RentManage.Models;
using RentManage.Services;

namespace RentManage;

public partial class ProfilePage : ContentPage
{
    private readonly LocalDbService _dbService;
    private UserProfile _currentProfile = new();

    public ProfilePage(LocalDbService dbService)
    {
        InitializeComponent();
        _dbService = dbService;

        RolePicker.ItemsSource = Enum.GetNames(typeof(UserRole));
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        var existingProfile = await _dbService.GetUserProfileAsync();
        if (existingProfile != null && !string.IsNullOrWhiteSpace(existingProfile.FullName))
        {
            _currentProfile = existingProfile;
            PopulateProfileData();

            // Show Readonly view, hide Edit form
            ReadonlyProfileCard.IsVisible = true;
            EditProfileForm.IsVisible = false;
        }
        else
        {
            // First time setup - show Edit form directly
            RolePicker.SelectedItem = UserRole.Owner.ToString();
            ReadonlyProfileCard.IsVisible = false;
            EditProfileForm.IsVisible = true;
            CancelButton.IsVisible = false;
            Grid.SetColumnSpan(SaveButton, 2);
        }
    }

    private void PopulateProfileData()
    {
        // Header
        HeaderNameLabel.Text = _currentProfile.FullName;
        HeaderRoleLabel.Text = $"Role: {_currentProfile.Role}";

        // Read-only labels
        DisplayFullNameLabel.Text = _currentProfile.FullName;
        DisplayEmailLabel.Text = string.IsNullOrWhiteSpace(_currentProfile.Email) ? "N/A" : _currentProfile.Email;
        DisplayPhoneLabel.Text = string.IsNullOrWhiteSpace(_currentProfile.PhoneNumber) ? "N/A" : _currentProfile.PhoneNumber;
        DisplayRoleLabel.Text = _currentProfile.Role.ToString();

        // Form fields
        FullNameEntry.Text = _currentProfile.FullName;
        EmailEntry.Text = _currentProfile.Email;
        PhoneEntry.Text = _currentProfile.PhoneNumber;
        RolePicker.SelectedItem = _currentProfile.Role.ToString();
    }

    private async void OnSaveProfileClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FullNameEntry.Text))
        {
            await DisplayAlert("Validation Error", "Please enter a full name.", "OK");
            return;
        }

        _currentProfile.FullName = FullNameEntry.Text.Trim();
        _currentProfile.Email = EmailEntry.Text?.Trim() ?? string.Empty;
        _currentProfile.PhoneNumber = PhoneEntry.Text?.Trim() ?? string.Empty;

        if (RolePicker.SelectedItem != null && Enum.TryParse<UserRole>(RolePicker.SelectedItem.ToString(), out var selectedRole))
        {
            _currentProfile.Role = selectedRole;
        }

        await _dbService.SaveUserProfileAsync(_currentProfile);

        PopulateProfileData();

        // Close edit form and display details card
        EditProfileForm.IsVisible = false;
        ReadonlyProfileCard.IsVisible = true;
    }

    private void OnEditProfileClicked(object sender, EventArgs e)
    {
        CancelButton.IsVisible = true;
        Grid.SetColumnSpan(SaveButton, 1);
        ReadonlyProfileCard.IsVisible = false;
        EditProfileForm.IsVisible = true;
    }

    private void OnCancelEditClicked(object sender, EventArgs e)
    {
        ReadonlyProfileCard.IsVisible = true;
        EditProfileForm.IsVisible = false;
    }
}