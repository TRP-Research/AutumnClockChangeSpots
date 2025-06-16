using System;
using System.Collections.Generic;

namespace AutumnClockChangeSpots
{
    public class clsSpotTimeSlot
    {
        private TimeSpan _SlotStart;
        public TimeSpan SlotStart
        {
            get => _SlotStart;
            set
            {
                _SlotStart = value;
                if (SlotEnd != null && SlotEnd > value)
                    MatchDuration = (int)(SlotEnd - value).TotalSeconds;
            }
        }

        private TimeSpan _SlotEnd;
        public TimeSpan SlotEnd
        {
            get => _SlotEnd;
            set
            {
                _SlotEnd = value;
                if (SlotStart != null && SlotStart < value)
                    MatchDuration = (int)(value - SlotStart).TotalSeconds;
            }
        }

        public int MatchDuration { get; set; }

        public TimeSpan Gap { get; set; }   //why?    

        public int SlotDuration { get; set; } = 0;
        public ReadSpotFoundAd SelectedSpot { get; set; }


        

        public List<ReadSpotFoundAd> OverlappedSpots
        {
            get;
            set;
        } = new List<ReadSpotFoundAd>();

       
    }
}