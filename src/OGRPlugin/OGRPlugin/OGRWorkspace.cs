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

using OSGeo.OGR;

namespace GDAL.OGRPlugin
{
    [ComVisible(false)]
    internal class OGRWorkspace : IPlugInWorkspaceHelper, IPlugInMetadataPath
    {
        private string m_connString;
        private OSGeo.OGR.DataSource m_datasource;

        public OGRWorkspace(OSGeo.OGR.DataSource ds, string connString)
        {
            m_connString = connString;
            m_datasource = ds;
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
            if (m_connString == null)
                return null;

            if (DatasetType == esriDatasetType.esriDTAny ||
                DatasetType == esriDatasetType.esriDTFeatureClass)
            {
                IArray datasets = new ArrayClass();

                int count = m_datasource.GetLayerCount();

                for (int i = 0; i < count; i++)
                {
                    OSGeo.OGR.Layer layer = m_datasource.GetLayerByIndex(i);

                    OGRDataset dataset = new OGRDataset(layer);

                    datasets.Add(dataset);
                }

                return datasets;
            }

            return null;

        }

        public IPlugInDatasetHelper OpenDataset(string localName)
        {
            if (m_connString == null)
                return null;

            OSGeo.OGR.Layer layer = m_datasource.GetLayerByName(localName);

            if (layer == null)
                return null;
            
            OGRDataset ds = new OGRDataset(layer);

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
                return false;
            }
        }

        #endregion

        #region IPlugInMetadataPath Members

        //ugh, sorry, I am doing this for free and I hate metadata. No man, *you* do it!

        public string get_MetadataPath(string localName)
        {
            return "";
        }

        #endregion
    }
}
