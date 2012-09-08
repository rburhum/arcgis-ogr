/******************************************************************************
 * $Id$
 *
 * Project:  GDAL/OGR ArcGIS Datasource
 * Purpose:  Workspace Implementation for ArcGIS
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
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;


namespace GDAL.OGRPlugin
{
    [ComVisible(false)]
    internal class OGRWorkspace : IPlugInWorkspaceHelper, IPlugInMetadataPath
    {
        private string m_sWkspPath;

        public OGRWorkspace(string wkspPath)
        {
            //HIGHLIGHT: set up workspace path
            if (System.IO.Directory.Exists(wkspPath))
                m_sWkspPath = wkspPath;
            else
                m_sWkspPath = null;
        }

        #region IPlugInWorkspaceHelper Members
        public bool OIDIsRecordNumber
        {
            get
            {
                return true;	//OID's are continuous
            }
        }

        public IArray get_DatasetNames(esriDatasetType DatasetType)
        {
            if (m_sWkspPath == null)
                return null;

            //HIGHLIGHT: get_DatasetNames - Go through wksString to look for csp files
            if (DatasetType != esriDatasetType.esriDTAny &&
                DatasetType != esriDatasetType.esriDTTable)
                return null;

            string[] sFiles = System.IO.Directory.GetFiles(m_sWkspPath, "*.csp");
            if (sFiles == null || sFiles.Length == 0)
                return null;

            IArray datasets = new ArrayClass();
            foreach (string sFileName in sFiles)
            {
                OGRDataset ds = new OGRDataset(m_sWkspPath, System.IO.Path.GetFileNameWithoutExtension(sFileName));
                datasets.Add(ds);
            }

            return datasets;
        }

        public IPlugInDatasetHelper OpenDataset(string localName)
        {
            //HIGHLIGHT: OpenDataset - give workspace path and local file name
            if (m_sWkspPath == null)
                return null;

            OGRDataset ds = new OGRDataset(m_sWkspPath, localName);
            return (IPlugInDatasetHelper)ds;
        }

        public INativeType get_NativeType(esriDatasetType DatasetType, string localName)
        {
            return null;
        }

        public bool RowCountIsCalculated
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region IPlugInMetadataPath Members

        //HIGHLIGHT: implement metadata so export data in arcmap works correctly
        public string get_MetadataPath(string localName)
        {
            return System.IO.Path.Combine(m_sWkspPath, localName + ".csp.xml");
        }

        #endregion
    }
}
