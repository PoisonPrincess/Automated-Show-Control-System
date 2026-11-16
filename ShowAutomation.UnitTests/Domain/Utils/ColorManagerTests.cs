using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Media;
using ShowAutomation.Domain.Utils;

namespace ShowAutomation.UnitTests.Domain.Utils
{
    class ColorManagerTests
    {


       
        public string LIST_ITEM_1Get()
        {
            string result = ColorManager.LIST_ITEM_1;
            return result;
           
        }

        public string IS_PLAYING_COLORGet()
        {
            string result = ColorManager.IS_PLAYING_COLOR;
            return result;
           
        }

      
        public Color ACCENTGet()
        {
            Color result = ColorManager.ACCENT;
            return result;
           
        }

        [TestMethod]
        public void IS_PLAYING_COLORGet1()
        {
            string s;
            s = this.IS_PLAYING_COLORGet();
            Assert.AreEqual<string>("LightGreen", s);
        }
        [TestMethod]
        public void LIST_ITEM_1Get2()
        {
            string s;
            s = this.LIST_ITEM_1Get();
            Assert.AreEqual<string>("#19f39611", s);
        }

    }

    public partial class ColorManagerTests1
    {

      
    }

}
