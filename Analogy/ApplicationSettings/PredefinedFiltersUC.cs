﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Analogy.Common.DataTypes;
using Analogy.Common.Interfaces;
using Analogy.CommonControls.DataTypes;

namespace Analogy.ApplicationSettings
{
    public partial class PredefinedFiltersUC : DevExpress.XtraEditors.XtraUserControl
    {
        private IUserSettingsManager Settings { get; } = UserSettingsManager.UserSettings;

        public PredefinedFiltersUC()
        {
            InitializeComponent();
        }

        private void PredefinedFiltersUC_Load(object sender, EventArgs e)
        {
            LoadSettings();
            SetupEventsHandlers();
        }

        private void SetupEventsHandlers()
        {
            sbtnAddFilter.Click += (s, e) =>
            {
                Settings.PreDefinedQueries.AddFilter(teName.Text,txtbIncludeTextFilter.Text, txtbExcludeFilter.Text,
                    txtbSourcesFilter.Text, txtbModulesFilter.Text);
                lboxFilters.DataSource = Settings.PreDefinedQueries.Filters;
                lboxFilters.Refresh();
            };

            sbtnDeleteFilter.Click += (s, e) =>
            {
                if (lboxFilters.SelectedItem is PreDefineFilter filter)
                {
                    Settings.PreDefinedQueries.RemoveFilter(filter);
                    lboxFilters.DataSource = Settings.PreDefinedQueries.Filters;
                    lboxFilters.Refresh();
                }
            };
        }

        private void LoadSettings()
        {
            lboxFilters.DataSource = Settings.PreDefinedQueries.Filters;
        }

        
    }
}
