using System.ComponentModel;
using System;

namespace AutumnClockChangeSpots
{
    public class clsChannelData
    {
        public int Idx { get; set; }
        public int Carrier { get; set; }
        public int Region { get; set; }

        public int Platform { get; set; } = 0;

        public string RTSPPath { get; set; }

        public string WorkName { get; set; }

        public string UIName { get; set; }

        public int StartTime { get; set; }
        public int Endtime { get; set; }

        public bool Active { get; set; }

        public int VideoStream { get; set; } = 0;
        public int AudioStream { get; set; } = 0;

        public int ScheduleCarrier { get; set; }
        public int ScheduleRegion { get; set; }

        /// <summary>
        /// flag to indicate if the channel has spots,  if not the channel will not be displayed in the app
        /// </summary>
        public bool HasSpots { get; set; } = true;

        #region"Origins output metadaata"
        public string DB1Code { get; set; } = string.Empty;
        public string LogStationCode { get; set; } = String.Empty;
        public string SalesHouse { get; set; } = String.Empty;


        #endregion
        public BindingList<ActiveDate> ActiveDates { get; set; } = new BindingList<ActiveDate>();//{ new ActiveDate()};


        public string FFmpegPath
        {
            get; set;
        } = "";

        private bool _Recording = false;

        private int intStartTime
        {
            get { return intTimeToIntegerMins(StartTime); }
        }

        private int intEndTime
        {
            get { return intTimeToIntegerMins(Endtime); }
        }
        private int intTimeToIntegerMins(int startTime)
        {
            string timestr = startTime.ToString().PadLeft(4, '0');
            string mins = timestr.Substring(2);
            string hrs = timestr.Substring(0, 2);

            return int.Parse(hrs) * 60 + int.Parse(mins);
        }

        public string TestID
        {
            get
            {
                return string.Format("{0}_{1}_{2}", Carrier, Region, Platform);
            }

        }

        public bool RecordingNow(DateTime CurrentIimeUTC)
        {
            int CurrentTimeInMins = CurrentIimeUTC.Minute + 60 * CurrentIimeUTC.Hour;
            if (StartTime < Endtime)
            {
                if (Active == true && intStartTime - 1 <= CurrentTimeInMins && intEndTime - 1 >= CurrentTimeInMins)//daytime
                {
                    _Recording = true;
                    return true;
                }
                else
                {
                    _Recording = false;
                    return false;
                }
            }
            else
            {
                if (Active == true && intStartTime - 2 <= CurrentTimeInMins || intEndTime - 1 >= CurrentTimeInMins)//nighttime
                {
                    _Recording = true;
                    return true;
                }
                else
                {
                    _Recording = false;
                    return false;
                }
            }
        }
    }

    public class ActiveDate
    {
        public ActiveDate() { }

        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = new DateTime(9999, 12, 31);
    }
}