using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using Tjs;

namespace ResConverter
{
    public partial class Wizard : Form
    {
        const string THEME_FOLDER = "\\skin";
        const string TEMPLATE_FOLDER = "\\project\\template";
        const string DATA_FOLDER = "\\data";
        const string PROJECT_FOLDER = "\\project";
        const string UI_LAYOUT = "macro\\ui*.tjs";

        const string NAME_DEFAULT_THEME = "默认皮肤";
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

        // 项目向导配置对象
        class ProjectConfig
        {
            #region 数据成员
            private string _baseFolder = string.Empty; // nvlmaker根目录
            private string _themeName = string.Empty; // 皮肤目录名

            public int _height; // 分辨率-高度
            public int _width;  // 分辨率-宽度

            private string _projectName = string.Empty;     // 项目名称
            private string _projectFolder = string.Empty;   // 项目目录，空则取名称作为目录

            // 目前缩放就按默认做
            private string _scaler = ResFile.SCALER_DEFAULT; // 缩放策略，目前只有这种:(
            private string _quality = ResFile.QUALITY_DEFAULT;   // 缩放质量，默认是高
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

            // 皮肤名称
            public string ThemeName
            {
                get
                {
                    return _themeName;
                }
                set
                {
                    // 处理下，保证不为空指针或空白字串
                    _themeName = (value == null ? string.Empty : value.Trim());
                }
            }

            // 皮肤路径
            public string ThemeFolder
            {
                get
                {
                    // 0长度字串表示没有使用皮肤
                    if(_themeName.Length == 0)
                    {
                        return _themeName;
                    }
                    else
                    {
                        // 连接皮肤目录和根目录
                        return this.BaseFolder + THEME_FOLDER + "\\" + _themeName;
                    }
                }
            }

            // 皮肤配置文件
            public string ThemeConfig
            {
                get
                {
                    return Path.Combine(this.ThemeFolder, "Config.tjs");
                }
            }

