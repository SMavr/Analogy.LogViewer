﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Analogy.Forms;

namespace Analogy.UserControls
{
    public partial class UCLogs : CommonControls.UserControls.LogMessagesUC
    {
        public UCLogs() : base(UserSettingsManager.UserSettings, ExtensionsManager.Instance, FactoriesManager.Instance, AnalogyLogger.Instance)
        {
            InitializeComponent();
            SetHighlightSettings(() =>
            {
                var user = new ApplicationSettingsForm("Color Highlighting");
                user.ShowDialog(this);
            });
        }
    }
}
