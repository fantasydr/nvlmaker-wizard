using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

namespace Wizard
{
    public class ResFile
    {
        // 文件属性可选值
        public static readonly string SCALER_DEFAULT = "auto";
        public static readonly string QUALITY_DEFAULT = "high";
        public static readonly string QUALITY_NORMAL = "low";
        public static readonly string QUALITY_LOW = "normal";

        // 文件路径
        [XmlAttribute]
        public string path = string.Empty;

        // 缩放策略
        [XmlAttribute]
        public string scaler = SCALER_DEFAULT;

        // 缩放质量
        [XmlAttribute]
        public string quality = QUALITY_DEFAULT;

        public override string ToString()
        {
            return path;
        }

        public ResFile(string path)
        {
            this.path = path;
        }
    }

    [XmlRootAttribute("Config", IsNullable = false)]
    public class ResConfig
    {
        // 只是个名字而已，可以随便写
        [XmlAttribute]
        public string name = "默认资源文件列表";

        [XmlElement("File")]
        public List<ResFile> files = new List<ResFile>();

        // 记录资源文件的根目录，一般就用资源文件所在的目录
        [XmlIgnoreAttribute]
        public string path = string.Empty;

        // 从文件中生成ResConfig对象
        static public ResConfig Load(string filename)
        {
            try
            {
                using (StreamReader r = new StreamReader(string.Format(filename)))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ResConfig));
                    ResConfig obj = (ResConfig)serializer.Deserialize(r);
                    r.Close();

                    obj.path = Path.GetDirectoryName(filename);
                    return obj;
                }
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        // 把当前ResConfig对象储存到文件中
        public bool Save(string filename)
        {
            try
            {
                using (StreamWriter w = new StreamWriter(string.Format(filename)))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ResConfig));
                    serializer.Serialize(w, this);
                    w.Close();
                    return true;
                }
            }
            catch (System.Exception)
            {
                return false;
            }
        }
    }

    class ResConverter
    {
        #region 事件：转换进度通知
        public class NotifyProcessEventArgs : EventArgs
        {
            public readonly string file;
            public readonly int index;
            public readonly int count;

            public NotifyProcessEventArgs(string file, int index, int count)
            {
                this.file = file;
                this.index = index;
                this.count = count;
            }
        }
        // 转换进度通知响应函数
        public delegate void NotifyProcessHandler(ResConverter sender, NotifyProcessEventArgs e);
        // 转换进度通知事件
        public event NotifyProcessHandler NotifyProcessEvent;
        // 转换进度通知处理函数
        protected void OnNotifyProcess(string file, int index, int count)
        {
            if(NotifyProcessEvent != null)
            {
                NotifyProcessEventArgs e = new NotifyProcessEventArgs(file, index, count);
                NotifyProcessEvent(this, e);
            }
        }
        #endregion

        enum Quality
        {
            LOW,
            NORMAL,
            HIGH,
        }

        // 按照配置转换指定的资源
        public void Start( // 资源文件列表
                           ResConfig config,
                           // 目标路径
                           string destFolder,
                           // 源屏幕大小
                           int srcWidth, int srcHeight,
                           // 目标屏幕大小
                           int destWidth, int destHeight )
        {
            // 忽略无效参数
            if (srcWidth <= 0 || srcHeight <= 0 || destWidth <= 0 || destHeight <= 0)
                return;

            int cur = 0;

            // 以root为根目录载入所有图片并缩放
            string baseDir = config.path;
            foreach (ResFile file in config.files)
            {
                OnNotifyProcess(file.path, ++cur, config.files.Count);

                string inputFile = Path.GetFullPath(Path.Combine(baseDir, file.path));
                string destFile = Path.GetFullPath(Path.Combine(destFolder, file.path));

                if (destFile == inputFile)
                {
                    // 忽略同名文件错误
                    continue;
                }

                // 选择质量参数
                Quality q = Quality.HIGH;
                if (file.quality.ToLower() == ResFile.QUALITY_LOW)
                    q = Quality.LOW;
                else if (file.quality.ToLower() == ResFile.QUALITY_NORMAL)
                    q = Quality.NORMAL;

                try
                {
                    // 读取源图片
                    Bitmap source = new Bitmap(inputFile);

                    // 根据策略计算区域映射（尚未实现）
                    Dictionary<Rectangle, Rectangle> rects =
                        CalcRects(source, srcWidth, srcHeight, destWidth, destHeight);

                    // 实施转换
                    Bitmap dest = Scale(source, srcWidth, srcHeight, destWidth, destHeight, q, rects);

                    if(dest != null)
                    {
                        dest.Save(destFile, source.RawFormat);
                    }

                    // 转换完毕

                }
                catch (System.Exception e)
                {
                    // 转换出现错误
                    Console.WriteLine(e.Message);
                }
            }
        }

        // 根据策略计算区域映射
        Dictionary<Rectangle, Rectangle> CalcRects( Bitmap source,
                                                    // 源屏幕大小
                                                    int srcWidth, int srcHeight,
                                                    // 目标屏幕大小
                                                    int destWidth, int destHeight )
        {
            // 尚未实现
            return null;
        }

        // 根据给定的区域映射和质量参数，将源图片缩放到目标大小
        Bitmap Scale( Image source,
                      // 源屏幕大小
                      int srcWidth, int srcHeight,
                      // 目标屏幕大小
                      int destWidth, int destHeight,
                      // 转换质量
                      Quality q,
                      // 转换区域映射
                      Dictionary<Rectangle, Rectangle> rects)
        {
            // 忽略无效参数
            if (srcWidth <= 0 || srcHeight <= 0 || destWidth <= 0 || destHeight <= 0)
                return null;

            int realWidth =  (source.Width * destWidth) / srcWidth;
            int realHeight = (source.Height * destHeight) / srcHeight;

            Bitmap dest = new Bitmap(realWidth, realHeight);
            using (Graphics g = Graphics.FromImage(dest))
            {
                // 根据质量参数决定缩放算法
                switch (q)
                {
                    case Quality.LOW:
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.Default;
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                        break;
                    case Quality.NORMAL:
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.AssumeLinear;
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bicubic;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        break;
                    case Quality.HIGH:
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        break;
                }

                if (rects == null || rects.Count == 0)
                {
                    // 直接缩放到指定的大小
                    g.DrawImage(source, 0, 0, realWidth, realHeight);
                }
                else
                {
                    // 按照规划的区域缩放
                    foreach (KeyValuePair<Rectangle, Rectangle> kp in rects)
                    {
                        if (kp.Value.Left >= realWidth || kp.Value.Top >= realHeight)
                        {
                            // 忽略无效数据
                            continue;
                        }

                        g.DrawImage(source, kp.Value, kp.Key, GraphicsUnit.Pixel);
                    }
                }
            }

            return dest;
        }
    }
}
