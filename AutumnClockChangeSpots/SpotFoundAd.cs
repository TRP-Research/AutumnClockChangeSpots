using System.Collections.Generic;
using System;

namespace AutumnClockChangeSpots
{
    public class SpotFoundAd
    {
        public SpotFoundAd(int AdLength)
        {
            SpotDuration = AdLength;

            ExpectedNumOfFrames = 8 * SpotDuration;
            //sort?



        }

        public int Carrier { get; set; } = 0;

        public int Region { get; set; } = 0;
        public int Platform { get; set; } = 0;

        public string Title { get; set; }
        public string Advertiser { get; set; }
        public string Brand { get; set; }
        public int StartSpotFrameIdx { get; set; }
        public int EndSpotFrameIdx { get; set; }
        private int ExpectedNumOfFrames { get; set; }

        public string FilmCode { get; set; }
        public string ComercialCode { get; set; }
        public string CampaignCode { get; set; }

        public string BrandIDX { get; set; }
        public int FilmCodeIdx { get; set; }
        public bool ManuallyAdded { get; set; }

        /// <summary>
        /// time and date of SpotMatchedStart in local time toa llow picking of coreect day
        /// </summary>
        public DateTime LocalTestDateTime { get; set; }


        /// <summary>
        /// Start time of Matched Sequence
        /// </summary>
        public TimeSpan SpotMatchedStart { get; set; }

        //TODO check we neeed this
        public string MatchedStartTimeStr { get; set; }

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
                //CalculatedStartStr = FormatedTime(_CalculatedStart);
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
                //CalculatedEndStr = FormatedTime(_CalculatedEnd);
            }
        }
        public TimeSpan Gap { get; set; }
        public string MatchedEndTimeStr { get; set; }

        /// <summary>
        /// Start time corrected to allow for frames missed from start of match
        /// </summary>
        public string CalculatedStartStr
        {
            get
            {
                return FormatedTime(_CalculatedStart); ;
            }
        }

        /// <summary>
        /// EndTime corrected to allow for frame missed from end of match
        /// </summary>
        public string CalculatedEndStr
        {
            get
            {
                return FormatedTime(_CalculatedEnd);
            }
        }


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


        public int AudioMatch { get; set; } = 0;

        public string OutputStr
        {
            get
            {
                //return AdName + "|" + Starttime + "|" + LengthTest + " Seq Qual:" + SequenceQuality.ToString("0.00") + "% hit info ";
                return FilmCode + "\t" + MatchedStartTimeStr + "\t" + LengthTest.ToString("0.000") + "\t" + SequenceQuality.ToString("0.00") + "\t" + BaseSequenceQuality.ToString("0.00");
            }
        }
        public string DebugShortOutputStr
        {
            get
            {
                return FilmCode + " " + MatchedStartTimeStr + "% Calc Len:" + LengthTest + " Seq Qual:" + SequenceQuality.ToString("0.00") + "% hit info ";
            }
        }


        //Channel	Date	GAP	Start time	End time	Programme Title
        public string QCOutputStr
        {
            get
            {//AdName|Offset| MatchQ%| Seq Qual%|Calc Len|Duration
             //ned to test for - gap.
                string GapString = Gap.ToString("hh\\:mm\\:ss");
                if (Gap.Ticks < 0)
                    GapString = "-" + GapString;

                return GapString + "\t" + CalculatedStartStr + "\t" + CalculatedEndStr + "\t" + MatchedStartTimeStr + "\t" + MatchedEndTimeStr + "\t" + FilmCode + "\t" + Advertiser + "\t" + Brand + "\t" + Title;
            }
        }

        public string QCToolOutputStr
        {
            get
            {
                return _CalculatedStart.ToString("dd'd 'hh'h 'mm'm'") + "\t" + CalculatedStartStr + "\t" + SpotDuration + "\t" + FilmCode + "\t" + Advertiser + "\t" + Brand + "\t" + Title;
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


        public double Robustness
        {
            get
            {
                double ans = BaseSequenceQuality / SequenceQuality;

                if (ans > 1.0)
                    ans = 1.0;

                return ans;
            }

        }

        // public string SourceFilePath { get; set; }


        public List<SpotFoundAd> OverlappedSpots
        {
            get;
            set;
        } = new List<SpotFoundAd>();

        private string FormatedTime(TimeSpan RoundedStart)
        {
            string timestr = string.Format("{0}:{1}:{2}", Math.Truncate(RoundedStart.TotalHours).ToString("00"), RoundedStart.Minutes.ToString("00"), RoundedStart.Seconds.ToString("00"));

            return timestr;
        }
        /// <summary>
        /// if true forces this spot to used over all others
        /// </summary>
        public bool PreferedSpot { get; set; } = false;


        /// <summary>
        /// if true forces this to be left as a gap in the schedule 
        /// </summary>
        public bool BlockSpot { get; set; } = false;
        public DateTime DateAdded { get; internal set; }
        public bool OnlyPromo { get; internal set; }
        public int ParentslotIdx { get; set; } = 0;
        public object HoldingCompany { get; internal set; }
        public object Agency { get; internal set; }
        public object TRPTitle { get; internal set; }
    }
}