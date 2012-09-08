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
            /*if (plural)
                return "OGR Data";
            else
                return "OGR Datasets";
             */ 
        }

        public bool CanSupportSQL
        {
            get { return false; }
        }

        public string DataSourceName
        {
            //HIGHLIGHT: ProgID = esriGeoDatabase.<DataSourceName>WorkspaceFactory
            get { return "OGRPlugin"; }
        }

        public bool ContainsWorkspace(string parentDirectory, IFileNames fileNames)
        {
            if (fileNames == null)
                return this.IsWorkspace(parentDirectory);

            if (!System.IO.Directory.Exists(parentDirectory))
                return false;

            string sFileName;
            while ((sFileName = fileNames.Next()) != null)
            {
                if (fileNames.IsDirectory())
                    continue;

                if (System.IO.Path.GetExtension(sFileName).Equals(".csp"))
                    return true;
            }

            return false;
        }

        public UID WorkspaceFactoryTypeID
        {
            //HIGHLIGHT: Generate a new GUID to identify the workspace factory
            get
            {
                UID wkspFTypeID = new UIDClass();
                wkspFTypeID.Value = "{6381befb-b2bc-4715-a836-8995112333de}";	//proxy
                return wkspFTypeID;
            }
        }

        public bool IsWorkspace(string wksString)
        {
            //TODO: IsWorkspace is True when folder contains csp files
            if (System.IO.Directory.Exists(wksString))
                return System.IO.Directory.GetFiles(wksString, "*.csp").Length > 0;
            return false;
        }

        public esriWorkspaceType WorkspaceType
        {
            //HIGHLIGHT: WorkspaceType - FileSystem type strongly recommended
            get
            {
                return esriWorkspaceType.esriFileSystemWorkspace;
            }
        }

        public IPlugInWorkspaceHelper OpenWorkspace(string wksString)
        {
            //HIGHLIGHT: OpenWorkspace
            //Don't have to check if wksString contains valid data file. 
            //Any valid folder path is fine since we want paste to work in any folder
            if (System.IO.Directory.Exists(wksString))
            {
                OGRWorkspace openWksp = new OGRWorkspace(wksString);
                return (IPlugInWorkspaceHelper)openWksp;
            }
            return null;
        }

        public string GetWorkspaceString(string parentDirectory, IFileNames fileNames)
        {
            //return the path to the workspace location if 
            if (!System.IO.Directory.Exists(parentDirectory))
                return null;

            if (fileNames == null)	//don't have to check .csp file
                return parentDirectory;

            //HIGHLIGHT: GetWorkspaceString - claim and remove file names from list
            string sFileName;
            bool fileFound = false;
            while ((sFileName = fileNames.Next()) != null)
            {
                if (fileNames.IsDirectory())
                    continue;

                if (System.IO.Path.GetExtension(sFileName).Equals(".csp"))
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
