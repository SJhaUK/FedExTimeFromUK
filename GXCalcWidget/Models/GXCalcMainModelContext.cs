using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Web;

namespace GXCalcTool.Models
{
    public class GxCalcMainModelContext
    {
        private readonly string _dbConnection;


        public GxCalcMainModelContext()
        {

            if (Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data"), "GXCalculator.db") == null)
                throw new NullReferenceException();
            var currentPath = Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data"));
            _dbConnection = $"Data Source = {Path.Combine(currentPath, "GXCalculator.db")};Version=3";
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


                //Logic for the Newcastle, Deeside and York depots
                
            }

            return list;
        }



        public string GetFedExTimeLine(int areaId, string postDate)
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

                    DateTime expectedDate = Convert.ToDateTime(postDate);
                    expectedDate = expectedDate.AddDays(Convert.ToDouble(timeLine.Goods));
                    string expectedDateString = FormatDateString(expectedDate);
                    
                    var cmdText_2 = $@"SELECT COUNT (1) FROM bankholiday LEFT JOIN country ON bankholiday.CountryId = country.CountryId WHERE bankholiday.countryid = (SELECT countryid FROM area WHERE areaid = {areaId}) AND bankholiday.Day BETWEEN ""{postDate}"" AND ""{expectedDateString}"" ORDER BY name";

                    var command_2 = new SQLiteCommand
                    {
                        CommandText = cmdText_2,
                        CommandType = CommandType.Text,
                        Connection = connection
                    };

                    using (var reader = command_2.ExecuteReader())
                    {
                        if (!reader.HasRows) return null;
                        while (reader.Read())
                        {
                            timeLine.AdditionalDays = reader.GetValue(0).ToString();
                        }
                    }

                    var cmdText_3 = $@"SELECT COUNT (1) FROM bankholiday LEFT JOIN country ON bankholiday.CountryId = country.CountryId WHERE bankholiday.CountryId = 250 AND bankholiday.Day BETWEEN ""{postDate}"" AND ""{expectedDateString}""";

                    var command_3 = new SQLiteCommand
                    {
                        CommandText = cmdText_3,
                        CommandType = CommandType.Text,
                        Connection = connection
                    };

                    using (var reader = command_3.ExecuteReader())
                    {
                        if (!reader.HasRows) return null;
                        while (reader.Read())
                        {
                            timeLine.AdditionalDays_Uk = reader.GetValue(0).ToString();
                        }

                    }

                }
                DateTime dt = DateTime.Parse(timeLine.Time);
                var time = dt.ToString(@"HH:mm");

                result = $"{timeLine.Docs}, {timeLine.Goods}, {time}, {timeLine.AdditionalDays}, {timeLine.AdditionalDays_Uk}";

                return result;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public string GetFedExTimeLine(string postcode)
        {
            var result = string.Empty;
            var timeLine = new FedExTimeline();
            var cmdText = $"SELECT D.name, D.america, D.row, D.additionaldays FROM sector S LEFT JOIN depo D ON S.depoid = D.depoid WHERE D.depoid = (select DepoId from Sector where Name Like (select substr('{postcode}',1,length('{postcode}')-2))) LIMIT 1";

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
                            timeLine.DepotName = reader.GetValue(0).ToString();
                            timeLine.TimeForAmerica = reader.GetValue(1).ToString();
                            timeLine.TimeForROW = reader.GetValue(2).ToString();
                            timeLine.AdditionalDays = reader.GetValue(3).ToString();
                        }
                    }
                }
                DateTime dt = DateTime.Parse(timeLine.TimeForAmerica);
                DateTime dt_2 = DateTime.Parse(timeLine.TimeForROW);
                var time = dt.ToString(@"HH:mm");
                var time_2 = dt_2.ToString(@"HH:mm");

                result = $"{timeLine.DepotName}, {time}, {time_2}, {timeLine.AdditionalDays}";

                return result;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public string FormatDateString(DateTime date)
        {
            string day = (date.Day < 10) ? $"0{date.Day}" : $"{date.Day}";
            string month = (date.Month < 10) ? $"0{date.Month}" : $"{date.Month}";
            return $"{date.Year}-{month}-{day}";
        }

    }
}
