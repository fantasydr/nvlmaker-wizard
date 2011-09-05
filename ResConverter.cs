using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Xml;
using System.IO;
using System.Xml.Serialization;

namespace ResConverter
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

                    obj.path = filename;
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
            catch (System.Exception e)
            {
                return false;
            }
        }
    }

    class ResConverter
    {
        enum Quality
        {
            LOW,
            NORMAL,
            HIGH,
        }

        // 按照配置转换指定的资源
        public void Start(ResConfig config, int destWidth, int destHeight, string destFolder)
        {
            if (destWidth <= 0 || destHeight <= 0) return;

            // 以root为根目录载入所有图片并缩放
            string baseDir = Path.GetDirectoryName(config.path);
            foreach (ResFile file in config.files)
            {
                string inputFile = Path.GetFullPath(Path.Combine(baseDir, file.path));
                string destFile = Path.GetFullPath(Path.Combine(destFolder, file.path));

                if (destFile == inputFile)
                {
                    // 忽略同名文件错误
                    continue;
                }

                Quality q = Quality.HIGH;
                if (file.quality.ToLower() == ResFile.QUALITY_LOW) q = Quality.LOW;
                else if (file.quality.ToLower() == ResFile.QUALITY_NORMAL) q = Quality.NORMAL;

                try
                {
                    Bitmap source = new Bitmap(inputFile);
                    Bitmap dest = Scale(source, destWidth, destHeight, q,
                                        CalcRects(file, destWidth, destHeight));

                    dest.Save(destFile, source.RawFormat);

                    // 转换完毕

                }
                catch (System.Exception e)
                {
                    // 转换出现错误
                }
            }
        }

        // 根据策略计算区域映射
        Dictionary<Rectangle, Rectangle> CalcRects(ResFile file, int destWidth, int destHeight)
        {
            // 根据策略计算区域映射
            return null;
        }

        // 根据给定的区域映射和质量参数，将源图片缩放到目标大小
        Bitmap Scale(Image source, int destWidth, int destHeight, Quality q,
                                Dictionary<Rectangle, Rectangle> rects)
        {
            if (destWidth <= 0 || destHeight <= 0) return null;

            Bitmap dest = new Bitmap(destWidth, destHeight);
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
                    g.DrawImage(source, 0, 0, destWidth, destHeight);
                }
                else
                {
                    // 按照规划的区域缩放
                    foreach (KeyValuePair<Rectangle, Rectangle> kp in rects)
                    {
                        if (kp.Value.Left >= destWidth || kp.Value.Top >= destHeight)
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
