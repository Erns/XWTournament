using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XWTournament.Models;

namespace XWTournament.Classes
{
    public static class Utilities
    {
        public static void InitializeTournamentMain(SQLite.SQLiteConnection conn)
        {
            //Need to create all tables for SQLite
            conn.CreateTable<TournamentMain>();
            conn.CreateTable<TournamentMainPlayer>();
            conn.CreateTable<TournamentMainRound>();
            //conn.CreateTable<TournamentMainRoundPlayer>();
            conn.CreateTable<TournamentMainRoundTable>();

            conn.CreateTable<Player>();
        }

        private static Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void CalculatePlayerScores(ref TournamentMain objTournMain)
        {

            //All the players in the tournament should be included, regardless of being currently active in the tournament or not.

            int intScoreDiff = 0;
            bool blnWinner = false;

            Dictionary<int, TournamentMainPlayer> dctPlayers = new Dictionary<int, TournamentMainPlayer>();
            Dictionary<int, List<int>> dctOpponents = new Dictionary<int, List<int>>();

            //Reset and calculate each player's score
            foreach (TournamentMainPlayer player in objTournMain.Players)
            {
                dctPlayers.Add(player.Id, new TournamentMainPlayer());
                dctOpponents.Add(player.Id, new List<int>());

                player.MOV = 0;
                player.RoundsPlayed = 0;
                player.Score = 0;
                player.SOS = 0;
                player.ByeCount = 0;

                //Go through each round, find their table and calculate the Margin of Victory (MOV) score
                foreach (TournamentMainRound round in objTournMain.Rounds)
                {
                    foreach (TournamentMainRoundTable table in round.Tables)
                    {

                        if ((table.Player1Id == player.Id || table.Player2Id == player.Id) && (table.Player1Winner || table.Player2Winner))
                        {
                            player.RoundsPlayed++;
                            blnWinner = false;

                            //Keep track of all the opponents faced for each player
                            if (table.Player1Id == player.Id)
                            {
                                if (table.Player2Id > 0)
                                {
                                    dctOpponents[player.Id].Add(table.Player2Id);
                                }
                                else if (table.Bye)
                                {
                                    player.ByeCount++;
                                }
                            }
                            else
                            {
                                dctOpponents[player.Id].Add(table.Player1Id);
                            }

                            //Set the table's score difference and if player is the winner
                            if (table.Player1Winner)
                            {
                                intScoreDiff = table.Player1Score - table.Player2Score;
                                if (player.Id == table.Player1Id) blnWinner = true;
                            }
                            else if (table.Player2Winner)
                            {
                                intScoreDiff = table.Player2Score - table.Player1Score;
                                if (player.Id == table.Player2Id) blnWinner = true;
                            }

                            //Set points if score tied for the table
                            if (table.ScoreTied) player.MOV += objTournMain.MaxPoints;

                            //Set score and MOV
                            if (blnWinner)
                            {
                                player.Score++;

                                if (!table.ScoreTied)
                                    player.MOV += (intScoreDiff + objTournMain.MaxPoints);
                            }
                            else
                            {
                                if (!table.ScoreTied)
                                    player.MOV += (objTournMain.MaxPoints - intScoreDiff);
                            }

                            break;
                        }
                    }
                }

                //Set player dictionary for quick access here soon
                dctPlayers[player.Id] = player;
            }


            //Now calculate the Strength of Schedule (SOS)
            foreach (TournamentMainPlayer player in objTournMain.Players)
            {
                if (player.RoundsPlayed == 0) continue;

                decimal decSoS = 0;
                foreach (int opponentId in dctOpponents[player.Id])
                {
                    TournamentMainPlayer opponent = dctPlayers[opponentId];
                    if (opponent.RoundsPlayed == 0) continue;
                    decSoS += Decimal.Divide(opponent.Score, opponent.RoundsPlayed);
                }

                decSoS /= player.RoundsPlayed;
                player.SOS = Math.Round(decSoS, 2);

            }


            //Determine standings/rank
            List<TournamentMainPlayer> lstStandings = GetStandings(objTournMain);
            int intRank = 1;
            foreach (TournamentMainPlayer standingPlayer in lstStandings)
            {
                foreach (TournamentMainPlayer mainPlayer in objTournMain.Players)
                {
                    if (standingPlayer.PlayerId == mainPlayer.PlayerId)
                    {
                        mainPlayer.Rank = intRank;
                        break;
                    }
                }
                intRank++;
            }

        }


        private static List<TournamentMainPlayer> GetStandings(TournamentMain objTournMain)
        {
            List<TournamentMainPlayer> lstTmpPlayers = new List<TournamentMainPlayer>();

            lstTmpPlayers = objTournMain.Players;
            lstTmpPlayers = lstTmpPlayers.OrderByDescending(obj => obj.SOS).ToList();
            lstTmpPlayers = lstTmpPlayers.OrderByDescending(obj => obj.MOV).ToList();
            lstTmpPlayers = lstTmpPlayers.OrderByDescending(obj => obj.Score).ToList();
            lstTmpPlayers = lstTmpPlayers.OrderByDescending(obj => obj.RoundsPlayed).ToList();

            return lstTmpPlayers;
        }

    }
}
