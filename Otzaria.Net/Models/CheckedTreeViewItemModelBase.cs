using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MyModels
{
    public class CheckedTreeViewItemModelBase : ViewModelBase
    {
        protected string _name;
        private bool? _isChecked = false;
        private ObservableCollection<CheckedTreeViewItemModelBase> _children;

        public CheckedTreeViewItemModelBase Parent;

        public virtual string Name { get => _name; set => SetProperty(ref _name, value); }
        public bool? IsChecked  {  get => _isChecked; set => SetCheckedValue(value, true); }
        public ObservableCollection<CheckedTreeViewItemModelBase> Children{ get => _children;  set => SetChildren(value); }

        public override string ToString() => Name;

        void SetChildren(ObservableCollection<CheckedTreeViewItemModelBase> children)
        {
            if (SetProperty(ref _children, children))
            {
                foreach (var child in _children)
                    if (child.Parent == null)
                        child.Parent = this;
            }
        }

        public void AddChild(CheckedTreeViewItemModelBase chiild)
        {
            if (Children == null) Children = new ObservableCollection<CheckedTreeViewItemModelBase> { chiild};
            else Children.Add(chiild);
        }

        public void SetCheckedValue(bool? isChecked, bool updateChildren)
        {
            if (SetProperty(ref _isChecked, isChecked, nameof(IsChecked)))
            {
                try
                {
                    if (updateChildren && Children != null)
                    {
                        foreach (var child in Children)
                        {
                            if (child.IsChecked != isChecked)
                                child.IsChecked = (isChecked == true);
                        }
                    }

                    if (Parent != null)
                    {
                        var parentCheckedValue = Parent.Children.All(c => c.IsChecked == true) ? true :
                            Parent.Children.All(c => c.IsChecked == false) ? (bool?)false : null;
                        Parent.SetCheckedValue(parentCheckedValue, false);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.ToString());
                }
            }  
        }

        public IEnumerable<CheckedTreeViewItemModelBase> GetAllCheckedChildren()
        {
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    if (child.IsChecked == true)
                        yield return child;

                    foreach (var item in child.GetAllCheckedChildren())
                        yield return item;
                }
            }
        }
    }
}
