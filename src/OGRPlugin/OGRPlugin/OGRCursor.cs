/******************************************************************************
 * $Id$
 *
 * Project:  GDAL/OGR ArcGIS Datasource
 * Purpose:  Cursor Implementation for ArcGIS
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
using ESRI.ArcGIS.Geometry;

namespace GDAL.OGRPlugin
{
    [ComVisible(false)]
    internal class OGRCursor : IPlugInCursorHelper
    {
        private bool m_isFinished;
        
        private OGRDataset m_pDataset;

        private String m_whereClause;
        private System.Array m_esriQueryFieldMap;

        private OSGeo.OGR.Feature m_currentOGRFeature;
        
        #region HRESULT definitions
        private const int E_FAIL = unchecked((int)0x80004005);
        private const int S_FALSE = 1;
        #endregion



        public OGRCursor(OGRDataset parent, String whereClause, System.Array esriQueryFieldMap)
        {
            
            m_isFinished = false;
            m_pDataset = parent;
            
            m_whereClause = whereClause;

            if (m_whereClause.Length > 0)
                m_pDataset.ogrLayer.SetAttributeFilter(m_whereClause);

            m_esriQueryFieldMap = esriQueryFieldMap;

                       
            m_pDataset.ogrLayer.ResetReading();

            m_currentOGRFeature = null;

            NextRecord();
        }

        private void ReadNextRow()
        {
            m_currentOGRFeature = m_pDataset.ogrLayer.GetNextFeature();
        }

        #region IPlugInCursorHelper Members

        public int QueryValues(IRowBuffer row)
        {
            try
            {
                if (m_currentOGRFeature == null)
                    return -1;

                // loop and only set values in the rowbuffer
                // that were requested from the fieldMap

                int count = m_esriQueryFieldMap.GetLength(0);

                for (int i = 0; i < count; i++)
                {
                    if (m_esriQueryFieldMap.GetValue(i).Equals(-1))
                        continue;

                    IField valField = m_pDataset.get_Fields(0).get_Field(i);

                    object val = ogr_utils.get_mapped_value(m_pDataset, i);
                    row.set_Value(i, val);                    
                }

                return m_currentOGRFeature.GetFID();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return -1;
            }

        }

        public void QueryShape(IGeometry pGeometry)
        {
            if (pGeometry == null)
                return;

            try
            {
                OSGeo.OGR.Geometry ogrGeometry = m_currentOGRFeature.GetGeometryRef();

                //export geometry from OGR to WKB
                int wkbSize = ogrGeometry.WkbSize();
                byte[] wkbBuffer = new byte[wkbSize];
                ogrGeometry.ExportToWkb(wkbBuffer);

                //import geometry from WKB to ESRI Shape
                IWkb pWKB = pGeometry as IWkb;
                pWKB.ImportFromWkb(wkbSize, wkbBuffer[0]);

                pGeometry.SpatialReference = m_pDataset.SpatialReference;

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(" Error: " + ex.Message);
                pGeometry.SetEmpty();
            }
        }

        
        public bool IsFinished()
        {
            return m_isFinished;
        }

        public void NextRecord()
        {
            if (m_isFinished)	//error already thrown once
                return;

            m_currentOGRFeature = m_pDataset.ogrLayer.GetNextFeature();

            if (m_currentOGRFeature == null)
            {
                m_isFinished = true;

                throw new COMException("End of OGR Plugin cursor", E_FAIL);             
            }

            return;

        }
        #endregion
    }
}
        
