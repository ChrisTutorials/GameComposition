using System;
using Xunit;
using BarkMoon.GameComposition.Core.Events;

namespace BarkMoon.GameComposition.Core.Tests.Events
{
    public class ServiceEventTests
    {
        private class TestServiceEvent : ServiceEvent
        {
            public TestServiceEvent(object source, object? data = null) : base(source, data) { }
        }

        [Fact]
        public void ServiceEvent_Constructor_SetsProperties()
        {
            // Arrange
            var source = "TestService";
            var data = "TestData";

            // Act
            var serviceEvent = new TestServiceEvent(source, data);

            // Assert
            Assert.Equal(source, serviceEvent.Source);
            Assert.Equal(data, serviceEvent.Data);
            Assert.NotEqual(default(DateTime), serviceEvent.Timestamp);
            Assert.True(serviceEvent.Timestamp <= DateTime.UtcNow);
        }

        [Fact]
        public void ServiceEvent_Timestamp_IsSetToCurrentTime()
        {
            // Arrange
            var beforeCreation = DateTime.UtcNow;
            var source = "TestService";

            // Act
            var serviceEvent = new TestServiceEvent(source);

            // Assert
            var afterCreation = DateTime.UtcNow;
            Assert.True(serviceEvent.Timestamp >= beforeCreation);
            Assert.True(serviceEvent.Timestamp <= afterCreation);
        }

        [Fact]
        public void ServiceEvent_CanSetDataToNull()
        {
            // Arrange
            var source = "TestService";

            // Act
            var serviceEvent = new TestServiceEvent(source, null);

            // Assert
            Assert.Equal(source, serviceEvent.Source);
            Assert.Null(serviceEvent.Data);
        }

        [Fact]
        public void ServiceEvent_WithVariousSourceTypes_HandlesCorrectly()
        {
            // Arrange
            var stringSource = "StringSource";
            var intSource = 42;
            var objectSource = new object();

            // Act
            var stringEvent = new TestServiceEvent(stringSource);
            var intEvent = new TestServiceEvent(intSource);
            var objectEvent = new TestServiceEvent(objectSource);

            // Assert
            Assert.Equal(stringSource, stringEvent.Source);
            Assert.Equal(intSource, intEvent.Source);
            Assert.Equal(objectSource, objectEvent.Source);
        }

        [Fact]
        public void ServiceEvent_MultipleEvents_HaveUniqueTimestamps()
        {
            // Arrange
            var source = "TestService";

            // Act
            var event1 = new TestServiceEvent(source);
            System.Threading.Thread.Sleep(1); // Ensure different timestamps
            var event2 = new TestServiceEvent(source);

            // Assert
            Assert.NotEqual(event1.Timestamp, event2.Timestamp);
            Assert.True(event2.Timestamp > event1.Timestamp);
        }

        [Fact]
        public void ServiceEvent_ToString_ReturnsMeaningfulString()
        {
            // Arrange
            var source = "TestService";
            var serviceEvent = new TestServiceEvent(source);

            // Act
            var result = serviceEvent.ToString();

            // Assert
            Assert.NotNull(result);
            Assert.Contains("BarkMoon.GameComposition.Core.Tests.Events.ServiceEventTests+TestServiceEvent", result);
        }

        [Fact]
        public void ServiceEvent_WithNullSource_HandlesGracefully()
        {
            // Act
            var serviceEvent = new TestServiceEvent(null!);

            // Assert
            Assert.Null(serviceEvent.Source);
            Assert.Null(serviceEvent.Data);
            Assert.NotEqual(default(DateTime), serviceEvent.Timestamp);
        }
    }

    public class ServiceStatusEventTests
    {
        [Fact]
        public void ServiceStatusEvent_Constructor_SetsProperties()
        {
            // Arrange
            var source = "TestService";
            var status = ServiceStatus.Started;
            var data = "TestData";

            // Act
            var statusEvent = new ServiceStatusEvent(source, status, data);

            // Assert
            Assert.Equal(source, statusEvent.Source);
            Assert.Equal(status, statusEvent.Status);
            Assert.Equal(data, statusEvent.Data);
            Assert.NotEqual(default(DateTime), statusEvent.Timestamp);
        }

        [Theory]
        [InlineData(ServiceStatus.Starting)]
        [InlineData(ServiceStatus.Started)]
        [InlineData(ServiceStatus.Stopping)]
        [InlineData(ServiceStatus.Stopped)]
        [InlineData(ServiceStatus.Error)]
        public void ServiceStatusEvent_WithVariousStatuses_HandlesCorrectly(ServiceStatus status)
        {
            // Arrange
            var source = "TestService";

            // Act
            var statusEvent = new ServiceStatusEvent(source, status);

            // Assert
            Assert.Equal(source, statusEvent.Source);
            Assert.Equal(status, statusEvent.Status);
            Assert.Null(statusEvent.Data);
        }

        [Fact]
        public void ServiceStatusEvent_InheritsFromServiceEvent()
        {
            // Arrange
            var source = "TestService";
            var status = ServiceStatus.Started;

            // Act
            var statusEvent = new ServiceStatusEvent(source, status);

            // Assert
            Assert.IsAssignableFrom<ServiceEvent>(statusEvent);
            Assert.NotEqual(default(DateTime), statusEvent.Timestamp);
        }
    }
}
