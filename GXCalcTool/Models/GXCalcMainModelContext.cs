using System;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace GXCalcTool.Models
{
    public class GxCalcMainModelContext
    {
        private readonly string _dbConnection;


        public GxCalcMainModelContext(IHostingEnvironment hostingEnvironment)
        {
            if (Path.Combine(hostingEnvironment.ContentRootPath, "App_Data", "GXCalculator.db") == null) throw new NullReferenceException();
            _dbConnection = $"Data Source = {Path.Combine(hostingEnvironment.ContentRootPath, "App_Data", "GXCalculator.db")};Version=3";
        }

        public GXCalcMainModel Model()
        {
            var result = new GXCalcMainModel();
            var cmdText = "select CountryId, Name  from country where CountryId not in (select CountryId from Area where Name='No Service Currently')";

            if (_dbConnection == null) throw new NullReferenceException();
            using (var connection = new SQLiteConnection(_dbConnection))
            {
                connection.Open();

                var command = new SQLiteCommand
                {
                    CommandText = cmdText,
                    CommandType = CommandType.Text,
                    Connection = connection
                };

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.HasRows) return result;
                    result.Countries = new List<Country>();
                    while (reader.Read())
                    {
                        result.Countries.Add(new Country
                        {
                            CountryId = reader.GetValue(0).ToString(),
                            CountryName = reader.GetValue(1).ToString()
                        }
                        );
                    }
                }
            }

            return result;
        }


        public string GetDepotSuggestion(string postcode)
        {
            var depotSuggestion = "";
            postcode = postcode.Replace(" ", string.Empty); //make sure there are no spaces in the postcode
            var cmdText =
                $"select Name, AdditionalDays from depo where DepoId = (select DepoId from Sector where Name Like (select substr('{postcode}',1,length('{postcode}')-2)))";

            if (_dbConnection == null) throw new NullReferenceException(); 
            using (var connection = new SQLiteConnection(_dbConnection))
            {
                connection.Open();

                var command = new SQLiteCommand
                {
                    CommandText = cmdText,
                    CommandType = CommandType.Text,
                    Connection = connection
                };

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.HasRows)
                        depotSuggestion = "Unknown Depot";
                    else
                        while (reader.Read())
                        {
                            depotSuggestion = reader.GetValue(0).ToString() + " Depot";
                            if (Convert.ToInt32(reader.GetValue(1).ToString()) != 0)
                            {
                                depotSuggestion += $" , expect extra {Convert.ToInt32(reader.GetValue(1).ToString())} day";
                            }
                        }
                }
            }

            return depotSuggestion;

        }



        public string GetRegion(int countryId, string keyword)
        {
            var list = @"<option selected>Select Region</option>";
            var cmdText = @"select  '<option value='''|| AreaId || '''>' || Name || '</option>' from Area where CountryId = " + countryId + " and Name like '" + keyword + "%' " + " order by Name";

            if (_dbConnection == null) throw new NullReferenceException();
            using (var connection = new SQLiteConnection(_dbConnection))
            {
                connection.Open();

                var command = new SQLiteCommand
                {
                    CommandText = cmdText,
                    CommandType = CommandType.Text,
                    Connection = connection
                };

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.HasRows) return list;
                    while (reader.Read())
                    {
                        list += reader.GetValue(0).ToString();
                    }
                }
            }

            return list;
        }



        public string GetFedExTimeLine(int areaId)
        {
            var result = string.Empty;
            var timeLine = new FedExTimeline();
            var cmdText = $"select SLADocs, SLAGoods, DeliveryTime from fedexinfo where AreaId = {areaId}";

            if (_dbConnection == null) throw new NullReferenceException();

            try
            {
                using (var connection = new SQLiteConnection(_dbConnection))
                {
                    connection.Open();

                    var command = new SQLiteCommand
                    {
                        CommandText = cmdText,
                        CommandType = CommandType.Text,
                        Connection = connection
                    };

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.HasRows) return null;
                        while (reader.Read())
                        {
                            timeLine.Docs = reader.GetValue(0).ToString();
                            timeLine.Goods = reader.GetValue(1).ToString();
                            timeLine.Time = reader.GetValue(2).ToString();
                        }
                    }
                }
                DateTime dt = DateTime.Parse(timeLine.Time);
                var time = dt.ToString(@"HH:mm");

                result = $"Docs {timeLine.Docs} day(s), Goods {timeLine.Goods} day(s), gauranteed by {time}";
                return result;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }



    }
}
