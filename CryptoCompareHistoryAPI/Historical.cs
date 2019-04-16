using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Linq;
using Newtonsoft.Json;

namespace CryptoCompareAPI
{
    /// <summary>
    /// Class for fetching stock historical price from Yahoo Finance
    /// Copyright Dennis Lee
    /// 19 May 2017
    /// 
    /// </summary>
    /// 

    public enum TIMEFRAME
    {
        MIN1 = 1,
        MIN5 = 5,
        MIN15 = 15,
        MIN30 = 30,
        MIN60 = 60,
        MIN240 = 240,
        MIN1440 = 1440,
        MIN10080 = 10080,
        MIN21600 = 21600
    };

    public enum EXCHANGES
    {
        CCCAGG,
        Binance
    }

    public enum LIMIT
    {
        Max
    }

    public class Historical
    {
        public static string MainSymbol = string.Empty;
        //"https://min-api.cryptocompare.com/data/" + action + "?fsym=" + fsym + "&tsym=" + tsym + "&limit=" + limit + "&aggregate=" + aggregate + "&e=CCCAGG&toTs=" + toTs;
        public static string CryptoCompURL="https://min-api.cryptocompare.com/data/{0}?fsym={1}&tsym={2}&limit={3}&aggregate={4}&e={5}&toTs={6}";
        public static string BinanceURL = "https://api.binance.com/api/v1/klines?symbol={0}&interval=1h&endTime={1}&limit=1000";
        public static EXCHANGES exchange = EXCHANGES.CCCAGG;
        public static string limit = "2000";
        private static int Maxlimit = 2000;
        /// <summary>
        /// Get stock historical price from Yahoo Finance
        /// </summary>
        /// <param name="symbol">Stock ticker symbol</param>
        /// <param name="start">Starting datetime</param>
        /// <param name="end">Ending datetime</param>
        /// <returns>List of history price</returns>
        public static int Get(string symbol, DateTime start, DateTime end, TIMEFRAME TimeFrame)
        {
            List<string> HistoryPrices = new List<string>();
            DateTime RawDataStartDate = DateTime.UtcNow;

            try
            {
                while (RawDataStartDate > start)
                {
                    end = RawDataStartDate;
                    string jsonData = GetRaw(symbol, start, end, TimeFrame);
                    if (jsonData != null && jsonData != "[]")
                    {
                        dynamic obj = JsonConvert.DeserializeObject(jsonData);
                        var DataArr = ((IEnumerable<dynamic>)obj["Data"]);
                        var lastElem = DataArr.ElementAt(DataArr.Count() - 2);

                        if (lastElem["open"].ToString() == "0" || lastElem["open"].ToString() == "null" ||
                            string.IsNullOrEmpty(lastElem["open"].ToString()))
                            break;

                        foreach (var x in DataArr.Reverse())
                        {
                            DateTime UnixTs = UnixTimestampToDateTime(System.Convert.ToDouble(x["time"].ToString()));
                            if (RawDataStartDate == UnixTs)
                            {
                                RawDataStartDate = DateTime.MinValue;
                                //break;
                            }

                            if (x == DataArr.ElementAt(0))
                                RawDataStartDate = UnixTs;

                            if (x["open"].ToString() != "0" && x["open"].ToString() != "null" && !string.IsNullOrEmpty(x["open"].ToString()))
                            {
                                string time =
                                    UnixTimestampToDateTime(System.Convert.ToDouble(x["time"].ToString()))
                                        .ToString("yyyy-MM-dd HH:mm");
                                string data = string.Format("{0},{1},{2},{3},{4},{5},{6}", time,
                                    System.Convert.ToDouble(x["open"]).ToString("0.0000000000"),
                                    System.Convert.ToDouble(x["high"]).ToString("0.0000000000"),
                                    System.Convert.ToDouble(x["low"]).ToString("0.0000000000"),
                                    System.Convert.ToDouble(x["close"]).ToString("0.0000000000"),
                                    System.Convert.ToDouble(x["close"]).ToString("0.0000000000"),
                                    //System.Convert.ToDouble(x["volumeto"]).ToString("0.0000000000"));
                                    exchange==EXCHANGES.Binance? System.Convert.ToDouble(x["volumefrom"]).ToString("0.0000000000"):System.Convert.ToDouble(x["volumeto"]).ToString("0.0000000000"));

                                HistoryPrices.Add(data);
                            }
                        }
                    }

                    if (Maxlimit.ToString() != limit)
                        break;
                }


                if (HistoryPrices.Count > 0)
                {
                    //HistoryPrices.Reverse();

                    string csv = "date,open,high,low,close,adjclose,volume\n" + String.Join("\n", HistoryPrices);
                    System.IO.File.WriteAllText(@"history\history.csv", csv);
                }


            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }

            return HistoryPrices.Count - 1;

        }

        /// <summary>
        /// Get raw stock historical price from Yahoo Finance
        /// </summary>
        /// <param name="symbol">Stock ticker symbol</param>
        /// <param name="start">Starting datetime</param>
        /// <param name="end">Ending datetime</param>
        /// <returns>Raw history price string</returns>

