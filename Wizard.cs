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
        const string SKIN_FOLDER = "\\skin";
        const string TEMPLATE_FOLDER = "\\project\\template";
        const string PROJECT_FOLDER = "\\project";
        const string NAME_DEFAULT_SKIN = "默认皮肤";
        const string NAME_CUSTOM_RESOLUTION = "(自定义)";

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

        class ProjectConfig
        {
            #region 数据成员
            public string _baseFolder = string.Empty; // nvlmaker根目录
            public string _themeFolder = string.Empty; // 皮肤目录名

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
                    // 处理下，保证不为空指针或空白字串
                    _baseFolder = (_baseFolder == null ? string.Empty : _baseFolder.Trim());
                    // 软件根目录绝对路径，不包括结尾的 “\”
                    return Path.GetFullPath(_baseFolder);
                }
            }

            // 皮肤目录
            public string ThemeFolder
            {
                get
                {
                    // 处理下，保证不为空指针或空白字串
                    _themeFolder = (_themeFolder == null ? string.Empty : _themeFolder.Trim());

                    // 0长度字串表示没有使用皮肤
                    if(_themeFolder.Length == 0)
                    {
                        return _themeFolder;
                    }
                    else
                    {
                        // 连接皮肤目录和根目录
                        return Path.Combine(this.BaseFolder, _themeFolder);
                    }
                }
            }

            public string ThemeConfig
            {
                get
                {
                    return Path.Combine(this.ThemeFolder, "Config.tjs");
                }
            }

            // 目标项目目录
            public string ProjectFolder
            {
                get
                {
                    // 处理下，保证不为空指针或空白字串
                    _projectName = (_projectName == null ? string.Empty : _projectName.Trim());

                    // 0长度字串表示没有单独设置项目目录
                    _projectFolder = (_projectFolder == null ? string.Empty : _projectFolder.Trim());

                    if (_projectFolder.Length == 0)
                    {
                        return Path.Combine(this.BaseFolder, "project\\" + _projectName);
                    }
                    else
                    {
                        return Path.Combine(this.BaseFolder, "project\\" + _projectFolder);
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
                            if (output != null) output.WriteLine("错误：皮肤目录不存在。");
                            return false;
                        }

                        path = this.ThemeConfig;
                        if (string.IsNullOrEmpty(path) || !File.Exists(path))
                        {
                            if (output != null) output.WriteLine("警告：皮肤缺少配置文件");
                        }
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

                if(string.IsNullOrEmpty(this._themeFolder))
                {
                    sb.AppendFormat("皮肤：{0}", NAME_DEFAULT_SKIN);
                }
                else
                {
                    sb.AppendFormat("皮肤：{0}", this._themeFolder);
                }
                sb.Append(Environment.NewLine);

                sb.AppendFormat("项目名称：{0}", this._projectName);sb.Append(Environment.NewLine);
                sb.AppendFormat("项目文件夹：{0}", this.ProjectFolder);sb.Append(Environment.NewLine);
                sb.AppendFormat("分辨率：{0}x{1}", this._width, this._height);sb.Append(Environment.NewLine);
                sb.AppendFormat("===详细信息===");sb.Append(Environment.NewLine);
                sb.AppendFormat("根目录：{0}", this.BaseFolder);sb.Append(Environment.NewLine);
                sb.AppendFormat("缩放策略：{0}", this._scaler); sb.Append(Environment.NewLine);
                sb.AppendFormat("缩放质量：{0}", this._quality); sb.Append(Environment.NewLine);
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

            this.SuspendLayout();

            // 设定启动时的工作路径为软件根目录
            _curConfig._baseFolder = Directory.GetCurrentDirectory();

            // 初始化分辨率设置
            cbResolution.Items.Clear();
            cbResolution.Items.Add(NAME_CUSTOM_RESOLUTION);
            foreach(Resolution res in Resolution.List)
            {
                cbResolution.Items.Add(res);
            }
            cbResolution.SelectedIndex = cbResolution.Items.Count - 1;

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

            this.ResumeLayout();
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
            int selected = 0;
            lstTemplate.BeginUpdate();
            lstTemplate.Items.Clear();
            lstTemplate.Items.Add(NAME_DEFAULT_SKIN);

            try
            {
                string lastSelect = _curConfig._themeFolder.ToLower();
                string root = _curConfig.BaseFolder;
                string[] skins = Directory.GetDirectories(root + SKIN_FOLDER);
                foreach(string skin in skins)
                {
                    // 只留目录名
                    lstTemplate.Items.Add(Path.GetFileName(skin));

                    // 匹配第一个目录名相同的皮肤作为选中项，返回的时候保持选项正确
                    if (selected == 0 && lastSelect == skin)
                    {
                        selected = lstTemplate.Items.Count - 1;
                    }
                }
            }
            catch (System.Exception e)
            {
            	// 出错了就不管啦
            }

            lstTemplate.SelectedIndex = selected;
            lstTemplate.EndUpdate();
        }

        void OnStep2()
        {
            // 记录上一步选的皮肤目录
            if(lstTemplate.SelectedIndex > 0)
            {
                string lastSelect = lstTemplate.SelectedItem as string;
                _curConfig._themeFolder = lastSelect.Trim();
            }
            else
            {
                _curConfig._themeFolder = string.Empty;
            }
        }

        void OnStep3()
        {
            // 保存上一步的结果
            _curConfig._width = (int)numWidth.Value;
            _curConfig._height = (int)numHeight.Value;
        }

        void OnStep4()
        {
            // 保存上一步的结果
            _curConfig._projectName = txtProjectName.Text;
            if (checkFolder.Checked)
                _curConfig._projectFolder = txtFolderName.Text;

            // 根据当前配置生成报告
            StringWriter otuput = new StringWriter();
            
            btnOK.Enabled = _curConfig.IsReady(otuput);
            btnOK.BringToFront();
            btnOK.Focus();

            txtReport.Text = otuput.ToString();
        }

        // 标记是否在操作下拉列表，防止和数字选择控件相互调用
        bool _isSelectingRes = false;
        private void cbResolution_SelectedIndexChanged(object sender, EventArgs e)
        {
            Resolution res = cbResolution.SelectedItem as Resolution;
            if(res != null)
            {
                _isSelectingRes = true;
                numWidth.Value = res._w;
                numHeight.Value = res._h;
                _isSelectingRes = false;
            }
        }
        private void numResolution_ValueChanged(object sender, EventArgs e)
        {
            if(!_isSelectingRes && cbResolution.Items.Count > 0)
            {
                cbResolution.SelectedIndex = 0;
            }
        }

        private void checkFolder_CheckedChanged(object sender, EventArgs e)
        {
            txtFolderName.ReadOnly = !checkFolder.Checked;
            if(!checkFolder.Checked)
            {
                txtFolderName.Text = txtProjectName.Text;
            }
        }

        private void txtProjectName_TextChanged(object sender, EventArgs e)
        {
            if(!checkFolder.Checked)
            {
                txtFolderName.Text = txtProjectName.Text;
            }
        }
    }
}