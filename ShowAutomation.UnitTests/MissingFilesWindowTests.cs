using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowAutomation.Domain.Models.Media;

namespace ShowAutomation.UI.Views.Tests
{
    [TestClass()]
    public class MissingFilesWindowTests
    {
        [TestMethod()]
        public MissingFilesWindow ConstructorTest(string path, MediaModel media)
        {
            path = @"Samples\WallonLilliShort.mp3";
            media = media as Song;
            MissingFilesWindow target = new MissingFilesWindow(path, media);
            
            return target;
           
        }

        /*
        /// <summary>Test stub for btnImportFileClick(Object, RoutedEventArgs)</summary>
        [TestMethod]
        public void btnImportFileClickTest(MissingFilesWindow target, object sender, RoutedEventArgs e)
        {
            target.btnImportFileClick(sender, e);
            Assert.Inconclusive();
        }
        */
    }
}
