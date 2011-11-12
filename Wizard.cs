using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;

using Tjs;

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

                // 禁用上一个页面
                if (_curStep >= 0 && _curStep < _stepGroups.Length)
                {
                    _stepGroups[_curStep].Enabled = false;
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
                _stepGroups[_curStep].Enabled = true;

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
                _stepGroups[i].Enabled = false;
            }

            // 绑定当前方法
            _stepHandlers = new StepHandler[] { 
                new StepHandler(this.OnStep1),
                new StepHandler(this.OnStep2), 
                new StepHandler(this.OnStep3),
                new StepHandler(this.OnStep4),
            };

            this.Step = 0;

            // 读取默认模板信息
            LoadProjectProperty();

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

            txtResolution.Text = "原始分辨率：";

            // 第二步的说明窗口，目前也只有这么一个属性可以显示
            string name = _curConfig.IsDefaultTheme ? WizardConfig.NAME_DEFAULT_THEME : _curConfig.ThemeName;
            txtResolution.Text += string.Format("{0}{0}【{3}】 {1}x{2}",
                                               Environment.NewLine, info.width, info.height, name);

            // 是否选择了默认主题，没选则附加默认主题属性
            if (!_curConfig.IsDefaultTheme)
            {
                ProjectProperty baseInfo = _curConfig.ReadBaseTemplateInfo();
                txtResolution.Text += string.Format("{0}{0}【{3}】 {1}x{2}",
                    Environment.NewLine, baseInfo.width, baseInfo.height, WizardConfig.NAME_DEFAULT_THEME);

                txtResolution.Text += string.Format("{0}{0}注意：【{2}】将覆盖【{1}】中的同名文件。",
                                               Environment.NewLine, WizardConfig.NAME_DEFAULT_THEME, name);
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
            {
                _curConfig.ProjectFolder = txtFolderName.Text;
            }

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

                // 创建项目转换器，关联项目配置，并绑定UI显示事件
                WizardConverter conv = new WizardConverter(_curConfig);
                conv.NotifyProcessEvent += new ResConverter.NotifyProcessHandler(conv_NotifyProcessEvent);
                conv.LoggingEvent += new WizardConverter.MessageHandler(conv_LoggingEvent);
                conv.ErrorEvent += new WizardConverter.MessageHandler(conv_ErrorEvent);

                // 启动一个线程来拷贝文件，防止UI死锁
                Thread t = new Thread(new ThreadStart(conv.Start));
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

                ReportAppend("项目创建完毕！");
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

        // 简单的Logging方法，直接打印在标题栏
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

        // 直接在文本控件中显示一些消息
        void ReportAppend(string report)
        {
            this.BeginInvoke(new ThreadStart(delegate()
            {
                txtReport.Text += report + Environment.NewLine;
            }));
        }

        // 读了主题目录中所有的目录和根目录下的文件
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

        private void LoadProjectProperty()
        {
            // 读取项目说明
            ProjectProperty info = _curConfig.ReadThemeInfo();
            txtTemplate.Text = info.readme;
            txtProjectName.Text = info.title;

            // 选定分辨率
            int w = info.width, h = info.height;
            for (int i = 0; i < cbResolution.Items.Count; i++)
            {
                Resolution r = cbResolution.Items[i] as Resolution;
                if (r != null && r._w == w && r._h == h)
                {
                    cbResolution.SelectedIndex = i;
                    break;
                }
            }
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

        private void txtProjectName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnNext_Click(sender, null);
            }
        }

        private void lstTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 记录选取的主题目录
            string theme = string.Empty;
            if (lstTemplate.SelectedIndex > 0)
            {
                string lastSelect = lstTemplate.SelectedItem as string;
                theme = lastSelect.Trim();
            }
            
            if(theme != _curConfig.ThemeName)
            {
                _curConfig.ThemeName = theme;
                LoadProjectProperty();
            }
        }

        private void Wizard_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!btnExit.Enabled)
            {
                MessageBox.Show("正在创建项目，请稍候……", (_titleSaved != null) ? _titleSaved : this.Text);
                e.Cancel = true;
            }
        }

        void conv_NotifyProcessEvent(ResConverter sender, ResConverter.NotifyProcessEventArgs e)
        {
            Logging(string.Format("({0}/{1}){2} 转换中……", e.index, e.count, e.file));
        }

        void conv_ErrorEvent(WizardConverter sender, WizardConverter.MessageEventArgs e)
        {
            ReportAppend(e.msg);
        }

        void conv_LoggingEvent(WizardConverter sender, WizardConverter.MessageEventArgs e)
        {
            Logging(e.msg);
        }
    }
}