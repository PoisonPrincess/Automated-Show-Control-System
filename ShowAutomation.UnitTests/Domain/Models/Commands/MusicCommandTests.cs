using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowAutomation.Domain.Models.Commands.Tests
{
    [TestClass()]
    public class MusicCommandTests
    {

        public MusicCommand Constructor( string name,Song media, Guid id)
        {
            MusicCommand target = new MusicCommand(name, media, id);
            return target;
         
        }

        public MusicCommand Constructor(string name, Song media)
        {
            MusicCommand target = new MusicCommand(name, media);
            return target;
        }

        [TestMethod()]
        public void MusicCommandTest()
        {
            MusicCommand musicCommand;
            Guid s0
               = new Guid(default(int), (short)32, (short)32, default(byte), default(byte),
                          default(byte), default(byte), default(byte),
                          default(byte), default(byte), default(byte));
            musicCommand = this.Constructor((string)null, (Song)null, s0);
            Assert.IsNotNull((object)musicCommand);
            Assert.AreEqual<string>((string)null, ((AbstractCommand)musicCommand).name);
            Assert.IsNull(((AbstractCommand)musicCommand).color.ColorContext);
            Assert.AreEqual<byte>
                ((byte)byte.MaxValue, ((AbstractCommand)musicCommand).color.A);
            Assert.AreEqual<byte>((byte)240, ((AbstractCommand)musicCommand).color.R);
            Assert.AreEqual<byte>((byte)230, ((AbstractCommand)musicCommand).color.G);
            Assert.AreEqual<byte>((byte)140, ((AbstractCommand)musicCommand).color.B);
            Assert.AreEqual<float>((float)1, ((AbstractCommand)musicCommand).color.ScA);
            Assert.AreEqual<float>
                ((float)0.87136709690093994, ((AbstractCommand)musicCommand).color.ScR);
            Assert.AreEqual<float>
                ((float)0.791297972202301, ((AbstractCommand)musicCommand).color.ScG);
            Assert.AreEqual<float>
                ((float)0.262250691652298, ((AbstractCommand)musicCommand).color.ScB);
            Assert.IsNull(((AbstractCommand)musicCommand).media);
            Assert.AreEqual<string>("Audio", ((AbstractCommand)musicCommand).type);
        }


        [TestMethod]
        public void MusicCommandTest01()
        {
            MusicCommand musicCommand;
            musicCommand = this.Constructor((string)null, (Song)null);
            Assert.IsNotNull((object)musicCommand);
            Assert.AreEqual<string>((string)null, ((AbstractCommand)musicCommand).name);
            Assert.IsNull(((AbstractCommand)musicCommand).color.ColorContext);
            Assert.AreEqual<byte>
                ((byte)byte.MaxValue, ((AbstractCommand)musicCommand).color.A);
            Assert.AreEqual<byte>((byte)240, ((AbstractCommand)musicCommand).color.R);
            Assert.AreEqual<byte>((byte)230, ((AbstractCommand)musicCommand).color.G);
            Assert.AreEqual<byte>((byte)140, ((AbstractCommand)musicCommand).color.B);
            Assert.AreEqual<float>((float)1, ((AbstractCommand)musicCommand).color.ScA);
            Assert.AreEqual<float>
                ((float)0.87136709690093994, ((AbstractCommand)musicCommand).color.ScR);
            Assert.AreEqual<float>
                ((float)0.791297972202301, ((AbstractCommand)musicCommand).color.ScG);
            Assert.AreEqual<float>
                ((float)0.262250691652298, ((AbstractCommand)musicCommand).color.ScB);
            Assert.IsNull(((AbstractCommand)musicCommand).media);
            Assert.AreEqual<string>("Audio", ((AbstractCommand)musicCommand).type);
        }
    }
}