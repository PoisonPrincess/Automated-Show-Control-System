using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.Events;
using System.Collections.Generic;

namespace ShowAutomation.UnitTests.Domain.Models.Commands
{
    [TestClass]
    public class MidiCommandTest
    {

        public MidiCommand MidiCommandConstructor(string name, List<MidiEvent> midiEvents)
        {
            MidiCommand target = new MidiCommand(name, midiEvents);
            return target;
          
        }
        [TestMethod]
        public void MidiCommandPauseTest()
        {
            MidiCommand cmd = new MidiCommand(new NAudio.Midi.MidiEvent(1, 1, commandCode: new NAudio.Midi.MidiCommandCode()));
            //cmd.pause();not a method anymore
            //Assert.AreEqual(true, true); //Simply returns, nothing to test
            Assert.Inconclusive();
        }

        [TestMethod]
        public void MidiCommandStopTest()
        {
            MidiCommand cmd = new MidiCommand(new NAudio.Midi.MidiEvent(1, 1, commandCode: new NAudio.Midi.MidiCommandCode()));
            //cmd.stop(); not a method anymore
            //Assert.AreEqual(true, true); //Simply returns, nothing to test
            Assert.Inconclusive();
        }

        [TestMethod]
        public void MidiCommandGetDurationTest()
        {
            ulong expected_return = 200;    //function simply returns 200
            ulong returnVal;
            MidiCommand cmd = new MidiCommand(new NAudio.Midi.MidiEvent(1, 1, commandCode: new NAudio.Midi.MidiCommandCode()));
            returnVal = cmd.getDuration();
            Assert.AreEqual(expected_return, returnVal); 
        }

        [TestMethod]
        public void MidiConstructorTest()
        {
            MidiCommand midiCommand;
            midiCommand = this.MidiCommandConstructor((string)null, (List<MidiEvent>)null);
            Assert.IsNotNull((object)midiCommand);
            Assert.IsNull(midiCommand.MidiEvents);
            Assert.AreEqual<string>((string)null, ((AbstractCommand)midiCommand).name);
            Assert.IsNull(((AbstractCommand)midiCommand).color.ColorContext);
            Assert.AreEqual<byte>
                ((byte)byte.MaxValue, ((AbstractCommand)midiCommand).color.A);
            Assert.AreEqual<byte>((byte)173, ((AbstractCommand)midiCommand).color.R);
            Assert.AreEqual<byte>((byte)216, ((AbstractCommand)midiCommand).color.G);
            Assert.AreEqual<byte>((byte)230, ((AbstractCommand)midiCommand).color.B);
            Assert.AreEqual<float>((float)1, ((AbstractCommand)midiCommand).color.ScA);
            Assert.AreEqual<float>
                ((float)0.4123456789, ((AbstractCommand)midiCommand).color.ScR);
            Assert.AreEqual<float>
                ((float)0.61234456789, ((AbstractCommand)midiCommand).color.ScG);
            Assert.AreEqual<float>
                ((float)0.1234545678, ((AbstractCommand)midiCommand).color.ScB);
            Assert.IsNull(((AbstractCommand)midiCommand).media);
            Assert.AreEqual<string>("MIDI", ((AbstractCommand)midiCommand).type);
        }

    }
}
