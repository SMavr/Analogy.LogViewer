﻿using DevExpress.XtraEditors;

namespace Analogy.WhatIsNew
{
    public partial class WhatIsNew4_8_4 : DevExpress.XtraEditors.XtraUserControl
    {
        public WhatIsNew4_8_4()
        {
            InitializeComponent();
        }
        private void OpenGithubIssue(object sender, DevExpress.Utils.HyperlinkClickEventArgs e)
        {
            ((HyperlinkLabelControl)sender).LinkVisited = true;
            Utils.OpenLink(e.Link);
        }

    }
}
