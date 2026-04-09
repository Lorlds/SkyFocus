using System;
using Microsoft.UI.Xaml.Controls;

namespace SkyFocus.Services;

internal interface INavigationService
{
    event EventHandler<Page?>? CurrentPageChanged;

    Page? CurrentPage { get; }

    Type? CurrentPageType { get; }

    bool CanGoBack { get; }

    void NavigateTo<TPage>() where TPage : Page;

    void GoBack();
}