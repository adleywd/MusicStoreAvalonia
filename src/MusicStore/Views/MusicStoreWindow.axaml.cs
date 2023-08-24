using System;
using Avalonia.ReactiveUI;
using MusicStore.ViewModels;
using ReactiveUI;

namespace MusicStore.Views;

public partial class MusicStoreWindow : ReactiveWindow<MusicStoreViewModel>
{
    public MusicStoreWindow()
    {
        InitializeComponent();
        this.WhenActivated(action => action(ViewModel!.BuyMusicCommand.Subscribe(Close)));
    }
}