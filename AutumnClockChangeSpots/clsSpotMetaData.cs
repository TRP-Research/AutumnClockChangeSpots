using System;
using System.Collections.Generic;

namespace AutumnClockChangeSpots
{
    public class clsSpotMetaData
    {
        //public string AgencyCode { get; set; }
        //public string BrandCode { get; set; }
        public string BrandIDX { get; set; }
        //public string FilmCodeDur { get; set; }

        public int ParentIDX { get; set; }
        //public string FilmCode { get; set; }

        private string _FilmCode { get; set; }
        public string FilmCode
        {
            get { return _FilmCode; }
            set
            {
                _FilmCode = value;
                //"DSSFBFB205030"
                //AgencyCode = _FilmCode.Substring(0, 3);
                //BrandCode = _FilmCode.Substring(3, 4);
                BrandIDX = _FilmCode.Substring(7, 3);
                //FilmCodeDur = _FilmCode.Substring(10, 3);
            }
        }


        public string Name { get; set; }

        public string Title { get; set; }
        public string Advertiser { get; set; }
        public string Brand { get; set; }

        public int KeyFrame { get; set; }

        public string SourceFile { get; set; }

        public int Duration { get; set; }

        public int OriginalOffset { get; set; }

        public bool SpotOrAd { get; set; }
        public bool OnlyPromo { get; set; }

        public string SourceFilePath { get; set; } = "";
        public DateTime DateAdded { get; internal set; }
        public DateTime ExpirytDate { get; internal set; }

        public int CampaignCodeidx { get; internal set; }

        public object HoldingCompany { get; internal set; }
        public object Agency { get; internal set; }
        public object TRPTitle { get; internal set; }

        public string CampaignCode
        {
            get
            {
                string ans = "TRP" + BrandPrefix + CampaignCodeidx.ToString();
                return ans;
            }
        }

        public string ComercialCode
        {
            get
            {
                string ans = "TRP" + BrandPrefix + ParentIDX.ToString();
                return ans;
            }
        }

        public string BrandPrefix
        {
            get
            {
                string[] splitstr = Brand.Split(' ');

                string ans = "";
                foreach (string word in splitstr)
                {
                    if (word.Trim() != "")
                    {
                        ans = ans + word.Trim().Substring(0, 1);
                    }
                }
                return ans;
            }
        }
        public List<clsHashData> AdHashRecords { get; set; } = new List<clsHashData>();
    }
}