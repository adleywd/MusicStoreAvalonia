using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using MusicStore.Models;
using ReactiveUI;

namespace MusicStore.ViewModels;

public class MusicStoreViewModel : ViewModelBase
{
    
    public string? SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }
    
    public ObservableCollection<AlbumViewModel> SearchResults { get; } = new();

    public ReactiveCommand<Unit, AlbumViewModel?> BuyMusicCommand { get; }

    public AlbumViewModel? SelectedAlbum
    {
        get => _selectedAlbum;
        set => this.RaiseAndSetIfChanged(ref _selectedAlbum, value);
    }

    public MusicStoreViewModel()
    {
        BuyMusicCommand = ReactiveCommand.Create(() => SelectedAlbum);
        
        this.WhenAnyValue(x => x.SearchText)
            .Throttle(TimeSpan.FromMilliseconds(400))
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(DoSearch!);
    }
    
    private string? _searchText;
    private bool _isBusy;
    private AlbumViewModel? _selectedAlbum;
    private CancellationTokenSource? _cancellationTokenSource;
    
    private async void DoSearch(string musicToSearch)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;
        
        IsBusy = true;
        SearchResults.Clear();

        if (!string.IsNullOrWhiteSpace(musicToSearch))
        {
            var albums = await Album.SearchAsync(musicToSearch);

            foreach (var album in albums)
            {
                var vm = new AlbumViewModel(album);
                SearchResults.Add(vm);
            }
            
            if (!cancellationToken.IsCancellationRequested)
            {
                LoadCovers(cancellationToken);
            }
        }

        IsBusy = false;
    }
    
    private async void LoadCovers(CancellationToken cancellationToken)
    {
        foreach (var album in SearchResults.ToList())
        {
            await album.LoadCover();

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }
}