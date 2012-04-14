﻿using System;
using System.Collections.Specialized;
using BoxKite;
using Dodo.Logic.Shared;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Dodo.Modules.Dashboard
{
    public sealed partial class DashboardView
    {
        DashboardViewModel _viewModel;
        Storyboard _storyboard;
        bool _isLoading = true;
        private CoreDispatcher _dispatcher;

        public DashboardView()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _dispatcher = Window.Current.Dispatcher;
            _storyboard = Resources["RotatingSquare"] as Storyboard;
            _storyboard.Begin();

            // TODO: wireup IoC bits and do this properly
            _viewModel = new DashboardViewModel(new TwitterService(), _dispatcher);
            _viewModel.Tweets.CollectionChanged += Tweets_CollectionChanged;
            _viewModel.Start();

            DataContext = _viewModel;
        }

        void Tweets_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                ExecuteInBackground(() =>
                {
                    _isLoading = true;
                    rectangle.Visibility = Visibility.Visible;
                });
            }

            if (_isLoading && e.Action == NotifyCollectionChangedAction.Add)
            {
                ExecuteInBackground(() =>
                {
                    _isLoading = false;
                    rectangle.Visibility = Visibility.Collapsed;
                });
            }
        }

        private void ExecuteInBackground(Action action)
        {
            _dispatcher.InvokeAsync(CoreDispatcherPriority.Low, (s, e) => action(), this, null);
        }

        private void CommandClicked(object sender, ItemClickEventArgs e)
        {
            // TODO: get a proper framework to handle this behaviour
            var task = e.ClickedItem as UserTask;
            if (task == null) return;

            var command = task.Command;
            if (command == null) return;

            if (task.Command.CanExecute(null))
                task.Command.Execute(null);
        }
    }
}
