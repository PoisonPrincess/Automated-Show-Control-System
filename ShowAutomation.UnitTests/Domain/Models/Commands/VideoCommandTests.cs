using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ShowAutomation.Domain.Models.Commands.Tests
{
    [TestClass()]
    public class VideoCommandTests
    {
        public VideoCommand Constructor(SerializationInfo info, StreamingContext context)
        {
            VideoCommand target = new VideoCommand(info, context);
            return target;
            // TODO: add assertions to method VideoCommandTest.Constructor02(SerializationInfo, StreamingContext)
        }
        [DataTestMethod]
        public VideoCommand Constructor01(string name, Video media)
        {
            VideoCommand target = new VideoCommand(name, media);
            return target;

        }

        [DataTestMethod]
        public VideoCommand Constructor02(
            string name,
            Video media,
            Guid id
        )
        {
            VideoCommand target = new VideoCommand(name, media, id);
            return target;

        }

        public void Constructor01()
        {
            VideoCommand videoCommand;
            videoCommand = this.Constructor01((string)null, (Video)null);
            Assert.IsNotNull((object)videoCommand);
            Assert.AreEqual<string>((string)null, ((AbstractCommand)videoCommand).name);
            Assert.IsNull(((AbstractCommand)videoCommand).color.ColorContext);
            Assert.AreEqual<byte>
                ((byte)byte.MaxValue, ((AbstractCommand)videoCommand).color.A);
            Assert.AreEqual<byte>((byte)46, ((AbstractCommand)videoCommand).color.R);
            Assert.AreEqual<byte>((byte)139, ((AbstractCommand)videoCommand).color.G);
            Assert.AreEqual<byte>((byte)87, ((AbstractCommand)videoCommand).color.B);
            Assert.AreEqual<float>((float)1, ((AbstractCommand)videoCommand).color.ScA);

            Assert.IsNull(((AbstractCommand)videoCommand).media);
            Assert.AreEqual<string>("Video", ((AbstractCommand)videoCommand).type);
        }

        [TestMethod]

        public void Constructor02()
        {
            VideoCommand videoCommand;
            Guid s0
               = new Guid(default(int), (short)32, (short)32, default(byte), default(byte),
                          default(byte), default(byte), default(byte),
                          default(byte), default(byte), default(byte));
            videoCommand = this.Constructor02((string)null, (Video)null, s0);
            Assert.IsNotNull((object)videoCommand);
            Assert.AreEqual<string>((string)null, ((AbstractCommand)videoCommand).name);
            Assert.IsNull(((AbstractCommand)videoCommand).color.ColorContext);
            Assert.AreEqual<byte>
                ((byte)byte.MaxValue, ((AbstractCommand)videoCommand).color.A);
            Assert.AreEqual<byte>((byte)46, ((AbstractCommand)videoCommand).color.R);
            Assert.AreEqual<byte>((byte)139, ((AbstractCommand)videoCommand).color.G);
            Assert.AreEqual<byte>((byte)87, ((AbstractCommand)videoCommand).color.B);
            Assert.AreEqual<float>((float)1, ((AbstractCommand)videoCommand).color.ScA);

            Assert.IsNull(((AbstractCommand)videoCommand).media);
            Assert.AreEqual<string>("Video", ((AbstractCommand)videoCommand).type);
        }

        [TestMethod()]
        public void VideoCommandConstructorThrowsNullReferenceException()
        {
            VideoCommand videoCommand;
            videoCommand =
              this.Constructor((SerializationInfo)null, default(StreamingContext));
            Assert.IsNull(videoCommand);
        }
    }
}