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
using System.Diagnostics;
using System.Text.RegularExpressions;

//
// app.ico == Any closet is a walk-in closet if you try hard enough..ico
// Based on icons by Paul Davey aka Mattahan. All rights reserved.
// 

namespace Wizard
{
    public partial class Wizard : Form
    {
        const string NAME_CUSTOM_RESOLUTION = "(自定义)";

        // 正在操作的配置
        WizardConfig _curConfig = new WizardConfig();

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
            return;

            string strTitle = ";System.title =\"模板工程\";";
            string strW = ";scWidth =1024;";
            string strH = ";scHeight =768;";

            Regex regTitle = new Regex(@"\s*;\s*System.title\s*=");
            Regex regW = new Regex(@"\s*;\s*scWidth\s*=");
            Regex regH = new Regex(@"\s*;\s*scHeight\s*=");

            bool ret = false;
            ret = regTitle.IsMatch(strTitle);
            ret = regW.IsMatch(strW);
            ret = regH.IsMatch(strH);

            string[] layouts = Directory.GetFiles(_curConfig.ThemeDataFolder, WizardConfig.UI_LAYOUT);

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
            if(MessageBox.Show("开始创建项目？", this.Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                OnBuild();
            }
        }
        
        void OnStep1()
        {
            // 刷新主题目录列表
            int selected = 0;
            lstTemplate.BeginUpdate();
            lstTemplate.Items.Clear();
            lstTemplate.Items.Add(WizardConfig.NAME_DEFAULT_THEME);

            try
            {
                string lastSelect = _curConfig.ThemeName.ToLower();
                string root = _curConfig.BaseFolder;
                string[] themes = Directory.GetDirectories(root + WizardConfig.THEME_FOLDER);
                foreach (string theme in themes)
                {
                    // 只留目录名
                    string name = Path.GetFileName(theme);
                    lstTemplate.Items.Add(name);

                    // 匹配第一个目录名相同的主题作为选中项，返回的时候保持选项正确
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
            ProjectProperty info = _curConfig.ReadThemeInfo();

            txtResolution.Text = "图片原始分辨率：";

            // 第二步的说明窗口，目前也只有这么一个属性可以显示
            string name = _curConfig.IsDefaultTheme ? WizardConfig.NAME_DEFAULT_THEME:_curConfig.ThemeName;
            txtResolution.Text += string.Format("{0}{0}【{3}】: {1}x{2}",
                                               Environment.NewLine, info.width, info.height, name);

            // 是否选择了默认主题，没选则附加默认主题属性
            if (!_curConfig.IsDefaultTheme)
            {
                ProjectProperty baseInfo = _curConfig.ReadBaseTemplateInfo();
                txtResolution.Text += string.Format("{0}{0}【{3}】: {1}x{2}",
                    Environment.NewLine, baseInfo.width, baseInfo.height, WizardConfig.NAME_DEFAULT_THEME);
            }

            // 选定分辨率
            int w = info.width, h = info.height;
            for(int i=0;i<cbResolution.Items.Count;i++)
            {
                Resolution r = cbResolution.Items[i] as Resolution;
                if (r != null && r._w == w && r._h == h )
                {
                    cbResolution.SelectedIndex = i;
                    break;
                }
            }            

            // 这里本来应该根据缩放策略配置来显示每个文件如何缩放
            // 先简单列一下文件和目录吧……
            LoadThemeFiles();

            // 调用下测试用的函数
            test();
        }

        void OnStep3()
        {
            // 保存上一步的结果
            _curConfig._width = (int)numWidth.Value;
            _curConfig._height = (int)numHeight.Value;
            
            txtProjectName.SelectAll();
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
            ReportRefresh(otuput.ToString());

            btnOK.BringToFront();
            btnOK.Show();
            btnOK.Focus();
            btnExit.Hide();
        }

        void OnBuild()
        {
            // 开启Logging
            LoggingBegin();

            // 开始建立项目
            try
            {
                // 禁止按钮
                btnPrev.Enabled = false;
                btnCancel.Enabled = false;
                btnOK.Enabled = false;
                btnExit.Enabled = false;

                // 启动一个线程来拷贝文件，防止UI死锁
                Thread t = new Thread(new ThreadStart(BuildProject));
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

                ReportAppend("项目建立完毕！");
            }
            catch (System.Exception e)
            {
                // 显示错误原因
                ReportAppend(e.Message);

                // 恢复按钮
                btnCancel.Enabled = true;
                btnPrev.Enabled = true;
            }

            // 结束Logging
            LoggingEnd();
        }

        // 根据配置创建目标项目
        private void BuildProject()
        {
            // 从配置中读取需要的源大小和目标大小
            int dw = _curConfig._width, dh = _curConfig._height;

            // 先从基础模板目录拷贝文件到项目目录
            string template = _curConfig.BaseTemplateFolder;
            string project = _curConfig.ProjectFolder;

            // 读取基础模板的配置
            ProjectProperty baseInfo = _curConfig.ReadBaseTemplateInfo();

            int sw = baseInfo.width;
            if (sw <= 0) sw = WizardConfig.DEFAULT_WIDTH;
            int sh = baseInfo.height;
            if (sh <= 0) sh = WizardConfig.DEFAULT_HEIGHT;

            ConvertFiles(template, sw, sh, project, dw, dh);

            // 修正所有坐标，写入项目名称
            AdjustSettings(sw, sh);

            // 如果选择了非默认主题，再从主题目录拷贝文件到项目资料文件夹
            if (_curConfig.ThemeFolder != template)
            {
                // 读取所选主题配置
                ProjectProperty themeInfo = _curConfig.ReadThemeInfo();

                sw = themeInfo.width;
                if (sw <= 0) sw = WizardConfig.DEFAULT_WIDTH;
                sh = themeInfo.height;
                if (sh <= 0) sh = WizardConfig.DEFAULT_HEIGHT;

                // 主题的文件直接拷入数据目录
                ConvertFiles(_curConfig.ThemeFolder, sw, sh, _curConfig.ProjectDataFolder, dw, dh);

                // 修正所有坐标，写入项目名称
                AdjustSettings(sw, sh);
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
                         (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".bmp") )
                    {
                        // 是图片则添加到转换器中
                        resource.files.Add(new ResFile(relFile));
                    }
                    else
                    {
                        // 直接拷贝
                        Logging(string.Format("拷贝{0}", relFile));
                        File.Copy(srcfile, Path.Combine(destPath, relFile), true);
                    }
                }

                Logging("图片转换中……");

                if (resource.files.Count > 0)
                {
                    // 创建一个图片转换器并开始转换
                    ResConverter conv = new ResConverter();
                    conv.NotifyProcessEvent += new ResConverter.NotifyProcessHandler(conv_NotifyProcessEvent);
                    conv.Start(resource, destPath, sw, sh, dw, dh);
                }

            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        // 工具函数：修正目标项目文件夹中的配置
        void AdjustSettings(int sw, int sh)
        {
            string dataPath = _curConfig.ProjectDataFolder;
            int dh = _curConfig._height;
            int dw = _curConfig._width;
            string title = _curConfig.ProjectName;

            try
            {
                WizardConfig.ModifySetting(dataPath, title, dh, dw);
            }
            catch (System.Exception e)
            {
                ReportAppend("修改setting.tjs失败:" + e.Message);
            }

            try
            {
                WizardConfig.ModifyConfig(dataPath, title, dh, dw);
            }
            catch (System.Exception e)
            {
                ReportAppend("修改Config.tjs失败:" + e.Message);
            }
            
            // 检查是否需要转换
            if (sw != dw || sh != dh)
            {
                try
                {
                    WizardConfig.ModifyLayout(dataPath, sw, sh, dh, dw);
                }
                catch (System.Exception e)
                {
                    ReportAppend("修改界面布局文件失败:" + e.Message);
                }
            }
        }

        // 读了主题目录中所有的目录和根目录下的问卷
        private void LoadThemeFiles()
        {
            // 主题的Data目录
            string theme = _curConfig.ThemeFolder;

            try
            {
                lstScale.BeginUpdate();
                lstScale.Items.Clear();

                // 读取主题目录下的文件列表
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
            catch (System.Exception e) { }
        }

        #region Logging函数
        string _titleSaved = null;
        void LoggingBegin()
        {
            // 保存窗口标题
            if (_titleSaved == null) { _titleSaved = this.Text; }
        }
        void LoggingEnd()
        {
            // 恢复窗口标题
            this.Invoke(new ThreadStart(delegate()
            {
                if (_titleSaved != null) { this.Text = _titleSaved; _titleSaved = null; }
            }));
        }
        void Logging(string msg)
        {
            if (_titleSaved == null)
            {
                Debug.Assert(false, "call LoggingBegin() first");
                return;
            }

            this.BeginInvoke(new ThreadStart(delegate()
            {
                this.Text = string.Format("{0}: {1}", _titleSaved, msg);
            }));
        }
        #endregion

        void ReportRefresh(string report)
        {
            this.Invoke(new ThreadStart(delegate()
            {
                txtReport.Text = report;
            }));
        }

        void ReportAppend(string report)
        {
            this.BeginInvoke(new ThreadStart(delegate()
            {
                txtReport.Text += report + Environment.NewLine;
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
            // 记录选取的主题目录
            if (lstTemplate.SelectedIndex > 0)
            {
                string lastSelect = lstTemplate.SelectedItem as string;
                _curConfig.ThemeName = lastSelect.Trim();
            }
            else
            {
                _curConfig.ThemeName = string.Empty;
            }

            ProjectProperty info = _curConfig.ReadThemeInfo();
            txtTemplate.Text = info.readme;
            txtProjectName.Text = info.title;
        }

        private void Wizard_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!btnExit.Enabled)
            {
                MessageBox.Show("正在创建项目，请稍候……");
                e.Cancel = true;
            }
        }

        void conv_NotifyProcessEvent(ResConverter sender, ResConverter.NotifyProcessEventArgs e)
        {
            Logging(string.Format("({0}/{1}){2} 转换中……", e.index, e.count, e.file));
        }
    }
}