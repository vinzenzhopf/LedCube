using System;
using System.Collections.Generic;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Buffer;
using LedCube.Core.Common.Model.Cube.Event;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace LedCube.Test.Core;

public abstract class CubeDataTestsBase<TCubeDataBuffer> : TestWithLoggingBase where TCubeDataBuffer : struct, ICubeDataBuffer<TCubeDataBuffer>
{
    public Point3D CubeSize { get; }
    
    public int CubeLength { get; }
    
    public CubeDataTestsBase(ITestOutputHelper output) : base(output)
    {
        CubeSize = TCubeDataBuffer.Size;
        CubeLength = TCubeDataBuffer.Length;
    }

    private void AssertCubeIsOff(CubeData<TCubeDataBuffer> sut)
    {
        for (var i = 0; i < CubeLength; i++)
        {
            Assert.False(sut.GetLed(i));
        }
    }

    private int CountActivePoints(CubeData<TCubeDataBuffer> sut)
    {
        int count = 0;
        for (var i = 0; i < CubeLength; i++)
        {
            if (sut.GetLed(i)) count++;
        }
        return count;
    }
    
    [Fact]
    public void TestCreation()
    {
        var sut = new CubeData<TCubeDataBuffer>();
        
        Assert.Equal(CubeSize, sut.Size);

        var logger = LoggerFactory.CreateLogger(GetType());
        logger.LogDebug("Test {Size}", sut.Size);
    }

    [Fact]
    public void TestSetAndGet()
    {
        var testPoints = new List<Point3D>()
        {
            new(0, 0, 0),
            new(1, 1, 1),
            new(CubeSize.X - 1, 0, 0),
            new(0, CubeSize.Y - 1, 0),
            new(0, 0, CubeSize.Z - 1),
            new(CubeSize.X - 1, CubeSize.Y - 1, CubeSize.Z - 1),
            new(1, 0, 0),
            new(0, 1, 0),
            new(0, 0, 1),
            new(2, 2, 2),
            new(2, 0, 2),
        };
        
        var sut = new CubeData<TCubeDataBuffer>();
        //Check cube is complete off
        AssertCubeIsOff(sut);

        //for each test-point: check, set, check and verify the active led count
        for (var i = 0; i < testPoints.Count; i++)
        {
            var testPoint = testPoints[i];
            Logger.LogInformation("Setting Point: {Number}={Point}", i, testPoint);
            Assert.False(sut.GetLed(testPoint));
            sut.SetLed(testPoint, true);
            Assert.True(sut.GetLed(testPoint));
            Assert.Equal(i + 1, CountActivePoints(sut));
        }
        
        //for each test-point: check, clear, check and verify the active led count
        for (var i = testPoints.Count-1; i >= 0; i--)
        {
            var testPoint = testPoints[i];
            Logger.LogInformation("Clearing Point: {Number}={Point}", i, testPoint);
            Assert.True(sut.GetLed(testPoint));
            sut.SetLed(testPoint, false);
            Assert.False(sut.GetLed(testPoint));
            Assert.Equal(i, CountActivePoints(sut));
        }
        AssertCubeIsOff(sut);
    }

    [Fact]
    public void TestGetAndSetOutsideBounds()
    {
        var testPoints = new List<Point3D>()
        {
            new(-1, -1, -1),
            new(CubeSize.X, 0, 0),
            new(0, CubeSize.Y, 0),
            new(0, 0, CubeSize.Z),
            new(CubeSize.X, CubeSize.Y, CubeSize.Z)
        };
        
        var sut = new CubeData<TCubeDataBuffer>();
        
        for (var i = 0; i < testPoints.Count; i++)
        {
            var testPoint = testPoints[i];
            Logger.LogInformation("Testing Point: {Number}={Point}", i, testPoint);
            Assert.ThrowsAny<Exception>(() => sut.GetLed(testPoint));
            Assert.ThrowsAny<Exception>(() => sut.SetLed(testPoint, true));
        }
    }
    
