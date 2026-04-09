using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace SkyFocus.Services;

internal sealed class NavigationService : INavigationService
{
    private readonly Stack<Type> _history = new();
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public event EventHandler<Page?>? CurrentPageChanged;

    public Page? CurrentPage { get; private set; }

    public Type? CurrentPageType => CurrentPage?.GetType();

    public bool CanGoBack => _history.Count > 0;

    public void GoBack()
    {
        if (_history.Count == 0)
        {
            return;
        }

        Type targetType = _history.Pop();
        CurrentPage = (Page)_serviceProvider.GetRequiredService(targetType);
        CurrentPageChanged?.Invoke(this, CurrentPage);
    }

    public void NavigateTo<TPage>() where TPage : Page
    {
        if (CurrentPageType == typeof(TPage))
        {
            return;
        }

        if (CurrentPageType is not null)
        {
            _history.Push(CurrentPageType);
        }

        CurrentPage = _serviceProvider.GetRequiredService<TPage>();
        CurrentPageChanged?.Invoke(this, CurrentPage);
    }
}