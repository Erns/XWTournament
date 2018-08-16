using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;
using XWTournament.Models;

namespace XWTournament.ViewModel
{
    class TournamentMainRoundTable_ViewModel : INotifyPropertyChanged
    {

        private bool _enabled;

        //Helps facilitate the GUI updating as needed
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //Copy of the table
        private TournamentMainRoundTable _roundTable;
        public TournamentMainRoundTable TournamentMainRoundTable
        {
            get { return _roundTable; }
            set
            {
                _roundTable = value;
                OnPropertyChanged();
            }
        }

        //Set the copy of the table
        public TournamentMainRoundTable_ViewModel(TournamentMainRoundTable table, bool enabled = true)
        {
            _enabled = enabled;
            TournamentMainRoundTable = new TournamentMainRoundTable();
            TournamentMainRoundTable = table;
            UpdatePlayerVisual();
        }


        private int _recursiveLvl = 0;  //Tracking how nested the recursion gets, trigger save on the last level to prevent unnecessary/excessive updates

        //Score tied status
        public bool ScoreTied_VM
        {
            get { return TournamentMainRoundTable.ScoreTied; }
            set
            {
                if (TournamentMainRoundTable.ScoreTied != value)
                {
                    TournamentMainRoundTable.ScoreTied = value;
                    OnPropertyChanged();
                }
            }
        }

        //Player 1/2 Winner status
        public bool Player1Winner_VM
        {
            get { return TournamentMainRoundTable.Player1Winner; }
            set
            {
                if (TournamentMainRoundTable.Player1Winner != value)
                {
                    _recursiveLvl++;
                    TournamentMainRoundTable.Player1Winner = value;
                    if (TournamentMainRoundTable.Player1Winner) Player2Winner_VM = false;
                    UpdatePlayerVisual();
                    OnPropertyChanged();
                    UpdateRoundTable();
                    _recursiveLvl--;
                }
            }
        }

        public bool Player2Winner_VM
        {
            get { return TournamentMainRoundTable.Player2Winner; }
            set
            {
                if (TournamentMainRoundTable.Player2Winner != value)
                {
                    _recursiveLvl++;
                    TournamentMainRoundTable.Player2Winner = value;
                    if (TournamentMainRoundTable.Player2Winner) Player1Winner_VM = false;
                    UpdatePlayerVisual();
                    OnPropertyChanged();
                    UpdateRoundTable();
                    _recursiveLvl--;
                }
            }
        }

        //Player 1/2 Scores
        public int Player1Score_VM
        {
            get { return TournamentMainRoundTable.Player1Score; }
            set
            {
                if (TournamentMainRoundTable.Player1Score != value)
                {
                    _recursiveLvl++;
                    TournamentMainRoundTable.Player1Score = value;
                    OnPropertyChanged();
                    UpdateScores();
                    UpdateRoundTable();
                    _recursiveLvl--;
                }
            }
        }

        public int Player2Score_VM
        {
            get { return TournamentMainRoundTable.Player2Score; }
            set
            {
                if (TournamentMainRoundTable.Player2Score != value)
                {
                    _recursiveLvl++;
                    TournamentMainRoundTable.Player2Score = value;
                    OnPropertyChanged();
                    UpdateScores();
                    UpdateRoundTable();
                    _recursiveLvl--;
                }
            }
        }

        //Player 1/2 winner bolding
        private FontAttributes _player1NamedWinnerFont = FontAttributes.None;
        public FontAttributes Player1NameWinnerFont_VM
        {
            get { return _player1NamedWinnerFont; }
            set
            {
                if (_player1NamedWinnerFont != value)
                {
                    _player1NamedWinnerFont = value;
                    OnPropertyChanged();
                }
            }
        }

        private FontAttributes _player2NamedWinnerFont = FontAttributes.None;
        public FontAttributes Player2NameWinnerFont_VM
        {
            get { return _player2NamedWinnerFont; }
            set
            {
                if (_player2NamedWinnerFont != value)
                {
                    _player2NamedWinnerFont = value;
                    OnPropertyChanged();
                }
            }
        }


        //Indicate if table should be active for edit, per Bye
        public bool TableRowEnabled_VM
        {
            get
            {
                if (!_enabled)
                    return false;
                else
                    return !TournamentMainRoundTable.Bye;
            }
        }

        //Update the visuals for winner/loser
        private void UpdatePlayerVisual()
        {
            if (TournamentMainRoundTable.Player1Winner)
            {
                Player1NameWinnerFont_VM = FontAttributes.Bold;
                Player2NameWinnerFont_VM = FontAttributes.None;
            }
            else if (TournamentMainRoundTable.Player2Winner)
            {
                Player1NameWinnerFont_VM = FontAttributes.None;
                Player2NameWinnerFont_VM = FontAttributes.Bold;
            }
        }

        //Update Scores - update via the ViewModel variants so the display is correct and the subsequent data is saved as well
        private void UpdateScores()
        {
            ScoreTied_VM = false;
            if (Player1Score_VM > Player2Score_VM)
            {
                Player1Winner_VM = true;
            }
            else if (Player2Score_VM > Player1Score_VM)
            {
                Player2Winner_VM = true;
            }
            else
            {
                ScoreTied_VM = true;
            }
        }


        //Update the SQL table after all the adjustments are made
        public void UpdateRoundTable()
        {
            if (TournamentMainRoundTable.Id == 0 || _recursiveLvl > 1) return;

            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
            {
                TournamentMainRoundTable roundTable = conn.Get<TournamentMainRoundTable>(TournamentMainRoundTable.Id);

                roundTable.Player1Score = TournamentMainRoundTable.Player1Score;
                roundTable.Player1Winner = TournamentMainRoundTable.Player1Winner;
                roundTable.Player2Score = TournamentMainRoundTable.Player2Score;
                roundTable.Player2Winner = TournamentMainRoundTable.Player2Winner;

                conn.Update(roundTable);
            }
        }


        //Example code if we went the route of binding a command (such as via a button)
        //public Command SaveCommand
        //{
        //    get
        //    {
        //        return new Command(() => {
        //            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
        //            {
        //                Utilities.InitializeTournamentMain(conn);

        //                TournamentMainRoundTable roundTable = conn.Get<TournamentMainRoundTable>(TournamentMainRoundTable.Id);

        //                roundTable.Player1Score = TournamentMainRoundTable.Player1Score;
        //                roundTable.Player1Winner = TournamentMainRoundTable.Player1Winner;
        //                roundTable.Player2Score = TournamentMainRoundTable.Player2Score;
        //                roundTable.Player2Winner = TournamentMainRoundTable.Player2Winner;

        //                conn.Update(roundTable);
        //            }
        //        });
        //    }
        //}

    }
}
