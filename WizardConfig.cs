using System;
using System.Collections.Generic;
using System.Text;
using Tjs;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Drawing;

namespace Wizard
{
    // 分辨率设置对象
    class Resolution
    {
        public int _w;
        public int _h;

        public Resolution(int w, int h) { _w = w; _h = h; }
        public override string ToString()
        {
            const float delta = 0.001f;

            string ratioStr = string.Empty;

            float ratio = (float)_w / _h;
            if (Math.Abs(ratio - 4.0 / 3.0) < delta)
            {
                ratioStr = "(4:3)";
            }
            else if (Math.Abs(ratio - 16.0 / 10.0) < delta)
            {
                ratioStr = "(16:10)";
            }
            else if (Math.Abs(ratio - 16.0 / 9.0) < delta)
            {
                ratioStr = "(16:9)";
            }
            else if (Math.Abs(ratio - 5.0 / 4.0) < delta)
            {
                ratioStr = "(5:4)";
            }

            return string.Format("{0}x{1} {2}", _w, _h, ratioStr);
        }

        public static Resolution[] List
        {
            get
            {
                return new Resolution[] {
                new Resolution(640, 480),
                new Resolution(800, 600),
                new Resolution(1024, 768),
                new Resolution(1152, 864),
                new Resolution(1280, 720),
                new Resolution(1280, 800),
                new Resolution(1280, 960),
                new Resolution(1280, 1024),
                new Resolution(1366, 768),
                new Resolution(1400, 1050),
                new Resolution(1440, 900),
                new Resolution(1680, 1050),
                new Resolution(1920, 1080),
                };
            }
        }
    }

    // 模板的基本属性
    class ProjectProperty
    {
        public string readme = string.Empty;

        public string title
        {
            get
            {
                // 读取标题
                string ret = null;
                if (setting != null)
                {
                    ret = setting.GetString("title");
                }
                return ret == null ? string.Empty : ret;
            }
        }
        
        public int width
        {
            get
            {
                // 读取预设宽度
                double ret = double.NaN;
                if (setting != null)
                {
                    ret = setting.GetNumber("width");
                }
                return double.IsNaN(ret) ? 0 : (int)ret;
            }
        }

        public int height
        {
            get
            {
                // 读取预设高度
                double ret = double.NaN;
                if (setting != null)
                {
                    ret = setting.GetNumber("height");
                }
                return double.IsNaN(ret) ? 0 : (int)ret;
            }
        }

        public int thumbnailwidth
        {
            get
            {
                // 读取缩略图大小
                double ret = double.NaN;
                if (setting != null)
                {
                    TjsString str = setting.GetValue("savedata/thumbnailwidth") as TjsString;
                    if (str != null)
                    {
                        double.TryParse(str.val, out ret);
                    }
                }
                return double.IsNaN(ret) ? 0 : (int)ret;
            }
        }

        public TjsDict setting = null;

        public void LoadSetting(string file)
        {
            setting = null;

            if (File.Exists(file))
            {
                using (StreamReader r = new StreamReader(file))
                {
                    TjsParser parser = new TjsParser();
                    setting = parser.Parse(r) as TjsDict;
                }
            }
        }
    }

    // 封装一些辅助的方法用于转换数据
    class TjsHelper
    {
        public static double ScaleInteger(TjsDict dict, string name, double scale)
        {
            double num = dict.GetNumber(name);
            if (!double.IsNaN(num))
            {
                num = num * scale;
                dict.SetNumber(name, Math.Floor(num));
                return num;
            }

            string str = dict.GetString(name);
            if(double.TryParse(str, out num))
            {
                num = num * scale;
                dict.SetString(name, Math.Floor(num).ToString());
                return num;
            }

            return double.NaN;
        }

