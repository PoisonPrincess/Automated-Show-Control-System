using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Models.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowAutomation.Domain.Models.Events.Tests
{
    [TestClass()]
    public class JsonMusicEventTests
    {
        public JsonMusicEvent Constructor(MusicEvent ev)
        {
            JsonMusicEvent target = new JsonMusicEvent(ev);
            return target;
            
        }

        [TestMethod()]
        public void JsonMusicEventTest()
        {
            Assert.Fail();
        }

        public void JsonMusicEventConstructorThrowsNullReferenceException()
        {
            JsonMusicEvent jsonMusicEvent;
            jsonMusicEvent = this.Constructor((MusicEvent)null);
            Assert.IsNotNull(jsonMusicEvent);
        }

    }
}