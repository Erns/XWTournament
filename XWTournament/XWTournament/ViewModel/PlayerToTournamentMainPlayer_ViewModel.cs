using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;
using XWTournament.Models;

namespace XWTournament.ViewModel
{
    class PlayerToTournamentMainPlayer_ViewModel : INotifyPropertyChanged
    {
        //Helps facilitate the GUI updating as needed
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _recursiveLvl = 0;

        //Copy of the player
        private TournamentMainPlayer _tournmentPlayer;
        public TournamentMainPlayer TournamentMainPlayer
        {
            get { return _tournmentPlayer; }
            set
            {
                _tournmentPlayer = value;
                OnPropertyChanged();
            }
        }

        //Set the copy of the player
        public PlayerToTournamentMainPlayer_ViewModel(Player player, int intTournamentId, int intRow, TournamentMainPlayer tournamentPlayer = null)
        {
            TournamentMainPlayer = new TournamentMainPlayer();

            if (tournamentPlayer is null)
            {
                TournamentMainPlayer.TournmentId = intTournamentId;
                TournamentMainPlayer.PlayerId = player.Id;
                TournamentMainPlayer.PlayerName = player.Name;
                TournamentMainPlayer.Active = player.Active;
            }
            else
            {
                TournamentMainPlayer = tournamentPlayer;
            }

            if (intRow % 2 == 0) RowBackgroundColor = Color.LightGray;
            else RowBackgroundColor = Color.Transparent;
        }

        public Color RowBackgroundColor { get; set; }

        //Indicate Player active for next round
        public bool PlayerActive_VM
        {
            get { return TournamentMainPlayer.Active; }
            set
            {
                if (TournamentMainPlayer.Active != value)
                {
                    _recursiveLvl++;
                    TournamentMainPlayer.Active = value;
                    if (!TournamentMainPlayer.Active)
                    {
                        PlayerBye_VM = false;
                    }
                    OnPropertyChanged();
                    UpdateTournamentPlayers();
                    _recursiveLvl--;
                }
            }
        }

        //Indicate Player bye for next round
        public bool PlayerBye_VM
        {
            get { return TournamentMainPlayer.Bye; }
            set
            {
                if (TournamentMainPlayer.Bye != value)
                {
                    _recursiveLvl++;
                    TournamentMainPlayer.Bye = value;
                    OnPropertyChanged();
                    UpdateTournamentPlayers();
                    _recursiveLvl--;
                }
            }
        }

        //Update the tournament's player roster active/bye
        private void UpdateTournamentPlayers()
        {
            if (_recursiveLvl > 1) return;

            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
            {

                if (TournamentMainPlayer.Id > 0)
                {
                    conn.Update(TournamentMainPlayer);
                }
                else if (TournamentMainPlayer.Active)
                {
                    conn.Insert(TournamentMainPlayer);
                }
            }
        }

    }
}
