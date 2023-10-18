using LedCube.Core.UI.Util;
using LedCube.Core.Util;
using Xunit.Abstractions;

namespace LedCube.Test.Util;

public class TestUtils : TestWithLoggingBase
{
    public TestUtils(ITestOutputHelper output) : base(output)
    {
    }
    
    [Fact]
    public void TestPercentile()
    {
        double[] data = new double[] {5, 7, 8, 9};
        Assert.Equal(7.5, StatisticList.Percentile(0.5, ref data));
        Assert.Equal(8.5, StatisticList.Percentile(0.75, ref data));
        Assert.Equal(6, StatisticList.Percentile(0.25, ref data));
        Assert.Equal(7, StatisticList.Percentile(2.0/5.0, ref data));
        Assert.Equal(9, StatisticList.Percentile(0.99, ref data));
        Assert.Equal(5, StatisticList.Percentile(0.11, ref data));
    }

    [Fact]
    public void Test1ElementList()
    {
        var list = new StatisticList(10);
        list.AddValue(42);

        Assert.Equal(42, list.Max);
        Assert.Equal(42, list.Min);
        Assert.Equal(42, list.Last);
        Assert.Equal(42, list.Mean);
        Assert.Equal(42, list.Median);
        Assert.Equal(42, list.Pct05);
        Assert.Equal(42, list.Pct95);
        Assert.Equal(42, list.Pct99);
        Assert.Equal(0, list.StdDev);
        Assert.Equal(0, list.StdErr);
        
        Assert.Equal(10, list.Samples);
        Assert.Equal(1, list.CurrentSamples);
        
    }
    
    [Fact]
    public void Test2ElementsList()
    {
        var list = new StatisticList(10);
        
        list.AddValue(20);
        list.AddValue(10);

        Assert.Equal(20, list.Max);
        Assert.Equal(10, list.Min);
        Assert.Equal(10, list.Last);
        Assert.Equal(15, list.Mean);
        Assert.Equal(15, list.Median);
        Assert.Equal(10, list.Pct05);
        Assert.Equal(20, list.Pct95);
        Assert.Equal(20, list.Pct99);
        Assert.Equal(5, list.StdDev);
        Assert.Equal(5/Math.Sqrt(2), list.StdErr);
        
        Assert.Equal(10, list.Samples);
        Assert.Equal(2, list.CurrentSamples);
        
    }
    
    [Fact]
    public void Test3ElementsList()
    {
        var list = new StatisticList(10);
        
        list.AddValue(20);
        list.AddValue(10);
        list.AddValue(30);

        Assert.Equal(30, list.Max);
        Assert.Equal(10, list.Min);
        Assert.Equal(30, list.Last);
        Assert.Equal(20, list.Mean);
        Assert.Equal(20, list.Median);
        Assert.Equal(10, list.Pct05);
        Assert.Equal(30, list.Pct95);
        Assert.Equal(30, list.Pct99);
        Assert.Equal(8.1649658093, Math.Round(list.StdDev, 10));
        Assert.Equal(4.7140452079, Math.Round(list.StdErr, 10));
        //8.164965809277260000000
        //4.714045207910320000000
        
        Assert.Equal(10, list.Samples);
        Assert.Equal(3, list.CurrentSamples);
    }
    
    [Fact]
    public void TestListOverflow()
    {
        var list = new StatisticList(5);
        
        list.AddValue(20); //Replaced
        list.AddValue(19); //Replaced
        list.AddValue(18); //Replaced
        list.AddValue(17);
        list.AddValue(16);
        list.AddValue(15);
        list.AddValue(14);
        list.AddValue(13);

        Assert.Equal(17, list.Max);
        Assert.Equal(13, list.Min);
        Assert.Equal(13, list.Last);
        Assert.Equal(5, list.Samples);
        Assert.Equal(5, list.CurrentSamples);
    }
}