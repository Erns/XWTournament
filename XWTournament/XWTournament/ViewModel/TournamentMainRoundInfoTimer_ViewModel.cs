using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace XWTournament.ViewModel
{
    public class TournamentMainRoundInfoTimer_ViewModel : INotifyPropertyChanged
    {
        //Helps facilitate the GUI updating as needed
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _timerValue = 0;

        private Button _timerButton;
        public Button TimerButton
        {
            get { return _timerButton; }
            set
            {
                _timerButton = value;
                OnPropertyChanged();
            }
        }

        public TournamentMainRoundInfoTimer_ViewModel()
        {
            TimerButton = new Button();
        }

        public string TimerValue
        {
            get
            {
                TimeSpan time = TimeSpan.FromSeconds(_timerValue);
                return time.ToString(@"hh\:mm\:ss\:fff");
            }
            set
            {
                _timerValue = Convert.ToInt32(value);
                OnPropertyChanged();
            }
        }

    }
}
