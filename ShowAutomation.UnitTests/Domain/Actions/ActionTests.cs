using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShowAutomation.Domain;
using ShowAutomation.Domain.Models.Events;

namespace ShowAutomation.Domain.Actions.Tests
{
    [TestClass()]
    public class ActionTests
    {

        public object GetFocus(Action target)
        {
            object result = target.GetFocus();
            return result;

        }


        public void Perform(Action target)
        {
            target.Perform();

        }


        public void Undo(Action target)
        {
            target.Undo();

        }


        public string Title(Action target)
        {
            string result = target.Title();
            return result;

        }



        [TestMethod]
        public void GetFocusTEst()
        {
            UngroupActionTest ungroupAction;
            object o;
            ungroupAction = new UngroupActionTest((Event)null, (EventGroup)null);
            o = this.GetFocus((Action)ungroupAction);
            Assert.IsNull(o);
        }

        [TestMethod]
        public void PerformTest()
        {
            UngroupActionTest ungroupAction;
            ungroupAction = new UngroupActionTest((Event)null, (EventGroup)null);
           this.Perform((Action)ungroupAction);
            Assert.Fail();
        }
        [TestMethod]
        public void TitleThrowsNullReferenceException18()
        {
            UngroupActionTest ungroupAction;
            string s;
            ungroupAction = new UngroupActionTest((Event)null, (EventGroup)null);
            s = this.Title((Action)ungroupAction);
            Assert.IsNotNull(s);
        }

        [TestMethod]
        public void UndoTest()
        {
            UngroupActionTest ungroupAction;
            ungroupAction = new Actions.UngroupActionTest((Event)null, (EventGroup)null);
            this.Undo((Action)ungroupAction);
            Assert.Fail();
        }

    

       
    }
}