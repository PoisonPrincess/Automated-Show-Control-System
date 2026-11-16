using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Models.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ShowAutomation.Domain.Models.Events.Tests
{
    [TestClass()]
    public class EventGroupTests
    {
        public EventGroup Constructor()
        {
            EventGroup target = new EventGroup();
            return target;
           
        }
        public EventGroup Constructor(SerializationInfo info, StreamingContext context)
        {
            EventGroup target = new EventGroup(info, context);
            return target;
         
        }
        [TestMethod()]
        public void EventGroupConstuctorTest()
        {
            EventGroup eventGroup;
            eventGroup = this.Constructor();
            Assert.IsNotNull((object)eventGroup);
        }
        public void EventGroupSerializaThrowsNullReferenceException108()
        {
            EventGroup eventGroup;
            eventGroup =
              this.Constructor((SerializationInfo)null, default(StreamingContext));
            Assert.IsNotNull(eventGroup);
        }

    }
}