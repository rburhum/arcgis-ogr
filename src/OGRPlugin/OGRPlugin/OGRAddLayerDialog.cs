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
    public partial class OGRAddLayerDialog : Form
    {
        #region class memebers
        private IHookHelper m_hookHelper = null;
        IWorkspace m_workspace = null;
        #endregion

        #region class constructor
        public OGRAddLayerDialog(IHookHelper hookHelper)
        {
            if (null == hookHelper)
                throw new Exception("Hook helper is not initialize");

            InitializeComponent();

            m_hookHelper = hookHelper;
        }
        #endregion

        #region UI event handlers
        private void btnOpenDataSource_Click(object sender, EventArgs e)
        {
           
            m_workspace = OpenPlugInWorkspace();
            System.Windows.Forms.MessageBox.Show("Opened");

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
            dlg.Filter = "Simple Point (*.csp)|*.csp";
            dlg.Title = "Open Simple Point file";
            dlg.RestoreDirectory = true;
            dlg.CheckPathExists = true;
            dlg.CheckFileExists = true;
            dlg.Multiselect = false;

            DialogResult dr = dlg.ShowDialog();
            if (DialogResult.OK == dr)
                return dlg.FileName;

            return string.Empty;
        }

        private IWorkspace OpenPlugInWorkspace()
        {
            try
            {
                string path = GetFileName();
                if (string.Empty == path)
                    return null;

                //update the path textbox
                txtPath.Text = path;

                //get the type using the ProgID
                Type t = Type.GetTypeFromProgID("esriGeoDatabase.OGRPluginWorkspaceFactory");

                if (t == null)
                    MessageBox.Show("T is null");
                else
                    MessageBox.Show("T was found");

                //Use activator in order to create an instance of the workspace factory
                IWorkspaceFactory workspaceFactory = Activator.CreateInstance(t) as IWorkspaceFactory;

                if (workspaceFactory == null)
                    MessageBox.Show("Factory is null");
                else
                    MessageBox.Show("Factory was found");

                //open the workspace
                return workspaceFactory.OpenFromFile(System.IO.Path.GetDirectoryName(path), 0);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
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
            try
            {
                if (null == m_hookHelper || null == m_workspace)
                    return;

                if (string.Empty == (string)lstFeatureClasses.SelectedItem)
                    return;

                //get the selected item from the listbox
                string dataset = (string)lstFeatureClasses.SelectedItem;

                //cast the workspace into a feature workspace
                IFeatureWorkspace featureWorkspace = m_workspace as IFeatureWorkspace;
                if (null == featureWorkspace)
                    return;

                //get a featureclass from the workspace
                IFeatureClass featureClass = featureWorkspace.OpenFeatureClass(dataset);

                //create a new feature layer and add it to the map
                IFeatureLayer featureLayer = new FeatureLayerClass();
                featureLayer.Name = featureClass.AliasName;
                featureLayer.FeatureClass = featureClass;
                m_hookHelper.FocusMap.AddLayer((ILayer)featureLayer);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
        }
        #endregion
    }
}