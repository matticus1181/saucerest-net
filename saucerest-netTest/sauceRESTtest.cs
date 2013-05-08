using NUnit.Framework;
using saucelabs.saucerest;

namespace saucelabs.saucerestnetTest {
    [TestFixture()]
    public class sauceRESTtest {

        [SetUp()]
        public void Init() {

        }

        [Test()]
        public void getJobInfoTest() {
            SauceREST tester = new SauceREST("username", "accesskey");

            //Assert.AreEqual(string.Empty, tester.getJobInfo("jobid"));

            //tester.jobPassed("jobid");

            tester.downloadVideo("jobid", "d:\\");
        }
    }
}