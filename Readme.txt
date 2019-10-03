//Need to install the .Net Framework

//Put the CryptoCompareHistoryAPI.exe and dependencies in the Zorro directory

//1 minute history available is only 2 weeks
//60 minute history starts from 2014 for older coins

//To download the hourly history Call the Zorro snippet below with:
dataFromCryptoCompare("ETH/BTC",60);


void removeChar(char* input, char ch)
{
    char* output = input;
    while (*input)
    {
        if (*input != ch)
        {
            *(output++) = *input;
        }
        ++input;
    }
    *output = 0;
}

var dataFromCryptoCompare(string Code, int timeframe)
{
	removeChar(Code,'/');

	string Format;
	Format = "%Y-%m-%d %H:%M,f3,f1,f2,,f4,f6";
	
	char cmdParam[256];
	sprintf(cmdParam, "%s %d", Code,timeframe);
	exec("CryptoCompareHistoryAPI.exe", cmdParam, 1);

	dataNew(1, 0, 7);
	int numRecord = dataParse(1, Format, "History\\history.csv");
	printf("\n%s %d records read from CRYPTOCOMPARE", Code, numRecord);
	dataSave(1, strf("%s.t6", Code));
	return numRecord;
	
}



- Donations Bitcoin Addrsss:
3KgJh9QUrWJBDKiDjLupVY3fqwgmkdUPzM
