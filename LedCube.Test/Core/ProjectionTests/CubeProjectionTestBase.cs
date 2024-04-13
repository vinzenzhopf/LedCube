using System;
using System.Collections.Generic;
using LedCube.Core.Common.Model;
using LedCube.Core.Common.Model.Cube;
using LedCube.Core.Common.Model.Cube.Buffer;
using LedCube.Core.Common.Model.Cube.Event;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace LedCube.Test.Core.ProjectionTests;


public abstract class CubeProjectionTestBase<TCubeDataBuffer> : TestWithLoggingBase where TCubeDataBuffer : struct, ICubeDataBuffer<TCubeDataBuffer>
{
    public ICubeData Sut { get; set; }
    public CubeData<TCubeDataBuffer> CubeData { get; }
    
    public CubeProjectionTestBase(ITestOutputHelper output, CubeData<TCubeDataBuffer> cubeData) : base(output)
    {
        CubeData = cubeData;
        Sut = CubeData;
    }
    
    private void AssertCubeIsOff()
    {
        for (var i = 0; i < CubeData.Size.ElementProduct; i++)
        {
            Assert.False(CubeData.GetLed(i));
        }
    }

    private int CountActivePoints()
    {
        var count = 0;
        for (var i = 0; i < CubeData.Size.ElementProduct; i++)
        {
            if (CubeData.GetLed(i)) count++;
        }
        return count;
    }

    [Fact]
    public void TestSutGetSize()
    {
        var p = Sut.Size;
        Assert.True(p.X > 0, "X is <= 0");
        Assert.True(p.Y > 0, "Y is <= 0");
        Assert.True(p.Z > 0, "Z is <= 0");
    }
    
    [Fact]
    public void TestSetAndGet()
    {
        var testPoints = new List<Point3D>()
        {
            new(0, 0, 0),
            new(1, 1, 1),
            new(Sut.Size.X - 1, 0, 0),
            new(0, Sut.Size.Y - 1, 0),
            new(0, 0, Sut.Size.Z - 1),
            new(Sut.Size.X - 1, Sut.Size.Y - 1, Sut.Size.Z - 1),
            new(1, 0, 0),
            new(0, 1, 0),
            new(0, 0, 1),
            new(2, 2, 2),
            new(2, 0, 2),
        };

        //Check cube is complete off
        AssertCubeIsOff();
        
        //for each test-point: check, set, check and verify the active led count
        for (var i = 0; i < testPoints.Count; i++)
        {
            var testPoint = testPoints[i];
            Logger.LogInformation("Setting Point: {Number}={Point}", i, testPoint);
            Assert.False(Sut.GetLed(testPoint));
            Sut.SetLed(testPoint, true);
            Assert.True(Sut.GetLed(testPoint));
        }
        
        //for each test-point: check, clear, check and verify the active led count
        for (var i = testPoints.Count-1; i >= 0; i--)
        {
            var testPoint = testPoints[i];
            Logger.LogInformation("Clearing Point: {Number}={Point}", i, testPoint);
            Assert.True(Sut.GetLed(testPoint));
            Sut.SetLed(testPoint, false);
            Assert.False(Sut.GetLed(testPoint));
            Assert.Equal(i, CountActivePoints());
        }
        AssertCubeIsOff();
    }
    
    [Fact]
    public void TestGetAndSetOutsideBounds()
    {
        var testPoints = new List<Point3D>()
        {
            new(-1, -1, -1),
            new(Sut.Size.X, 0, 0),
            new(0, Sut.Size.Y, 0),
            new(0, 0, Sut.Size.Z),
            new(Sut.Size.X, Sut.Size.Y, Sut.Size.Z)
        };
        
        for (var i = 0; i < testPoints.Count; i++)
        {
            var testPoint = testPoints[i];
            Logger.LogInformation("Testing Point: {Number}={Point}", i, testPoint);
            Assert.ThrowsAny<Exception>(() => Sut.GetLed(testPoint));
            Assert.ThrowsAny<Exception>(() => Sut.SetLed(testPoint, true));
        }
    }

    [Fact]
    public void TestLedChangedEvent()
    {
        var testPoints = new List<Point3D>()
        {
            new(0, 0, 0),
            new(1, 1, 1),
            new(Sut.Size.X - 1, 0, 0),
            new(0, Sut.Size.Y - 1, 0),
            new(0, 0, Sut.Size.Z - 1),
            new(Sut.Size.X - 1, Sut.Size.Y - 1, Sut.Size.Z - 1),
            new(1, 0, 0),
            new(0, 1, 0),
            new(0, 0, 1),
            new(2, 2, 2),
            new(2, 0, 2),
        };
        Sut.LedChanged += OnLedChanged;
        
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
            Sut.SetLed(testPoint, ledData);
            AssertLedChangeTriggered(testPoint, ledData);
        }
        for (var i = 0; i < testPoints.Count; i++)
        {
            var testPoint = testPoints[i];
            eventData = null;
            const bool ledData = false;
            Logger.LogInformation("Clearing Point: {Number}={Point}", i, testPoint);
            Sut.SetLed(testPoint, ledData);
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
            new(Sut.Size.X - 1, 0, 0),
            new(0, Sut.Size.Y - 1, 0),
            new(0, 0, Sut.Size.Z - 1),
            new(Sut.Size.X - 1, Sut.Size.Y - 1, Sut.Size.Z - 1),
        };
        Sut.LedChanged += OnLedChanged;
        
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
            Sut.SetLed(testPoint, false);
            //Assume no retrigger when value is already set.
            Assert.Null(eventData);

            eventData = null;
            Logger.LogInformation("Setting Point: {Number}={Point}", i, testPoint);
            Sut.SetLed(testPoint, true);
            AssertLedChangeTriggered(testPoint, true);
            
            eventData = null;
            Logger.LogInformation("Setting Point: {Number}={Point}", i, testPoint);
            Sut.SetLed(testPoint, true);
            //Assume no retrigger when value is already set.
            Assert.Null(eventData);
            
            eventData = null;
            Logger.LogInformation("Clearing Point: {Number}={Point}", i, testPoint);
            Sut.SetLed(testPoint, false);
            AssertLedChangeTriggered(testPoint, false);
        }
    }
}