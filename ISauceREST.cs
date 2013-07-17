using System;
namespace saucerest_net {
    interface ISauceREST {
        void downloadLog(string jobId, string location);
        void downloadVideo(string jobId, string location);
        string encodeAuthentication();
        global::System.Collections.Generic.Dictionary<string, string>[] getJobIDList(DateTime start_time, DateTime end_time, int limit, int time_out);
        string getJobInfo(string jobId);
        void jobFailed(string jobId);
        void jobPassed(string jobId);
        string retrieveResults(string path);
        void updateJobInfo(string jobId, global::System.Collections.Generic.Dictionary<string, object> updates);
    }
}
