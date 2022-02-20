/*
    The MIT License (MIT)
    Copyright © 2022 David Zangger

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
    and associated documentation files (the “Software”), to deal in the Software without 
    restriction, including without limitation the rights to use, copy, modify, merge, publish,
    distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom 
    the Software is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all copies or 
    substantial portions of the Software.

    THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
    INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
    PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
    FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
    OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
    DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Spectre.Console;

namespace HdHomerun
{
    /// <summary>
    /// This is the main HDHomerun API. 
    /// </summary>
    public class Keep
    {
        public string SeriesID { get; set; }
        public int? EpisodesToKeep { get; set; }

        public Keep(string seriesID)
        {
            this.SeriesID = seriesID;
        }
    }
    public class Discovery
    {
        public string FriendlyName { get; set; }
        public string ModelNumber { get; set; }
        public string FirmwareName { get; set; }
        public string FirmwareVersion { get; set; }
        public string DeviceID { get; set; }
        public string DeviceAuth { get; set; }
        public string BaseURL { get; set; }
        public string LineupURL { get; set; }
        public int TunerCount { get; set; }
        public string StorageID { get; set; }
        public string StorageURL { get; set; }
        public long TotalSpace { get; set; }
        public long FreeSpace { get; set; }
        public long UsedSpace { 
            get { return TotalSpace - FreeSpace; }
        }
    }
    public class Device
    {
        public string DeviceId { get; set; }
        public string LocalIP { get; set; }
        public string BaseURL { get; set; }
        public string DiscoverURL { get; set; }
        public string LineupURL { get; set; }
    }
    public class Channel
    {
        public int Seq { get; set; }
        public string GuideNumber { get; set; }
        public string GuideName { get; set; }
        public string VideoCodec { get; set; }
        public string AudioCodec { get; set; }
        public string URL { get; set; }
    }
    public class Serial
    {
        public List<Recording> Recordings { get; set; }
        public int Seq { get; set; }
        public string SeriesID { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public string ImageURL { get; set; }
        public long StartTime { get; set; }
        public string EpisodesURL { get; set; }
        public string UpdateID { get; set; }
        public int? EpisodesToKeep { get; set; }

        public Serial()
        {
            this.Recordings = new List<Recording>();
            this.EpisodesToKeep = null;
        }

        /*
         * Clean
         * This will remove all the extra recordings beyond what the user wants to keep
         */
        public int Clean(bool DoClean)
        {
            int RecordingsRemoved = 0;

            // Make sure we check for recordings if we don't have any
            if (Recordings.Count == 0)
            {
                Homerun.GetRecordingsForSerial(this);
            } 

            // Make sure there is a number here.  If not, we intend to keep all the recordings
            if (EpisodesToKeep != null)
            {
                // Find all the episodes after the last one we want to keep
                var RecordingsToClean = Recordings.FindAll(r => r.Seq > EpisodesToKeep.Value);

                foreach(Recording recording in RecordingsToClean)
                {
                    Console.WriteLine($"Seq #{recording.Seq} - {recording.Title}");
                    RecordingsRemoved++;

                    recording.Delete(DoClean);
                    recording.Deleted = true;
                }
            }

            return RecordingsRemoved;
        }
    }
    public class Recording
    {
        public int Seq { get; set; }
        public string Category { get; set; }
        public string ChannelAffiliate { get; set; }
        public string ChannelImageURL { get; set; }
        public string ChannelName { get; set; }
        public string ChannelNumber { get; set; }
        public long EndTime { get; set; }
        public string EpisodeNumber { get; set; }
        public string EpisodeTitle { get; set; }
        public string ImageURL { get; set; }
        public long OriginalAirdate { get; set; }
        public string ProgramID { get; set; }
        public long RecordedEndTime { get; set; }
        public long RecordStartTime { get; set; }
        public string SeriesID { get; set; }
        public long StartTime { get; set; }
        public string Synopsis { get; set; }
        public string Title { get; set; }
        public string Filename { get; set; }
        public string PlayURL { get; set; }
        public string CmdURL { get; set; }
        public bool Deleted { get; set; }
        public long? FileSize { get; set; }

        public Recording()
        {
            Deleted = false;
            FileSize = null;
        }

        /// <summary>
        /// Deletes the recording.
        /// </summary>
        /// <param name="DoDelete">If we are in simulation mode, this will be false.</param>
        public void Delete(bool DoDelete)
        {
            // If we are not in simulation mode, then delete the file
            if (DoDelete)
            {
                // Use the recordings CmdURL and append &cmd=delete to it
                string uri = $"{this.CmdURL}&cmd=delete";

                // Try and delete the recording
                try
                {
                    Deleted = WebAPI.DeleteRecording(uri);
                }
                catch (Exception)
                {
                    Deleted = false;
                }
            }
            else
            {
                AnsiConsole.WriteLine("[red]We didn't really delete your recording...[/]");
                Deleted = true;
            }
        }

        /// <summary>
        /// Gets the filesize of the recording using a web request.  The FileSize property will be 
        /// set so we don't have to keep making extra web requests.
        /// </summary>
        /// <returns>The filesize in Megabytes. -1 if there was an error</returns>
        public long GetFileSize()
        {
            try
            {
                // If we haven't determined the filesize yet, do it now.  
                if (FileSize == null)
                {
                    long fileSize = WebAPI.GetRecoringFileSize(this.PlayURL);
                    FileSize = (fileSize / 1024 / 1024);
                }

                // return the filesize
                return FileSize.Value;
            }
            catch (Exception)
            {
                return -1;
            }
         }
    }
    
    internal static class Homerun
    {
        public static Device DeviceInfo;
        public static Discovery DiscoveryInfo;
        public static List<Serial> Series = new List<Serial>();
        public static List<Channel> Channels = new List<Channel>();
        public static List<Keep> Keeps = new List<Keep>();

        /// <summary>
        /// Do the discovery 
        /// </summary>
        public static void DoDiscovery()
        {
            // Discover the other details
            string sDiscoveryInfoAsJson = WebAPI.GetContents(DeviceInfo.DiscoverURL);
            // Convert the JSON returned into an actual Discovery Info object
            var discoveryInfo = JsonConvert.DeserializeObject<Discovery>(sDiscoveryInfoAsJson);

            DiscoveryInfo = discoveryInfo;
        }

        /// <summary>
        /// Init the device by querying the data from hdhomerun.com
        /// </summary>
        public static void Init()
        {
            try
            {
                // This will query the device information
                string sDeviceInfoasJson = WebAPI.GetContents(@"https://ipv4-api.hdhomerun.com/discover");
                // Convert the JSON returned into an actual Device Info object
                var deviceInfo = JsonConvert.DeserializeObject<Device[]>(sDeviceInfoasJson);
                // Assign the information to the object
                DeviceInfo = deviceInfo[0];

                // Do the discovery
                DoDiscovery();

                // Look for serial keeps.  These are the number of recorings to keep for each serial
                string keepsFile = $"{DeviceInfo.DeviceId}_Keeps.json";

                if (File.Exists(keepsFile))
                {
                    // Read the current values
                    Keeps = JsonConvert.DeserializeObject<List<Keep>>(File.ReadAllText(keepsFile));
                }
                else
                {
                    // Save an empty file
                    SaveKeeps();
                }
            }
            catch (Exception)
            {
                throw;
            } 
        }

        /*
         * SaveKeeps
         * Save the keeps data
         */
        private static void SaveKeeps()
        {
            // Set the name of the keeps file
            string keepsFile = $"{DeviceInfo.DeviceId}_Keeps.json";
            // Write the keeps data to the file
            File.WriteAllText(keepsFile, JsonConvert.SerializeObject(Keeps));
        }

        /*
         * GetRecordingsForSerial
         * Get all the recordings for a serial
         */
        public static void GetRecordingsForSerial(Serial serial)
        {
            // if a serial was found, find the recordings
            if (serial != null)
            {
                int sequence = 1;
                string sRecordingsAsJson = WebAPI.GetContents(serial.EpisodesURL);

                if (sRecordingsAsJson != null)
                {
                    // We need to replace any recordings we've already seen since we are
                    // getting a new list of recordings
                    serial.Recordings.Clear();
                    var recordings = JsonConvert.DeserializeObject<Recording[]>(sRecordingsAsJson);

                    foreach (Recording recording in recordings)
                    {
                        recording.Seq = sequence++;
                        serial.Recordings.Add(recording);
                    }
                }
            }
            else
                throw new Exception("Serial not found");
        }

        /*
         * GetChannels
         * Obtains a list of all the available channels
         */
        public static void GetChannels(bool force = false)
        {
            if (Channels.Count == 0 || force)
            {
                int sequence = 1;

                string uri = DeviceInfo.LineupURL;
                string sChannelsAsJson = WebAPI.GetContents(uri);
                Channels.Clear();

                var channels = JsonConvert.DeserializeObject<Channel[]>(sChannelsAsJson);

                foreach (Channel channel in channels)
                {
                    channel.Seq = sequence++;
                    Channels.Add(channel);
                }
            }
        }

        /*
         * GetAllSeries
         * Obtains a list of all the scheduled series (shows)
         */
        public static void GetAllSeries(bool force = false)
        {
            // if we haven't been here yet, get a list of the series
            if (Series.Count == 0 || force)
            {
                try
                {
                    int sequence = 1;
                    string sSeriesAsJson = WebAPI.GetContents(DiscoveryInfo.StorageURL);
                    Series.Clear();

                    var series = JsonConvert.DeserializeObject<Serial[]>(sSeriesAsJson);

                    foreach (Serial serial in series)
                    {
                        serial.Seq = sequence++;

                        // Look for keeps
                        Keep keep = Keeps.FirstOrDefault(k => k.SeriesID == serial.SeriesID);

                        // If we found a matching seriesId,
                        //   set the number of recordings to keep for the series
                        if (keep != null)
                            serial.EpisodesToKeep = keep.EpisodesToKeep;

                        // Add the serial to the collection 
                        Series.Add(serial);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        /* 
         * SetEpisodesToKeep
         * Sets the number of episodes to keep for a serial.  If episodesToKeep is null,
         * it means all episodes will be saved
         */
        public static void SetEpisodesToKeep(Serial serial, int? episodesToKeep)
        {
            // Set the filename for the keeps file
            string keepsFile = $"{DeviceInfo.DeviceId}_Keeps.json";

            try
            {
                // Do we already have this in our list?
                Keep keep = Keeps.First(k => k.SeriesID == serial.SeriesID);

                // If we want to keep some episodes, update the number
                if (episodesToKeep != null)
                {
                    keep.EpisodesToKeep = episodesToKeep.Value;
                }
                else
                {
                    serial.EpisodesToKeep = null;
                    Keeps.Remove(keep);
                }
            }
            catch (Exception)
            {
                // Add a new record to the keeps list
                Keep keep = new Keep(serial.SeriesID);
                keep.EpisodesToKeep = episodesToKeep;
                Keeps.Add(keep);
            }

            // Save the changes
            serial.EpisodesToKeep = episodesToKeep; 
            SaveKeeps(); 
        }

        /*
         * ToLocalTime
         * Converts a unix time to the local time
         */
        public static string ToLocalTime(long l)
        {
            DateTimeOffset offset = DateTimeOffset.FromUnixTimeSeconds(l);
            DateTime currentTime = offset.DateTime.ToLocalTime();

            return currentTime.ToString("dd-MMM-yyyy HH:mm:ss");
        }
    }
}
