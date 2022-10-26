using Microsoft.VisualStudio.Shell.Interop;
using SimpleVSIX;
using Moq;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;


namespace SimpleVSIX.UnitTests
{
    [TestClass]
    public class ILoggerTests
    {
        [TestMethod]
        public void CreatePane_Verify_ReturnTrue()
        {
            var outputWindow = SetupOutputWindow();
            var serviceProvider = SetupServiceProvider();
            serviceProvider.Setup(p => p.GetService(typeof(IVsOutputWindow))).Returns(outputWindow.Object);
            
            var pane = SetupPane();
            var paneObj = pane.Object;
            outputWindow.Setup(p => p.GetPane(ref It.Ref<Guid>.IsAny, out paneObj));

            var logger = new Logger(serviceProvider.Object);
            string loggerMsg = "Hello";
            logger.Log(loggerMsg);

            outputWindow.Verify(p => p.CreatePane(ref It.Ref<Guid>.IsAny, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()));
            outputWindow.Verify(p => p.GetPane(ref It.Ref<Guid>.IsAny, out paneObj));
            pane.Verify(p => p.OutputStringThreadSafe(loggerMsg), Times.Once());
        }

        private Mock<IServiceProvider> SetupServiceProvider()
        {
            var serviceProvider = new Mock<IServiceProvider>();

            return serviceProvider;
        }

        private Mock<IVsOutputWindow> SetupOutputWindow()
        {
            var outputWindow = new Mock<IVsOutputWindow>();
            outputWindow.Setup(p => p.CreatePane(ref It.Ref<Guid>.IsAny, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()));

            return outputWindow;
        }

        private Mock<IVsOutputWindowPane> SetupPane()
        {
            var pane = new Mock<IVsOutputWindowPane>();
            pane.Setup(p => p.OutputStringThreadSafe(It.IsAny<string>()));

            return pane;
        }
    }
}