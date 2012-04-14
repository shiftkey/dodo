using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dodo.Logic.Shared
{
    public class TweetGroup<T> : ObservableCollection<T>
    {
        public object Key { get; set; }

        public DateTimeOffset? RangeStart { get; set; }
        public DateTimeOffset? RangeEnd { get; set; }

        public new IEnumerator<object> GetEnumerator()
        {
            return (IEnumerator<object>)base.GetEnumerator();
        }
    }
}