using System;
using System.Collections.Generic;
using NUnit.Framework;
using saucelabs.saucerest;

namespace saucelabs.saucerestnetTest {
    [Ignore]
    [TestFixture()]
    public class sauceRESTtest {

        [SetUp()]
        public void Init() {

        }

        [Test()]
        public void getJobInfoTest() {
            SauceREST tester = new SauceREST("username", "access-key");

            //Dictionary<string, string>[] test = tester.getJobIDList(DateTime.Today.AddDays(-3).Ticks, DateTime.Today.Ticks, 1000);

            //tester.jobPassed("b786a5b7eeb5407ebf90dfe81598c5c5");

            //tester.jobPassed("jobid");

            //tester.downloadVideo("jobid", "d:\\");
        }
    }
}