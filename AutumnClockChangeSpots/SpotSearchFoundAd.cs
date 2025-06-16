using System;
using System.Collections.Generic;
using System.Linq;

namespace AutumnClockChangeSpots
{
    public class SpotSearchFoundAd
    {
        /// <summary>
        ///  frame rate to use frame numbers
        /// </summary>
        private const int CONST_FrameRate = 8;

        //tries to make list of matches into  start and end time
        //CalculatedStart----SpotMatchedStart-----SpotMatchedEnd-------CalculatedEnd
        public SpotSearchFoundAd(List<OffsetSpotAndMatchFrameNum> lst, int AdLength)
        {
            SpotDuration = AdLength;

            ExpectedNumOfFrames = CONST_FrameRate * SpotDuration;

            AllAdtimes = lst;

            SpotMatchedStart = TimeSpan.FromMilliseconds(AllAdtimes.OrderBy(o => o.MatchFrameNum).First().MatchFrameNum * 125);
            SpotMatchedEnd = TimeSpan.FromMilliseconds(AllAdtimes.OrderBy(o => o.MatchFrameNum).Last().MatchFrameNum * 125);

            //sort?
            StartSpotFrameNumber = AllAdtimes.OrderBy(o => o.SpotFrameNum).First().SpotFrameNum;

            //this can be less than max but endtime can be correct
            EndSpotFrameNumber = AllAdtimes.OrderBy(o => o.SpotFrameNum).Last().SpotFrameNum;

            int MissingNumEndFrames = ExpectedNumOfFrames - EndSpotFrameNumber;

            CalculatedStart = TimeSpan.FromMilliseconds(SpotMatchedStart.TotalMilliseconds - 125 * (StartSpotFrameNumber - 1));

            TimeSpan NotionalEnd = TimeSpan.FromMilliseconds(CalculatedStart.TotalMilliseconds + (125 * ExpectedNumOfFrames));

            CalculatedEnd = TimeSpan.FromMilliseconds(SpotMatchedEnd.TotalMilliseconds + 125 * MissingNumEndFrames);

            //limit Calc end to max ligitimate 
            if (CalculatedEnd > NotionalEnd)
                CalculatedEnd = NotionalEnd;

            RoundedStart = TimeSpan.FromMilliseconds(500 + CalculatedStart.TotalMilliseconds);
            RoundedEnd = TimeSpan.FromMilliseconds(500 + CalculatedEnd.TotalMilliseconds);
        }


        public int StartSpotFrameNumber { get; set; }
        public int EndSpotFrameNumber { get; set; }

        private int ExpectedNumOfFrames { get; set; }

        public string FilmCode { get; set; }

        public int FilmCodeIdx { get; set; }
        /// <summary>
        /// Start time of Matched Sequence
        /// </summary>
        public TimeSpan SpotMatchedStart { get; set; }

        /// <summary>
        /// Endtime of matched Sequence
        /// </summary>
        public TimeSpan SpotMatchedEnd { get; set; }

        /// <summary>
        /// Starttime based on the matched frames idx
        /// </summary>
        public TimeSpan CalculatedStart { get; set; }

        /// <summary>
        /// endtime adjusted based on the matched frames idx
        /// </summary>
        public TimeSpan CalculatedEnd { get; set; }




        /// <summary>
        /// Calculated start correctly rounded for fractional seconds
        /// </summary>
        public TimeSpan RoundedStart { get; set; }

        /// <summary>
        /// Calculated end correctly rounded for fractional seconds
        /// </summary>
        public TimeSpan RoundedEnd { get; set; }

        /// <summary>
        /// Ideal duration of Spot
        /// </summary>
        public int SpotDuration { get; set; } = 0;

        public List<OffsetSpotAndMatchFrameNum> AllAdtimes { get; set; } = new List<OffsetSpotAndMatchFrameNum>();

        //len in seconds
        public double LengthTest
        {
            get
            {
                var AdTimes = from cap in AllAdtimes select cap.MatchFrameNum;
                double FirstAd = AdTimes.OrderBy(o => o).First();
                double LastAAd = AdTimes.OrderBy(o => o).Last();

                double FoundLength = (LastAAd - FirstAd) / CONST_FrameRate;//num of fps
                //test is 50% of length
                return FoundLength;
            }
        }


        public double SequenceQuality
        {
            get
            {
                int IdealFrames = SpotDuration * CONST_FrameRate;

                double ans = 100.0;

                var uniqueMatchTimes = (from time in AllAdtimes select time.MatchFrameNum).Distinct();
                if (uniqueMatchTimes.Count() < IdealFrames)
                    ans = 100.0 * uniqueMatchTimes.Count() / IdealFrames;

                return ans;
            }
        }

        public double BaseSequenceQuality
        {
            get
            {
                int IdealFrames = SpotDuration * CONST_FrameRate;

                double ans = 100.0;

                List<int> BaseFrameNumbers = (from OandM in AllAdtimes select OandM.SpotFrameNum).Distinct().ToList();

                // if (IdealFrames < BaseFrameNumbers.Count)
                ans = 100.0 * BaseFrameNumbers.Count / IdealFrames;

                return ans;
            }
        }
    }
}