        public static TjsArray ScalePosArray(TjsDict dict, string name, double scaleX, double scaleY)
        {
            TjsValue v = null;
            if (dict.val.TryGetValue(name, out v))
            {
                // 检查是不是数组
                TjsArray arr = v as TjsArray;
                if (arr != null)
                {
                    // 从中读取两个元素的坐标数组
                    List<TjsValue> arraynew = new List<TjsValue>();
                    foreach (TjsValue pos in arr.val)
                    {
                        Point p = Point.Empty;
                        if (TryGetPos(pos, out p))
                        {
                            // 按比例缩放
                            TjsArray posnew = CreatePos((int)(p.X * scaleX), (int)(p.Y * scaleY));
                            arraynew.Add(posnew);
                        }
                        else
                        {
                            Debug.Assert(false, "invalid struct in pos array");
                        }
                    }

                    dict.val[name] = new TjsArray(arraynew);
                    return arr;
                }
            }

            return null;
        }

        public static TjsArray ScaleButton(TjsDict dict, string name, double scaleX, double scaleY)
        {
            TjsValue v = null;
            if (dict.val.TryGetValue(name, out v))
            {
                // 按钮上多记录了一个是否显示: x, y, shown
                TjsArray xys = v as TjsArray;
                if (xys != null && xys.val.Count == 3)
                {
                    TjsNumber x = xys.val[0] as TjsNumber;
                    TjsNumber y = xys.val[1] as TjsNumber;
                    TjsNumber s = xys.val[2] as TjsNumber;

                    if (x != null && y != null && s != null)
                    {
                        TjsArray xysnew = CreatePos((int)(x.val * scaleX), (int)(y.val * scaleY));
                        xysnew.val.Add(new TjsNumber(s.val));
                        dict.val[name] = xysnew;
                        return xysnew;
                    }
                    else
                    {
                        Debug.Assert(false, "invalid element in button struct");
                    }
                }
                else
                {
                    Debug.Assert(false, "invalid button struct");
                }
            }

            return null;
        }

        public static TjsArray CreatePos(int x, int y)
        {
            List<TjsValue> inner = new List<TjsValue>();
            inner.Add(new TjsNumber(x));
            inner.Add(new TjsNumber(y));
            return new TjsArray(inner);
        }

        public static bool TryGetPos(TjsValue pos, out Point p)
        {
            TjsArray xy = pos as TjsArray;
            if (xy != null && xy.val.Count == 2)
            {
                TjsNumber x = xy.val[0] as TjsNumber;
                TjsNumber y = xy.val[1] as TjsNumber;
                if (x != null && y != null)
                {
                    p = new Point((int)x.val, (int)y.val);
                    return true;
                }
                else
                {
                    Debug.Assert(false, "invalid element in pos struct");
                }
            }
            else
            {
                Debug.Assert(false, "invalid pos struct");
            }

            p = Point.Empty;
            return false;
        }
    }

    // 项目向导配置对象
    class WizardConfig
    {
        // 一些常量
        public const string THEME_FOLDER = "\\skin";
        public const string TEMPLATE_FOLDER = "\\project\\template";
        public const string DATA_FOLDER = "\\data";
        public const string PROJECT_FOLDER = "\\project";

        public const string UI_LAYOUT = "macro\\ui*.tjs";
        public const string UI_SETTING = "macro\\setting.tjs";
        public const string UI_CONFIG = "Config.tjs";

        public const int DEFAULT_WIDTH = 1024;
        public const int DEFAULT_HEIGHT = 768;

        public const string NAME_DEFAULT_THEME = "默认主题";

        // 忽略指定的图片文件
        const string PIC_IGNORE1 = @"data\system";
        const string PIC_IGNORE2 = @"system";
        public static bool IgnorePicture(string relFile)
        {
            string file = relFile.ToLower();
            return file.StartsWith(PIC_IGNORE1) || file.StartsWith(PIC_IGNORE2);
        }

        #region 数据成员
        private string _baseFolder = string.Empty; // nvlmaker根目录
        private string _themeName = string.Empty; // 主题目录名