    [Fact]
    public void TestLedChangedEvent()
    {
        var testPoints = new List<Point3D>()
        {
            new(0, 0, 0),
            new(1, 1, 1),
            new(CubeSize.X - 1, 0, 0),
            new(0, CubeSize.Y - 1, 0),
            new(0, 0, CubeSize.Z - 1),
            new(CubeSize.X - 1, CubeSize.Y - 1, CubeSize.Z - 1),
            new(1, 0, 0),
            new(0, 1, 0),
            new(0, 0, 1),
            new(2, 2, 2),
            new(2, 0, 2),
        };
        var sut = new CubeData<TCubeDataBuffer>();
        sut.LedChanged += OnLedChanged;
        
        Tuple<Point3D, bool>? eventData = null;
        void OnLedChanged(object? sender, LegChangedEventArgs<Point3D> args)
        {
            Logger.LogDebug("Receiving event for Point: {Point} Value: {Value}", args.Position, args.Value);
            eventData = new Tuple<Point3D, bool>(args.Position, args.Value);
        }
        void AssertLedChangeTriggered(Point3D point3D, bool value)
        {
            Assert.NotNull(eventData);
            Assert.Equal(point3D, eventData.Item1);
            Assert.Equal(value, eventData.Item2);
        }
        
        for (var i = 0; i < testPoints.Count; i++)
        {
            var testPoint = testPoints[i];
            eventData = null;
            const bool ledData = true;
            Logger.LogInformation("Setting Point: {Number}={Point}", i, testPoint);
            sut.SetLed(testPoint, ledData);
            AssertLedChangeTriggered(testPoint, ledData);
        }
        for (var i = 0; i < testPoints.Count; i++)
        {
            var testPoint = testPoints[i];
            eventData = null;
            const bool ledData = false;
            Logger.LogInformation("Clearing Point: {Number}={Point}", i, testPoint);
            sut.SetLed(testPoint, ledData);
            AssertLedChangeTriggered(testPoint, ledData);
        }
    }

    [Fact]
    public void TestLedChangedByNoChangeEvent()
    {
        var testPoints = new List<Point3D>()
        {
            new(0, 0, 0),
            new(1, 1, 1),
            new(CubeSize.X - 1, 0, 0),
            new(0, CubeSize.Y - 1, 0),
            new(0, 0, CubeSize.Z - 1),
            new(CubeSize.X - 1, CubeSize.Y - 1, CubeSize.Z - 1),
        };
        var sut = new CubeData<TCubeDataBuffer>();
        sut.LedChanged += OnLedChanged;
        
        Tuple<Point3D, bool>? eventData = null;
        void OnLedChanged(object? sender, LegChangedEventArgs<Point3D> args)
        {
            Logger.LogDebug("Receiving event for Point: {Point} Value: {Value}", args.Position, args.Value);
            eventData = new Tuple<Point3D, bool>(args.Position, args.Value);
        }
        void AssertLedChangeTriggered(Point3D point3D, bool value)
        {
            Assert.NotNull(eventData);
            Assert.Equal(point3D, eventData.Item1);
            Assert.Equal(value, eventData.Item2);
        }
        
        for (var i = 0; i < testPoints.Count; i++)
        {
            var testPoint = testPoints[i];
            
            eventData = null;
            Logger.LogInformation("Clearing Point: {Number}={Point}", i, testPoint);
            sut.SetLed(testPoint, false);
            //Assume no retrigger when value is already set.
            Assert.Null(eventData);

            eventData = null;
            Logger.LogInformation("Setting Point: {Number}={Point}", i, testPoint);
            sut.SetLed(testPoint, true);
            AssertLedChangeTriggered(testPoint, true);
            
            eventData = null;
            Logger.LogInformation("Setting Point: {Number}={Point}", i, testPoint);
            sut.SetLed(testPoint, true);
            //Assume no retrigger when value is already set.
            Assert.Null(eventData);
            
            eventData = null;
            Logger.LogInformation("Clearing Point: {Number}={Point}", i, testPoint);
            sut.SetLed(testPoint, false);
            AssertLedChangeTriggered(testPoint, false);
        }
    }
}