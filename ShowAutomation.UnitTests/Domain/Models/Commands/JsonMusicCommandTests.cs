using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Models.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowAutomation.Domain.Models.Commands.Tests
{
    [TestClass()]
    public class JsonMusicCommandTests
    {
        public JsonMusicCommand Constructor(MusicCommand command)
        {
            JsonMusicCommand target = new JsonMusicCommand(command);
            return target;
           
        }
        [TestMethod()]
        public void JsonMusicCommandConstructorThrowsNullReferenceException26()
        {
            JsonMusicCommand jsonMusicCommand;
            jsonMusicCommand = this.Constructor((MusicCommand)null);
            Assert.IsNull(jsonMusicCommand);
        }
    }
}