using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CryptoCompareAPI
{
    public class Entry
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">args[0]: Symbol, args[1]: Timeframe optional</param>
        public static void Main(String[] args)
        {
            if (args.Count() == 1 &&args[0] != null )
            {
                int records = Historical.Get(args[0].Replace("/",""), new DateTime(2009, 1, 1).ToUniversalTime(), DateTime.UtcNow, TIMEFRAME.MIN1440);
                Console.WriteLine(args[0] + " " + records.ToString() + " downloaded");
            }
            else if (args.Count() == 2 && args[0] != null &&  args[1] != null)
            {
                int timeframe = 1440;
                int.TryParse(args[1], out timeframe);

                int records = Historical.Get(args[0].Replace("/", ""), new DateTime(2009, 1, 1).ToUniversalTime(), DateTime.UtcNow, (TIMEFRAME)timeframe);
                Console.WriteLine(args[0] + " " + records.ToString() + " downloaded");

            }
            else
                Console.WriteLine("No Symbol");
        }
    }
}
