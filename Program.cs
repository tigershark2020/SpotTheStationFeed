using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.ServiceModel.Syndication;
using System.Globalization;
using MySql.Data.MySqlClient;

namespace SpotTheStation
{
    class Program
    {

        public struct ISS_Spotting_Details
        {
            public String DateTime;
            public String Duration;
            public int MaximumElevationDegrees;
            public String Approach;
            public String Departure;
        }

        public struct ISS_Spotting
        {
            public String Title;
            public ISS_Spotting_Details Summary;
            public double Latitude;
            public double Longitude;
        }

        public struct Notification
        {
            public String EventAgency;
            public String EventTitle;
            public String EventDescription;
            public String EventDatetime;
            public String EventURL;
            public String EventCategory;
            public String EventType;
            public String EventIdent;
            public Double EventLatitude;
            public Double EventLongitude;
        }

        private static int AddNotification(Notification event_notification)
        {
            int story_inserted = 0;

            MySqlConnectionStringBuilder conn_string = new MySqlConnectionStringBuilder();
            conn_string.Server = "localhost";
            conn_string.UserID = "mysql_username";
            conn_string.Password = "mysql_password";
            conn_string.Database = "geo_data";

            MySql.Data.MySqlClient.MySqlConnection conn = new MySqlConnection(conn_string.ToString());
            conn.Open();
            if (true)
            {
                try
                {
                    MySql.Data.MySqlClient.MySqlCommand cmd = conn.CreateCommand();
                    cmd.Connection = conn;

                    cmd.CommandText = "INSERT INTO `geo_data`.`geo_events` (`geo_event_agency`,`geo_event_title`,`geo_event_url`,`geo_event_starttime`,`geo_event_category`,`geo_event_type`,`geo_event_ident`,`geo_event_location_latitude`,`geo_event_location_longitude`,`geo_event_notify`) VALUES (@event_agency,@event_title,@event_url,@event_datetime,@event_category,@event_type,@event_ident,@event_latitude,@event_longitude,1);";

                    cmd.Parameters.AddWithValue("@event_agency", event_notification.EventAgency);
                    cmd.Parameters.AddWithValue("@event_title", event_notification.EventTitle);
                    cmd.Parameters.AddWithValue("@event_url", event_notification.EventURL);
                    cmd.Parameters.AddWithValue("@event_datetime", event_notification.EventDatetime);
                    cmd.Parameters.AddWithValue("@event_category", event_notification.EventCategory);
                    cmd.Parameters.AddWithValue("@event_type", event_notification.EventType);
                    cmd.Parameters.AddWithValue("@event_ident", event_notification.EventIdent);
                    cmd.Parameters.AddWithValue("@event_latitude", event_notification.EventLatitude);
                    cmd.Parameters.AddWithValue("@event_longitude", event_notification.EventLongitude);
                    cmd.Prepare();

                    int insert_status = cmd.ExecuteNonQuery();
                    if (insert_status == 1)
                    {
                        Console.WriteLine(event_notification.EventAgency + " " + event_notification.EventTitle + " " + event_notification.EventURL);
                        Console.WriteLine(event_notification.EventDatetime);
                    }
                }
                catch (MySqlException ex)
                {
                    int errorcode = ex.Number;
                    Console.WriteLine(errorcode);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }
            conn.Close();

            return story_inserted;
        }


        static void Main(string[] args)
        {
            String rss = "https://spotthestation.nasa.gov/sightings/xml_files/United_States_Texas_Johnson_Space_Center.xml";

            double stationLatitude = 29.559357;
            double stationLongitude = -95.08994;

            System.Collections.Generic.List<string> DaysOfWeek = new System.Collections.Generic.List<string>();
            DaysOfWeek.Add("Monday");
            DaysOfWeek.Add("Tuesday");
            DaysOfWeek.Add("Wednesday");
            DaysOfWeek.Add("Thursday");
            DaysOfWeek.Add("Friday");
            DaysOfWeek.Add("Saturday");
            DaysOfWeek.Add("Sunday");

            try
            {
                XmlReader reader = XmlReader.Create(rss);
                SyndicationFeed feed = SyndicationFeed.Load(reader);

                reader.Close();


                foreach (SyndicationItem item in feed.Items)
                {
                    try
                    {
                        ISS_Spotting iss_sighting = new ISS_Spotting();

                        iss_sighting.Title = item.Title.Text.Trim();
                        String SummaryDetails = item.Summary.Text.Trim();

                        // string formatForMySql = item.PublishDate.ToString("dd MMM yyyy H:mm:ss tt");

                        String[] sighting_info = SummaryDetails.Split(new string[] { "<br/>" }, StringSplitOptions.None);

                        iss_sighting.Latitude = stationLatitude;
                        iss_sighting.Longitude = stationLongitude;

                        Console.WriteLine(iss_sighting.Title);
                        ISS_Spotting_Details iss_spotting_details = new ISS_Spotting_Details();
                        String DateString = null, TimeString = null, DurationString = null, MaximumElevationString = null, ApproachString = null, DepartureString = null;

                        foreach (String data in sighting_info)
                        {
                            String data_trimmed = data.Trim();
                            if (data_trimmed.Contains("Date: "))
                            {
                                String[] DateDetailsArr = data_trimmed.Split(new string[] { ":" }, StringSplitOptions.None);
                                DateString = DateDetailsArr[1].Trim();
                                foreach (String day_of_week in DaysOfWeek)
                                {
                                    DateString = DateString.Replace(day_of_week, "").Trim();
                                }
                            }
                            if (data_trimmed.Contains("Time: "))
                            {
                                // String[] TimeDetailsArr = data_trimmed.Split(new string[] { ":" }, StringSplitOptions.None);
                                TimeString = data_trimmed.Replace("Time: ", "").Trim();
                            }

                            if (data_trimmed.Contains("Duration: "))
                            {
                                String[] DurationDetailsArr = data_trimmed.Split(new string[] { ":" }, StringSplitOptions.None);
                                DurationString = DurationDetailsArr[1].Trim();
                                iss_spotting_details.Duration = DurationString;
                            }
                            if (data_trimmed.Contains("Maximum Elevation:"))
                            {
                                String[] MaximumElvationArr = data_trimmed.Split(new string[] { ":" }, StringSplitOptions.None);
                                MaximumElevationString = MaximumElvationArr[1].Replace("�", "").Trim();
                                try
                                {
                                    iss_spotting_details.MaximumElevationDegrees = Convert.ToInt32(MaximumElevationString);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            }
                            if (data_trimmed.Contains("Approach: "))
                            {
                                String[] ApproachDetailsArr = data_trimmed.Split(new string[] { ":" }, StringSplitOptions.None);
                                ApproachString = ApproachDetailsArr[1].Trim();
                                iss_spotting_details.Approach = ApproachString;
                            }
                            if (data_trimmed.Contains("Departure: "))
                            {
                                String[] DepartureDetailsArr = data_trimmed.Split(new string[] { ":" }, StringSplitOptions.None);
                                DepartureString = DepartureDetailsArr[1].Trim();
                                iss_spotting_details.Departure = DepartureString;
                            }
                        }

                        String DateStringUnformatted = DateString + " " + TimeString;

                        // string formatForMySql = item.PublishDate.ToString("dd MMM yyyy H:mm:ss tt");
                        String DateStringFormmated = null;
                        CultureInfo MyCultureInfo = new CultureInfo("en-US");
                        try
                        {
                            DateTime SightingDateTime = DateTime.ParseExact(DateStringUnformatted, "MMM d, yyyy h:m tt", MyCultureInfo);
                            iss_spotting_details.DateTime = SightingDateTime.ToString("yyyy-MM-dd H:mm:ss");

                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("Unable to parse '{0}'", DateStringUnformatted);
                        }

                        iss_sighting.Summary = iss_spotting_details;

                        if (iss_sighting.Summary.MaximumElevationDegrees > 54)
                        {
                            Notification iss_notification = new Notification();

                            iss_notification.EventAgency = "48942";
                            iss_notification.EventTitle = iss_sighting.Title + " (Elevation: " + iss_sighting.Summary.MaximumElevationDegrees.ToString() + "�)";
                            iss_notification.EventDatetime = iss_sighting.Summary.DateTime;
                            iss_notification.EventCategory = "Space";
                            iss_notification.EventType = "Outdoor";
                            iss_notification.EventIdent = "iss_" + iss_sighting.Summary.DateTime;
                            iss_notification.EventLatitude = stationLatitude;
                            iss_notification.EventLongitude = stationLongitude;
                            iss_notification.EventURL = "https://spotthestation.nasa.gov/sightings/xml_files/United_States_Texas_Johnson_Space_Center.xml";

                            int result = AddNotification(iss_notification);
                            if (result != 0)
                            {
                                Console.WriteLine(iss_sighting.Summary.DateTime);
                                Console.WriteLine(iss_sighting.Summary.MaximumElevationDegrees);
                                Console.WriteLine(iss_sighting.Summary.Approach);
                                Console.WriteLine(iss_sighting.Summary.Departure);
                            }

                        }

                        // AddNewsStory(news_agencies[i], feed_article);
                    }
                    catch (Exception erro)
                    {
                        Console.WriteLine(erro);
                    }
                }
            }
            catch (Exception caughtexc)
            {
                Console.WriteLine(caughtexc);
            }

        }
    }
}
