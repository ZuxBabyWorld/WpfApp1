using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WpfApp1;

//凹凸缺陷检测
public class BumpDefectProcessor : IImageProcessor
{
    //调参
    private HTuple _sigma1 = 5, _sigma2 = 1;
    private double _thresholdRateMax = 0.5;
    private int _selectAreaMin = 1;
    private int _selectAreaMax = 99999;

    //中间变量
    private HObject fftImage, gaussFilter1, gaussFilter2, filter;
    private HObject filteredImage, convolImage, resultImage;
    private HObject region;
    private HTuple min, max, range;
    private HObject connectedRegions, selectedRegions, dilatedRegion, unionRegion;
    private HObject closedRegion, finalRegions, finalSelected;
    private HTuple finalArea, finalRow, finalColumn;

    private int ngCount = 0;

    // 添加参数设置方法
    public void SetParametersByConfig(AlgorithmConfig config)
    {
        _sigma1 = new HTuple(config.Sigma1);
        _sigma2 = new HTuple(config.Sigma2);
        _thresholdRateMax = config.ThresholdRateMax;
        _selectAreaMin = config.SelectAreaMin;
        _selectAreaMax = config.SelectAreaMax;
    }

    public HObject Process(HObject sourceImage, HTuple width, HTuple height)
    {
        // 傅里叶变换滤波
        HOperatorSet.RftGeneric(sourceImage, out fftImage, "to_freq", "none", "complex", width);
        HOperatorSet.GenGaussFilter(out gaussFilter1, _sigma1, _sigma1, 0, "none", "rft", width, height);
        HOperatorSet.GenGaussFilter(out gaussFilter2, _sigma2, _sigma2, 0, "none", "rft", width, height);
        HOperatorSet.SubImage(gaussFilter1, gaussFilter2, out filter, 1, 0);
        HOperatorSet.ConvolFft(fftImage, filter, out convolImage);
        HOperatorSet.RftGeneric(convolImage, out filteredImage, "from_freq", "n", "real", width);

        // 灰度范围增强
        HOperatorSet.GrayRangeRect(filteredImage, out resultImage, 5, 5);

        // 动态阈值分割
        HOperatorSet.MinMaxGray(resultImage, resultImage, 0, out min, out max, out range);
        HOperatorSet.Threshold(resultImage, out region, ((new HTuple(5.55)).TupleConcat(max * _thresholdRateMax)).TupleMax(), 255);

        // 形态学处理
        HOperatorSet.Connection(region, out connectedRegions);
        HOperatorSet.SelectShape(connectedRegions, out selectedRegions, "area", "and", 1, 99999);
        HOperatorSet.DilationCircle(selectedRegions, out dilatedRegion, 5.5);
        HOperatorSet.Union1(dilatedRegion, out unionRegion);
        HOperatorSet.ClosingCircle(unionRegion, out closedRegion, 10);
        HOperatorSet.Connection(closedRegion, out finalRegions);
        HOperatorSet.SelectShape(finalRegions, out finalSelected, "area", "and", _selectAreaMin, _selectAreaMax);
        HOperatorSet.AreaCenter(finalSelected, out finalArea, out finalRow, out finalColumn);
        ngCount = finalArea.TupleLength();

        return finalSelected;
    }

    public int NgCount()
    {
        return ngCount;
    }

    ~BumpDefectProcessor()
    {
        fftImage?.Dispose();
        gaussFilter1?.Dispose();
        gaussFilter2?.Dispose();
        filter?.Dispose();
        filteredImage?.Dispose();
        convolImage?.Dispose();
        resultImage?.Dispose();
        region?.Dispose();
        connectedRegions?.Dispose();
        selectedRegions?.Dispose();
        dilatedRegion?.Dispose();
        unionRegion?.Dispose();
        closedRegion?.Dispose();
        finalRegions?.Dispose();
        finalSelected?.Dispose();
    }
}