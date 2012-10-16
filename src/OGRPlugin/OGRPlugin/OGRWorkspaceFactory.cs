/******************************************************************************
 * $Id$
 *
 * Project:  GDAL/OGR ArcGIS Datasource
 * Purpose:  Workspace Factory Implementation for ArcGIS
 * Author:   Ragi Yaser Burhum, ragi@burhum.com
 *
 ******************************************************************************
 * Copyright (c) 2012, Ragi Yaser Burhum
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;

using System.Windows.Forms;

using OSGeo.OGR;


namespace GDAL.OGRPlugin
{
    [Guid("36eb2528-077b-4d83-a73e-2d83e284c000")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("OGRPlugin.OGRWorkspaceFactory")]
    [ComVisible(true)]
    public sealed class OGRWorkspaceFactory : IPlugInWorkspaceFactoryHelper
    {
        #region "Component Category Registration"

        [ComRegisterFunction()]
        public static void RegisterFunction(String regKey)
        {
            PlugInWorkspaceFactoryHelpers.Register(regKey);
        }

        [ComUnregisterFunction()]
        public static void UnregisterFunction(String regKey)
        {
            PlugInWorkspaceFactoryHelpers.Unregister(regKey);
        }
        #endregion


        #region class constructor
        public OGRWorkspaceFactory()
        {
            System.Environment.SetEnvironmentVariable("GDAL_DATA", System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\gdal-data");
        }
        #endregion

        #region IPlugInWorkspaceFactoryHelper Members

        public string get_DatasetDescription(esriDatasetType DatasetType)
        {
            switch (DatasetType)
            {
                case esriDatasetType.esriDTTable:
                    return "OGR Table";
                case esriDatasetType.esriDTFeatureClass:
                    return "OGR Feature Class";
                case esriDatasetType.esriDTFeatureDataset:
                    return "OGR Feature Dataset";

                default:
                    return null;
            }
        }

        public string get_WorkspaceDescription(bool plural)
        {
            return "OGR Workspace";
        }

        public bool CanSupportSQL
        {
            get { return true; }
        }

        public string DataSourceName
        {
            /*
             * This is how the prog id will be found by the PluginWorkspaceHelper
             * it will expect a string in the form of
             * 
             *        ProgID = esriGeoDatabase.<DataSourceName>WorkspaceFactory
             *        
             * so in our case it will be:
             * 
             *       ProgID = esriGeoDatabase.OGRPluginWorkspaceFactory
             *       
             * see: 
             *    http://help.arcgis.com/en/sdk/10.0/arcobjects_net/conceptualhelp/index.html#/Adding_a_plug_in_data_source_programmatically/000100000305000000/
             */
            get { return "OGRPlugin"; }
        }


        public bool ContainsWorkspace(string parentDirectory, IFileNames fileNames)
        {
            // TODO: Look into openshared to see if we can optimize it

            if (this.IsWorkspace(parentDirectory))
                return true;
            else if (fileNames == null)
                return false; //isWorkspace is false and no filenames

            // isWorkspace is false, but at least we can try other files
            if (!System.IO.Directory.Exists(parentDirectory))
                return false;

            string sFileName;
            while ((sFileName = fileNames.Next()) != null)
            {
                if (fileNames.IsDirectory())
                    continue;

                if (this.IsWorkspace(parentDirectory + "\\" + sFileName))
                    return true;
            }

            return false;
        }

        public UID WorkspaceFactoryTypeID
        {
            get
            {
                UID wkspFTypeID = new UIDClass();
                wkspFTypeID.Value = "{6381befb-b2bc-4715-a836-8995112333de}";	//proxy
                return wkspFTypeID;
            }
        }

        public bool IsWorkspace(string wksString)
        {
            OSGeo.OGR.DataSource ds = null;
            ds = OSGeo.OGR.Ogr.OpenShared(wksString, 0);

            if (ds != null)
                return true;
            else
                return false;
        }

        public esriWorkspaceType WorkspaceType
        {
            //TODO: WorkspaceType in OGR can be remote - test later what happens when we change this
            get
            {
                return esriWorkspaceType.esriFileSystemWorkspace;
            }
        }

        public IPlugInWorkspaceHelper OpenWorkspace(string wksString)
        {
            OSGeo.OGR.DataSource ds = OSGeo.OGR.Ogr.OpenShared(wksString, 0);

            if (ds != null)
            {
                OGRWorkspace openWksp = new OGRWorkspace(ds, wksString);
                return (IPlugInWorkspaceHelper)openWksp; 
            }

            return null;
        }

        public string GetWorkspaceString(string parentDirectory, IFileNames fileNames)
        {
            if (fileNames == null)
            {
                //could be a database connection
                if (this.IsWorkspace(parentDirectory))
                    return parentDirectory;

                return null;
            }

            // GetWorkspaceString - claim and remove file names from list. What a silly design!

            string sFileName;
            bool fileFound = false;
            while ((sFileName = fileNames.Next()) != null)
            {
                if (this.IsWorkspace(sFileName))
                {
                    fileFound = true;
                    fileNames.Remove();
                }
            }

            if (fileFound)
                return parentDirectory;
            else
                return null;
        }

        #endregion
    }
}
