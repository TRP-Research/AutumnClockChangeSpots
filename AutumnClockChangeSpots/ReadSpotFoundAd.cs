using System;
using System.Collections.Generic;

namespace AutumnClockChangeSpots
{
    public class ReadSpotFoundAd
    {
        public ReadSpotFoundAd(int AdLength)
        {
            SpotDuration = AdLength;

            ExpectedNumOfFrames = 8 * SpotDuration;
        }

        public string Title { get; set; }
        public string Advertiser { get; set; }
        public string Brand { get; set; }
        public int StartSpotFrameIdx { get; set; }
        public int EndSpotFrameIdx { get; set; }
        private int ExpectedNumOfFrames { get; set; }

        public string FilmCode { get; set; }

        public int FilmCodeIdx { get; set; }
        /// <summary>
        /// Start time of Matched Sequence
        /// </summary>
        public TimeSpan SpotMatchedStart { get; set; }

        //TODO check we neeed this
        // public string MatchedStartTimeStr { get; set; }

        /// <summary>
        /// Endtime of matched Sequence
        /// </summary>
        public TimeSpan SpotMatchedEnd { get; set; }

        private TimeSpan _CalculatedStart { get; set; }
        /// <summary>
        /// Calculated start correctly rounded for fractional seconds
        /// </summary>
        public TimeSpan CalculatedStart
        {
            get { return _CalculatedStart; }
            set
            {
                _CalculatedStart = value;
                CalculatedStartStr = FormatedTime(_CalculatedStart);
            }
        }

        private TimeSpan _CalculatedEnd { get; set; }
        /// <summary>
        /// Calculated end correctly rounded for fractional seconds
        /// </summary>
        public TimeSpan CalculatedEnd
        {
            get { return _CalculatedEnd; }
            set
            {
                _CalculatedEnd = value;
                CalculatedEndStr = FormatedTime(_CalculatedEnd);
            }
        }
        public TimeSpan Gap { get; set; }
        // public string MatchedEndTimeStr { get; set; }

        /// <summary>
        /// Start time corrected to allow for frames missed from start of match
        /// </summary>
        public string CalculatedStartStr { get; set; }

        /// <summary>
        /// EndTime corrected to allow for frame missed from end of match
        /// </summary>
        public string CalculatedEndStr { get; set; }


        /// <summary>
        /// Ideal duration of Spot ffrom spot data
        /// </summary>
        public int SpotDuration { get; set; } = 0;

        public double LenPercentTest
        {
            get
            {
                return 100.0 * LengthTest / SpotDuration;
            }
        }

        public double LengthTest
        {
            get
            {
                double FirstAd = SpotMatchedStart.TotalSeconds;
                double LastAAd = SpotMatchedEnd.TotalSeconds;

                double FoundLength = LastAAd - FirstAd;
                //test is 50% of length
                return FoundLength;
            }
        }

        public bool Overlapped { get; set; } = false;
        public int OverlappedCount { get; set; } = 0;

        public bool Partial { get; set; } = false;

        public double SequenceQuality
        {
            get; set;
        }

        public double BaseSequenceQuality
        {
            get;
            set;
        }


        // public string SourceFilePath { get; set; }
        public List<ReadSpotFoundAd> OverlappedSpots
        {
            get;
            set;
        } = new List<ReadSpotFoundAd>();



        private string FormatedTime(TimeSpan RoundedStart)
        {
            string timestr = "";
            if (RoundedStart.Days == 1)
            {
                //cheat to 6-6 time
                switch (RoundedStart.Hours)
                {
                    case 0:
                        timestr = string.Format("24:{0}:{1}", RoundedStart.Minutes.ToString("00"), RoundedStart.Seconds.ToString("00"));
                        break;
                    case 1:
                        timestr = string.Format("25:{0}:{1}", RoundedStart.Minutes.ToString("00"), RoundedStart.Seconds.ToString("00"));
                        break;
                    case 2:
                        timestr = string.Format("26:{0}:{1}", RoundedStart.Minutes.ToString("00"), RoundedStart.Seconds.ToString("00"));
                        break;
                    case 3:
                        timestr = string.Format("27:{0}:{1}", RoundedStart.Minutes.ToString("00"), RoundedStart.Seconds.ToString("00"));
                        break;
                    case 4:
                        timestr = string.Format("28:{0}:{1}", RoundedStart.Minutes.ToString("00"), RoundedStart.Seconds.ToString("00"));
                        break;
                    case 5:
                        timestr = string.Format("29:{0}:{1}", RoundedStart.Minutes.ToString("00"), RoundedStart.Seconds.ToString("00"));
                        break;

                }
            }
            else
            {
                timestr = string.Format("{2}:{0}:{1}", RoundedStart.Minutes.ToString("00"), RoundedStart.Seconds.ToString("00"), RoundedStart.Hours.ToString("00"));
            }


            return timestr;
        }

    }
}