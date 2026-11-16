using Newtonsoft.Json;
using ShowAutomation.UI.ViewModels.Commands;
using System;
using System.Collections.Generic;
using ShowAutomation.Domain.Models.Commands;
using ShowAutomation.Domain.Models.Events;
using ShowAutomation.Domain.Models.Scenes;
using ShowAutomation.Domain.Models.Media;
using ShowAutomation.UI.ViewModels.Scenes;
using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.Domain.Utils;
using System.Runtime.Serialization;
using ShowAutomation.UI.ViewModels.OutputDevices;

namespace ShowAutomation.Domain.Models
{/// <summary>
 /// This class is used to load in saved files 
 /// </summary>
    [Serializable()]
    public class Project : ISerializable
    {
        private dynamic commandList;
        private dynamic sceneList;
        public CommandList commandListObject;
        public SceneList sceneListObject;
        public OutputManager outputManager;

        public Project(String jsonStr)
        {
            dynamic json = JsonConvert.DeserializeObject(jsonStr);
            this.commandList = json.commandList;
            this.sceneList = json.sceneList;
            commandListObject = ConvertCommandList();
            sceneListObject = ConvertSceneList();
            outputManager = new OutputManager();
        }

        public Project(CommandList commandList, SceneList sceneList, OutputManager output) {
            commandListObject = commandList;
            sceneListObject = sceneList;
            outputManager = output;
        }

        public Project(SerializationInfo info, StreamingContext context)
        {
            commandListObject = (CommandList)info.GetValue("commands", typeof(CommandList));
            sceneListObject = (SceneList)info.GetValue("scenes", typeof(SceneList));
            try
            {
                outputManager = (OutputManager)info.GetValue("outputs", typeof(OutputManager));
            }
            catch
            {
                outputManager = new OutputManager();
            }
        }

        /// <summary>
        /// Reads json and creates show from saved Data
        /// </summary>
        /// <returns></returns>
        public SceneList ConvertSceneList()
        {
            List<Scene> listOfScenes = new List<Scene>();
            List<dynamic> scenes = new List<dynamic>(sceneList.scenes);

            for (var i = 0; i < scenes.Count; i++)
            {
                dynamic scene = scenes[i];
                Scene sceneToAdd = new Scene((String)scene.name, (Boolean)scene.autoPlay, Guid.Parse((String)scene.id));

                for (var j = 0; j < scene.events.Count; j++)
                {
                    dynamic ev = scene.events[j];
                    dynamic command = ev.command;

                    if (ev.type == Types.AUDIO)
                    { 
                        AudioDevice device= new AudioDevice((String)ev.outputDevice.deviceName);
                        sceneToAdd.addEvent(new MusicEvent(this.parseCommand(command), (ulong)ev.startTime, sceneToAdd, device));

                    } else if (ev.type == Types.MIDI)
                    {
                        MidiDevice device = new MidiDevice((String)ev.outputDevice.deviceName);
                        sceneToAdd.addEvent(new MidiEvent(this.parseCommand(command), (ulong)ev.startTime, sceneToAdd, device));
                    }
                    else
                    {
                        int deviceIndex = VideoDevice.getDeviceIndexByName((String)ev.outputDevice.deviceName);
                        VideoDevice device = new VideoDevice((String)ev.outputDevice.deviceName);
                        sceneToAdd.addEvent(new VideoEvent(this.parseCommand(command), (ulong)ev.startTime, sceneToAdd, device));
                    }
                }
                listOfScenes.Add(sceneToAdd);
            }
            return new SceneList(listOfScenes);
        }

        protected AbstractCommand parseCommand(dynamic command)
        {
            String name = command.name;
            Guid id = Guid.Parse((String)command.id);
            if (command.type == Types.AUDIO)
            {
                return new MusicCommand(name, new Song((String)command.songPath), id);
            }
            else if (command.type == Types.MIDI)
            {
                List<NAudio.Midi.MidiEvent> midiEvents = new List<NAudio.Midi.MidiEvent>();

                foreach (int midiEvent in command.MidiEvents)
                {
                    midiEvents.Add(NAudio.Midi.MidiEvent.FromRawMessage(midiEvent));
                }

                MidiCommand midiCommand = new MidiCommand(name, midiEvents);
                midiCommand.id = id;
                return midiCommand;
            }
            else 
            {
                return new VideoCommand(name, new Video((String)command.videoPath), id);
            }
        }

        public CommandList ConvertCommandList()
        {
            List<AbstractCommand> listOfCommands = new List<AbstractCommand>();
            List<dynamic> commands = new List<dynamic>(commandList.commands);

            for (var i = 0; i < commands.Count; i++)
            {
                dynamic command = commands[i];
                listOfCommands.Add(this.parseCommand(command));
            }
            return new CommandList(listOfCommands);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("commands", commandListObject);
            info.AddValue("scenes", sceneListObject);
            //info.AddValue("outputs", outputManager);
        }
    }
}
