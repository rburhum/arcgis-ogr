using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;

namespace GDAL.OGRPlugin
{
	[System.Runtime.InteropServices.ComVisible(false)]
    public partial class OGRAddLayerDialog : Form
    {
        #region class members
        private IHookHelper m_hookHelper = null;
        IWorkspace m_workspace = null;
        #endregion

        #region class constructor
        public OGRAddLayerDialog(IHookHelper hookHelper)
        {
            if (null == hookHelper)
                throw new Exception("Hook helper is not initialized");

            InitializeComponent();

            m_hookHelper = hookHelper;
        }
        #endregion

        #region UI event handlers
        private void btnOpenDataSource_Click(object sender, EventArgs e)
        {
            resetWorkspace();

            string path = GetFileName();
            if (string.Empty == path)
                return;

            //update the path textbox
            txtPath.Text = path;

            m_workspace = OpenPlugInWorkspace(path);

            ListFeatureClasses();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            resetWorkspace();

            if (txtConnString.Text == string.Empty)
                return;

            m_workspace = OpenPlugInWorkspace(txtConnString.Text);

            ListFeatureClasses();
        }

        private void lstDeatureClasses_DoubleClick(object sender, EventArgs e)
        {
            this.Hide();
            OpenDataset();
            this.Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Hide();
            OpenDataset();
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        #region private methods
        private string GetFileName()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            
            dlg.Title = "Open file";
            dlg.RestoreDirectory = true;
            dlg.CheckPathExists = true;
            dlg.CheckFileExists = true;
            dlg.Multiselect = false;

            DialogResult dr = dlg.ShowDialog();
            if (DialogResult.OK == dr)
                return dlg.FileName;

            return string.Empty;
        }

        private IWorkspace OpenPlugInWorkspace(string connString)
        {
            try
            {
                //get the type using the ProgID
                Type t = Type.GetTypeFromProgID("esriGeoDatabase.OGRPluginWorkspaceFactory");

                //Use activator in order to create an instance of the workspace factory
                IWorkspaceFactory workspaceFactory = Activator.CreateInstance(t) as IWorkspaceFactory;
              
                IWorkspace pWorspace = workspaceFactory.OpenFromFile(connString, 0);               
               
                return pWorspace;              
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                
                return null;
            }
        }

        private void ListFeatureClasses()
        {
            lstFeatureClasses.Items.Clear();

            if (null == m_workspace)
                return;

            IEnumDatasetName datasetNames = m_workspace.get_DatasetNames(esriDatasetType.esriDTAny);
            datasetNames.Reset();
            IDatasetName dsName;
            while ((dsName = datasetNames.Next()) != null)
            {
                lstFeatureClasses.Items.Add(dsName.Name);
            }

            //select the first dataset on the list
            if (lstFeatureClasses.Items.Count > 0)
            {
                lstFeatureClasses.SelectedIndex = 0;
                lstFeatureClasses.Refresh();
            }
        }

        private void OpenDataset()
        {

            if (null == m_hookHelper || null == m_workspace)
                return;

            if (string.Empty == (string)lstFeatureClasses.SelectedItem)
                return;

            bool refreshActiveView = false;

            foreach (string dataset in lstFeatureClasses.SelectedItems)
            {

                //get the selected item from the listbox
                //string dataset = (string)lstFeatureClasses.SelectedItem;

                //cast the workspace into a feature workspace
                IFeatureWorkspace featureWorkspace = m_workspace as IFeatureWorkspace;
                if (null == featureWorkspace)
                    return;

                //get a featureclass (or standalone table) from the workspace
                ITable table = featureWorkspace.OpenTable(dataset) as ITable;

                if (table == null)
                {
                    System.Windows.Forms.MessageBox.Show("Failed to open " + dataset);
                    return;
                }

                // figure out if it is a table or featureclass

                IFeatureClass featureClass = table as IFeatureClass;

                if (featureClass == null)
                {
                    // add as table

                    IStandaloneTableCollection tableCollection = m_hookHelper.FocusMap as IStandaloneTableCollection;
                    IStandaloneTable standaloneTable = new StandaloneTableClass();
                    standaloneTable.Name = ((IDataset)table).Name;
                    standaloneTable.Table = table;
                    tableCollection.AddStandaloneTable(standaloneTable);

                    refreshActiveView = true;                   

                }
                else
                {
                    // add as feature class

                    IFeatureLayer featureLayer = new FeatureLayerClass();
                    featureLayer.Name = featureClass.AliasName;
                    featureLayer.FeatureClass = featureClass;
                    m_hookHelper.FocusMap.AddLayer((ILayer)featureLayer);

                }
            }

            if (refreshActiveView)
                m_hookHelper.ActiveView.ContentsChanged();
            
        }
        #endregion

        private void resetWorkspace()
        {
            m_workspace = null;
            lstFeatureClasses.Items.Clear();
            txtPath.Text = "";

            if (radioFromConnstring.Checked != true) //leave it alone if it is checked
                txtConnString.Text = "";
        }

        private void toggleChildControlState(System.Windows.Forms.GroupBox groupBox, bool newState)
        {
            foreach (Control subControl in groupBox.Controls)
                subControl.Enabled = newState;
        }

        private void ToggleRadioButtons()
        {
            resetWorkspace();

            if (radioFromFile.Checked == true)
            {
                toggleChildControlState(groupFromConnString, false);
                toggleChildControlState(groupFromFile, true);
            }
            else
            {
                toggleChildControlState(groupFromConnString, true);
                toggleChildControlState(groupFromFile, false);
            }
        }

        private void radioFromConnstring_CheckedChanged(object sender, EventArgs e)
        {
            ToggleRadioButtons();
        }

        private void radioFromFile_CheckedChanged(object sender, EventArgs e)
        {
            ToggleRadioButtons();
        }

    }
}