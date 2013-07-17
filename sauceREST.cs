using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace saucelabs.saucerest {
    public class SauceREST : saucerest_net.ISauceREST {

        protected string username;
        protected string accessKey;

        public static readonly string RESTURL = "https://saucelabs.com/rest/v1/{0}";
        private static readonly string USER_RESULT_FORMAT = RESTURL + "/{1}";
        private static readonly string JOB_RESULT_FORMAT = RESTURL + "/jobs/{1}";
        private static readonly string JOBLIST_RESULT_FORMAT = RESTURL + "/jobs?from={1}&to={2}&limit={3}";
        private static readonly string DOWNLOAD_VIDEO_FORMAT = "https://saucelabs.com/rest/{0}/jobs/{1}/results/video.flv";
        private static readonly string DOWNLOAD_LOG_FORMAT = JOB_RESULT_FORMAT + "/results/video.flv";
        private static readonly string DATE_FORMAT = "yyyyMMdd_HHmmSS";

        public SauceREST(string username, string accessKey) {
            this.username = username;
            this.accessKey = accessKey;
        }

        /**
         * Marks a Sauce Job as 'passed'.
         *
         * @param jobId the Sauce Job Id, typically equal to the Selenium/WebDriver sessionId
         * @throws IOException thrown if an error occurs invoking the REST request
         */
        public void jobPassed(string jobId) {
            Dictionary<string, Object> updates = new Dictionary<string, Object>();
            updates.Add("passed", true);
            updateJobInfo(jobId, updates);
        }

        /**
         * Marks a Sauce Job as 'failed'.
         *
         * @param jobId the Sauce Job Id, typically equal to the Selenium/WebDriver sessionId
         * @throws IOException thrown if an error occurs invoking the REST request
         */
        public void jobFailed(string jobId) {
            Dictionary<string, Object> updates = new Dictionary<string, Object>();
            updates.Add("passed", false);
            updateJobInfo(jobId, updates);
        }

        /**
         * Downloads the video for a Sauce Job to the filesystem.  The file will be stored in
         * a directory specified by the <code>location</code> field.
         *
         * @param jobId    the Sauce Job Id, typically equal to the Selenium/WebDriver sessionId
         * @param location
         * @throws IOException thrown if an error occurs invoking the REST request
         */
        public void downloadVideo(string jobId, string location) {
            Uri restEndpoint = null;
            try {
                restEndpoint = new Uri(String.Format(DOWNLOAD_VIDEO_FORMAT, username, jobId));
            } catch (UriFormatException e) {
                Log("Error constructing Sauce URL: " + e.ToString());
            }
            downloadFile(jobId, location, restEndpoint);
        }

        /**
         * Downloads the log file for a Sauce Job to the filesystem.  The file will be stored in
         * a directory specified by the <code>location</code> field.
         *
         * @param jobId    the Sauce Job Id, typically equal to the Selenium/WebDriver sessionId
         * @param location
         * @throws IOException thrown if an error occurs invoking the REST request
         */
        public void downloadLog(string jobId, string location) {
            Uri restEndpoint = null;
            try {
                restEndpoint = new Uri(String.Format(DOWNLOAD_LOG_FORMAT, username, jobId));
            } catch (UriFormatException e) {
                Log("Error constructing Sauce URL: " + e.ToString());
            }
            downloadFile(jobId, location, restEndpoint);
        }

        public string retrieveResults(string path) {
            Uri restEndpoint = null;
            try {
                restEndpoint = new Uri(String.Format(USER_RESULT_FORMAT, username, path));
            } catch (UriFormatException e) {
                Log("Error constructing Sauce URL: " + e.ToString());
            }
            return retrieveResults(restEndpoint);
        }

        public string getJobInfo(string jobId) {
            Uri restEndpoint = null;
            try {
                restEndpoint = new Uri(String.Format(JOB_RESULT_FORMAT, username, jobId));
            } catch (UriFormatException e) {
                Log("Error constructing Sauce URL: " + e.ToString());
            }
            return retrieveResults(restEndpoint);
        }

        private string retrieveResults(Uri restEndpoint) {
            return retrieveResults(restEndpoint, 7000);
        }

        private string retrieveResults(Uri restEndpoint, int timeOut) {
            string results = string.Empty;

            try {
                WebRequest request = WebRequest.Create(restEndpoint);
                request.Method = "GET";
                request.Timeout = timeOut;

                String auth = encodeAuthentication();
                request.Headers.Add("Authorization", "Basic " + auth);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK) {
                    StreamReader stream = new StreamReader(response.GetResponseStream());
                    results = stream.ReadToEnd();
                }
                response.Close();

            } catch (IOException e) {
                Log("Error retrieving Sauce Results: " + e.ToString());
            }

            return results;
        }

        private void downloadFile(string jobId, string location, Uri restEndpoint) {
            string results = string.Empty;
            StreamReader stream;

            try {
                WebRequest request = WebRequest.Create(restEndpoint);
                request.Method = "GET";
                request.Timeout = 20000;

                string auth = encodeAuthentication();
                request.Headers.Add("Authorization", "Basic " + auth);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK) {
                    stream = new StreamReader(response.GetResponseStream()); // might need to just use the Stream and create a file directly from the stream
                    results = stream.ReadToEnd();
                }
                
                string saveName = jobId + DateTime.Now.ToString(DATE_FORMAT);
                
                if (restEndpoint.AbsoluteUri.EndsWith(".flv")) {
                    saveName = saveName + ".flv";
                } else {
                    saveName = saveName + ".log";
                }

                System.IO.StreamWriter file = new System.IO.StreamWriter(location + saveName, false);
                file.WriteLine(results);
                file.Close();
                Log("Error downloading Sauce Results: ");
            } catch (IOException e) {
               Log("Error downloading Sauce Results: " + e.ToString());
            }
        }

        public void updateJobInfo(string jobId, Dictionary<string, Object> updates) {
            string results = string.Empty;
            StreamReader stream;

            try {
                Uri restEndpoint = new Uri(String.Format(JOB_RESULT_FORMAT, username, jobId));

                WebRequest request = WebRequest.Create(restEndpoint);
                request.Method = "PUT";
                request.Timeout = 10000;

                string auth = encodeAuthentication();
                request.Headers.Add("Authorization", "Basic " + auth);

                string jsonText = JsonConvert.SerializeObject(updates);
                Stream requestStream = request.GetRequestStream();
                ASCIIEncoding encoding = new ASCIIEncoding ();
                byte[] byte1 = encoding.GetBytes (jsonText);
                requestStream.Write(byte1, 0, byte1.Length);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK) {
                    stream = new StreamReader(response.GetResponseStream()); // might need to just use the Stream and create a file directly from the stream
                    results = stream.ReadToEnd();
                }

                response.Close();
            } catch (IOException e) {
                Log("Error updating Sauce Results: " + e.ToString());
            }
        }

        public Dictionary<string, string>[] getJobIDList(DateTime start_time, DateTime end_time, int limit, int time_out) {
            Uri restEndpoint = null;
            try {

                restEndpoint = new Uri(String.Format(JOBLIST_RESULT_FORMAT, username, DateTimeToUnixTimestamp(start_time), DateTimeToUnixTimestamp(end_time), limit));
            } catch (UriFormatException e) {
                Log("Error constructing Sauce URL: " + e.ToString());
            }
            string result = retrieveResults(restEndpoint,time_out);

            if (!result.Contains("error"))
                return JsonConvert.DeserializeObject<Dictionary<string, string>[]>(result);
            else
                return JsonConvert.DeserializeObject<Dictionary<string, string>[]>("");
        }

        private double DateTimeToUnixTimestamp(DateTime dateTime) {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = dateTime - origin;
            return Math.Floor(diff.TotalSeconds);
        }

        public string encodeAuthentication() {
            string auth = username + ":" + accessKey;

            try {
                byte[] encData_byte = new byte[auth.Length];
                encData_byte = System.Text.Encoding.UTF8.GetBytes(auth);
                string encodedData = Convert.ToBase64String(encData_byte);
                return encodedData;
            } catch (Exception e) {
                throw new Exception("Error in base64Encode" + e.Message);
            }
        }

        private void Log(String message) {
            System.IO.StreamWriter file = new System.IO.StreamWriter("\\log.txt", true);
            file.WriteLine(message);
            file.Close();
        }
    }
}