        public int _height; // 分辨率-高度
        public int _width;  // 分辨率-宽度

        private string _projectName = string.Empty;     // 项目名称
        private string _projectFolder = string.Empty;   // 项目目录，空则取名称作为目录

        // 目前缩放就按默认做
        private string _scaler = ResFile.SCALER_DEFAULT; // 缩放策略，目前只有这种:(
        private string _quality = ResFile.QUALITY_DEFAULT;   // 缩放质量，默认是高

        // 储存上次读取的主体属性，避免多次读取
        private ProjectProperty _themeInfo = null;
        #endregion

        // nvlmaker根路径
        public string BaseFolder
        {
            get
            {
                // 软件根目录绝对路径，不包括结尾的 “\”
                return _baseFolder;
            }
            set
            {
                // 处理下，保证不为空指针或空白字串
                _baseFolder = (value == null ? string.Empty : value.Trim());
            }
        }

        // 基础模板路径
        public string BaseTemplateFolder
        {
            get
            {
                return this.BaseFolder + TEMPLATE_FOLDER;
            }
        }

        // 是否选择了默认主题
        public bool IsDefaultTheme
        {
            get
            {
                return string.IsNullOrEmpty(this.ThemeName);
            }
        }

        // 主题名称
        public string ThemeName
        {
            get
            {
                return _themeName;
            }
            set
            {
                // 处理下，保证不为空指针或空白字串
                string themeName = (value == null ? string.Empty : value.Trim());

                // 如果主题更换则清空预读的设置
                if(themeName != _themeName)
                {
                    this._themeName = themeName;
                    this._themeInfo = null;
                }
            }
        }

        // 主题路径
        public string ThemeFolder
        {
            get
            {
                if (this.IsDefaultTheme)
                {
                    return this.BaseTemplateFolder;
                }
                else
                {
                    // 连接主题目录和根目录
                    return this.BaseFolder + THEME_FOLDER + "\\" + this.ThemeName;
                }
            }
        }

        // 主题配置文件
        public string ThemeSetting
        {
            get
            {
                return Path.Combine(this.ThemeDataFolder, UI_SETTING);
            }
        }

        // 主题的数据目录
        public string ThemeDataFolder
        {
            get
            {
                if (this.IsDefaultTheme)
                {
                    return this.ThemeFolder + DATA_FOLDER; 
                }
                else
                {
                    return this.ThemeFolder;
                }
            }
        }

        // 目标项目路径
        public string ProjectFolder
        {
            get
            {
                if (_projectFolder.Length == 0)
                {
                    return this.BaseFolder + PROJECT_FOLDER + "\\" + _projectName;
                }
                else
                {
                    return this.BaseFolder + PROJECT_FOLDER + "\\" + _projectFolder;
                }
            }
            set
            {
                // 0长度字串表示没有单独设置项目目录
                _projectFolder = (value == null ? string.Empty : value.Trim());
            }
        }

        // 目标项目数据路径
        public string ProjectDataFolder
        {
            get
            {
                return this.ProjectFolder + DATA_FOLDER;
            }
        }

        // 目标项目名称
        public string ProjectName
        {
            get
            {
                return _projectName;
            }
            set
            {
                // 处理下，保证不为空指针或空白字串
                _projectName = (value == null ? string.Empty : value.Trim());
            }
        }

