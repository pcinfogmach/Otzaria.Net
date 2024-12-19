using System.Windows.Controls;
using System.Windows.Input;
using MyHelpers;
using MyModels;

namespace MyControls
{
    public  class TabStripListBox : ListBox
    {
        public ICommand GoForwardCommand => new RelayCommand(GoForward);
        public ICommand GoBackCommand => new RelayCommand(GoBack);

        void GoForward()
        {
            if (this.SelectedIndex + 1 >= this.Items.Count)
                this.SelectedIndex = 0;
            else
                this.SelectedIndex++;
        }

        void GoBack()
        {
            if (this.SelectedIndex <= 0)
                this.SelectedIndex = this.Items.Count - 1;
            else
                this.SelectedIndex--;
        }
        
        public TabStripListBox() 
        {
            this.SelectionChanged += (s,e) => ScrollIntoView(this.SelectedItem);
        }
    }
}
