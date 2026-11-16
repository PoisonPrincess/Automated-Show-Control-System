using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Utils.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowAutomation.UnitTests.Domain.Utils.Tests
{
    public partial class BehringerXAirMidiValuesTest
    {

    
        public object[] CommandCodeGet()
        {
            object[] result = BehringerXAirMidiValues.CommandCode;
            return result;
        }
    }
    public partial class BehringerXAirMidiValuesTest
    {

        [TestMethod]
        public void CommandCodeGet62()
        {
            object[] os;
            os = this.CommandCodeGet();
            Assert.IsNotNull((object)os);
            Assert.AreEqual<int>(2, os.Length);
            Assert.IsNotNull(os[0]);
            Assert.IsNotNull(os[1]);
        }
    }

}