        // 检查这个配置是否已经完备，把出错信息写入output
        public bool IsReady(TextWriter output)
        {
            try
            {
                string path = this.BaseFolder;
                if (string.IsNullOrEmpty(_baseFolder) || !Directory.Exists(path))
                {
                    if (output != null) output.WriteLine("错误：软件根目录不存在。");
                    return false;
                }

                if (_height <= 0 || _width <= 0)
                {
                    if (output != null) output.WriteLine("错误：无效的分辨率设置。");
                    return false;
                }

                path = this.ProjectFolder;
                if (string.IsNullOrEmpty(_projectName))
                {
                    if (output != null) output.WriteLine("错误：无效的项目名称。");
                    return false;
                }
                else if (Directory.Exists(path))
                {
                    if (output != null) output.WriteLine("错误：项目文件夹已存在，请更换项目名或设置其他路径。");
                    return false;
                }

                path = this.ThemeFolder;
                if (!string.IsNullOrEmpty(path))
                {
                    if (!Directory.Exists(path))
                    {
                        if (output != null) output.WriteLine("错误：主题目录不存在。");
                        return false;
                    }

                    path = this.ThemeSetting;
                    if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    {
                        if (output != null) output.WriteLine("警告：主题缺少配置文件");
                    }
                }

                ProjectProperty info = ReadThemeInfo();
                if(info == null || info.height <= 0 || info.width <= 0)
                {
                    if (output != null) output.WriteLine("警告：主题分辨率错误。");
                }

                ProjectProperty baseInfo = ReadBaseTemplateInfo();
                if (baseInfo != info && (baseInfo == null || baseInfo.height <= 0 || baseInfo.width <= 0))
                {
                    if (output != null) output.WriteLine("警告：默认主题分辨率错误。");
                }

                // 生成配置报告
                if(output != null)
                {
                    output.WriteLine(this.ToString());
                }
            }
            catch (System.Exception e)
            {
                if (output != null) output.WriteLine("无效的项目配置：" + e.Message);
                return false;
            }

            return true;
        }

        // 根据配置的内容生成报告
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("== 项目配置清单 =="); sb.Append(Environment.NewLine);

            sb.Append(Environment.NewLine);
            string theme = (this.IsDefaultTheme) ? NAME_DEFAULT_THEME : this.ThemeName;
            sb.AppendFormat("所选主题：{0}", theme); sb.Append(Environment.NewLine);
            sb.AppendFormat("分辨率设定：{0}x{1}", this._width, this._height); 
            ProjectProperty info = ReadThemeInfo();
            if(info.width != _width || info.height != this._height)
            {
                sb.AppendFormat(" (非原始分辨率)");
            }
            sb.Append(Environment.NewLine);

            sb.Append(Environment.NewLine);
            sb.AppendFormat("项目名称：{0}", this._projectName);sb.Append(Environment.NewLine);
            sb.AppendFormat("项目位置：{0}", this.ProjectFolder); sb.Append(Environment.NewLine);
            
            sb.Append(Environment.NewLine);
            sb.AppendFormat("缩放策略：{0}", this._scaler); sb.Append(Environment.NewLine);
            sb.AppendFormat("缩放质量：{0}", this._quality); sb.Append(Environment.NewLine);
            sb.AppendFormat("NVLMaker目录：{0}", this.BaseFolder);sb.Append(Environment.NewLine);
            return sb.ToString();
        }

        // 读取所选主题的属性
        public ProjectProperty ReadThemeInfo()
        {
            // 直接返回读取值
            if (this._themeInfo != null)
            {
                return this._themeInfo;
            }

            ProjectProperty info = new ProjectProperty();
            this._themeInfo = info;

            // 读取readme文件作为显示内容
            try
            {
                string readmefile = Path.Combine(this.ThemeDataFolder, "Readme.txt");
                if (File.Exists(readmefile))
                {
                    using (StreamReader r = new StreamReader(readmefile))
                    {
                        info.readme = r.ReadToEnd();
                    }
                }
            }
            catch (System.Exception e)
            {
                // 出错的不保留
                this._themeInfo = null;
                info.readme = e.Message;
            }

            // 读取设置文件
            try
            {
                info.LoadSetting(this.ThemeSetting);
            }
            catch (System.Exception e)
            {
                // 出错的不保留
                this._themeInfo = null;
                info.readme = e.Message;
            }
            
            return info;
        }

