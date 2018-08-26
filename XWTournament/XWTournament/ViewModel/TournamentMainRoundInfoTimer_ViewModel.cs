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
        private static double dblLargeBtnFnt = Device.GetNamedSize(NamedSize.Large, typeof(Button));
        private static double dblSmallBtnFnt = Device.GetNamedSize(NamedSize.Small, typeof(Button));

        //Helps facilitate the GUI updating as needed
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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

        private int _timerValue = 0;
        public string TimerValue
        {
            get
            {
                TimeSpan time = TimeSpan.FromSeconds(_timerValue);
                string strReturn = time.ToString(@"mm\:ss");

                if (_timerValue == 0)
                {
                    strReturn = "\uf017";
                    TimerFontSize = dblLargeBtnFnt;
                }
                else
                {
                    TimerFontSize = dblSmallBtnFnt;
                }

                return strReturn;
            }
            set
            {
                _timerValue = Convert.ToInt32(value);
                OnPropertyChanged();
            }
        }

        private double _timerFontSize = Device.GetNamedSize(NamedSize.Large, typeof(Button));
        public double TimerFontSize
        {
            get { return _timerFontSize; }
            set
            {
                if (_timerFontSize != value)
                {
                    _timerFontSize = value;
                    OnPropertyChanged();
                }
            }
        }

    }
}
