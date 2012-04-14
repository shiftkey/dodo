using System;
using System.Collections.ObjectModel;
using System.Linq;
using BoxKite;
using BoxKite.Models;
using BoxKite.Modules;
using Dodo.Logic.Shared;
using Windows.UI.Core;
using BindableBase = Dodo.Logic.Shared.BindableBase;

namespace Dodo.Modules.Dashboard
{
    public class DashboardViewModel : BindableBase
    {
        private readonly ITwitterService _twitter;
        private IUserSession _session;
        private readonly CoreDispatcher _dispatcher;

        public DashboardViewModel(ITwitterService twitter, CoreDispatcher dispatcher)
        {
            _twitter = twitter;
            _dispatcher = dispatcher;
        }

        private readonly ObservableCollection<UserTask> _tasks = new ObservableCollection<UserTask>();
        public ObservableCollection<UserTask> Tasks
        {
            get { return _tasks; }
        }

        private readonly ObservableCollection<TweetGroup<Tweet>> _tweets = new ObservableCollection<TweetGroup<Tweet>>();
        public ObservableCollection<TweetGroup<Tweet>> Tweets
        {
            get { return _tweets; }
        }

        public void Start()
        {
            _tasks.Add(new UserTask { Title = "Sign In", Command = new DelegateCommand(StartOAuthFlow) });

            _twitter.GetSession()
                    .SearchFor("twitter", pages: 5)
                    .Subscribe(OnNext);
        }

        private async void StartOAuthFlow()
        {
            // these creds are from a sample app that @shiftkey owns - you shouldn't trust them :)
            const string client = "pdFvR4kdbQgugKVIQ205Cw";
            const string secret = "J9xJaIBBfH2by22bRATVC6HfVf1WGermItci7Cx0Yw";

            _credentials = await TwitterAuthenticator.AuthenticateUser(client, "http://code52.org/twitter", secret);

            if (!_credentials.Valid)
                return;

            // TODO: save credentials to store

            _session = _twitter.GetUserSession(_credentials);

            _dispatcher.InvokeAsync(CoreDispatcherPriority.Low, SetupApplication, this, null);
        }

        private TwitterCredentials _credentials;

        private void SetupApplication(object sender, InvokedHandlerArgs e)
        {
            Tweets.Clear();

            Tasks.Clear();
            Tasks.Add(new UserTask { Title = "Timeline", Command = new DelegateCommand(GetTimeline) });
            Tasks.Add(new UserTask { Title = "Mentions", Command = new DelegateCommand(GetMentions) });
            Tasks.Add(new UserTask { Title = "Retweets", Command = new DelegateCommand(GetRetweets) });
            Tasks.Add(new UserTask { Title = "DMs", Command = new DelegateCommand(GetDirectMessages) });
            Tasks.Add(new UserTask { Title = "New Followers" });
            Tasks.Add(new UserTask { Title = "Lost Followers" });

            GetTimeline();
        }

        private void GetTimeline()
        {
            Tweets.Clear();
            _session.GetHomeTimeline()
                    .Subscribe(OnNext);
        }

        private void GetRetweets()
        {
            Tweets.Clear();
            _session.GetRetweets()
                    .Subscribe(OnNext);
        }

        private void GetMentions()
        {
            Tweets.Clear();
            _session.GetMentions()
                    .Subscribe(OnNext);
        }

        private void GetDirectMessages()
        {
            Tweets.Clear();

            _session.GetDirectMessages()
                .Subscribe(OnNext);

            _session.GetSentDirectMessages()
                .Subscribe(OnNext);
        }

        private void OnNext(Tweet tweet)
        {
            _dispatcher.InvokeAsync(CoreDispatcherPriority.Low, AddTweet, this, tweet);
        }

        private void AddTweet(object sender, InvokedHandlerArgs e)
        {
            var tweet = e.Context as Tweet;
            if (tweet == null) return;

            var time = tweet.Timestamp;
            var existing = Tweets.FirstOrDefault(c => c.RangeStart < time && c.RangeEnd >= time);

            if (existing != null)
            {
                existing.Add(tweet);
                return;
            }

            var timeSince = DateTime.UtcNow - time;
            if (timeSince.Hours == 0 && timeSince.Minutes < 10)
            {
                var group = new TweetGroup<Tweet> { Key = "Just Now", RangeStart = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromMinutes(10)), RangeEnd = DateTimeOffset.UtcNow };
                group.Add(tweet);
                Tweets.Add(group);
                return;
            }

            if (timeSince.Hours == 0)
            {
                var lastHour = new TweetGroup<Tweet> { Key = "Last Hour", RangeStart = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromHours(1)), RangeEnd = DateTimeOffset.UtcNow };
                lastHour.Add(tweet);
                Tweets.Add(lastHour);
            }

            if (timeSince.Hours < 24)
            {
                var lastHour = new TweetGroup<Tweet> { Key = "Today", RangeStart = DateTimeOffset.UtcNow.Subtract(TimeSpan.FromHours(24)), RangeEnd = DateTimeOffset.UtcNow };
                lastHour.Add(tweet);
                Tweets.Add(lastHour);
            }
        }

        private DateTime ToLocal(DateTime time)
        {
            return time.Subtract(TimeSpan.FromHours(10));
        }
    }
}