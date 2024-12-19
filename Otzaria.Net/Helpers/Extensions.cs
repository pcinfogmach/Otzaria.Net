using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MyHelpers
{
    public static class Extensions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this List<T> list)  => new ObservableCollection<T>(list);
    }
}
