using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TweetSharp;

namespace TweetAnalyser
{
    class Program
    {
        public static string _consumerKey = "Hbr638SHLTfru6mnMFGjWyLLK"; // Add your Key
        public static string _consumerSecret = "ItOs0SznVDMYbv0gwEXbf3ewI7gM5jtGUAkx4bxAlJRIqN59cZ"; // Add your Key
        public static string _accessToken = "59375508-rRY3UB7FBBeswoOTXP10pGTm6og5UDVjYIGZyUDNu"; // Add your Key
        public static string _accessTokenSecret = "yd1U4jd0jeiiJTcNmh00pXZJ8IIFOwttVptUEd3jXbw5y"; // Add your Key

        static bool canBreak = false;

        static void Main(string[] args)
        {
            //NLP.GetSentiment();
            long maxId = GetTweets(null);

            while (!canBreak)
            {
             maxId =    GetTweets(maxId);
            }
            Console.ReadLine();
        }

        private static long GetTweets(long? maxId)
        {
            TwitterService twitterService = new TwitterService(_consumerKey, _consumerSecret);
            twitterService.AuthenticateWith(_accessToken, _accessTokenSecret);

            int tweetcount = 1;
            TwitterSearchResult tweets_search = null;
            if (maxId.HasValue)
            {
                tweets_search = twitterService.Search(new SearchOptions { Q = "@NatWest_Help", MaxId = maxId });
            }
            else
            {
                tweets_search = twitterService.Search(new SearchOptions { Q = "@NatWest_Help" });
            }
            //Resulttype can be TwitterSearchResultType.Popular or TwitterSearchResultType.Mixed or TwitterSearchResultType.Recent
            
            List<TwitterStatus> resultList = new List<TwitterStatus>(tweets_search.Statuses);
            if (resultList.Count == 0)
            {
                canBreak = true;
            }
            foreach (var tweet in tweets_search.Statuses)
            {
                try
                {


                    //Above are the things we can also get using TweetSharp.
                    Console.WriteLine("TweetId : " + tweet.Id + " Message: " + tweet.Text);

                    InsertMongoDB(tweet);
                    maxId = tweet.Id;
                    if (tweet.CreatedDate.CompareTo(DateTime.Now.AddYears(-1)) < 0)
                    {
                        canBreak = true;
                    }
                    tweetcount++;
                }
                catch { }
            }

            return maxId.Value;

        }

        private static void InsertMongoDB(TwitterStatus tweet)
        {
            try
            {
                string connectionString = "mongodb://localhost:27017";
                MongoClient client = new MongoClient(connectionString);
                var database = client.GetDatabase("TweetsAnalyzer");
                var tweetsCollection = database.GetCollection<TwitterStatus>("Tweets");
                tweetsCollection.InsertOne(tweet);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}