            public string[] UILayouts
            {
                get
                {
                    return Directory.GetFiles(this.ThemeFolder, UI_LAYOUT);
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

            // 根据配置的内容生成报告
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("== 项目配置清单 =="); sb.Append(Environment.NewLine);

                sb.Append(Environment.NewLine);
                string theme = this._themeName;
                if (string.IsNullOrEmpty(theme)) theme = NAME_DEFAULT_THEME;
                sb.AppendFormat("所选皮肤：{0}", theme); sb.Append(Environment.NewLine);
                sb.AppendFormat("分辨率设定：{0}x{1}", this._width, this._height); sb.Append(Environment.NewLine);
                
                sb.Append(Environment.NewLine);
                sb.AppendFormat("项目名称：{0}", this._projectName);sb.Append(Environment.NewLine);
                sb.AppendFormat("项目位置：{0}", this.ProjectFolder); sb.Append(Environment.NewLine);
                
                sb.Append(Environment.NewLine);
                sb.AppendFormat("缩放策略：{0}", this._scaler); sb.Append(Environment.NewLine);
                sb.AppendFormat("缩放质量：{0}", this._quality); sb.Append(Environment.NewLine);
                sb.AppendFormat("NVLMaker目录：{0}", this.BaseFolder);sb.Append(Environment.NewLine);
                return sb.ToString();
            }

            // 读取当前所选皮肤目录中的说明文件
            public string GetProjectInfo()
            {
                string path = this.ThemeFolder;
                if (string.IsNullOrEmpty(path))
                {
                    path = this.BaseFolder + TEMPLATE_FOLDER + DATA_FOLDER;
                }

                try
                {
                    string readmefile = Path.Combine(path, "Readme.txt");
                    if (File.Exists(readmefile))
                    {
                        using (StreamReader r = new StreamReader(readmefile))
                        {
                            // 读取readme文件作为显示内容
                            return r.ReadToEnd();
                        }
                    }

                    string configfile = Path.Combine(path, "Config.tjs");
                    if (File.Exists(readmefile))
                    {
                        using (StreamReader r = new StreamReader(configfile))
                        {
                            // 读取config文件作为显示内容
                            return r.ReadToEnd();
                        }
                    }
                }
                catch (System.Exception e)
                {
                    return e.Message;
                }

                return string.Empty;
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
            _curConfig.BaseFolder = Directory.GetCurrentDirectory();

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
            //return;

            string[] layouts = _curConfig.UILayouts;

            // 测试tjs值读取
            foreach (string layout in layouts)
            {
                using (StreamReader r = new StreamReader(layout))
                {
                    TjsParser parser = new TjsParser();
                    TjsValue val = null;
                    do 
                    {
                        val = parser.Parse(r);
                    } while (val != null);
                }
            }

            // 测试tjs符号读取
            using (StreamReader r = new StreamReader(layouts[0]))
            {
                TjsParser parser = new TjsParser();
                TjsParser.Token token = null;
                do
                {
                    token = parser.GetNext(r);
                } while (token != null && token.t != TjsParser.TokenType.Unknow);
            }

            // 资源转换器对象的测试用例
            ResConfig config = new ResConfig();
            config.files.Add(new ResFile(@"a.png"));
            config.files.Add(new ResFile(@"b.png"));
            config.name = "TestTest";
            config.path = @"c:\";

            config.Save(@"c:\test.xml");
            ResConfig newConfig = ResConfig.Load(@"c:\test.xml");

            ResConverter cov = new ResConverter();
            cov.Start(config, @"d:\", 1024, 768, 1920, 1080);
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
            if(MessageBox.Show("开始创建项目？", "确认", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                OnBuild();
            }
        }
        
        void OnStep1()
        {
            // 刷新皮肤目录列表
            int selected = 0;
            lstTemplate.BeginUpdate();
            lstTemplate.Items.Clear();
            lstTemplate.Items.Add(NAME_DEFAULT_THEME);

            try
            {
                string lastSelect = _curConfig.ThemeName.ToLower();
                string root = _curConfig.BaseFolder;
                string[] themes = Directory.GetDirectories(root + THEME_FOLDER);
                foreach (string theme in themes)
                {
                    // 只留目录名
                    string name = Path.GetFileName(theme);
                    lstTemplate.Items.Add(name);

                    // 匹配第一个目录名相同的皮肤作为选中项，返回的时候保持选项正确
                    if (selected == 0 && lastSelect == name)
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
            // 第二步的说明窗口暂时想不到写啥，继续留着这个说明吧
            txtResolution.Text = txtTemplate.Text;

            // 读取皮肤目录下的文件列表
            string theme = _curConfig.ThemeFolder;
            if (string.IsNullOrEmpty(theme))
            {
                theme = _curConfig.BaseFolder + TEMPLATE_FOLDER + DATA_FOLDER;
            }

            // 这里本来应该根据缩放策略配置来显示每个文件如何缩放
            // 先简单列一下文件和目录吧……
            try
            {
                lstScale.BeginUpdate();
                lstScale.Items.Clear();
                string[] subDirs = Directory.GetDirectories(theme);
                foreach (string dir in subDirs)
                {
                    lstScale.Items.Add(string.Format("<dir> {0}", Path.GetFileName(dir)));
                }
                string[] files = Directory.GetFiles(theme);
                foreach (string file in files)
                {
                    lstScale.Items.Add(Path.GetFileName(file));
                }
                lstScale.EndUpdate();
            }
            catch (System.Exception){}

            test();
        }

        void OnStep3()
        {
            // 保存上一步的结果
            _curConfig._width = (int)numWidth.Value;
            _curConfig._height = (int)numHeight.Value;
            txtProjectName.Focus();
        }

        void OnStep4()
        {
            // 保存上一步的结果
            _curConfig.ProjectName = txtProjectName.Text;
            if (checkFolder.Checked)
                _curConfig.ProjectFolder = txtFolderName.Text;

            // 根据当前配置生成报告
            StringWriter otuput = new StringWriter();
            
            btnOK.Enabled = _curConfig.IsReady(otuput);
            txtReport.Text = otuput.ToString();

            btnOK.BringToFront();
            btnOK.Show();
            btnOK.Focus();
            btnExit.Hide();
        }

        void OnBuild()
        {
            // 开始建立项目
            try
            {
                // 禁止按钮
                btnPrev.Enabled = false;
                btnCancel.Enabled = false;
                btnOK.Enabled = false;
                btnExit.Enabled = false;

                ThreadStart func = delegate()
                {
                    // 先从模板目录拷贝文件到项目目录
                    string template = _curConfig.BaseFolder + TEMPLATE_FOLDER;
                    string project = _curConfig.ProjectFolder;
                    ConvertFiles(template, project);

                    // 再从皮肤目录拷贝文件到项目资料文件夹
                    string theme = _curConfig.ThemeFolder;
                    if (!string.IsNullOrEmpty(theme))
                    {
                        ConvertFiles(theme, project + DATA_FOLDER);
                    }

                    // TODO: 修正所有坐标，写入项目名称

                };

                // 启动一个线程来拷贝文件，防止UI死锁
                Thread t = new Thread(func);
                t.Start();
                while(!t.Join(100))
                {
                    Application.DoEvents();
                }
                
                // 建立完成，显示退出按钮
                btnOK.Hide();
                btnExit.BringToFront();
                btnExit.Show();
                btnExit.Enabled = true;

                txtReport.Text += "项目建立完毕！";
            }
            catch (System.Exception e)
            {
                // 显示错误原因
                txtReport.Text += e.Message;

                // 恢复按钮
                btnCancel.Enabled = true;
                btnPrev.Enabled = true;
            }
        }

        // 工具函数：创建文件夹，并记录其中的文件
        void CreateDir(string source, string dest, List<string> files)
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

        // 工具函数：拷贝并缩放文件
        void ConvertFiles(string srcPath, string destPath)
        {
            string title = this.Text;

            // 源文件列表
            List<string> srcFiles = new List<string>();
            try
            {
                // 建立目录并获取文件列表
                CreateDir(srcPath, destPath, srcFiles);

                // 转换图片文件，其他文件直接拷贝
                ResConfig resource = new ResConfig();
                resource.path = srcPath;
                resource.name = NAME_DEFAULT_THEME;

                // TODO: 从配置里读取源图片大小
                int sw = 1024, sh = 768;
                int dw = _curConfig._width, dh = _curConfig._height;

                int cutLen = srcPath.Length;
                foreach (string srcfile in srcFiles)
                {
                    // 截掉模板目录以径获取相对路径
                    string relFile = srcfile.Substring(cutLen + 1);

                    // 取得扩展名
                    string ext = Path.GetExtension(relFile).ToLower();

                    if ( (sw != dw || sh != dh) && // 宽高如果和源文件相同那就不用转换了
                         (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp") )
                    {
                        // 是图片则添加到转换器中
                        resource.files.Add(new ResFile(relFile));
                    }
                    else
                    {
                        // 直接拷贝
                        this.BeginInvoke(new ThreadStart(delegate()
                        {
                            this.Text = string.Format("{0}: 拷贝{1}", title, relFile);
                        }));

                        File.Copy(srcfile, Path.Combine(destPath, relFile), true);
                    }
                }

                this.BeginInvoke(new ThreadStart(delegate()
                {
                    this.Text = string.Format("{0}: 图片转换中……", title);
                }));

                if (resource.files.Count > 0)
                {
                    ResConverter conv = new ResConverter();
                    conv.Start(resource, destPath, sw, sh, dw, dh);
                }

            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }

            this.Invoke(new ThreadStart(delegate()
            {
                this.Text = title;
            }));
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
            if (!checkFolder.Checked)
            {
                txtFolderName.Text = txtProjectName.Text;
            }
        }

        private void lstTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 记录选取的皮肤目录
            if (lstTemplate.SelectedIndex > 0)
            {
                string lastSelect = lstTemplate.SelectedItem as string;
                _curConfig.ThemeName = lastSelect.Trim();
            }
            else
            {
                _curConfig.ThemeName = string.Empty;
            }

            txtTemplate.Text = _curConfig.GetProjectInfo();
        }

        private void Wizard_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!btnExit.Enabled)
            {
                MessageBox.Show("正在创建项目，请稍候……");
                e.Cancel = true;
            }
        }
    }
}