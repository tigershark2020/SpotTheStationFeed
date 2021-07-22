namespace SpotTheStation
{
    class Program
    {
        public struct ISS_Spotting_Details
        {
            public string DateTime;
            public string Duration;
            public int MaximumElevationDegrees;
            public string Approach;
            public string Departure;
        }

        public struct ISS_Spotting
        {
            public string Title;
            public ISS_Spotting_Details Summary;
            public double Latitude;
            public double Longitude;
        }

        public struct Notification
        {
            public string EventAgency;
            public string EventTitle;
            public string EventDescription;
            public string EventDatetime;
            public string EventURL;
            public string EventCategory;
            public string EventType;
            public string EventIdent;
            public double EventLatitude;
            public double EventLongitude;
        }

        private static int Add_Event_Notification(Notification event_notification)
        {
            int story_inserted = 0;

            MySql.Data.MySqlClient.MySqlConnectionStringBuilder conn_string = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder();
            conn_string.Server = "localhost";
            conn_string.UserID = "mysql_username";
            conn_string.Password = "mysql_password";
            conn_string.Database = "geo_data";

            MySql.Data.MySqlClient.MySqlConnection conn = new MySql.Data.MySqlClient.MySqlConnection(conn_string.ToString());
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
                        System.Console.WriteLine(event_notification.EventAgency + " " + event_notification.EventTitle + " " + event_notification.EventURL);
                        System.Console.WriteLine(event_notification.EventDatetime);
                    }
                }
                catch(MySql.Data.MySqlClient.MySqlException ex)
                {
                    int errorcode = ex.Number;
                    System.Console.WriteLine(errorcode);
                }
                catch(System.Exception e)
                {
                    System.Console.WriteLine(e);
                }

            }
            conn.Close();

            return story_inserted;
        }


        static void Main(string[] args)
        {
            string rss = "https://spotthestation.nasa.gov/sightings/xml_files/United_States_Texas_Johnson_Space_Center.xml";

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
                System.Xml.XmlReader reader = System.Xml.XmlReader.Create(rss);
                System.ServiceModel.Syndication.SyndicationFeed feed = System.ServiceModel.Syndication.SyndicationFeed.Load(reader);

                reader.Close();


                foreach (System.ServiceModel.Syndication.SyndicationItem item in feed.Items)
                {
                    try
                    {
                        ISS_Spotting iss_sighting = new ISS_Spotting();

                        iss_sighting.Title = item.Title.Text.Trim();
                        string SummaryDetails = item.Summary.Text.Trim();

                        // string formatForMySql = item.PublishDate.ToString("dd MMM yyyy H:mm:ss tt");

                        string[] sighting_info = SummaryDetails.Split(new string[] { "<br/>" }, System.StringSplitOptions.None);

                        iss_sighting.Latitude = stationLatitude;
                        iss_sighting.Longitude = stationLongitude;

                        System.Console.WriteLine(iss_sighting.Title);
                        ISS_Spotting_Details iss_spotting_details = new ISS_Spotting_Details();
                        string DateString = null, TimeString = null, DurationString = null, MaximumElevationString = null, ApproachString = null, DepartureString = null;

                        foreach (string data in sighting_info)
                        {
                            string data_trimmed = data.Trim();
                            if (data_trimmed.Contains("Date: "))
                            {
                                string[] DateDetailsArr = data_trimmed.Split(new string[] { ":" }, System.StringSplitOptions.None);
                                DateString = DateDetailsArr[1].Trim();
                                foreach (string day_of_week in DaysOfWeek)
                                {
                                    DateString = DateString.Replace(day_of_week, "").Trim();
                                }
                            }
                            if (data_trimmed.Contains("Time: "))
                            {
                                // string[] TimeDetailsArr = data_trimmed.Split(new string[] { ":" }, StringSplitOptions.None);
                                TimeString = data_trimmed.Replace("Time: ", "").Trim();
                            }

                            if (data_trimmed.Contains("Duration: "))
                            {
                                string[] DurationDetailsArr = data_trimmed.Split(new string[] { ":" }, System.StringSplitOptions.None);
                                DurationString = DurationDetailsArr[1].Trim();
                                iss_spotting_details.Duration = DurationString;
                            }
                            if (data_trimmed.Contains("Maximum Elevation:"))
                            {
                                string[] MaximumElvationArr = data_trimmed.Split(new string[] { ":" }, System.StringSplitOptions.None);
                                MaximumElevationString = MaximumElvationArr[1].Replace("�", "").Trim();
                                try
                                {
                                    iss_spotting_details.MaximumElevationDegrees = System.Convert.ToInt32(MaximumElevationString);
                                }
                                catch(System.Exception e)
                                {
                                    System.Console.WriteLine(e);
                                }
                            }
                            if (data_trimmed.Contains("Approach: "))
                            {
                                string[] ApproachDetailsArr = data_trimmed.Split(new string[] { ":" }, System.StringSplitOptions.None);
                                ApproachString = ApproachDetailsArr[1].Trim();
                                iss_spotting_details.Approach = ApproachString;
                            }
                            if (data_trimmed.Contains("Departure: "))
                            {
                                string[] DepartureDetailsArr = data_trimmed.Split(new string[] { ":" }, System.StringSplitOptions.None);
                                DepartureString = DepartureDetailsArr[1].Trim();
                                iss_spotting_details.Departure = DepartureString;
                            }
                        }

                        string DateStringUnformatted = DateString + " " + TimeString;

                        // string formatForMySql = item.PublishDate.ToString("dd MMM yyyy H:mm:ss tt");
                        string DateStringFormmated = null;
                        System.Globalization.CultureInfo MyCultureInfo = new System.Globalization.CultureInfo("en-US");
                        try
                        {
                            System.DateTime SightingDateTime = System.DateTime.ParseExact(DateStringUnformatted, "MMM d, yyyy h:m tt", MyCultureInfo);
                            iss_spotting_details.DateTime = SightingDateTime.ToString("yyyy-MM-dd H:mm:ss");

                        }
                        catch(System.FormatException)
                        {
                            System.Console.WriteLine("Unable to parse '{0}'", DateStringUnformatted);
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

                            int result = Add_Event_Notification(iss_notification);
                            if (result != 0)
                            {
                                System.Console.WriteLine(iss_sighting.Summary.DateTime);
                                System.Console.WriteLine(iss_sighting.Summary.MaximumElevationDegrees);
                                System.Console.WriteLine(iss_sighting.Summary.Approach);
                                System.Console.WriteLine(iss_sighting.Summary.Departure);
                            }

                        }

                        // AddNewsStory(news_agencies[i], feed_article);
                    }
                    catch(System.Exception erro)
                    {
                        System.Console.WriteLine(erro);
                    }
                }
            }
            catch(System.Exception caughtexc)
            {
                System.Console.WriteLine(caughtexc);
            }

        }
    }
}