        public static string GetRaw(string symbol, DateTime start, DateTime end, TIMEFRAME timeFrame)
        {

            string csvData = null;

            try
            {
                int pos = symbol.IndexOf("BTC", 2);
                if (pos <= 0)
                    pos = symbol.IndexOf("LTC", 2);
                if (pos <= 0)
                    pos = symbol.IndexOf("ETH", 2);
                if (pos <= 0)
                    pos = symbol.IndexOf("BNB", 2);
                if (pos <= 0)
                    pos = symbol.IndexOf("TUSD", 2);
                if (pos <= 0)
                    pos = symbol.IndexOf("USDT", 2);
                if (pos <= 0)
                    pos = symbol.IndexOf("USD", 2);
                if (pos <= 0)
                    throw new Exception("ERROR: Pair not found " + symbol);

                string fsym = symbol.Substring(0, pos);
                string tsym = symbol.Substring(pos, symbol.Length - fsym.Length);
                MainSymbol = tsym;
                
                string toTs = ((int)DateTimeToUnixTimestamp(end)).ToString();
                string aggregate = "1";
                string action = "histominute";
                if (timeFrame == TIMEFRAME.MIN1 || timeFrame == TIMEFRAME.MIN15 || timeFrame == TIMEFRAME.MIN30)
                {
                    action = "histominute";
                    aggregate = timeFrame.ToString();
                }
                else if (timeFrame == TIMEFRAME.MIN60 || timeFrame == TIMEFRAME.MIN240)
                {
                    action = "histohour";
                    aggregate = "1";
                    if (timeFrame == TIMEFRAME.MIN240)
                        aggregate = "4";
                }
                else if (timeFrame == TIMEFRAME.MIN1440 || timeFrame != TIMEFRAME.MIN10080 || timeFrame != TIMEFRAME.MIN21600)
                {
                    action = "histoday";
                    aggregate = "1";
                    if (timeFrame == TIMEFRAME.MIN10080)
                        aggregate = "7";
                    if (timeFrame == TIMEFRAME.MIN21600)
                        aggregate = "30";
                }

                //string url = "https://min-api.cryptocompare.com/data/" + action + "?fsym=" + fsym + "&tsym=" + tsym + "&limit=" + limit + "&aggregate=" + aggregate + "&e=CCCAGG&toTs=" + toTs;
                string url = string.Format(CryptoCompURL, action, fsym, tsym, limit, aggregate, exchange.ToString(), toTs);
                //else
                    //url = "https://min-api.cryptocompare.com/data/" + action + "?fsym=" + fsym + "&tsym=" + tsym + "&limit=" + limit + "&aggregate=" + aggregate + "&e=CCCAGG&toTs=" + toTs;
                //url = string.Format(BinanceURL,  fsym+tsym, toTs);



                using (WebClient wc = new WebClient())
                {
                    csvData = wc.DownloadString(url);
                }

            }
            catch (WebException webEx)
            {
                throw;
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }

            return csvData;

        }

        /// <summary>
        /// Parse raw historical price data into list
        /// </summary>
        /// <param name="csvData"></param>
        /// <returns></returns>
        private static List<HistoryPrice> Parse(string csvData)
        {

            List<HistoryPrice> hps = new List<HistoryPrice>();

            try
            {
                string[] rows = csvData.Split(Convert.ToChar(10));

                //row(0) was ignored because is column names 
                //data is read from oldest to latest
                for (int i = 1; i <= rows.Length - 1; i++)
                {

                    string row = rows[i];
                    if (string.IsNullOrEmpty(row))
                        continue;

                    string[] cols = row.Split(',');
                    if (cols[1] == "null")
                        continue;

                    HistoryPrice hp = new HistoryPrice();
                    hp.Date = DateTime.Parse(cols[0]);
                    hp.Open = Convert.ToDouble(cols[1]);
                    hp.High = Convert.ToDouble(cols[2]);
                    hp.Low = Convert.ToDouble(cols[3]);
                    hp.Close = Convert.ToDouble(cols[4]);
                    hp.AdjClose = Convert.ToDouble(cols[5]);

                    //fixed issue in some currencies quote (e.g: SGDAUD=X)
                    if (cols[6] != "null")
                        hp.Volume = Convert.ToDouble(cols[6]);

                    hps.Add(hp);

                }

            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }

            return hps;

        }

        #region Unix Timestamp Converter

        //credits to ScottCher
        //reference http://stackoverflow.com/questions/249760/how-to-convert-a-unix-timestamp-to-datetime-and-vice-versa
        private static DateTime UnixTimestampToDateTime(double unixTimeStamp)
        {
            //Unix timestamp Is seconds past epoch
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimeStamp);
        }

        //credits to Dmitry Fedorkov
        //reference http://stackoverflow.com/questions/249760/how-to-convert-a-unix-timestamp-to-datetime-and-vice-versa
        private static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            //Unix timestamp Is seconds past epoch
            return (dateTime.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
        }

        #endregion

        public static void GetTopListByVolume()
        {
            if (string.IsNullOrEmpty(MainSymbol))
                throw new Exception("GetTopListByVolume Symbol is undefined");

            string url = "https://min-api.cryptocompare.com/data//top/volumes?tsym=" + MainSymbol;
            string jsonData = null;
            string csv = string.Empty;

            using (WebClient wc = new WebClient())
            {
                jsonData = wc.DownloadString(url);
            }

            if (jsonData != null)
            {
                dynamic obj = JsonConvert.DeserializeObject(jsonData);
                var DataArr = ((IEnumerable<dynamic>)obj["Data"]);

                foreach (var x in DataArr)
                {
                    csv += x["SYMBOL"].ToString() + "/" + MainSymbol + ",";
                }
            }

            csv.TrimEnd(',');
            if (!string.IsNullOrEmpty(csv))
                System.IO.File.WriteAllText(@"history\" + MainSymbol + "TopListByVolume.csv", csv);

        }

    }


    public class HistoryPrice
    {
        public DateTime Date { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
        public double AdjClose { get; set; }
    }
}