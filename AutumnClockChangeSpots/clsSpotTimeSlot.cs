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
        public int Duration { get; set; } = 0;

        public int masterspot { get; set; } = 0;

        public bool delete { get; set; } = false;
    }
}