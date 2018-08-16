using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XWTournament.Classes;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XWTournament.Models;

namespace XWTournament.Pages.Tournaments
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Tournaments_RoundInfoTableEdit : ContentPage
	{
        private List<clsPlayerInfo> lstPlayers;
        private TournamentMainRound currentRound;
        private TournamentMainRoundTable currentTable;

        private int intPlayer1Id = 0;
        private int intPlayer2Id = 0;

        public Tournaments_RoundInfoTableEdit (int intRoundId, int intTableId)
		{
			InitializeComponent ();

            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
            {
                currentRound = new TournamentMainRound();
                currentRound = conn.GetWithChildren<TournamentMainRound>(intRoundId);

                lstPlayers = new List<clsPlayerInfo>();

                foreach(TournamentMainRoundTable table in currentRound.Tables)
                {
                    if (table.Player1Id > 0)
                        lstPlayers.Add(new clsPlayerInfo(table.Player1Id, table.Player1Name, table.Id));

                    if (table.Player2Id > 0)
                        lstPlayers.Add(new clsPlayerInfo(table.Player2Id, table.Player2Name, table.Id));

                    if (table.Id == intTableId)
                    {
                        currentTable = table;

                        Title = table.TableName;
                        lblPlayer1.Text = table.Player1Name;
                        lblPlayer2.Text = table.Player2Name;

                        intPlayer1Id = table.Player1Id;
                        intPlayer2Id = table.Player2Id;                       
                    }
                        
                }

                pckPlayer1.ItemsSource = lstPlayers.OrderBy(obj => obj.PlayerName).Where(o => o.PlayerId != intPlayer2Id).ToList();
                pckPlayer2.ItemsSource = lstPlayers.OrderBy(obj => obj.PlayerName).Where(o => o.PlayerId != intPlayer1Id).ToList();

                //Set the first selected item as the current player
                pckPlayer1.SelectedIndexChanged -= pckPlayer1_SelectedIndexChanged;
                pckPlayer2.SelectedIndexChanged -= pckPlayer2_SelectedIndexChanged;

                for (int i = 1; i <= 2; i++)
                {
                    int intIndex = -1;
                    Picker pckPlayer = (i == 1 ? pckPlayer1 : pckPlayer2);

                    foreach (clsPlayerInfo item in pckPlayer.ItemsSource)
                    {
                        intIndex++;
                        if (item.PlayerId == (i == 1 ? intPlayer1Id : intPlayer2Id))
                        {
                            pckPlayer.SelectedIndex = intIndex;
                            break;
                        }
                    }
                }

                pckPlayer1.SelectedIndexChanged += pckPlayer1_SelectedIndexChanged;
                pckPlayer2.SelectedIndexChanged += pckPlayer2_SelectedIndexChanged;
            }
        }

        private class clsPlayerInfo
        {
            public int PlayerId { get; set; }
            public string PlayerName { get; set; }
            public int TableId { get; set; }

            public clsPlayerInfo(int Id, string Name, int TableId)
            {
                this.PlayerId = Id;
                this.PlayerName = Name;
                this.TableId = TableId;
            }

        }

        private void pckPlayer1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePlayerPickers(sender, 2);
        }

        private void pckPlayer2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdatePlayerPickers(sender, 1);
        }

        private void UpdatePlayerPickers(object sender, int playerNumber)
        {
            Picker tmpPicker = (Picker)sender;

            if (tmpPicker.SelectedIndex < 0) return;

            Picker pckPlayer;

            if (playerNumber == 1)
                pckPlayer = pckPlayer1;
            else
                pckPlayer = pckPlayer2;

            if (playerNumber == 1)
                pckPlayer.SelectedIndexChanged -= pckPlayer1_SelectedIndexChanged;
            else
                pckPlayer.SelectedIndexChanged -= pckPlayer2_SelectedIndexChanged;

            clsPlayerInfo otherPlayer = (clsPlayerInfo)pckPlayer.SelectedItem;
            clsPlayerInfo thisPlayer = (clsPlayerInfo)tmpPicker.SelectedItem;

            pckPlayer.ItemsSource = lstPlayers.OrderBy(obj => obj.PlayerName).AsQueryable().Where(o => o.PlayerId != thisPlayer.PlayerId).ToList();

            int intIndex = -1;
            foreach (clsPlayerInfo item in pckPlayer.ItemsSource)
            {
                intIndex++;
                if (item.PlayerId == otherPlayer.PlayerId)
                {
                    pckPlayer.SelectedIndex = intIndex;
                    break;
                }
            }

            if (playerNumber == 1)
                pckPlayer.SelectedIndexChanged += pckPlayer1_SelectedIndexChanged;
            else
                pckPlayer.SelectedIndexChanged += pckPlayer2_SelectedIndexChanged;
        }

        private void saveButton_Clicked(object sender, EventArgs e)
        {
            clsPlayerInfo player1 = (clsPlayerInfo)pckPlayer1.SelectedItem;
            clsPlayerInfo player2 = (clsPlayerInfo)pckPlayer2.SelectedItem;


            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
            {
                TournamentMainRoundTable tmpOtherTable;
                TournamentMainRoundTable tmpCurrentTable;
                clsPlayerInfo tmpPlayer;
                int tmpPlayerId;

                for (int i = 1; i <= 2; i++)
                {
                    if (i == 1)
                    {
                        tmpPlayer = player1;
                        tmpPlayerId = intPlayer1Id;
                    }

                    else
                    {
                        tmpPlayer = player2;
                        tmpPlayerId = intPlayer2Id;
                    }

                    if (tmpPlayer.PlayerId != tmpPlayerId)
                    {
                        tmpOtherTable = new TournamentMainRoundTable();
                        tmpCurrentTable = new TournamentMainRoundTable();

                        TournamentMainRoundTable otherTable = conn.Get<TournamentMainRoundTable>(tmpPlayer.TableId);

                        tmpOtherTable.Player1Id = otherTable.Player1Id;
                        tmpOtherTable.Player1Name = otherTable.Player1Name;
                        tmpOtherTable.Player2Id = otherTable.Player2Id;
                        tmpOtherTable.Player2Name = otherTable.Player2Name;

                        tmpCurrentTable.Player1Id = currentTable.Player1Id;
                        tmpCurrentTable.Player1Name = currentTable.Player1Name;
                        tmpCurrentTable.Player2Id = currentTable.Player2Id;
                        tmpCurrentTable.Player2Name = currentTable.Player2Name;

                        if (otherTable.Player1Id == tmpPlayer.PlayerId)
                        {
                            otherTable.Player1Id = tmpCurrentTable.Player1Id;
                            otherTable.Player1Name = tmpCurrentTable.Player1Name;
                            otherTable.TableName = string.Format("{0} vs {1}", otherTable.Player1Name, otherTable.Player2Name);

                            currentTable.Player1Id = tmpOtherTable.Player1Id;
                            currentTable.Player1Name = tmpOtherTable.Player1Name;
                            currentTable.TableName = string.Format("{0} vs {1}", currentTable.Player1Name, currentTable.Player2Name);
                        }
                        else if (otherTable.Player2Id == tmpPlayer.PlayerId)
                        {
                            otherTable.Player2Id = tmpCurrentTable.Player1Id;
                            otherTable.Player2Name = tmpCurrentTable.Player1Name;
                            otherTable.TableName = string.Format("{0} vs {1}", otherTable.Player1Name, otherTable.Player2Name);

                            currentTable.Player1Id = tmpOtherTable.Player2Id;
                            currentTable.Player1Name = tmpOtherTable.Player2Name;
                            currentTable.TableName = string.Format("{0} vs {1}", currentTable.Player1Name, currentTable.Player2Name);
                        }

                        conn.Update(otherTable);
                        conn.Update(currentTable);
                    }
                }

                //if (player1.PlayerId != intPlayer1Id)
                //{
                //    tmpTable = new TournamentMainRoundTable();

                //    TournamentMainRoundTable otherTable = conn.Get<TournamentMainRoundTable>(player1.TableId);

                //    tmpTable = otherTable;

                //    if (otherTable.Player1Id == player1.PlayerId)
                //    {
                //        otherTable.Player1Id = currentTable.Player1Id;
                //        otherTable.Player1Name = currentTable.Player1Name;
                //        otherTable.TableName = string.Format("{0} vs {1}", otherTable.Player1Name, otherTable.Player2Name);

                //        currentTable.Player1Id = tmpTable.Player1Id;
                //        currentTable.Player1Name = tmpTable.Player1Name;
                //        currentTable.TableName = string.Format("{0} vs {1}", currentTable.Player1Name, currentTable.Player2Name);
                //    }
                //    else if (otherTable.Player2Id == player1.PlayerId)
                //    {
                //        otherTable.Player2Id = currentTable.Player1Id;
                //        otherTable.Player2Name = currentTable.Player1Name;
                //        otherTable.TableName = string.Format("{0} vs {1}", otherTable.Player1Name, otherTable.Player2Name);

                //        currentTable.Player1Id = tmpTable.Player2Id;
                //        currentTable.Player1Name = tmpTable.Player2Name;
                //        currentTable.TableName = string.Format("{0} vs {1}", currentTable.Player1Name, currentTable.Player2Name);
                //    }
                //}

                //if (player2.PlayerId != intPlayer2Id)
                //{
                //    tmpTable = new TournamentMainRoundTable();

                //    TournamentMainRoundTable otherTable = conn.Get<TournamentMainRoundTable>(player2.TableId);

                //    tmpTable = otherTable;

                //    if (otherTable.Player1Id == player2.PlayerId)
                //    {
                //        otherTable.Player1Id = currentTable.Player1Id;
                //        otherTable.Player1Name = currentTable.Player1Name;
                //        otherTable.TableName = string.Format("{0} vs {1}", otherTable.Player1Name, otherTable.Player2Name);

                //        currentTable.Player1Id = tmpTable.Player1Id;
                //        currentTable.Player1Name = tmpTable.Player1Name;
                //        currentTable.TableName = string.Format("{0} vs {1}", currentTable.Player1Name, currentTable.Player2Name);
                //    }
                //    else if (otherTable.Player2Id == player2.PlayerId)
                //    {
                //        otherTable.Player2Id = currentTable.Player1Id;
                //        otherTable.Player2Name = currentTable.Player1Name;
                //        otherTable.TableName = string.Format("{0} vs {1}", otherTable.Player1Name, otherTable.Player2Name);

                //        currentTable.Player1Id = tmpTable.Player2Id;
                //        currentTable.Player1Name = tmpTable.Player2Name;
                //        currentTable.TableName = string.Format("{0} vs {1}", currentTable.Player1Name, currentTable.Player2Name);
                //    }
                //}

            }

            Navigation.PopAsync();
        }
    }
}