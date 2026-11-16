using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowAutomation.UnitTests.Domain.Utils
{
    public partial class ModeTest
    {

     
        public void projectNameSet(string value)
        {
            Mode.projectName = value;
           
        }

 
        public string projectNameGet()
        {
            string result = Mode.projectName;
            return result;
            
        }

        public void isEditModeSet(bool value)
        {
            Mode.isEditMode = value;
            
        }
    }
    public partial class ModeTest
    {

        [TestMethod]
        public void isEditModeSet719()
        {
            this.isEditModeSet(false);
        }
        [TestMethod]
        public void projectNameGet670()
        {
            string s;
            s = this.projectNameGet();
            Assert.AreEqual<string>("", s);
        }

        [TestMethod]
        public void projectNameSet666()
        {
            this.projectNameSet((string)null);
            Assert.Fail();
        }

    }


}
