using HalconDotNet;
using System;
using System.Windows;

public class HalconCore : IDisposable
{
    private HTuple _acqHandle = null;
    private bool _isCameraOpen = false;

    // 缓存数据
    private HTuple _acqWidth = null;
    public HTuple AcqWidth { get { return _acqWidth; } }

    private HTuple _acqHeight = null;
    public HTuple AcqHeight { get { return _acqHeight; } }

    private HObject _image = null;
    public HObject Image { get { return _image; } }

    private HObject _grayImage = null;
    public HObject GrayImage { get { return _grayImage; } }

    private int _ngCount;
    public int NgCount {  get { return _ngCount; } }

    /// <summary>
    /// 打开相机（支持DirectShow设备）
    /// </summary>
    /// <param name="deviceIndex">相机设备索引（默认0）</param>
    /// <returns>相机句柄</returns>
    public HTuple OpenCamera(int deviceIndex = 0)
    {
        if (_isCameraOpen)
        {
            return _acqHandle;
        }
        try
        {
            // 释放旧资源
            CloseCamera();

            // 打开相机
            HOperatorSet.OpenFramegrabber("DirectShow", 1, 1, 0, 0, 0, 0, "default", 8, "rgb",
                -1, "false", "default", "[0] ", 0, -1, out _acqHandle);

            // 获取图像尺寸
            HOperatorSet.GetFramegrabberParam(_acqHandle, "image_width", out _acqWidth);
            HOperatorSet.GetFramegrabberParam(_acqHandle, "image_height", out _acqHeight);

            _isCameraOpen = true;
            return _acqHandle;
        }
        catch (HOperatorException ex)
        {
            MessageBox.Show($"打开相机失败：{ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 采集单帧图像
    /// </summary>
    /// <returns>采集到的Halcon图像对象</returns>
    public HObject CaptureGrayImage(HTuple acqHanle = null)
    {
        if (acqHanle == null)
        {
            acqHanle = _acqHandle;
        }
        if (!_isCameraOpen)
        {
            MessageBox.Show("请先打开相机");
            return null;
        }

        try
        {
            HOperatorSet.GrabImageAsync(out _image, acqHanle, -1);
            HOperatorSet.Rgb1ToGray(_image, out _grayImage); // 转为灰度图
            return _grayImage;
        }
        catch (HOperatorException ex)
        {
            MessageBox.Show($"图像采集失败：{ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 关闭相机并释放资源
    /// </summary>
    public void CloseCamera()
    {
        if (_acqHandle != null && !_acqHandle.Equals(0))
        {
            HOperatorSet.CloseFramegrabber(_acqHandle);
        }
        _acqHandle = null;
        _acqWidth = null;
        _acqHeight = null;
        _isCameraOpen = false;
        ReleaseImageResources();
    }

    /// <summary>
    /// 执行图像处理
    /// </summary>
    /// <param name="sourceImage">输入图像</param>
    /// <returns>处理后的图像或区域</returns>
    public HObject ProcessImage(HObject sourceImage, IImageProcessor processor, HTuple width = null, HTuple height = null)
    {
        if (width == null && height == null)
        {
            width = _acqWidth; height = _acqHeight;
        }
        if (processor == null)
        {
            MessageBox.Show("未设置图像处理算法");
            return null;
        }
        try
        {
            HObject result = processor.Process(sourceImage, width, height);
            _ngCount = processor.NgCount();
            return result;
        }
        catch (HOperatorException ex)
        {
            MessageBox.Show($"图像处理失败：{ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 释放图像资源
    /// </summary>
    private void ReleaseImageResources()
    {
        _image?.Dispose();
        _grayImage?.Dispose();
        HOperatorSet.GenEmptyObj(out _image);
        HOperatorSet.GenEmptyObj(out _grayImage);
    }

    /// <summary>
    /// 实现IDisposable接口
    /// </summary>
    public void Dispose()
    {
        CloseCamera();
        GC.SuppressFinalize(this);
    }

    ~HalconCore()
    {
        Dispose();
    }
}

/// <summary>
/// 图像处理策略接口
/// </summary>
public interface IImageProcessor
{
    HObject Process(HObject sourceImage, HTuple width, HTuple height);
    int NgCount();
}