using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using FluentAssertions;

namespace SimpleVSIX.UnitTests
{
    [TestClass]
    public class LoggerTests
    {
        [TestInitialize]
        public void Initialize()
        {
            SetCurrentThreadAsUIThread();
        }

        [TestMethod]
        public void Ctor_CheckPaneIsInitializedCorrectly()
        {
            var pane = new Mock<IVsOutputWindowPane>();
            var paneObj = pane.Object;

            var outputWindow = SetupOutputWindow(pane.Object);
            var serviceProvider = SetupServiceProvider(outputWindow.Object);

            _ = new Logger(serviceProvider.Object);

            var expectedGuid = Logger.PaneId;

            serviceProvider.Verify(p => p.GetService(typeof(IVsOutputWindow)), Times.Once);
            outputWindow.Verify(p => p.CreatePane(ref expectedGuid, "Step 5", 1, 0), Times.Once);
            outputWindow.Verify(p => p.GetPane(ref expectedGuid, out paneObj), Times.Once);
        }

        [TestMethod]
        public void Log_ExpectedMessageIsLogged()
        {
            var pane = new Mock<IVsOutputWindowPane>();
            var paneObj = pane.Object;
            var outputWindow = SetupOutputWindow(paneObj);
            var serviceProvider = SetupServiceProvider(outputWindow.Object);

            var testSubject = new Logger(serviceProvider.Object);
            string loggerMsg = "Hello";
            testSubject.Log(loggerMsg);

            pane.Verify(p => p.OutputStringThreadSafe(loggerMsg), Times.Once());
        }

        private Mock<IServiceProvider> SetupServiceProvider(IVsOutputWindow outputWindow = null)
        {
            outputWindow ??= Mock.Of<IVsOutputWindow>();

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(p => p.GetService(typeof(IVsOutputWindow))).Returns(outputWindow);

            return serviceProvider;
        }

        private Mock<IVsOutputWindow> SetupOutputWindow(IVsOutputWindowPane pane = null)
        {
            pane ??= Mock.Of<IVsOutputWindowPane>();

            var outputWindow = new Mock<IVsOutputWindow>();
            outputWindow.Setup(p => p.CreatePane(ref It.Ref<Guid>.IsAny, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()));
            outputWindow.Setup(p => p.GetPane(ref It.Ref<Guid>.IsAny, out pane));

            return outputWindow;
        }

        public static void SetCurrentThreadAsUIThread()
        {
            var methodInfo = typeof(Microsoft.VisualStudio.Shell.ThreadHelper).GetMethod("SetUIThread", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            methodInfo.Should().NotBeNull("Could not find ThreadHelper.SetUIThread");
            methodInfo.Invoke(null, null);
        }
    }
}