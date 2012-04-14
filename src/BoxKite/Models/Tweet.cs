using System;
using BoxKite.Extensions;

namespace BoxKite.Models
{
    public class Tweet : BindableBase
    {
        private string _text;
        public string Text
        {
            get { return _text; }
            set { SetProperty(ref _text, value); }
        }

        private string _avatar;
        public string Avatar
        {
            get { return _avatar; }
            set { SetProperty(ref _avatar, value); }
        }

        private string _author;
        public string Author
        {
            get { return _author; }
            set { SetProperty(ref _author, value); }
        }

        private DateTimeOffset _timestamp;
        public DateTimeOffset Timestamp
        {
            get { return _timestamp; }
            set { SetProperty(ref _timestamp, value); }
        }

        public string FriendlyTime
        {
            get { return Timestamp.DateTime.ToFriendlyText(); }
        }
    }
}
