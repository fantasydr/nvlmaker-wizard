using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ResConverter
{
    public partial class Wizard : Form
    {
        int _curStep = -1;

        GroupBox[] _steps = null;

        //当前页面
        int Step
        {
            get { return _curStep; }
            set
            {
                // 初始化向导各步骤面板
                if (_steps == null)
                {
                    _steps = new GroupBox[] { gbStep1, gbStep2, gbStep3, gbStep4 };
                    for (int i = 1; i < _steps.Length; i++)
                    {
                        _steps[i].Location = _steps[0].Location;
                    }
                }

                _curStep = value;
                if (_curStep < 0) 
                {
                    _curStep = 0; 
                }
                else if (_curStep >= _steps.Length) 
                {
                    _curStep = _steps.Length - 1; 
                }

                // 按照当前步骤显式隐藏对应面板
                _steps[_curStep].BringToFront();

                // 控制按钮
                btnNext.Enabled = _curStep < _steps.Length - 1;
                btnPrev.Enabled = _curStep > 0;
                btnOK.Enabled = _curStep == _steps.Length - 1;

                if (!btnPrev.Enabled)
                {
                    btnNext.Focus();
                }

                if (btnOK.Enabled)
                {
                    btnOK.BringToFront();
                    btnOK.Focus();
                }
                
                if (btnNext.Enabled)
                {
                    btnNext.BringToFront();
                }
            }
        }

        public Wizard()
        {
            InitializeComponent();

            this.Step = 0;
        }

        private void test()
        {
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
    }
}