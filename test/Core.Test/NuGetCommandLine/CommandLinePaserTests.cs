using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace NuGet.Test.NuGetCommandLine {
    [TestClass]
    public class CommandLinePaserTests {

        [TestMethod]
        public void GetNextCommandLineItem_ReturnsNullWithNullInput() {
            // Act
            string actualItem = CommandLineParser.GetNextCommandLineItem(null);
            // Assert
            Assert.IsNull(actualItem);
        }

        [TestMethod]
        public void GetNextCommandLineItem_ReturnsNullWithEmptyInput() {
            // Arrange
            var argsEnumerator = new List<string>().GetEnumerator();
            // Act
            string actualItem = CommandLineParser.GetNextCommandLineItem(argsEnumerator);
            // Assert
            Assert.IsNull(actualItem);
        }
        
        [TestMethod]
        public void ParseCommandLine_ThrowsCommandLineExpectionWhenUnknownCommand() {
            // Arrange 
            var cmdMgr = new Mock<ICommandManager>();
            cmdMgr.Setup(cm => cm.GetCommand(It.IsAny<string>())).Returns<ICommand>(null);
            CommandLineParser parser = new CommandLineParser(cmdMgr.Object);
            List<string> input = new List<string>() { "SomeUnknownCommand", "SomeArgs" };
            string expectedExceptionMessage = "Unknown command: 'SomeUnknownCommand'";
            // Act & Assert
            ExceptionAssert.Throws<CommandLineException>(() => parser.ParseCommandLine(input), expectedExceptionMessage);
        }

        [TestMethod]
        public void ExtractOptions_ReturnsEmptyCommandWhenCommandLineIsEmpty() {
            // Arrange
            var cmdMgr = new Mock<ICommandManager>();
            var MockCommandOptions = new Dictionary<OptionAttribute, PropertyInfo>();
            var mockOptionAttribute = new OptionAttribute("Mock Option");
            var mockPropertyInfo = typeof(MockCommand).GetProperty("Message");
            MockCommandOptions.Add(mockOptionAttribute, mockPropertyInfo);
            cmdMgr.Setup(cm => cm.GetCommandOptions(It.IsAny<ICommand>())).Returns(MockCommandOptions);
            cmdMgr.Setup(cm => cm.GetCommandAttribute(It.IsAny<ICommand>())).Returns(new CommandAttribute("Mock", "Mock Command Attirbute"));
            CommandLineParser parser = new CommandLineParser(cmdMgr.Object);
            var ExpectedCommand = new MockCommand();
            var argsEnumerator = new List<string>().GetEnumerator();
            // Act
            ICommand actualCommand = parser.ExtractOptions(ExpectedCommand, argsEnumerator);
            // Assert
            Assert.AreEqual(0, actualCommand.Arguments.Count);
            Assert.IsNull(((MockCommand)actualCommand).Message);
        }

        [TestMethod]
        public void ExtractOptions_AddsArgumentsWhenItemsDoNotStartWithSlashOrDash() {
            // Arrange
            var cmdMgr = new Mock<ICommandManager>();
            var MockCommandOptions = new Dictionary<OptionAttribute, PropertyInfo>();
            var mockOptionAttribute = new OptionAttribute("Mock Option");
            var mockPropertyInfo = typeof(MockCommand).GetProperty("Message");
            MockCommandOptions.Add(mockOptionAttribute, mockPropertyInfo);
            cmdMgr.Setup(cm => cm.GetCommandOptions(It.IsAny<ICommand>())).Returns(MockCommandOptions);
            cmdMgr.Setup(cm => cm.GetCommandAttribute(It.IsAny<ICommand>())).Returns(new CommandAttribute("Mock", "Mock Command Attirbute"));
            CommandLineParser parser = new CommandLineParser(cmdMgr.Object);
            var ExpectedCommand = new MockCommand();
            var argsEnumerator = new List<string>() { "optionOne", "optionTwo" }.GetEnumerator();
            // Act
            ICommand actualCommand = parser.ExtractOptions(ExpectedCommand, argsEnumerator );
            // Assert
            Assert.AreEqual(2, actualCommand.Arguments.Count);
            Assert.AreEqual("optionOne", actualCommand.Arguments[0]);
            Assert.AreEqual("optionTwo", actualCommand.Arguments[1]);
            Assert.IsNull(((MockCommand)actualCommand).Message);
        }

        [TestMethod]
        public void ExtractOptions_ThrowsCommandLineExpectionWhenOptionUnknow() {
            // Arrange
            var cmdMgr = new Mock<ICommandManager>();
            var MockCommandOptions = new Dictionary<OptionAttribute, PropertyInfo>();
            var mockOptionAttribute = new OptionAttribute("Mock Option");
            var mockPropertyInfo = typeof(MockCommand).GetProperty("Message");
            MockCommandOptions.Add(mockOptionAttribute, mockPropertyInfo);
            cmdMgr.Setup(cm => cm.GetCommandOptions(It.IsAny<ICommand>())).Returns(MockCommandOptions);
            cmdMgr.Setup(cm => cm.GetCommandAttribute(It.IsAny<ICommand>())).Returns(new CommandAttribute("Mock", "Mock Command Attirbute"));
            CommandLineParser parser = new CommandLineParser(cmdMgr.Object);
            var ExpectedCommand = new MockCommand();
            string expectedErrorMessage = "Unknown option: '/NotAnOption'";
            var argsEnumerator = new List<string>() {"/NotAnOption"}.GetEnumerator();
            // Act & Assert
            ExceptionAssert.Throws<CommandLineException>(() => parser.ExtractOptions(ExpectedCommand, argsEnumerator), expectedErrorMessage);
        }

        [TestMethod]
        public void ExtractOptions_ParsesOptionsThatStartWithSlash() {
            // Arrange
            var cmdMgr = new Mock<ICommandManager>();
            var MockCommandOptions = new Dictionary<OptionAttribute, PropertyInfo>();
            var mockOptionAttribute = new OptionAttribute("Mock Option");
            var mockPropertyInfo = typeof(MockCommand).GetProperty("Message");
            MockCommandOptions.Add(mockOptionAttribute, mockPropertyInfo);
            cmdMgr.Setup(cm => cm.GetCommandOptions(It.IsAny<ICommand>())).Returns(MockCommandOptions);
            cmdMgr.Setup(cm => cm.GetCommandAttribute(It.IsAny<ICommand>())).Returns(new CommandAttribute("Mock", "Mock Command Attirbute"));
            CommandLineParser parser = new CommandLineParser(cmdMgr.Object);
            var ExpectedCommand = new MockCommand();
            var argsEnumerator = new List<string>() { "/Message", "foo bar" }.GetEnumerator();
            // Act
            ICommand actualCommand = parser.ExtractOptions(ExpectedCommand, argsEnumerator);
            // Assert
            Assert.AreEqual("foo bar", ((MockCommand)actualCommand).Message);
        }

        [TestMethod]
        public void ExtractOptions_ParsesOptionsThatStartWithDash() {
            // Arrange
            var cmdMgr = new Mock<ICommandManager>();
            var MockCommandOptions = new Dictionary<OptionAttribute, PropertyInfo>();
            var mockOptionAttribute = new OptionAttribute("Mock Option");
            var mockPropertyInfo = typeof(MockCommand).GetProperty("Message");
            MockCommandOptions.Add(mockOptionAttribute, mockPropertyInfo);
            cmdMgr.Setup(cm => cm.GetCommandOptions(It.IsAny<ICommand>())).Returns(MockCommandOptions);
            cmdMgr.Setup(cm => cm.GetCommandAttribute(It.IsAny<ICommand>())).Returns(new CommandAttribute("Mock", "Mock Command Attirbute"));
            CommandLineParser parser = new CommandLineParser(cmdMgr.Object);
            var ExpectedCommand = new MockCommand();
            var argsEnumerator = new List<string>() { "-Message", "foo bar" }.GetEnumerator();
            // Act
            ICommand actualCommand = parser.ExtractOptions(ExpectedCommand, argsEnumerator);
            // Assert
            Assert.AreEqual("foo bar", ((MockCommand)actualCommand).Message);
        }

        [TestMethod]
        public void ExtractOptions_ThrowsWhenOptionHasNoValue() {
            // Arrange
            var cmdMgr = new Mock<ICommandManager>();
            var MockCommandOptions = new Dictionary<OptionAttribute, PropertyInfo>();
            var mockOptionAttribute = new OptionAttribute("Mock Option");
            var mockPropertyInfo = typeof(MockCommand).GetProperty("Message");
            MockCommandOptions.Add(mockOptionAttribute, mockPropertyInfo);
            cmdMgr.Setup(cm => cm.GetCommandOptions(It.IsAny<ICommand>())).Returns(MockCommandOptions);
            cmdMgr.Setup(cm => cm.GetCommandAttribute(It.IsAny<ICommand>())).Returns(new CommandAttribute("Mock", "Mock Command Attirbute"));
            CommandLineParser parser = new CommandLineParser(cmdMgr.Object);
            var ExpectedCommand = new MockCommand();
            string expectedErrorMessage = "Missing option value for: '/Message'";
            var argsEnumerator = new List<string>() { "/Message" }.GetEnumerator();

            // Act & Assert
            ExceptionAssert.Throws<CommandLineException>(() => parser.ExtractOptions(ExpectedCommand, argsEnumerator), expectedErrorMessage);
        }

        [TestMethod]
        public void ExtractOptions_ParsesBoolOptionsAsTrueIfPresent() {
            // Arrange
            var cmdMgr = new Mock<ICommandManager>();
            var MockCommandOptions = new Dictionary<OptionAttribute, PropertyInfo>();
            var mockOptionAttribute = new OptionAttribute("Mock Option");
            var mockPropertyInfo = typeof(MockCommand).GetProperty("IsWorking");
            MockCommandOptions.Add(mockOptionAttribute, mockPropertyInfo);
            cmdMgr.Setup(cm => cm.GetCommandOptions(It.IsAny<ICommand>())).Returns(MockCommandOptions);
            cmdMgr.Setup(cm => cm.GetCommandAttribute(It.IsAny<ICommand>())).Returns(new CommandAttribute("Mock", "Mock Command Attirbute"));
            CommandLineParser parser = new CommandLineParser(cmdMgr.Object);
            var ExpectedCommand = new MockCommand();
            var argsEnumerator = new List<string>() { "-IsWorking" }.GetEnumerator();
            // Act
            ICommand actualCommand = parser.ExtractOptions(ExpectedCommand, argsEnumerator);
            // Assert
            Assert.IsTrue(((MockCommand)actualCommand).IsWorking);
        }

        [TestMethod]
        public void ExtractOptions_ParsesBoolOptionsAsFalseIfFollowedByDash() {
            // Arrange
            var cmdMgr = new Mock<ICommandManager>();
            var MockCommandOptions = new Dictionary<OptionAttribute, PropertyInfo>();
            var mockOptionAttribute = new OptionAttribute("Mock Option");
            var mockPropertyInfo = typeof(MockCommand).GetProperty("IsWorking");
            MockCommandOptions.Add(mockOptionAttribute, mockPropertyInfo);
            cmdMgr.Setup(cm => cm.GetCommandOptions(It.IsAny<ICommand>())).Returns(MockCommandOptions);
            cmdMgr.Setup(cm => cm.GetCommandAttribute(It.IsAny<ICommand>())).Returns(new CommandAttribute("Mock", "Mock Command Attirbute"));
            CommandLineParser parser = new CommandLineParser(cmdMgr.Object);
            var ExpectedCommand = new MockCommand();
            var argsEnumerator = new List<string>() { "-IsWorking-" }.GetEnumerator();
            // Act
            ICommand actualCommand = parser.ExtractOptions(ExpectedCommand, argsEnumerator);
            // Assert
            Assert.IsFalse(((MockCommand)actualCommand).IsWorking);
        }

        [TestMethod]
        public void ExtractOptions_ThrowsIfUnableToConvertType() {
            // Arrange
            var cmdMgr = new Mock<ICommandManager>();

            var MockCommandOptions = new Dictionary<OptionAttribute, PropertyInfo>();
            var mockOptionAttribute = new OptionAttribute("Mock Option");
            var mockPropertyInfo = typeof(MockCommand).GetProperty("Count");
            MockCommandOptions.Add(mockOptionAttribute, mockPropertyInfo);

            cmdMgr.Setup(cm => cm.GetCommandOptions(It.IsAny<ICommand>())).Returns(MockCommandOptions);
            cmdMgr.Setup(cm => cm.GetCommandAttribute(It.IsAny<ICommand>())).Returns(new CommandAttribute("Mock", "Mock Command Attirbute"));
            CommandLineParser parser = new CommandLineParser(cmdMgr.Object);
            var ExpectedCommand = new MockCommand();
            string expectedErrorMessage = "Invalid option value: '/Count null'";
            var argsEnumerator = new List<string>() { "/Count", "null" }.GetEnumerator();
            // Act & Assert
            ExceptionAssert.Throws<CommandLineException>(() => parser.ExtractOptions(ExpectedCommand, argsEnumerator), expectedErrorMessage);
        }

        private class MockCommand : ICommand {

            public List<string> Arguments { get; set; }

            public string Message { get; set; }

            public bool IsWorking { get; set; }

            public int Count { get; set; }

            public void Execute() {
                throw new NotImplementedException();
            }
        }
    }
}