        // 读取基础模板的配置
        public ProjectProperty ReadBaseTemplateInfo()
        {
            // 如果选的是默认的主题，则返回主题属性
            if(this.IsDefaultTheme)
            {
                return this.ReadThemeInfo();
            }

            // 这里就不读readme了，也不做保存，每次调用都从文件读一次
            string file = Path.Combine(this.BaseTemplateFolder + DATA_FOLDER, UI_SETTING);
            ProjectProperty info = new ProjectProperty();
            try
            {
                info.LoadSetting(file);
            }
            catch (System.Exception e)
            {
                info.readme = e.Message;
            }
            return info;
        }
    }

    // 根据项目配置转换项目文件
    class WizardConverter : ResConverter
    {
        #region 事件：Logging及Report消息通知
        public class MessageEventArgs : EventArgs
        {
            public readonly string msg;
            public MessageEventArgs(string msg)
            {
                this.msg = msg;
            }
        }
        // 消息通知响应函数
        public delegate void MessageHandler(WizardConverter sender, MessageEventArgs e);
        
        // Logging事件
        public event MessageHandler LoggingEvent;
        protected void OnLogging(string msg)
        {
            if (LoggingEvent != null)
            {
                LoggingEvent(this, new MessageEventArgs(msg));
            }
        }

        // 错误消息通知事件
        public event MessageHandler ErrorEvent;
        protected void OnError(string msg)
        {
            if (ErrorEvent != null)
            {
                ErrorEvent(this, new MessageEventArgs(msg));
            }
        }
        #endregion

        WizardConfig _config;

        public WizardConverter(WizardConfig config)
        {
            _config = config;
        }

        // 根据配置创建目标项目
        public void Start()
        {
            // 从配置中读取需要的源大小和目标大小
            int dw = _config._width, dh = _config._height;

            // 先从基础模板目录拷贝文件到项目目录
            string template = _config.BaseTemplateFolder;
            string project = _config.ProjectFolder;

            // 读取基础模板的配置
            ProjectProperty baseInfo = _config.ReadBaseTemplateInfo();

            int sw = baseInfo.width;
            if (sw <= 0) sw = WizardConfig.DEFAULT_WIDTH;
            int sh = baseInfo.height;
            if (sh <= 0) sh = WizardConfig.DEFAULT_HEIGHT;

            ConvertFiles(template, sw, sh, project, dw, dh);

            // 修正所有坐标，写入项目名称
            AdjustSettings(sw, sh);

            // 如果选择了非默认主题，再从主题目录拷贝文件到项目资料文件夹
            if (_config.ThemeFolder != template)
            {
                // 读取所选主题配置
                ProjectProperty themeInfo = _config.ReadThemeInfo();

                sw = themeInfo.width;
                if (sw <= 0) sw = WizardConfig.DEFAULT_WIDTH;
                sh = themeInfo.height;
                if (sh <= 0) sh = WizardConfig.DEFAULT_HEIGHT;

                // 主题的文件直接拷入数据目录
                ConvertFiles(_config.ThemeFolder, sw, sh, _config.ProjectDataFolder, dw, dh);

                // 修正所有坐标，写入项目名称
                AdjustSettings(sw, sh);
            }
        }

        // 拷贝并缩放文件
        void ConvertFiles(string srcPath, int sw, int sh, string destPath, int dw, int dh)
        {
            // 源文件列表
            List<string> srcFiles = new List<string>();
            try
            {
                // 建立目录并获取文件列表
                CreateDir(srcPath, destPath, srcFiles);

                // 建立图片转换配置，用于记录需要转换的图片文件，其他文件则直接拷贝
                ResConfig resource = new ResConfig();
                resource.path = srcPath;
                resource.name = WizardConfig.NAME_DEFAULT_THEME;

                // 遍历所有文件
                int cutLen = srcPath.Length;
                foreach (string srcfile in srcFiles)
                {
                    // 截掉模板目录以径获取相对路径
                    string relFile = srcfile.Substring(cutLen + 1);

                    // 取得扩展名
                    string ext = Path.GetExtension(relFile).ToLower();

                    if ( // 宽高如果和源文件相同那就不用转换了
                         (sw != dw || sh != dh) &&
                        // 忽略某些图片
                         !WizardConfig.IgnorePicture(relFile) &&
                        // 只转换这些扩展名对应的文件
                         (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp"))
                    {
                        // 是图片则添加到转换器中
                        resource.files.Add(new ResFile(relFile));
                    }
                    else
                    {
                        // 直接拷贝
                        OnLogging(string.Format("拷贝{0}", relFile));
                        File.Copy(srcfile, Path.Combine(destPath, relFile), true);
                    }
                }

                OnLogging("图片转换中……");

                if (resource.files.Count > 0)
                {
                    // 调用资源转换器方法，并开始转换
                    Start(resource, destPath, sw, sh, dw, dh);
                }

            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        // 修正目标项目文件夹中的配置
        void AdjustSettings(int sw, int sh)
        {
            string dataPath = _config.ProjectDataFolder;
            int dh = _config._height;
            int dw = _config._width;
            string title = _config.ProjectName;

            try
            {
                ModifySetting(dataPath, title, sw, sh, dh, dw);
            }
            catch (System.Exception e)
            {
                OnError("修改setting.tjs失败:" + e.Message);
            }

            try
            {
                ModifyConfig(dataPath, title, sw, sh, dh, dw);
            }
            catch (System.Exception e)
            {
                OnError("修改Config.tjs失败:" + e.Message);
            }

            // 检查是否需要转换
            if (sw != dw || sh != dh)
            {
                try
                {
                    ModifyLayout(dataPath, sw, sh, dh, dw);
                }
                catch (System.Exception e)
                {
                    OnError("修改界面布局文件失败:" + e.Message);
                }
            }
        }

        // 修改字典文件
        static void ModifyDict(TjsDict dict, int sw, int sh, int dw, int dh)
        {
            double scaleX = (double)dw / sw;
            double scaleY = (double)dh / sh;

            TjsHelper.ScaleInteger(dict, "left", scaleX);
            TjsHelper.ScaleInteger(dict, "x", scaleX);
            TjsHelper.ScaleInteger(dict, "marginr", scaleX);
            TjsHelper.ScaleInteger(dict, "marginl", scaleX);

            TjsHelper.ScaleInteger(dict, "top", scaleY);
            TjsHelper.ScaleInteger(dict, "y", scaleY);
            TjsHelper.ScaleInteger(dict, "margint", scaleY);
            TjsHelper.ScaleInteger(dict, "marginb", scaleY);

            // 修改locate数组
            TjsHelper.ScalePosArray(dict, "locate", scaleX, scaleY);

            foreach (KeyValuePair<string, TjsValue> kv in dict.val)
            {
                TjsDict inner = kv.Value as TjsDict;
                if(inner != null)
                {
                    ModifyDict(inner, sw, sh, dw, dh);
                }
            }
        }

        // 修改UI布局文件
        static void ModifyLayout(string dataPath, int sw, int sh, int dh, int dw)
        {
            // 更新layout
            string[] layouts = Directory.GetFiles(dataPath, WizardConfig.UI_LAYOUT);
            foreach (string layout in layouts)
            {
                TjsDict setting = null;
                using (StreamReader r = new StreamReader(layout))
                {
                    TjsParser parser = new TjsParser();
                    setting = parser.Parse(r) as TjsDict;
                }

                if (setting != null)
                {
                    ModifyDict(setting, sw, sh, dw, dh);

                    // 对这个文件里的按钮作特殊处理
                    if(layout.ToLower().EndsWith("uislpos.tjs"))
                    {
                        double scaleX = (double)dw / sw;
                        double scaleY = (double)dh / sh;
                        TjsHelper.ScaleButton(setting, "back", scaleX, scaleY);
                        TjsHelper.ScaleButton(setting, "up", scaleX, scaleY);
                        TjsHelper.ScaleButton(setting, "down", scaleX, scaleY);
                    }
                }

                using (StreamWriter w = new StreamWriter(layout, false, Encoding.Unicode))
                {
                    w.Write(setting.ToString());
                }
            }
        }

        // 修改config.tjs
        static void ModifyConfig(string dataPath, string title, int sw, int sh, int dh, int dw)
        {
            // 更新config
            string configFile = Path.Combine(dataPath, WizardConfig.UI_CONFIG);
            if (File.Exists(configFile))
            {
                Regex regTitle = new Regex(@"\s*;\s*System.title\s*=");
                Regex regW = new Regex(@"\s*;\s*scWidth\s*=");
                Regex regH = new Regex(@"\s*;\s*scHeight\s*=");
                Regex regThumb = new Regex(@"\s*;\s*thumbnailWidth\s*=");

                StringBuilder buf = new StringBuilder();
                using (StreamReader r = new StreamReader(configFile))
                {
                    while (!r.EndOfStream)
                    {
                        string line = r.ReadLine();
                        if (regTitle.IsMatch(line))
                        {
                            buf.AppendLine(string.Format(";System.title = \"{0}\";", title));
                        }
                        else if (regW.IsMatch(line))
                        {
                            buf.AppendLine(string.Format(";scWidth = {0};", dw));
                        }
                        else if (regH.IsMatch(line))
                        {
                            buf.AppendLine(string.Format(";scHeight = {0};", dh));
                        }
                        else if (regThumb.IsMatch(line))
                        {
                            // 临时创建一个属性对象用于读取setting
                            string settingFile = Path.Combine(dataPath, WizardConfig.UI_SETTING);
                            ProjectProperty info = new ProjectProperty();
                            info.LoadSetting(settingFile);

                            // 修正缩略图
                            int tw = info.thumbnailwidth;
                            if (tw > 0)
                            {
                                buf.AppendLine(string.Format(";thumbnailWidth = {0};", tw));
                            }
                        }
                        else
                        {
                            buf.AppendLine(line);
                        }
                    }
                }

                using (StreamWriter w = new StreamWriter(configFile, false, Encoding.Unicode))
                {
                    w.Write(buf.ToString());
                }
            }
        }

        // 修改setting.tjs
        static void ModifySetting(string dataPath, string title, int sw, int sh, int dh, int dw)
        {
            // 更新setting
            string settingFile = Path.Combine(dataPath, WizardConfig.UI_SETTING);
            
            // 临时创建一个属性对象用于读取setting
            ProjectProperty info = new ProjectProperty();
            info.LoadSetting(settingFile);
            TjsDict setting = info.setting;

            if (setting != null)
            {
                setting.SetString("title", title);
                setting.SetNumber("width", dw);
                setting.SetNumber("height", dh);

                // 修正缩略图宽度
                int tw = info.thumbnailwidth;
                if (tw > 0)
                {
                    tw = tw * dw / sw;
                    setting.SetValue("savedata/thumbnailwidth", new TjsString(tw.ToString()));
                }

                using (StreamWriter w = new StreamWriter(settingFile, false, Encoding.Unicode))
                {
                    w.Write(setting.ToString());
                }
            }
        }

        // 工具函数：创建文件夹，并记录其中的文件
        static void CreateDir(string source, string dest, List<string> files)
        {
            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
            }

            if (files != null)
            {
                string[] curFiles = Directory.GetFiles(source);
                files.AddRange(curFiles);
            }

            string[] subDirs = Directory.GetDirectories(source);
            if (subDirs.Length == 0)
            {
                // 木有找到任何子目录
                return;
            }

            foreach (string dir in subDirs)
            {
                string name = Path.GetFileName(dir);
                CreateDir(dir, Path.Combine(dest, name), files);
            }
        }
    }
}
