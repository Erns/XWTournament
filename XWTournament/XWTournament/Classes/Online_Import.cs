using Newtonsoft.Json;
using RestSharp;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using XWTournament.Models;

namespace XWTournament.Classes
{
    public class Online_Import
    {
        static RestClient client = Utilities.InitializeRestClient();

        public static string ImportAll()
        {
            client = Utilities.InitializeRestClient();

            if (App.IsUserLoggedIn)
            {

                try
                {
                    //Get the current players saved locally
                    List<Player> lstCurrentPlayers = new List<Player>();
                    List<TournamentMain> lstCurrentTournaments = new List<TournamentMain>();

                    using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
                    {
                        Utilities.InitializeTournamentMain(conn);

                        lstCurrentPlayers = conn.Query<Player>("SELECT * FROM Player WHERE Active = ? AND DateDeleted IS NULL ORDER BY Name", true);
                        lstCurrentTournaments = conn.Query<TournamentMain>("SELECT * FROM TournamentMain WHERE DateDeleted IS NULL ORDER BY StartDate");
                    }

                    //Import general Players and general Tournament info
                    ImportPlayers(lstCurrentPlayers);
                    ImportTournaments(lstCurrentTournaments);


                    //Import specific player, round, and table data for each tournament
                    using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
                    {

                        //Collect most up-to-date Player info since they've been imported, collecting API Ids
                        lstCurrentPlayers = new List<Player>();
                        lstCurrentPlayers = conn.Query<Player>("SELECT * FROM Player WHERE Active = ? AND DateDeleted IS NULL ORDER BY Name", true);

                        Dictionary<int, int> dctAPIPlayerId = new Dictionary<int, int>();
                        foreach (Player player in lstCurrentPlayers)
                        {
                            if (!dctAPIPlayerId.ContainsKey(player.API_Id))
                            {
                                dctAPIPlayerId.Add(player.API_Id, player.Id);
                            }
                        }


                        lstCurrentTournaments = new List<TournamentMain>();
                        lstCurrentTournaments = conn.Query<TournamentMain>("SELECT * FROM TournamentMain WHERE DateDeleted IS NULL ORDER BY StartDate");

                        //Go through each saved Tournament on device, pulling the detailed round/table data for each
                        TournamentMain objTournMain;
                        foreach (TournamentMain localTournament in lstCurrentTournaments)
                        {
                            objTournMain = new TournamentMain();
                            objTournMain = conn.GetWithChildren<TournamentMain>(localTournament.Id, true);

                            if (objTournMain.API_Id > 0)
                            {
                                var request = new RestRequest("Tournaments/{userid}/{id}", Method.GET);
                                request.AddUrlSegment("userid", App.CurrentUser.Id);
                                request.AddUrlSegment("id", objTournMain.API_Id);

                                // execute the request
                                IRestResponse response = client.Execute(request);
                                var content = response.Content;

                                List<TournamentMain> result = JsonConvert.DeserializeObject<List<TournamentMain>>(JsonConvert.DeserializeObject(content).ToString());
                                foreach (TournamentMain ApiTournament in result)
                                {

                                    //Add/update Tournament Player data
                                    foreach (TournamentMainPlayer ApiTournPlayer in ApiTournament.Players)
                                    {
                                        TournamentMainPlayer updatePlayer = new TournamentMainPlayer()
                                        {
                                            Active = ApiTournPlayer.Active,
                                            API_Id = ApiTournPlayer.Id,
                                            Bye = ApiTournPlayer.Bye,
                                            ByeCount = ApiTournPlayer.ByeCount,
                                            MOV = ApiTournPlayer.MOV,
                                            PlayerId = (dctAPIPlayerId.ContainsKey(ApiTournPlayer.PlayerId) ? dctAPIPlayerId[ApiTournPlayer.PlayerId] : 0),
                                            PlayerName = ApiTournPlayer.PlayerName,
                                            Rank = ApiTournPlayer.Rank,
                                            RoundsPlayed = ApiTournPlayer.RoundsPlayed,
                                            TournamentId = objTournMain.Id
                                        };

                                        foreach (TournamentMainPlayer localPlayer in objTournMain.Players)
                                        {
                                            if (localPlayer.PlayerId == updatePlayer.PlayerId)
                                            {
                                                updatePlayer.Id = localPlayer.Id;
                                                break;
                                            }
                                        }

                                        if (updatePlayer.Id == 0)
                                            conn.Insert(updatePlayer);
                                        else
                                            conn.Update(updatePlayer);
                                    }

                                    //Add/Update/Delete Rounds
                                    foreach (TournamentMainRound ApiRound in ApiTournament.Rounds)
                                    {
                                        TournamentMainRound updateRound = new TournamentMainRound()
                                        {
                                            API_Id = ApiRound.Id,
                                            Number = ApiRound.Number,
                                            RoundTimeEnd = ApiRound.RoundTimeEnd,
                                            Swiss = ApiRound.Swiss,
                                            TournamentId = objTournMain.Id
                                        };

                                        //Grab the device's SQL round Id
                                        foreach (TournamentMainRound localRound in objTournMain.Rounds)
                                        {
                                            if (localRound.API_Id == updateRound.API_Id)
                                            {
                                                updateRound.Id = localRound.Id;
                                                break;
                                            }
                                        }
                                        if (updateRound.Id == 0)
                                        {
                                            conn.Insert(updateRound);
                                            TournamentMainRound tmp = conn.Query<TournamentMainRound>("SELECT * FROM TournamentMainRound WHERE TournamentId = ? ORDER BY Id DESC", updateRound.TournamentId)[0];
                                            updateRound.Id = tmp.Id; //We need this latest RoundId so that the tables will be associated with the correct round
                                        }
                                        else
                                        {
                                            conn.Update(updateRound);
                                        }


                                        //Add/update table data
                                        foreach (TournamentMainRoundTable ApiTable in ApiRound.Tables)
                                        {
                                            TournamentMainRoundTable updateTable = new TournamentMainRoundTable()
                                            {
                                                API_Id = ApiTable.Id,
                                                Bye = ApiTable.Bye,
                                                Number = ApiTable.Number,
                                                Player1Id = (dctAPIPlayerId.ContainsKey(ApiTable.Player1Id) ? dctAPIPlayerId[ApiTable.Player1Id] : 0),
                                                Player1Name = ApiTable.Player1Name,
                                                Player1Score = ApiTable.Player1Score,
                                                Player1Winner = ApiTable.Player1Winner,
                                                Player2Id = (dctAPIPlayerId.ContainsKey(ApiTable.Player2Id) ? dctAPIPlayerId[ApiTable.Player2Id] : 0),
                                                Player2Name = ApiTable.Player2Name,
                                                Player2Score = ApiTable.Player2Score,
                                                Player2Winner = ApiTable.Player2Winner,
                                                ScoreTied = ApiTable.ScoreTied,
                                                TableName = ApiTable.TableName,
                                                RoundId = updateRound.Id
                                            };


                                            //Grab the device's SQL table Id
                                            foreach(TournamentMainRound localRound in objTournMain.Rounds)
                                            {
                                                if (localRound.Id == updateTable.RoundId)
                                                {
                                                    foreach(TournamentMainRoundTable localTable in localRound.Tables)
                                                    {
                                                        if (localTable.API_Id == updateTable.API_Id)
                                                        {
                                                            updateTable.Id = localTable.Id;
                                                            break;
                                                        }
                                                    }
                                                    break;
                                                }
                                            }

                                            if (updateTable.Id == 0)
                                            {
                                                conn.Insert(updateTable);
                                            }
                                            else
                                            {
                                                conn.Update(updateTable);
                                            }
                                        }

                                    }

                                    //Delete any local rounds that are saved, but were removed online
                                    foreach (TournamentMainRound localRound in objTournMain.Rounds)
                                    {
                                        bool blnDeleteRound = true;
                                        foreach(TournamentMainRound ApiRound in ApiTournament.Rounds)
                                        {
                                            if (ApiRound.Id == localRound.API_Id)
                                            {
                                                blnDeleteRound = false;
                                                break;
                                            }
                                        }
                                        if (blnDeleteRound)
                                            conn.Delete(localRound);
                                    }

                                    break;
                                }
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.Write("Error:" + ex.Message);
                    return "Error: " + ex.Message;
                }

                return "Import success";
            }

            return "";
        }

        private static void ImportPlayers(List<Player> lstCurrentPlayers)
        {
            //Get Players
            RestRequest request = new RestRequest("Players/{userid}", Method.GET);
            request.AddUrlSegment("userid", App.CurrentUser.Id);

            // execute the request
            IRestResponse response = client.Execute(request);
            string content = response.Content;

            List<Player> lstApiPlayers = JsonConvert.DeserializeObject<List<Player>>(JsonConvert.DeserializeObject(content).ToString());

            //Compare players from API with what's saved locally, insert/updated as needed
            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
            {
                foreach (Player apiPlayer in lstApiPlayers)
                {
                    if (apiPlayer.Active && apiPlayer.DateDeleted == null)
                    {

                        Player updatePlayer = new Player()
                        {
                            Name = apiPlayer.Name,
                            Email = apiPlayer.Email,
                            Group = apiPlayer.Group,
                            Active = true,
                            API_Id = apiPlayer.Id
                        };

                        //Attempt to associate a player from the API with one saved locally
                        foreach (Player localPlayer in lstCurrentPlayers)
                        {
                            if (localPlayer.API_Id == apiPlayer.Id || (apiPlayer.Name.ToUpper() == localPlayer.Name.ToUpper() && apiPlayer.Email.ToUpper() == localPlayer.Email.ToUpper()))
                            {
                                updatePlayer.Id = localPlayer.Id;
                                break;
                            }
                        }

                        if (updatePlayer.Id == 0)
                        {
                            conn.Insert(updatePlayer);
                        }
                        else
                        {
                            conn.Update(updatePlayer);
                        }
                    }
                }
            }
        }

        private static void ImportTournaments(List<TournamentMain> lstCurrentTournaments)
        {
            //Get Tournaments
            RestRequest request = new RestRequest("Tournaments/{userid}", Method.GET);
            request.AddUrlSegment("userid", App.CurrentUser.Id);

            // execute the request
            IRestResponse response = client.Execute(request);
            string content = response.Content;

            List<TournamentMain> lstApiTournaments = JsonConvert.DeserializeObject<List<TournamentMain>>(JsonConvert.DeserializeObject(content).ToString());
            using (SQLite.SQLiteConnection conn = new SQLite.SQLiteConnection(App.DB_PATH))
            {
                foreach (TournamentMain apiTournament in lstApiTournaments)
                {
                    TournamentMain updateTournament = new TournamentMain()
                    {
                        API_Id = apiTournament.Id,
                        Name = apiTournament.Name,
                        MaxPoints = apiTournament.MaxPoints,
                        Players = apiTournament.Players,
                        Rounds = apiTournament.Rounds,
                        RoundTimeLength = apiTournament.RoundTimeLength,
                        StartDate = apiTournament.StartDate
                    };

                    foreach (TournamentMain localTournament in lstCurrentTournaments)
                    {
                        if (localTournament.API_Id == apiTournament.Id || (localTournament.Name == apiTournament.Name && localTournament.StartDate == apiTournament.StartDate))
                        {
                            updateTournament.Id = localTournament.Id;
                            break;
                        }
                    }

                    if (updateTournament.Id == 0)
                    {
                        conn.Insert(updateTournament);
                    }
                    else
                    {
                        conn.Update(updateTournament);
                    }
                }
            }
        }

    }
}
