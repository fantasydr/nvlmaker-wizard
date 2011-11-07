using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ResConverter
{
    public partial class Wizard : Form
    {
        class ProjectConfig
        {
            #region 数据成员
            public string _baseFolder = string.Empty; // nvlmaker根目录
            public string _themeFolder = string.Empty; // 主题目录名

            public int _height; // 分辨率-高度
            public int _width;  // 分辨率-宽度

            public string _projectName = string.Empty;     // 项目名称
            public string _projectFolder = string.Empty;   // 项目目录，空则取名称作为目录

            // 目前缩放就按默认做
            public string _scaler = ResFile.SCALER_DEFAULT; // 缩放策略，目前只有这种:(
            public string _quality = ResFile.QUALITY_DEFAULT;   // 缩放质量，默认是高
            #endregion

            // nvlmaker根目录
            public string BaseFolder
            {
                get
                {
                    // 处理下，保证不为空
                    _baseFolder = (_baseFolder == null ? string.Empty : _baseFolder.Trim());
                    // 软件根目录绝对路径，不包括结尾的 “\”
                    return Path.GetFullPath(_baseFolder);
                }
            }

            // 主题目录
            public string ThemeFolder
            {
                get
                {
                    // 处理下，保证不为空
                    _themeFolder = (_themeFolder == null ? string.Empty : _themeFolder.Trim());
                    // 连接主题目录和根目录
                    return Path.Combine(this.BaseFolder, _themeFolder);
                }
            }

            public string ThemeConfig
            {
                get
                {
                    return Path.Combine(this.ThemeFolder, "Config.tjs");
                }
            }

            // 目标工程目录
            public string ProjectFolder
            {
                get
                {
                    // 处理下，保证不为空
                    _projectName = (_projectName == null ? string.Empty : _projectName.Trim());
                    _projectFolder = (_projectFolder == null ? string.Empty : _projectFolder.Trim());

                    if(!string.IsNullOrEmpty(_projectFolder))
                    {
                        return Path.Combine(this.BaseFolder, "project\\" + _projectFolder);
                    }
                    else
                    {
                        return Path.Combine(this.BaseFolder, "project\\" + _projectName);
                    }
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
                        if (output != null) output.WriteLine("软件根目录不存在。");
                        return false;
                    }

                    path = this.ThemeFolder;
                    if (string.IsNullOrEmpty(_themeFolder) || !Directory.Exists(path))
                    {
                        if (output != null) output.WriteLine("错误：主题目录不存在。");
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
                        if (output != null) output.WriteLine("错误：无效的工程名称。");
                        return false;
                    }
                    else if (Directory.Exists(path))
                    {
                        if (output != null) output.WriteLine("错误：工程目录已存在，请更换工程名或设置其他路径。");
                        return false;
                    }

                    path = this.ThemeConfig;
                    if(string.IsNullOrEmpty(path) || !File.Exists(path))
                    {
                        if (output != null) output.WriteLine("警告：主题缺少配置文件");
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

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("主题：{0}\n", this._themeFolder);
                sb.AppendFormat("项目名称：{0}\n", this._projectName);
                sb.AppendFormat("分辨率：{0}x{1}\n", this._width, this._height);
                sb.AppendFormat("===详细配置===\n");
                sb.AppendFormat("软件根目录：{0}\n", this.BaseFolder);
                sb.AppendFormat("缩放策略：{0}\n", this._scaler);
                sb.AppendFormat("缩放质量：{0}\n", this._quality);
                return sb.ToString();
            }
        }

        // 正在操作的配置
        ProjectConfig _curConfig = new ProjectConfig();

        // 记录目前的步骤
        int _curStep = -1;

        // 步骤面板数组
        GroupBox[] _stepGroups = null;

        // 步骤处理函数指针
        delegate void StepHandler();
        StepHandler[] _stepHandlers;

        // 获取/设置当前步骤
        int Step
        {
            get { return _curStep; }
            set
            {
                // 步骤未更新
                if(_curStep == value)
                {
                    return;
                }

                // 更新步骤
                _curStep = value;
                if (_curStep < 0) 
                {
                    _curStep = 0; 
                }
                else if (_curStep >= _stepGroups.Length) 
                {
                    _curStep = _stepGroups.Length - 1; 
                }

                // 按照当前步骤显式隐藏对应面板
                _stepGroups[_curStep].BringToFront();

                // 控制按钮显示
                btnNext.Enabled = _curStep < _stepGroups.Length - 1;
                btnPrev.Enabled = _curStep > 0;
                if (!btnPrev.Enabled) btnNext.Focus();
                if (btnNext.Enabled) btnNext.BringToFront();

                // 按照当前步骤调用对应的处理函数
                if (_curStep < _stepHandlers.Length)
                {
                    StepHandler handler = _stepHandlers[_curStep];
                    handler();
                }
            }
        }

        public Wizard()
        {
            InitializeComponent();

            // 初始化向导各步骤面板的位置，保存到数组里以备后用
            _stepGroups = new GroupBox[] { gbStep1, gbStep2, gbStep3, gbStep4 };
            for (int i = 1; i < _stepGroups.Length; i++)
            {
                // 把版面位置都同步到第一个的位置
                _stepGroups[i].Location = _stepGroups[0].Location;
            }

            // 绑定当前方法
            _stepHandlers = new StepHandler[] { 
                new StepHandler(this.OnStep1),
                new StepHandler(this.OnStep2), 
                new StepHandler(this.OnStep3),
                new StepHandler(this.OnStep4),
            };

            this.Step = 0;

            // 设定启动时的工作路径为软件根目录
            _curConfig._baseFolder = Directory.GetCurrentDirectory();
        }

        private void test()
        {
            // 资源转换器对象的测试用例
            ResConverter cov = new ResConverter();

            ResConfig config = new ResConfig();
            ResFile f1 = new ResFile();
            f1.path = @"c:\a.png";
            ResFile f2 = new ResFile();
            f2.path = @"c:\b.png";
            config.files.Add(f1);
            config.files.Add(f2);
            config.name = "TestTest";
            config.path = @"c:\test.xml";

            config.Save(config.path);

            ResConfig newConfig = ResConfig.Load(config.path);
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            Step = Step + 1;
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            Step = Step - 1;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            // 开始建立项目

        }

        void OnStep1()
        {
            //
            
        }

        void OnStep2()
        {
            //
        }

        void OnStep3()
        {
            //
        }

        void OnStep4()
        {
            // 根据当前配置生成报告
            StringWriter otuput = new StringWriter();
            
            btnOK.Enabled = _curConfig.IsReady(otuput);
            btnOK.BringToFront();
            btnOK.Focus();

            txtReport.Text = otuput.ToString();
        }
    }
}