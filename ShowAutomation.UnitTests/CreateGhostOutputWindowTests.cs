using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Models.OutputDevices;
using ShowAutomation.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowAutomation.UI.Views.Tests
{
    
        [TestClass()]
        public class CreateGhostOutputWindowTests
        {
            [TestMethod()]
            public CreateGhostOutputWindow Constructor(OutputDevice device, Func<OutputDevice, OutputDevice> addGhostCallback)
            {
                CreateGhostOutputWindow target = new CreateGhostOutputWindow(device, addGhostCallback);
                return target;
            }

        }

      
    
}