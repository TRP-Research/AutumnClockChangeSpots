using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using DevExpress.Data.Helpers;
using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Native;
using DevExpress.XtraPrinting.Native.WebClientUIControl;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutumnClockChangeSpots
{
    public partial class frmAutumnClockChange : Form
    {
        #region"Spot search parameters"
        /// <summary>
        /// sets the hamming distance test size for hashes 6/64 = 9.4%
        /// </summary>
        private const int CONST_HashSearchLevel = 7;

        /// <summary>
        ///  frame rate to use frame numbers
        /// </summary>
        private const int CONST_FrameRate = 8;

        /// <summary>
        /// sets min percent sequence quality for match to be considered an ad
        /// </summary>
        private const Double CONST_QualityFilterPercent = 16;

        /// <summary>
        /// sets min percent length for match to be considered an ad
        /// </summary>
        private const Double CONST_LengthFilterPercent = 55.0;

        /// <summary>
        /// ablsolute min length for ad
        /// </summary>
        private const Double CONST_MinAdLength = 3.0;

        /// <summary>
        /// sets maximum gap allowed in continious sequence testing   //multiply by frame rate to use frame numbers
        /// </summary>
        private const int CONST_AllowedGapSeconds = 4 * CONST_FrameRate;

        /// <summary>
        /// in memory B tree for k-nearest neighbour testing
        /// </summary>
        private static BiTree BiNodeRootLR = new BiTree();
        #endregion


        #region"AWS related data"

        private const string PostgresUserName = "postgres";
        //private const string BBPostgresUserName = "BBPostgres";
        private const string BBPostgresUserName = "BBProdHelixPostgres";

        /// <summary>
        /// used to get db password/coection via secrets mananger
        /// </summary>
        private const string STR_DB_Secret_Name = "helix-db";

        public static Dictionary<string, string> DBChannelKey = new Dictionary<string, string>();

        public static string CurrentTableName = "";

        private bool CheckingLoaded = false;

        //read the json confog file for channel metadata, all log station codes and DB1 codes are now set here
        private const string AWS_TheConfigfileBucketName = "lqclips-bb-settings-prod";

        //needed to read credentials
        private const string UserNameForConfig = "BB_Prod_TVClips_Manage";

        #endregion

        private DateTime AutumnChangeDate;

        /// <summary>
        /// root path to the Dir for all the json data files
        /// </summary>
        private static string AutumnTopLevelDir = @"X:\Clients\Collaborative Promo Logging\Spot Logs\FullResultReports\AutumnClockChange\";


        /// <summary>
        /// list of channel information, now read from json file
        /// </summary>
        private BindingList<clsChannelData> AllCarriers = new BindingList<clsChannelData>();

        /// <summary>
        ///  load all days frame data to prevent repeated DB reads
        /// </summary>
        private static Dictionary<long, List<int>> DaysFrameData = new Dictionary<long, List<int>>();
        private static Dictionary<long, List<SpotsData>> SpotsFrameData = new Dictionary<long, List<SpotsData>>();


        /// <summary>
        /// all the Spot metadata
        /// </summary>
        //private List<clsSpotMetaData> AllSpotsData = new List<clsSpotMetaData>();
        private static Dictionary<int, clsSpotMetaData> AllSpotsData = new Dictionary<int, clsSpotMetaData>();

        public frmAutumnClockChange()
        {
            InitializeComponent();
        }

        private void frmAutumnClockChange_Load(object sender, EventArgs e)
        {
            dtpYear.Format = DateTimePickerFormat.Custom;
            dtpYear.CustomFormat = "yyyy";
            dtpYear.ShowUpDown = true;

            dtpYear.Value = DateTime.Now;

            //Annual End: The Last Sunday of October at 2:00 AM
            UpdateAutumnChangeInfo(dtpYear.Value.Year);

            //TODO remove this date is only for testing
            AutumnChangeDate = new DateTime(dtpYear.Value.Year, 6, 14); 

            try
            {
                Cursor = Cursors.WaitCursor;



                //uses config file from AWS Bucket
                LoadChannelData();

                LoadAllSpotsAndHashData();

                Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                Cursor = Cursors.Default;
                // throw;
            }
            finally
            {
                Cursor = Cursors.Default;
            }

        }

        private void LoadAllSpotsAndHashData()
        {
            using (NpgsqlConnection connection = ReturnBBConnection())
            {
                connection.Open();
                //updated to get latest fields from clearcase metadata
                //get spot library data, this is used to support the compare process for loading frames
                var sql = @"select a.idx,a.filmcode,COALESCE(m.advertisername,a.advertiser) as Ad,COALESCE(m.brand,a.brand) as brand ,COALESCE(m.commercialtitle, a.commercialtitle,'') as title ,
sourcefile,duration,originaloffset,spotoradd, COALESCE(min(F.originalfileframe),0) as framenum, dateadded, expirydate, COALESCE(c.campaigncode, 0) as CCode, onlypromo ,COALESCE(m.holdingcompanyname,'') as HoldingCompany,COALESCE(m.buyingagencyname,'') as Agency

from tbl_spotad_recorddata a
left join public.tbl_adspot_frames f on F.parentad = a.idx
left join public.lut_trp_campaign_codes c on c.filmcodegroup = substring(a.filmcode,0,8)
left join public.tbl_clearcase_metadata m on m.filmcode = a.filmcode and M.matchgroup = a.matchgroup
group by a.idx,a.filmcode,m.advertisername,a.advertiser,m.brand,a.brand,m.commercialtitle, a.commercialtitle,sourcefile,duration,originaloffset,spotoradd,c.campaigncode, onlypromo,m.holdingcompanyname,m.buyingagencyname
       ";
                using (var cmd = new NpgsqlCommand(sql, connection))
                {
                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())

                        // Output rows
                        while (rdr.Read())
                        {
                            clsSpotMetaData MyData = new clsSpotMetaData();
                            MyData.ParentIDX = rdr.GetInt32(0);
                            MyData.FilmCode = rdr.GetString(1);
                            MyData.Advertiser = rdr.GetString(2);
                            MyData.Brand = rdr.GetString(3);
                            MyData.Title = rdr.GetString(4);
                            MyData.KeyFrame = 0; ;// rdr.Field<int>("KeyFrame");
                            MyData.SourceFile = rdr.GetString(5); ;
                            MyData.Duration = rdr.GetInt32(6);
                            MyData.OriginalOffset = rdr.GetInt32(9);
                            MyData.SpotOrAd = rdr.GetBoolean(8);
                            MyData.DateAdded = rdr.GetDateTime(10);
                            MyData.ExpirytDate = rdr.GetDateTime(11);
                            MyData.OnlyPromo = rdr.GetBoolean(13);

                            //new records 
                            MyData.HoldingCompany = rdr.GetString(14);
                            MyData.Agency = rdr.GetString(15);
                            MyData.TRPTitle = "";

                            MyData.CampaignCodeidx = rdr.GetInt32(12);

                            AllSpotsData.Add(MyData.ParentIDX, MyData);
                        }
                }

                ///get spots master video files for use in compare
                string SourceFilePath = @"X:\Clients\Collaborative Promo Logging\Spot Logs\AutoClippedfiles\";

                string[] allpaths = Directory.GetFiles(SourceFilePath, "*.mp4", SearchOption.TopDirectoryOnly);

                Dictionary<string, string> FilmcodePaths = new Dictionary<string, string>();

                foreach (string path in allpaths)
                {
                    string[] splitstr = Path.GetFileNameWithoutExtension(path).Split('_');

                    if (!FilmcodePaths.ContainsKey(splitstr[0]))
                    {
                        FilmcodePaths.Add(splitstr[0], path);
                    }
                    else
                    {
                        if (splitstr.Last() != "999")
                            FilmcodePaths[splitstr[0]] = path;
                    }
                }

                foreach (clsSpotMetaData spot in AllSpotsData.Values)
                {
                    if (FilmcodePaths.ContainsKey(spot.FilmCode))
                    {
                        spot.SourceFilePath = FilmcodePaths[spot.FilmCode];
                    }
                }
            }
            LoadAllSpotsHash(AutumnChangeDate.AddDays(-1).ToString("yyyy-MM-dd"));
        }

        private void UpdateAutumnChangeInfo(int Year)
        {
            AutumnChangeDate = GetLastWeekdayOfMonth(new DateTime(Year, 10, 1), DayOfWeek.Sunday);
            //need to work out reportdate a this can only be run after the autumn change date

            DateTime AutumnReportDate = AutumnChangeDate.AddDays(-1); //report date is the day before the change

            //TODO Testing remove
            if (true)//(DateTime.Now > AutumnReportDate)
            {
                REportDateInfoLabel.Text = String.Format("For the year {0} autumn clock change is on the {1}", dtpYear.Value.Year, AutumnChangeDate.ToShortDateString());
                btnRunAll.Enabled = true;
                btnRunChannel.Enabled = true;
            }
            else
            {
                REportDateInfoLabel.Text = String.Format("This report can only be run in the week after the report date {0}", AutumnReportDate.ToShortDateString());
                btnRunAll.Enabled = false;
                btnRunChannel.Enabled = false;
            }
        }

        /// <summary>
        /// gets the last day in in the month
        /// </summary>
        /// <param name="date"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        private static DateTime GetLastWeekdayOfMonth(DateTime date, DayOfWeek day)
        {
            DateTime lastDayOfMonth = new DateTime(date.Year, date.Month, 1)
                .AddMonths(1).AddDays(-1);
            int wantedDay = (int)day;
            int lastDay = (int)lastDayOfMonth.DayOfWeek;
            return lastDayOfMonth.AddDays(
                lastDay >= wantedDay ? wantedDay - lastDay : wantedDay - lastDay - 7);
        }

        private void dtpYear_ValueChanged(object sender, EventArgs e)
        {
            UpdateAutumnChangeInfo(dtpYear.Value.Year);
        }

        /// <summary>
        /// object to load carrier data from json config file in S3 bucket
        /// </summary>
        private async void LoadChannelData()
        {
            string fullpath = "ChannelList.json";
            AllCarriers = await GetFileFromS3Async(AWS_TheConfigfileBucketName, fullpath);

            var FilteredCarriers = AllCarriers.Where(o => o.HasSpots == true);
            lueChannel.Properties.DataSource = FilteredCarriers;
            lueChannel.ItemIndex = 0;
        }

        /// <summary>
        /// taaen from https://docs.aws.amazon.com/AmazonS3/latest/dev/RetrievingObjectUsingNetSDK.html
        /// task to get file from s3 as channel data list
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public async Task<BindingList<clsChannelData>> GetFileFromS3Async(string bucketName, string keyName)
        {
            BindingList<clsChannelData> ans = new BindingList<clsChannelData>();
            try
            {
                var chain = new CredentialProfileStoreChain();
                AWSCredentials awsCredentials;
                if (chain.TryGetAWSCredentials(UserNameForConfig, out awsCredentials))
                    using (AmazonS3Client client = new AmazonS3Client(awsCredentials, Amazon.RegionEndpoint.EUWest2))
                    {
                        var request = new GetObjectRequest
                        {
                            BucketName = bucketName,
                            Key = keyName
                        };
                        var memoryStream = new MemoryStream();
                        using (var response = await client.GetObjectAsync(request))
                        {
                            using (var responseStream = response.ResponseStream)
                            {

                                await responseStream.CopyToAsync(memoryStream);
                                memoryStream.Position = 0; // Reset the memory stream position for subsequent reads
                                var S3Str = new StreamReader(memoryStream).ReadToEnd();
                                ans = JsonConvert.DeserializeObject<BindingList<clsChannelData>>(S3Str);
                            }
                        }
                    }
                return ans;
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine($"Error encountered on server. Message:'{e.Message}' when reading an object");
                throw;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unknown encountered on server. Message:'{e.Message}' when reading an object");
                throw;
            }
        }

        private void lueChannel_Closed(object sender, DevExpress.XtraEditors.Controls.ClosedEventArgs e)
        {

        }

        private void btnRunChannel_Click(object sender, EventArgs e)
        {
            //TODO@@
            progressBar1.Value = 0;
            progressBar1.Maximum = 1;

            clsChannelData CarrierData = (clsChannelData)lueChannel.GetSelectedDataRow();

            ReadExtraHourChannelData(CarrierData.Carrier.ToString(), CarrierData.Region.ToString(), CarrierData.Platform.ToString(), AutumnChangeDate.AddDays(-1).ToString("yyyy-MM-dd"));

            FindAllAds(CarrierData.Carrier.ToString(), CarrierData.Region.ToString(), CarrierData.Platform.ToString(), AutumnChangeDate.AddDays(-1).ToString("yyyy-MM-dd"));
                      

            progressBar1.Value = 1;
            ReportAllChannelsWithTimeSlots();
        }

        private static void ReadExtraHourChannelData(string carrier, string region, string platform, string dateStr)
        {
            try
            {
                string expectedFormat = "yyyy-MM-dd";
                DateTime theDate;
                bool result = DateTime.TryParseExact(
                    dateStr,
                    expectedFormat,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out theDate);
                DaysFrameData.Clear();
                //test times to filter out the 6am - 2am times
                //this is too slow to run on the DB so fetch full days and filter import
                //TimeSpan SixAM = new TimeSpan(6, 0, 0);
                TimeSpan TwoAM = new TimeSpan(2, 0, 0);
                TimeSpan OneAM = new TimeSpan(1, 0, 0);
                TimeSpan OneDay = new TimeSpan(24, 0, 0);

                using (NpgsqlConnection connection = ReturnBBConnection())
                {
                    connection.Open();

                     string NextDayQuery = "select recorddate,frametime,  longhash from tbl_carrier_hash_timestamps where recorddate = '" + theDate.AddDays(1).ToString("yyyy-MM-dd") + "' and carrier = " + carrier + " and Region = " + region + " and source = " + platform;

                    //load next dash hash data to select the 1-2 hour to use as 27th hr
                    using (var cmd = new NpgsqlCommand(NextDayQuery, connection))
                    {
                        using (NpgsqlDataReader NextDayReader = cmd.ExecuteReader())
                        {
                            while (NextDayReader.Read())
                            {
                                Int64 lHash = NextDayReader.GetInt64(2);

                                TimeSpan TimeStr = NextDayReader.GetTimeSpan(1);
                                DateTime RecordDate = NextDayReader.GetDateTime(0);

                                //TODO consider getting n seconds before 1 to allow for overlaps, tweak 1 am time to TimeSpan(0, 57, 0);/ allow 3 mins?
                                if (TimeStr >= OneAM && TimeStr <= TwoAM)
                                {
                                    string LongHash = Convert.ToString(lHash, 2).PadLeft(64, '0');
                                    TimeStr = TimeStr.Add(OneDay);//to code with 0 to two an
                                    int DayFrame = (int)(CONST_FrameRate * TimeStr.TotalSeconds);

                                    if (!DaysFrameData.ContainsKey(lHash))
                                    {
                                        //add all frame times to dictionary of hash values
                                        List<int> MyLst = new List<int>();
                                        MyLst.Add(DayFrame);
                                        DaysFrameData.Add(lHash, MyLst);
                                    }
                                    else
                                    {
                                        DaysFrameData[lHash].Add(DayFrame);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static void FindAllAds(string carrier, string region, string platform, string dateStr)
        {
            
            ConcurrentBag<SpotSearchFoundAd> AllSpotDaysMatches = new ConcurrentBag<SpotSearchFoundAd>();

            ConcurrentBag<OffsetSpotAndMatchFrameNum> AllSpotsAndFrames = new ConcurrentBag<OffsetSpotAndMatchFrameNum>();

            //invert to run spots against day
            Parallel.ForEach(DaysFrameData.Keys, key =>
            //foreach (Int64 key in DaysFrameData.Keys)
            {
                string LongHash = Convert.ToString(key, 2).PadLeft(64, '0');
                HashSet<SpotsData> results = GetAllFakeMatches(LongHash);

                foreach (SpotsData stamp in results)
                {
                    foreach (int frametime in DaysFrameData[key])
                    {
                        //clsSimpleResultTime time = new clsSimpleResultTime(frametime);
                        OffsetSpotAndMatchFrameNum MyMatch = new OffsetSpotAndMatchFrameNum();
                        MyMatch.SpotFrameNum = stamp.SpotFrameNum;
                        MyMatch.MatchFrameNum = frametime;
                        MyMatch.ParentIDX = stamp.ParentAd;

                        AllSpotsAndFrames.Add(MyMatch);
                    }
                }
            }
            //
             );


            //try filling dict here as something was stopping following section from parallleling
            Dictionary<int, List<OffsetSpotAndMatchFrameNum>> TimesGroupedBySpot = new Dictionary<int, List<OffsetSpotAndMatchFrameNum>>();

            List<OffsetSpotAndMatchFrameNum> uniqueMatches = AllSpotsAndFrames.ToList();

            foreach (OffsetSpotAndMatchFrameNum item in uniqueMatches)
            {
                if (!TimesGroupedBySpot.ContainsKey(item.ParentIDX))
                {
                    List<OffsetSpotAndMatchFrameNum> MyLst = new List<OffsetSpotAndMatchFrameNum>();
                    MyLst.Add(item);
                    TimesGroupedBySpot.Add(item.ParentIDX, MyLst);
                }
                else
                {
                    TimesGroupedBySpot[item.ParentIDX].Add(item);
                }
            }

            Parallel.ForEach(AllSpotsData.Values, Spot =>
            //foreach (clsSpotMetaData Spot in AllSpotsData)
            {
                if (TimesGroupedBySpot.ContainsKey(Spot.ParentIDX))
                {
                    var Allspans = TimesGroupedBySpot[Spot.ParentIDX];
                    List<List<OffsetSpotAndMatchFrameNum>> AllResults = RunSpotOffsetMatchIslandGaps(Allspans.OrderBy(o => o.MatchFrameNum).ToList());

                    foreach (List<OffsetSpotAndMatchFrameNum> timelst in AllResults)
                    {
                        SpotSearchFoundAd NewAd = new SpotSearchFoundAd(timelst, Spot.Duration);
                        NewAd.FilmCode = Spot.FilmCode;
                        NewAd.FilmCodeIdx = Spot.ParentIDX;
                        if (NewAd.SequenceQuality > CONST_QualityFilterPercent && NewAd.LengthTest >= CONST_MinAdLength)
                        {
                            AllSpotDaysMatches.Add(NewAd);
                        }
                    }
                }
            }
             );

            List<SpotSearchFoundAd> SpotsAdsFromBag = AllSpotDaysMatches.ToList();

            //TODO replace this with op to json 
            if (SpotsAdsFromBag.Count > 0)
            {
                String opDir = AutumnTopLevelDir + @"Foundads\";

                List<SpotFoundAd> OPList = new List<SpotFoundAd>();
                foreach (SpotSearchFoundAd item in SpotsAdsFromBag)
                {
                    SpotFoundAd MyAd = new SpotFoundAd(item.SpotDuration);
                    MyAd.Carrier = int.Parse(carrier);
                    MyAd.Region = int.Parse(region);
                    MyAd.Platform = int.Parse(platform);
                    MyAd.Title = "";
                    MyAd.Advertiser = "";
                    MyAd.Brand = "";
                    MyAd.FilmCodeIdx = item.FilmCodeIdx;
                    MyAd.FilmCode = item.FilmCode;
                    MyAd.SpotMatchedStart = item.SpotMatchedStart;
                    MyAd.SpotMatchedEnd = item.SpotMatchedEnd;
                    MyAd.CalculatedStart = item.CalculatedStart;
                    MyAd.CalculatedEnd = item.CalculatedEnd;
                    MyAd.SequenceQuality = item.SequenceQuality;
                    MyAd.BaseSequenceQuality = item.BaseSequenceQuality;
                    MyAd.StartSpotFrameIdx = item.StartSpotFrameNumber;
                    MyAd.EndSpotFrameIdx = item.EndSpotFrameNumber;
                    OPList.Add(MyAd);
                }

                if (!Directory.Exists(opDir))
                {
                    Directory.CreateDirectory(opDir);
                }

                string filename = String.Format("FoundAds_{0}_{1}_{2}_{3}.json", carrier, region, platform, dateStr);

                var json = JsonConvert.SerializeObject(OPList);
                File.WriteAllText(Path.Combine(opDir, filename), json);

                //now opspots 
                OPTimeSlotJson(carrier, region, platform, dateStr, OPList, opDir);
            }
        }
        private void LoadAllSpotsHash(string dateStr)
        {
            BiNodeRootLR = new BiTree();
            try
            {
                using (NpgsqlConnection connection = ReturnBBConnection())
                {
                    connection.Open();

                    bool loaded = false;
                    int loadcount = 0;
                    //get spot data

                    //Bitree of spots 9 gb an 5 mins to load so no

                    //get hash data
                    var HashSql = "select * from tbl_adspot_frames";
                    using (var HashCmd = new NpgsqlCommand(HashSql, connection))
                    {
                        using (NpgsqlDataReader HashRdr = HashCmd.ExecuteReader())
                        {
                            // Output rows
                            while (HashRdr.Read())
                            {
                                clsHashData MyData = new clsHashData();
                                MyData.ParentIDX = HashRdr.GetInt32(1);
                                MyData.hash = HashRdr.GetInt64(2);
                                MyData.frametime = HashRdr.GetDecimal(3);

                                if (AllSpotsData.Keys.Contains(MyData.ParentIDX))
                                {
                                    //AllSpotsData[MyData.ParentIDX].AdHashRecords.Add(MyData);
                                    if (!SpotsFrameData.ContainsKey(MyData.hash))
                                    {
                                        string LongHash = Convert.ToString(MyData.hash, 2).PadLeft(64, '0');
                                        BiNodeRootLR.HashInsert(LongHash, MyData.hash);
                                        List<SpotsData> MyLst = new List<SpotsData>();
                                        SpotsData MySpot = new SpotsData();
                                        MySpot.ParentAd = MyData.ParentIDX;
                                        MySpot.SpotFrameNum = (int)(CONST_FrameRate * (double)MyData.frametime);
                                        MyLst.Add(MySpot);
                                        SpotsFrameData.Add(MyData.hash, MyLst);
                                    }
                                    else
                                    {
                                        SpotsData MySpot = new SpotsData();
                                        MySpot.ParentAd = MyData.ParentIDX;
                                        MySpot.SpotFrameNum = (int)(CONST_FrameRate * (double)MyData.frametime);
                                        SpotsFrameData[MyData.hash].Add(MySpot);
                                    }
                                    
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                // throw;
            }
            finally
            {

            }
        }

        private static void OPTimeSlotJson(string carrier, string region, string platform, string dateStr, List<SpotFoundAd> OPList, string opDir)
        {
            List<ReadSpotFoundAd> allfoundads = new List<ReadSpotFoundAd>();
            foreach (SpotFoundAd foundAd in OPList)
            {
                ReadSpotFoundAd MyAd = new ReadSpotFoundAd(foundAd.SpotDuration);
                //MyAd.Title = Spot.Title;
                //MyAd.Advertiser = Spot.Advertiser;
                //MyAd.Brand = Spot.Brand;
                MyAd.FilmCodeIdx = foundAd.FilmCodeIdx;
                MyAd.FilmCode = foundAd.FilmCode;
                MyAd.SpotMatchedStart = foundAd.SpotMatchedStart;
                MyAd.SpotMatchedEnd = foundAd.SpotMatchedEnd;
                MyAd.CalculatedStart = foundAd.CalculatedStart;
                MyAd.CalculatedEnd = foundAd.CalculatedEnd;
                MyAd.SequenceQuality = foundAd.SequenceQuality;
                MyAd.BaseSequenceQuality = foundAd.BaseSequenceQuality;
                MyAd.StartSpotFrameIdx = foundAd.StartSpotFrameIdx;
                MyAd.EndSpotFrameIdx = foundAd.EndSpotFrameIdx;

                //tweak data for display on 2-2 clock
                //if (MyAd.SpotMatchedStart < TwoAM)
                //    MyAd.SpotMatchedStart = MyAd.SpotMatchedStart.Add(OneDay);

                //if (MyAd.SpotMatchedEnd < TwoAM)
                //    MyAd.SpotMatchedEnd = MyAd.SpotMatchedEnd.Add(OneDay);

                //if (MyAd.CalculatedStart < TwoAM)
                //    MyAd.CalculatedStart = MyAd.CalculatedStart.Add(OneDay);

                //if (MyAd.CalculatedEnd < TwoAM)
                //    MyAd.CalculatedEnd = MyAd.CalculatedEnd.Add(OneDay);

                allfoundads.Add(MyAd);
            }

            //run through overlapping                    
            var filteredads = from ad in allfoundads where ad.SequenceQuality > CONST_QualityFilterPercent && ad.LenPercentTest >= CONST_LengthFilterPercent select ad;

            // this overlength test may be fragile or better frame selection will correct it
            //remove all overlong matches andd sort by Ad.duration reduce to 1.3 from 1.5
            List<ReadSpotFoundAd> LengthFiteredOrderedDayLst = filteredads.Where(o => o.LengthTest < 1.3 * o.SpotDuration && o.BaseSequenceQuality >= 5.0).OrderByDescending(o => o.SpotDuration).ToList();

            List<TimeAndGap> AdGaps = new List<TimeAndGap>();

            //TODO make a new global list to be edited

            FindAllOverlappingSpotsOAndMAds(LengthFiteredOrderedDayLst, out LengthFiteredOrderedDayLst);

            //list of directly selected ads
            List<ReadSpotFoundAd> FiteredOrderedDayLst = LengthFiteredOrderedDayLst.Where(o => o.Overlapped == false).ToList();

            List<ReadSpotFoundAd> finallst = (from spot in LengthFiteredOrderedDayLst where spot.Overlapped == false select spot).ToList();

            foreach (ReadSpotFoundAd spot in finallst)
            {
                var overlaped = from overlap in LengthFiteredOrderedDayLst where overlap.Overlapped == true && Math.Abs(spot.CalculatedStart.TotalSeconds - overlap.CalculatedStart.TotalSeconds) < 3 select overlap;
                if (overlaped.Count() > 0)
                    spot.OverlappedSpots = overlaped.ToList();
            }

            List<clsSpotTimeSlot> TimeSlotLst = new List<clsSpotTimeSlot>();


            foreach (ReadSpotFoundAd spot in finallst)
            {
                clsSpotTimeSlot timeslot = new clsSpotTimeSlot();
                timeslot.carrier = carrier;
                timeslot.region = region;
                timeslot.platform = platform;
                timeslot.SlotIdx = spot.FilmCodeIdx;


                timeslot.SlotStart = spot.CalculatedStart;
                timeslot.SlotEnd = spot.CalculatedEnd;

                timeslot.SlotDuration = spot.SpotDuration;

                TimeSlotLst.Add(timeslot);
            }

            //(broadcastdate, carrier, region, slotstart, duration, masterspot
            string filename = String.Format("TimeSlots_{0}_{1}_{2}_{3}.json", carrier, region, platform, dateStr);
            if (TimeSlotLst.Count() > 0)
            {
                var json = JsonConvert.SerializeObject(TimeSlotLst);
                File.WriteAllText(Path.Combine(opDir, filename), json);
            }
        }

        private static NpgsqlConnection ReturnBBConnection()
        {//curent logger
            string connectionString = GetDBConnectionString(STR_DB_Secret_Name);
            return new NpgsqlConnection(connectionString);
        }
        private static string GetDBConnectionString(string secretName)
        {

            string connectionString = "";
            var chain = new CredentialProfileStoreChain();
            AWSCredentials awsCredentials;
            if (chain.TryGetAWSCredentials(BBPostgresUserName, out awsCredentials))
            {
                IAmazonSecretsManager client = new AmazonSecretsManagerClient(awsCredentials, Amazon.RegionEndpoint.EUWest2);
                GetSecretValueRequest request = new GetSecretValueRequest
                {
                    SecretId = secretName
                };
                GetSecretValueResponse response = null;

                try
                {
                    response = client.GetSecretValueAsync(request).Result;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return connectionString;
                }

                if (response.SecretString != null)
                {
                    dynamic objects = JObject.Parse(response.SecretString);
                    connectionString = string.Format("Host={0};Port={1};Database={2};User ID={3};Password={4};CommandTimeout=120",
                   objects["host"], objects["port"], objects["dbname"], objects["username"], objects["password"]);

                    return connectionString;
                }
                else
                {
                    return connectionString;
                }

            }
            return connectionString;
        }

        /// <summary>
        /// update to rreturn timespan
        /// </summary>
        /// <param name="BaseHash"></param>
        /// <param name="Carrier"></param>
        /// <returns></returns>
        private static HashSet<SpotsData> GetAllFakeMatches(string BaseHash)
        {
            ConcurrentBag<SpotsData> answer = new ConcurrentBag<SpotsData>();

            HashSet<Int64> ans = SearchLRBITree(BaseHash, CONST_HashSearchLevel);

            if (ans.Count > 0)
            {
                Parallel.ForEach(ans, item =>
                {//now returns times
                    //if(SpotsFrameData.ContainsKey(item))
                    foreach (SpotsData spotdata in SpotsFrameData[item])
                    {
                        answer.Add(spotdata);
                    }
                }
                );
            }
            return new HashSet<SpotsData>(answer.ToList());
        }

        private static HashSet<long> SearchLRBITree(string word, int Maxcost)
        {
            HashSet<long> Results = new HashSet<long>();

            int CurrentDiff = 0;

            char FirstLetter = word[0];

            //if (FirstLetter == '1')
            {
                searchRecursiveLRBiNode(BiNodeRootLR.IsTrue, word, 0, CurrentDiff, Results, Maxcost);
            }
            //else
            {
                searchRecursiveLRBiNode(BiNodeRootLR.IsFalse, word, 0, CurrentDiff, Results, Maxcost);
            }
            return Results;
        }

        /// <summary>
        /// search btree and return list of hashes within hamming distance of MaxCost
        /// </summary>
        /// <param name="BiNode"></param>
        /// <param name="word"></param>
        /// <param name="WordIDX"></param>
        /// <param name="priorDiff"></param>
        /// <param name="results"></param>
        /// <param name="maxCost"></param>
        private static void searchRecursiveLRBiNode(BiTree BiNode, string word, int WordIDX, int priorDiff, HashSet<long> results, int maxCost)
        {
            int mike = 1;
            int CurrentDiff = priorDiff;

            char NextLetter = word[WordIDX];

            if (BiNode.NodeValue != NextLetter)
                CurrentDiff = CurrentDiff + 1;

            //if the last entry in the row indicates the optimal cost is less than the          
            if (CurrentDiff <= maxCost && WordIDX == word.Length - 1)
            {
                results.Add(BiNode.LongHash);
            }

            if (CurrentDiff <= maxCost && WordIDX < word.Length)
            {
                if (BiNode.IsTrue != null)
                    searchRecursiveLRBiNode(BiNode.IsTrue, word, WordIDX + 1, CurrentDiff, results, maxCost);

                if (BiNode.IsFalse != null)
                    searchRecursiveLRBiNode(BiNode.IsFalse, word, WordIDX + 1, CurrentDiff, results, maxCost);
            }
        }

        private static List<List<OffsetSpotAndMatchFrameNum>> RunSpotOffsetMatchIslandGaps(List<OffsetSpotAndMatchFrameNum> Allspans)
        {

            //in new system issue when several spot frames all call in the last frame number this duplicates the last working list
            List<List<OffsetSpotAndMatchFrameNum>> AllResults = new List<List<OffsetSpotAndMatchFrameNum>>();

            List<OffsetSpotAndMatchFrameNum> WorkingList = new List<OffsetSpotAndMatchFrameNum>();

            int AllowedGapSeconds = CONST_AllowedGapSeconds;

            int idx = 1;
            foreach (OffsetSpotAndMatchFrameNum time in Allspans)
            {
                if (WorkingList.Count == 0)
                {
                    WorkingList.Add(time);
                }
                else if ((time.MatchFrameNum - WorkingList.Last().MatchFrameNum) < AllowedGapSeconds)
                {
                    WorkingList.Add(time);
                }
                else
                {
                    AllResults.Add(new List<OffsetSpotAndMatchFrameNum>(WorkingList));
                    WorkingList.Clear();
                }
                //if last case ends on the last value it wasn't geting added
                // if (time.MatchTime == Allspans.Last().MatchTime && idx == Allspans.Count && WorkingList.Count > 0)
                if (idx == Allspans.Count && WorkingList.Count > 0)
                {
                    AllResults.Add(new List<OffsetSpotAndMatchFrameNum>(WorkingList));
                }
                idx = idx + 1;
            }

            return AllResults;
        }

        private void btnRunAll_Click(object sender, EventArgs e)
        {
            //load spots hashes into btree and download and run CC hor
            progressBar1.Value = 0;
           

            var Carriertorun = from Car in AllCarriers where Car.HasSpots && Car.Active select Car;

            progressBar1.Maximum = Carriertorun.Count();
            
            foreach (clsChannelData carrier in Carriertorun)
            {
                //clsChannelData CarrierData = (clsChannelData)lueChannel.GetSelectedDataRow();

                ReadExtraHourChannelData(carrier.Carrier.ToString(), carrier.Region.ToString(), carrier.Platform.ToString(), AutumnChangeDate.AddDays(-1).ToString("yyyy-MM-dd"));

                FindAllAds(carrier.Carrier.ToString(), carrier.Region.ToString(), carrier.Platform.ToString(), AutumnChangeDate.AddDays(-1).ToString("yyyy-MM-dd"));
                            
                progressBar1.Value = progressBar1.Value + 1;
            }
            ReportAllChannelsWithTimeSlots();
        }

        
        /// <summary>
        /// take list of all day matches for all ads and mark all overlaps and return sorted lsit
        /// </summary>
        /// <param name="AllDaysMatches"></param>
        /// <param name="OrderedDayLst"></param>
        private static void FindAllOverlappingSpotsOAndMAds(List<ReadSpotFoundAd> AllDaysMatches, out List<ReadSpotFoundAd> OrderedDayLst)
        {
            //==============================
            //rewrite to use calculated end times
            //sort longest duration s first to search for overlaps, also remove all that fail the length test
            List<ReadSpotFoundAd> FullOrderedDayLst = AllDaysMatches.OrderByDescending(o => o.SpotDuration).ToList();

            //run a covering ad search
            for (int idx = 0; idx < FullOrderedDayLst.Count; idx++)
            {
                ReadSpotFoundAd CurrentAd = FullOrderedDayLst[idx];
                //for next ads if a dureations is < 
                if (CurrentAd.Overlapped == false)
                    for (int SeekIdx = idx + 1; SeekIdx < FullOrderedDayLst.Count; SeekIdx++)
                    {
                        ReadSpotFoundAd TestAd = FullOrderedDayLst[SeekIdx];

                        if (TestAd.Overlapped == false)
                            if (CurrentAd.SpotDuration > TestAd.SpotDuration)
                            {
                                double StartSecs = CurrentAd.CalculatedStart.TotalSeconds;
                                double EndSecs = CurrentAd.CalculatedEnd.TotalSeconds;

                                //get latest start
                                if (TestAd.CalculatedStart.TotalSeconds > StartSecs)
                                    StartSecs = TestAd.CalculatedStart.TotalSeconds;

                                //get earliest end
                                if (TestAd.CalculatedEnd.TotalSeconds < EndSecs)
                                    EndSecs = TestAd.CalculatedEnd.TotalSeconds;

                                int TestGap = (CurrentAd.SpotDuration - TestAd.SpotDuration) / 2;

                                double OverlapInSecs = EndSecs - StartSecs;
                                if (OverlapInSecs > TestAd.SpotDuration * 0.2)      //was 50%                              
                                { //test longes is not a partial match
                                    if (CurrentAd.LengthTest < TestAd.LengthTest + TestGap)
                                    {
                                        CurrentAd.Overlapped = true;
                                        //must break out of the loop the moment the CurrentAd spot is overlapped
                                        break;
                                    }
                                    else
                                        TestAd.Overlapped = true;
                                }
                            }
                        //deal with case where slightly diff ads of same length overlap talk higest Sqn Quality
                        if (CurrentAd.SpotDuration == TestAd.SpotDuration)
                        {
                            double StartSecs = CurrentAd.CalculatedStart.TotalSeconds;
                            double EndSecs = CurrentAd.CalculatedEnd.TotalSeconds;

                            //get latest start
                            if (TestAd.CalculatedStart.TotalSeconds > StartSecs)
                            {
                                StartSecs = TestAd.CalculatedStart.TotalSeconds;
                            }

                            //get earliest end
                            if (TestAd.CalculatedEnd.TotalSeconds < EndSecs)
                            {
                                EndSecs = TestAd.CalculatedEnd.TotalSeconds;
                            }

                            double OverlapInSecs = EndSecs - StartSecs;

                            if (OverlapInSecs > TestAd.SpotDuration * 0.25)
                            {
                                //test longes is not a partial match
                                if (CurrentAd.SequenceQuality == TestAd.SequenceQuality)
                                {//break on base quality
                                    if (CurrentAd.BaseSequenceQuality < TestAd.BaseSequenceQuality)
                                    {
                                        CurrentAd.Overlapped = true;
                                        TestAd.OverlappedCount = TestAd.OverlappedCount + 1;
                                    }
                                    else
                                    {
                                        TestAd.Overlapped = true;
                                        CurrentAd.OverlappedCount = CurrentAd.OverlappedCount + 1;
                                    }
                                }
                                else if (CurrentAd.SequenceQuality < TestAd.SequenceQuality)
                                {
                                    CurrentAd.Overlapped = true;
                                    TestAd.OverlappedCount = TestAd.OverlappedCount + 1;
                                }
                                else
                                {
                                    TestAd.Overlapped = true;
                                    CurrentAd.OverlappedCount = CurrentAd.OverlappedCount + 1;
                                }
                            }
                        }
                    }
            }

            OrderedDayLst = new List<ReadSpotFoundAd>();
            foreach (ReadSpotFoundAd item in FullOrderedDayLst.OrderBy(o => o.SpotMatchedStart).ToList())
            {
                //if (item.Duplicate == false)//gayer all in outputs
                OrderedDayLst.Add(item);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //get all TimeSlots_1107_0_0_2025-06-13 filea and report channels affaaected

            ReportAllChannelsWithTimeSlots();
        }

        private void ReportAllChannelsWithTimeSlots()
        {
            String AllTimeSlotChannels = "Channels With Timeslots" + Environment.NewLine;
            string[] allpaths = Directory.GetFiles(AutumnTopLevelDir + @"FoundAds\", "TimeSlots*.json", SearchOption.TopDirectoryOnly);

            foreach (string item in allpaths)
            {
                string filename = Path.GetFileName(item);
                string[] splitstr = filename.Split('_');
                var channnel = from cnl in AllCarriers where cnl.Carrier.ToString() == splitstr[1] && cnl.Region.ToString() == splitstr[2] && cnl.Platform.ToString() == splitstr[3] select cnl;

                if (channnel != null)
                {
                    AllTimeSlotChannels = AllTimeSlotChannels + channnel.First().UIName + Environment.NewLine;
                }
            }
            File.WriteAllText(Path.Combine(AutumnTopLevelDir + @"FoundAds\", "ChannelsWithTimeSlotsForHr27.txt"), AllTimeSlotChannels);
        }
    }

    /// <summary>
    /// struct to link ad offset to match to alllow aditional quality measure
    /// </summary>
    public struct OffsetSpotAndMatchFrameNum
    {
        public int ParentIDX { get; set; }
        public int SpotFrameNum { get; set; }
        public int MatchFrameNum { get; set; }
    }
    public struct SpotsData
    {
        public int ParentAd { get; set; }
        public int SpotFrameNum { get; set; }
    }

    public struct TimeAndGap
    {
        public TimeSpan StartOfGap { get; set; }
        public TimeSpan LengthOfGap { get; set; }
    }
}
