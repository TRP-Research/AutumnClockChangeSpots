using System;
using System.Collections.Generic;

namespace AutumnClockChangeSpots
{
    public class clsSpotTimeSlot
    {
        public string carrier { get; set; }
        public string region { get; set; }
        public string platform { get; set; }
        private TimeSpan _SlotStart;
        public TimeSpan SlotStart
        {
            get => _SlotStart;
            set
            {
                _SlotStart = value;


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
        public int SlotDuration { get; set; } = 0;

        public int SlotIdx { get; set; } = 0;

        public bool Deleted { get; set; } = false;
        public int MatchDuration { get; set; }

    }
}